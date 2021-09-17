using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Generics;
using Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Exceptions;
using Server.Services.Singleton;

namespace Server.Services
{
    public interface IProblemService
    {
        public Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex);
        public Task<ProblemViewDto> GetProblemViewAsync(int id);
        public Task<List<HackInfoDto>> DownloadHackResultAsync(int id);
    }

    public class ProblemService : LoggableService<ProblemService>, IProblemService
    {
        private const int PageSize = 50;
        private readonly ProblemStatisticsService _statisticsService;

        public ProblemService(IServiceProvider provider) : base(provider)
        {
            _statisticsService = provider.GetRequiredService<ProblemStatisticsService>();
        }

        private async Task EnsureProblemExistsAsync(int id)
        {
            if (!await Context.Problems.AnyAsync(p => p.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private async Task EnsureUserCanViewProblemAsync(int id)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (await Manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                await Manager.IsInRoleAsync(user, ApplicationRoles.ContestManager))
            {
                return;
            }

            var problem = await Context.Problems.FindAsync(id);
            await Context.Entry(problem).Reference<Contest>(p => p.Contest).LoadAsync();

            if (Config.Value.ExamId.HasValue && problem.ContestId != Config.Value.ExamId.Value)
            {
                throw new UnauthorizedAccessException("Not authorized to view this problem.");
            }

            if (problem.Contest.IsPublic)
            {
                if (DateTime.Now.ToUniversalTime() < problem.Contest.BeginTime)
                {
                    throw new UnauthorizedAccessException("Not authorized to view this problem.");
                }
            }
            else
            {
                var registered = await Context.Registrations
                    .AnyAsync(r => r.ContestId == problem.Contest.Id && r.UserId == user.Id);
                if (DateTime.Now.ToUniversalTime() < problem.Contest.BeginTime ||
                    (!registered && DateTime.Now.ToUniversalTime() < problem.Contest.EndTime))
                {
                    throw new UnauthorizedAccessException("Not authorized to view this problem.");
                }
            }
        }

        public async Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            var userId = user.Id;
            var problems = await Context.Problems.PaginateAsync(pageIndex ?? 1, PageSize);
            var infos = new List<ProblemInfoDto>();
            foreach (var problem in problems.Items)
            {
                var query = Context.Submissions.Where(s => s.ProblemId == problem.Id && !s.Hidden);
                var attempted = await query.AnyAsync(s => s.UserId == userId);
                var solved = await query.AnyAsync(s => s.UserId == userId && s.Verdict == Verdict.Accepted);
                var reviews = await Context.SubmissionReviews
                    .Where(s => s.UserId == userId)
                    .Include(s => s.Submission)
                    .ToListAsync();
                var scored = reviews.Exists(s => s.Submission.ProblemId == problem.Id);
                var statistics = await _statisticsService.GetStatisticsAsync(problem.Id);
                infos.Add(new ProblemInfoDto(problem, attempted, solved, scored, statistics));
            }

            return new PaginatedList<ProblemInfoDto>(problems.TotalItems, pageIndex ?? 1, PageSize, infos);
        }

        public async Task<ProblemViewDto> GetProblemViewAsync(int id)
        {
            await EnsureProblemExistsAsync(id);
            await EnsureUserCanViewProblemAsync(id);

            var problem = await Context.Problems.FindAsync(id);
            await Context.Entry(problem).Collection(p => p.Submissions).LoadAsync();
            problem.Submissions = problem.Submissions.Where(s => !s.Hidden).ToList();
            var statistics = await _statisticsService.GetStatisticsAsync(problem.Id);
            return new ProblemViewDto(problem, statistics);
        }

        public async Task<List<HackInfoDto>> DownloadHackResultAsync(int id)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);

            if (!(await Manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                  await Manager.IsInRoleAsync(user, ApplicationRoles.ContestManager) ||
                  await Manager.IsInRoleAsync(user, ApplicationRoles.SubmissionManager)))
            {
                throw new UnauthorizedAccessException("Can not Download.");
            }

            var dict = new Dictionary<int, int>();
            var submissions = Context.SubmissionReviews
                .Join(Context.Submissions,
                    r => r.SubmissionId,
                    s => s.Id,
                    (r, s) => s)
                .Include(s => s.User)
                .ToList();
            Console.Write(submissions.Count + "\n");
            var results = new Dictionary<string, double>();
            foreach (var submission in submissions)
            {
                if (!dict.ContainsKey(submission.Id))
                {
                    dict.Add(submission.Id, 1);
                    var failedOn = submission.FailedOn;
                    if (failedOn != null)
                    {
                        foreach (var test in failedOn)
                        {
                            if (!results.TryGetValue(test, out var score)) score = 0;
                            results[test] = score + 5.0 / failedOn.Count;
                        }
                    }
                }
            }

            return results.Select(p => new HackInfoDto(p.Key, p.Value)).ToList();
        }
    }
}