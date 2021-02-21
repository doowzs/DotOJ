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
using WebApp.RabbitMQ;

namespace WebApp.Services.Singleton
{
    public class WorkerStatisticsService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JudgeRequestProducer _producer;

        private static readonly Dictionary<string, (string Token, DateTime TimeStamp)> WorkerDictionary = new();
        // private static readonly Dictionary<int, DateTime> SubmissionTimestampDictionary = new();

        public WorkerStatisticsService(IServiceProvider provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _producer = provider.GetRequiredService<JudgeRequestProducer>();
        }

        private async Task HandleBrokenWorkerAsync(string name)
        {
            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
                submission.Verdict = await _producer.SendAsync(submission.Id, submission.RequestVersion + 1)
                    ? Verdict.InQueue
                    : Verdict.Pending;
            }

            context.UpdateRange(submissions);
            await context.SaveChangesAsync();
        }

        public async Task CheckWorkersAndSubmissionsAsync(CancellationToken stoppingToken)
        {
            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.Now.ToUniversalTime();

            #region Check for broken workers

            var workers = WorkerDictionary
                .Where(p => p.Value.TimeStamp < now.AddMinutes(-2))
                .Select(p => p.Key).ToList();
            foreach (var worker in workers)
            {
                await HandleBrokenWorkerAsync(worker);
            }

            #endregion

            #region Try send judge request for pending and stuck submissions

            var submissions = await context.Submissions
                .Where(s => s.Verdict == Verdict.Pending ||
                            (s.Verdict == Verdict.InQueue && s.UpdatedAt <= now.AddMinutes(-5)))
                .ToListAsync(stoppingToken);
            foreach (var submission in submissions)
            {
                submission.Verdict = await _producer.SendAsync(submission.Id, submission.RequestVersion + 1)
                    ? Verdict.InQueue
                    : Verdict.Pending;
            }

            context.UpdateRange(submissions);
            await context.SaveChangesAsync(stoppingToken);

            #endregion
        }

        public async Task HandleWorkerHeartbeatAsync(WorkerHeartbeatMessage message)
        {
            var now = DateTime.Now.ToUniversalTime();
            if (WorkerDictionary.ContainsKey(message.Name))
            {
                var (token, timestamp) = WorkerDictionary[message.Name];
                if (token != message.Token)
                {
                    await HandleBrokenWorkerAsync(message.Name);
                    token = message.Token;
                }

                WorkerDictionary[message.Name] = (token, now);
            }
            else
            {
                WorkerDictionary.Add(message.Name, (message.Token, now));
            }
        }
    }
}