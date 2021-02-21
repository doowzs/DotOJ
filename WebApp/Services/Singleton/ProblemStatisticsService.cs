using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Data.DTOs;
using Data.Models;
using Data.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebApp.Services.Singleton
{
    public class ProblemStatisticsService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly ILogger<ProblemStatisticsService> _logger;
        private readonly Dictionary<int, ProblemStatistics> _dictionary = new(); // TODO: replace with LRU dict

        public ProblemStatisticsService(IServiceProvider provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _logger = provider.GetRequiredService<ILogger<ProblemStatisticsService>>();
        }

        private async Task<ProblemStatistics> BuildAndCacheStatisticsAsync(int problemId)
        {
            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var totalSubmissions = await context.Submissions
                .Where(s => s.ProblemId == problemId).CountAsync();
            var acceptedSubmissions = await context.Submissions
                .Where(s => s.ProblemId == problemId && s.Verdict == Verdict.Accepted).CountAsync();
            var totalContestants = await context.Submissions
                .Where(s => s.ProblemId == problemId)
                .Select(s => s.UserId).Distinct().CountAsync();
            var acceptedContestants = await context.Submissions
                .Where(s => s.ProblemId == problemId && s.Verdict == Verdict.Accepted)
                .Select(s => s.UserId).Distinct().CountAsync();

            var byVerdict = await context.Submissions
                .Where(s => s.ProblemId == problemId)
                .GroupBy(s => s.Verdict)
                .Select(g => new {Key = g.Key, Value = g.Count()})
                .ToDictionaryAsync(p => p.Key, q => q.Value);

            var statistics = new ProblemStatistics()
            {
                TotalSubmissions = totalSubmissions,
                AcceptedSubmissions = acceptedSubmissions,
                TotalContestants = totalContestants,
                AcceptedContestants = acceptedContestants,
                ByVerdict = byVerdict,
                UpdatedAt = DateTime.Now.ToUniversalTime()
            };
            _dictionary.Add(problemId, statistics);
            return statistics;
        }

        public async Task<ProblemStatistics> GetStatisticsAsync(int problemId)
        {
            var contains = _dictionary.TryGetValue(problemId, out var statistics);
            if (contains)
            {
                return statistics;
            }
            else
            {
                return await BuildAndCacheStatisticsAsync(problemId);
            }
        }

        public async Task UpdateStatisticsAsync(JudgeCompleteMessage message)
        {
            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var submission = await context.Submissions.FindAsync(message.SubmissionId);
            if (submission is null || submission.CompleteVersion >= message.CompleteVersion)
            {
                _logger.LogDebug($"IgnoreJudgeCompleteMessage" +
                                $" SubmissionId={message.SubmissionId}" +
                                $" CompleteVersion={message.CompleteVersion}");
                return;
            }
            
            var problem = await context.Problems.FindAsync(submission.ProblemId);
            var contains = _dictionary.TryGetValue(problem.Id, out var statistics);
            if (contains)
            {
                var attempted = await context.Submissions
                    .AnyAsync(s => s.Id != submission.Id && s.UserId == submission.UserId);
                var solved = await context.Submissions
                    .AnyAsync(s => s.Id != submission.Id && s.UserId == submission.UserId
                                                         && s.Verdict == Verdict.Accepted);
                statistics.TotalSubmissions += 1;
                statistics.AcceptedSubmissions += submission.Verdict == Verdict.Accepted ? 1 : 0;
                statistics.TotalContestants += attempted ? 0 : 1;
                statistics.AcceptedContestants += solved ? 0 : 1;

                if (statistics.ByVerdict.ContainsKey(submission.Verdict))
                {
                    statistics.ByVerdict[submission.Verdict] += 1;
                }
                else
                {
                    statistics.ByVerdict[submission.Verdict] = 1;
                }

                statistics.UpdatedAt = DateTime.Now.ToUniversalTime();
            }
            else
            {
                await BuildAndCacheStatisticsAsync(problem.Id);
            }
        }
    }
}