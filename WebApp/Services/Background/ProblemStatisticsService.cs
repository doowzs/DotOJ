using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Data.DTOs;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebApp.Services.Background
{
    public class ProblemStatisticsService : CronJobService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProblemStatisticsService> _logger;
        private static readonly Dictionary<int, ProblemStatistics> Dictionary = new();

        public ProblemStatisticsService(IServiceProvider provider) : base("* * * * *")
        {
            _context = provider.GetRequiredService<ApplicationDbContext>();
            _logger = provider.GetRequiredService<ILogger<ProblemStatisticsService>>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var limit = DateTime.Now.ToUniversalTime().AddHours(-24);

            var removals = Dictionary
                .Where(e => e.Value.UpdatedAt < limit);
            foreach (var removal in removals)
            {
                Dictionary.Remove(removal.Key);
            }

            return Task.CompletedTask;
        }

        private async Task<ProblemStatistics> BuildAndCacheStatisticsAsync(int problemId)
        {
            var totalSubmissions = await _context.Submissions
                .Where(s => s.ProblemId == problemId).CountAsync();
            var acceptedSubmissions = await _context.Submissions
                .Where(s => s.ProblemId == problemId && s.Verdict == Verdict.Accepted).CountAsync();
            var totalContestants = await _context.Submissions
                .Where(s => s.ProblemId == problemId)
                .Select(s => s.UserId).Distinct().CountAsync();
            var acceptedContestants = await _context.Submissions
                .Where(s => s.ProblemId == problemId && s.Verdict == Verdict.Accepted)
                .Select(s => s.UserId).Distinct().CountAsync();

            var byVerdict = await _context.Submissions
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
            Dictionary.Add(problemId, statistics);
            return statistics;
        }

        public async Task<ProblemStatistics> GetStatisticsAsync(int problemId)
        {
            var contains = Dictionary.TryGetValue(problemId, out var statistics);
            if (contains)
            {
                return statistics;
            }
            else
            {
                return await BuildAndCacheStatisticsAsync(problemId);
            }
        }

        public async Task UpdateStatisticsAsync(int submissionId)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission is null)
            {
                _logger.LogError($"Invalid submission ID {submissionId}");
                return;
            }

            var problem = await _context.Problems.FindAsync(submission.ProblemId);
            var contains = Dictionary.TryGetValue(problem.Id, out var statistics);
            if (contains)
            {
                var attempted = await _context.Submissions
                    .AnyAsync(s => s.Id != submission.Id && s.UserId == submission.UserId);
                var solved = await _context.Submissions
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