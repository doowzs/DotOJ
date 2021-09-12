using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared;
using Shared.DTOs;
using Shared.Generics;
using Shared.Models;
using Shared.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Server.Services.Singleton
{
    public class ProblemStatisticsService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly ILogger<ProblemStatisticsService> _logger;
        private readonly LruCache<int, ProblemStatistics> _cache = new(100000, null);

        public ProblemStatisticsService(IServiceProvider provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _logger = provider.GetRequiredService<ILogger<ProblemStatisticsService>>();
        }

        private async Task<ProblemStatistics> BuildAndCacheStatisticsAsync(int problemId)
        {
            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var problem = await context.Problems.FindAsync(problemId);
            var contest = await context.Contests.FindAsync(problem.ContestId);

            System.Linq.Expressions.Expression<Func<Submission, bool>> totalPredicate =
                (s) => s.CreatedAt >= contest.BeginTime &&
                       s.ProblemId == problemId &&
                       (s.Verdict == Verdict.Accepted || (s.Verdict != Verdict.Accepted && s.FailedOn > 0));
            System.Linq.Expressions.Expression<Func<Submission, bool>> acceptedPredicate =
                (s) => s.CreatedAt >= contest.BeginTime &&
                       s.ProblemId == problemId &&
                       s.Verdict == Verdict.Accepted;

            var totalSubmissions = await context.Submissions
                .Where(totalPredicate)
                .CountAsync();
            var acceptedSubmissions = await context.Submissions
                .Where(acceptedPredicate)
                .CountAsync();
            var totalContestants = await context.Submissions
                .Where(totalPredicate)
                .Select(s => s.UserId)
                .Distinct()
                .CountAsync();
            var acceptedContestants = await context.Submissions
                .Where(acceptedPredicate)
                .Select(s => s.UserId)
                .Distinct()
                .CountAsync();

            var byVerdict = await context.Submissions
                .Where(totalPredicate)
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
            _ = _cache.TryAddAsync(problemId, statistics);
            return statistics;
        }

        public async Task<ProblemStatistics> GetStatisticsAsync(int problemId)
        {
            if (await _cache.TryGetValueAsync(problemId) is (true, var statistics))
            {
                return statistics;
            }
            else
            {
                return await BuildAndCacheStatisticsAsync(problemId);
            }
        }

        public async Task UpdateStatisticsAsync(JobCompleteMessage message)
        {
            if (message.JobType != JobType.JudgeSubmission) return;

            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var submission = await context.Submissions.FindAsync(message.TargetId);
            if (submission is null || submission.CompleteVersion >= message.CompleteVersion)
            {
                _logger.LogDebug($"IgnoreJudgeCompleteMessage" +
                                 $" SubmissionId={message.TargetId}" +
                                 $" CompleteVersion={message.CompleteVersion}");
                return;
            }
            else
            {
                submission.CompleteVersion = message.CompleteVersion;
                context.Update(submission);
                await context.SaveChangesAsync();
            }

            if (submission.Program.Input != null) return; // ignore custom tests

            var problem = await context.Problems.FindAsync(submission.ProblemId);
            var contest = await context.Contests.FindAsync(problem.ContestId);
            var now = DateTime.Now.ToUniversalTime();
            if (now < contest.BeginTime) return;

            if (await _cache.TryGetValueAsync(problem.Id) is (true, var ps))
            {
                var attempted = await context.Submissions
                    .AnyAsync(s => s.Id != submission.Id &&
                                   s.UserId == submission.UserId &&
                                   s.ProblemId == submission.ProblemId &&
                                   s.CreatedAt >= contest.BeginTime);
                var solved = await context.Submissions
                    .AnyAsync(s => s.Id != submission.Id &&
                                   s.UserId == submission.UserId &&
                                   s.ProblemId == submission.ProblemId &&
                                   s.CreatedAt >= contest.BeginTime &&
                                   s.Verdict == Verdict.Accepted);
                var byVerdict = new Dictionary<Verdict, int>(ps.ByVerdict);
                if (byVerdict.ContainsKey(submission.Verdict))
                {
                    byVerdict[submission.Verdict] += 1;
                }
                else
                {
                    byVerdict[submission.Verdict] = 1;
                }

                var statistics = new ProblemStatistics
                {
                    TotalSubmissions = ps.TotalSubmissions +
                                       (submission.Verdict == Verdict.Accepted ||
                                        (submission.Verdict != Verdict.Accepted && submission.FailedOn > 0)
                                           ? 1
                                           : 0),
                    AcceptedSubmissions = ps.AcceptedSubmissions + (submission.Verdict == Verdict.Accepted ? 1 : 0),
                    TotalContestants = ps.TotalContestants + (attempted ? 0 : 1),
                    AcceptedContestants = ps.AcceptedContestants +
                                          (solved ? 0 : (submission.Verdict == Verdict.Accepted ? 1 : 0)),
                    ByVerdict = byVerdict,
                    UpdatedAt = DateTime.Now.ToUniversalTime()
                };
                await _cache.TryUpdateAsync(problem.Id, statistics);
            }
            else
            {
                await BuildAndCacheStatisticsAsync(problem.Id);
            }
        }

        public async Task InvalidStatisticsAsync(int problemId)
        {
            if (await _cache.ContainsKeyAsync(problemId))
            {
                _ = _cache.TryRemoveAsync(problemId);
            }
        }
    }
}