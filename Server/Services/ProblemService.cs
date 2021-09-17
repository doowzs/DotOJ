using System;
using System.Collections.Generic;
using System.Linq;
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

            var query = Context.Submissions
                .Include(s => s.User)
                .Where(s => s.ProblemId == id);
            
            var submissions = await query.ToListAsync();
            
            var contestantIdDict = new Dictionary<string, int>();
            var resultDict = new Dictionary<string, double>();
            
            foreach (var submission in submissions)
            {
                if (!contestantIdDict.ContainsKey(submission.User.ContestantId))
                {
                    contestantIdDict.Add(submission.User.ContestantId, 1);
                    
                    var failSubmission = await query
                        .Where(s => s.User.ContestantId == submission.User.ContestantId)
                        .OrderByDescending(s => s.Score)
                        .FirstOrDefaultAsync();

                    var data  = failSubmission.FailedOn;
                    if (data != null)
                    {
                        foreach (var item in data)
                        {
                            if (!resultDict.ContainsKey(item))
                            {
                                resultDict.Add(item, 5.0 / data.Count);
                            }
                            else
                            {
                                var oldScore = resultDict[item];
                                resultDict.Remove(item);
                                resultDict.Add(item, oldScore + 5.0 / data.Count);
                            }
                        }
                    }
                }
            }

            var hackResult = new List<HackInfoDto>();
            foreach (var result in resultDict)
            {
                hackResult.Add(new HackInfoDto(result.Key, result.Value));
            }
            return hackResult;
        }
    }
}