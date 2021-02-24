using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Data.Models;
using Data.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using WebApp.RabbitMQ;

namespace WebApp.Services.Singleton
{
    public class WorkerStatisticsService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly ILogger<WorkerStatisticsService> _logger;
        private readonly JobRequestProducer _producer;

        private readonly AsyncReaderWriterLock _lock = new();
        private readonly Dictionary<string, (string Token, DateTime TimeStamp)> _dictionary = new();

        public WorkerStatisticsService(IServiceProvider provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _logger = provider.GetRequiredService<ILogger<WorkerStatisticsService>>();
            _producer = provider.GetRequiredService<JobRequestProducer>();
        }

        public async Task<int> GetAvailableWorkerCountAsync()
        {
            using var locked = await _lock.ReaderLockAsync();
            return _dictionary.Count;
        }

        private async Task HandleBrokenWorkerAsync(string name)
        {
            _logger.LogInformation($"HandleBrokenWorkerAsync Name={name}");

            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var problemStatisticsService = scope.ServiceProvider.GetRequiredService<ProblemStatisticsService>();

            #region Check for broken submission jobs

            var submissions = await context.Submissions
                .Where(s => s.Verdict == Verdict.Running && s.JudgedBy == name)
                .ToListAsync();

            foreach (var submission in submissions)
            {
                submission.ResetVerdictFields();
            }

            context.UpdateRange(submissions);
            await context.SaveChangesAsync();

            foreach (var submission in submissions)
            {
                // Trigger a rejudge for failed submissions
                if (await _producer.SendAsync(JobType.JudgeSubmission, submission.Id, submission.RequestVersion + 1))
                {
                    submission.Verdict = Verdict.InQueue;
                    await problemStatisticsService.InvalidStatisticsAsync(submission.ProblemId);
                }
                else
                {
                    submission.Verdict = Verdict.Pending;
                }
            }

            context.UpdateRange(submissions);
            await context.SaveChangesAsync();

            #endregion

            #region Check for broken plagiarism checks

            var plagiarisms = await context.Plagiarisms
                .Where(p => p.ResultsSerialized == "null" && p.CheckedBy == name)
                .ToListAsync();
            foreach (var plagiarism in plagiarisms)
            {
                plagiarism.CheckedBy = null;
                await _producer.SendAsync(JobType.CheckPlagiarism, plagiarism.Id, plagiarism.RequestVersion + 1);
            }
            context.UpdateRange(plagiarisms);
            await context.SaveChangesAsync();

            #endregion
        }

        public async Task CheckWorkersAndSubmissionsAsync(CancellationToken stoppingToken)
        {
            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var problemStatisticsService = scope.ServiceProvider.GetRequiredService<ProblemStatisticsService>();

            #region Check for broken workers

            var locked = await _lock.WriterLockAsync();
            var now = DateTime.Now.ToUniversalTime();
            var workers = _dictionary
                .Where(p => p.Value.TimeStamp < now.AddMinutes(-2))
                .Select(p => p.Key).ToList();
            foreach (var worker in workers)
            {
                await HandleBrokenWorkerAsync(worker);
                _ = _dictionary.Remove(worker);
            }
            locked.Dispose();

            #endregion

            #region Try send judge request for pending and stuck submissions

            var submissions = await context.Submissions
                .Where(s => s.Verdict == Verdict.Pending ||
                            (s.Verdict == Verdict.InQueue &&
                             s.UpdatedAt <= now.AddMinutes(-2) &&
                             s.UpdatedAt >= now.AddMinutes(-4)))
                .ToListAsync(stoppingToken);
            foreach (var submission in submissions)
            {
                if (await _producer.SendAsync(JobType.JudgeSubmission, submission.Id, submission.RequestVersion + 1))
                {
                    if (submission.Verdict != Verdict.InQueue)
                    {
                        submission.Verdict = Verdict.InQueue;
                    }
                    await problemStatisticsService.InvalidStatisticsAsync(submission.ProblemId);
                }
                else
                {
                    submission.Verdict = Verdict.Pending;
                }
            }

            context.UpdateRange(submissions);
            await context.SaveChangesAsync(stoppingToken);

            #endregion

            #region Try send check request for pending and stuck plagiarisms

            var plagiarisms = await context.Plagiarisms
                .Where(p => string.IsNullOrEmpty(p.CheckedBy) ||
                            (p.ResultsSerialized == "null" &&
                             p.UpdatedAt <= now.AddMinutes(-2) &&
                             p.UpdatedAt >= now.AddMinutes(-4)))
                .ToListAsync(stoppingToken);
            foreach (var plagiarism in plagiarisms)
            {
                await _producer.SendAsync(JobType.CheckPlagiarism, plagiarism.Id, plagiarism.RequestVersion + 1);
            }

            #endregion
        }

        public async Task HandleWorkerHeartbeatAsync(WorkerHeartbeatMessage message)
        {
            using var locked = await _lock.WriterLockAsync();
            var now = DateTime.Now.ToUniversalTime();
            if (_dictionary.ContainsKey(message.Name))
            {
                var (token, timestamp) = _dictionary[message.Name];
                if (token != message.Token)
                {
                    await HandleBrokenWorkerAsync(message.Name);
                    token = message.Token;
                }

                _dictionary[message.Name] = (token, now);
            }
            else
            {
                _dictionary.Add(message.Name, (message.Token, now));
            }
        }
    }
}