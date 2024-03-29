﻿using System;
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
    public interface IContestService
    {
        public Task<List<ContestInfoDto>> GetCurrentContestInfosAsync();
        public Task<PaginatedList<ContestInfoDto>> GetPaginatedContestInfosAsync(int? pageIndex);
        public Task<ContestViewDto> GetContestViewAsync(int id);
        public Task<List<RegistrationInfoDto>> GetRegistrationInfosAsync(int id);
        public Task<List<SubmissionReviewInfoDto>> GetReviewListAsync(int id);
    }

    public class ContestService : LoggableService<ContestService>, IContestService
    {
        private const int PageSize = 20;
        private readonly ProblemStatisticsService _statisticsService;

        public ContestService(IServiceProvider provider) : base(provider)
        {
            _statisticsService = provider.GetRequiredService<ProblemStatisticsService>();
        }

        private async Task EnsureContestExistsAsync(int id)
        {
            if (!await Context.Contests.AnyAsync(c => c.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private async Task EnsureUserCanViewContestAsync(int id)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (await Manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                await Manager.IsInRoleAsync(user, ApplicationRoles.ContestManager))
            {
                return;
            }

            if (Config.Value.ExamId.HasValue && id != Config.Value.ExamId.Value)
            {
                throw new UnauthorizedAccessException("Not authorized to view this contest.");
            }

            var contest = await Context.Contests.FindAsync(id);
            if (contest.IsPublic)
            {
                if (DateTime.Now.ToUniversalTime() < contest.BeginTime)
                {
                    throw new UnauthorizedAccessException("Not authorized to view this contest.");
                }
            }
            else
            {
                var registered = await Context.Registrations
                    .AnyAsync(r => r.ContestId == contest.Id && r.UserId == user.Id);
                if (DateTime.Now.ToUniversalTime() < contest.BeginTime ||
                    (!registered && DateTime.Now.ToUniversalTime() < contest.EndTime))
                {
                    throw new UnauthorizedAccessException("Not authorized to view this contest.");
                }
            }
        }

        public async Task<List<ContestInfoDto>> GetCurrentContestInfosAsync()
        {
            var now = DateTime.Now.ToUniversalTime();
            var contests = await Context.Contests
                .Where(c => c.EndTime > now)
                .OrderBy(c => c.EndTime)
                .ThenBy(c => c.BeginTime)
                .ThenBy(c => c.Id)
                .ToListAsync();
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);

            if (Accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = user.Id;
                var infos = new List<ContestInfoDto>();
                foreach (var contest in contests)
                {
                    var registered = await Context.Registrations
                        .AnyAsync(r => r.ContestId == contest.Id && r.UserId == userId);
                    infos.Add(new ContestInfoDto(contest, registered));
                }

                return infos;
            }
            else
            {
                return contests.Select(c => new ContestInfoDto(c, false)).ToList();
            }
        }

        public async Task<PaginatedList<ContestInfoDto>> GetPaginatedContestInfosAsync(int? pageIndex)
        {
            // See https://github.com/dotnet/efcore/issues/17068 for GroupJoin issues.
            var contests = await Context.Contests
                .OrderByDescending(c => c.Id)
                .PaginateAsync(pageIndex ?? 1, PageSize);
            IList<ContestInfoDto> infos;
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (Accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = user.Id;
                infos = new List<ContestInfoDto>();
                foreach (var contest in contests.Items)
                {
                    var registered = await Context.Registrations
                        .AnyAsync(r => r.ContestId == contest.Id && r.UserId == userId);
                    infos.Add(new ContestInfoDto(contest, registered));
                }
            }
            else
            {
                infos = contests.Items.Select(c => new ContestInfoDto(c, false)).ToList();
            }

            return new PaginatedList<ContestInfoDto>(contests.TotalItems, pageIndex ?? 1, PageSize, infos);
        }

        public async Task<ContestViewDto> GetContestViewAsync(int id)
        {
            await EnsureContestExistsAsync(id);
            await EnsureUserCanViewContestAsync(id);

            var contest = await Context.Contests.FindAsync(id);
            if (contest is null)
            {
                throw new NotFoundException();
            }

            await Context.Entry(contest).Collection(c => c.Problems).LoadAsync();

            IList<ProblemInfoDto> problemInfos;
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);

            if (Accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = user.Id;
                problemInfos = new List<ProblemInfoDto>();
                foreach (var problem in contest.Problems)
                {
                    var query = Context.Submissions.Where(s => s.ProblemId == problem.Id && !s.Hidden);
                    var attempted = await query.AnyAsync(s => s.UserId == userId);
                    var solved = await query.AnyAsync(s => s.UserId == userId && s.Verdict == Verdict.Accepted);
                    var statistics = await _statisticsService.GetStatisticsAsync(problem.Id);
                    var reviews = await Context.SubmissionReviews
                        .Where(s => s.UserId == userId)
                        .Include(s => s.Submission)
                        .ToListAsync();
                    var scored = reviews.Exists(s => s.Submission.ProblemId == problem.Id);
                    problemInfos.Add(new ProblemInfoDto(problem, attempted, solved, scored, statistics));
                }
            }
            else
            {
                problemInfos = contest.Problems.Select(p => new ProblemInfoDto(p)).ToList();
            }

            return new ContestViewDto(contest, problemInfos);
        }

        public async Task<List<RegistrationInfoDto>> GetRegistrationInfosAsync(int id)
        {
            await EnsureContestExistsAsync(id);
            await EnsureUserCanViewContestAsync(id);

            return await Context.Registrations
                .Where(r => r.ContestId == id)
                .Include(r => r.User)
                .Select(r => new RegistrationInfoDto(r))
                .ToListAsync();
        }

        public async Task<List<SubmissionReviewInfoDto>> GetReviewListAsync(int id)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User); 
            if (!(await Manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                  await Manager.IsInRoleAsync(user, ApplicationRoles.ContestManager) ||
                  await Manager.IsInRoleAsync(user, ApplicationRoles.SubmissionManager)))
            {
                throw new UnauthorizedAccessException("Can not Download.");
            }
            var contest = await Context.Contests
                .Where(s => s.Id == id)
                .Include(s => s.Problems)
                .FirstOrDefaultAsync();

            var reviews = Context.SubmissionReviews
                .Include(s => s.User)
                .Include(s => s.Submission)
                .ToList();

            var legalReviews = new List<SubmissionReviewInfoDto>();
            if (contest != null)
            {
                foreach (var problem in contest.Problems)
                {
                    var currentReviews = reviews.FindAll(s => s.Submission.ProblemId == problem.Id);
                    foreach (var review in currentReviews)
                    {
                        var submission = await Context.Submissions
                            .Where(s => s.Id == review.Submission.Id)
                            .Include(s => s.User)
                            .FirstOrDefaultAsync();
                        if (submission != null)
                        {
                            legalReviews.Add(new SubmissionReviewInfoDto(review.Score,
                                review.TimeComplexity,
                                review.SpaceComplexity,
                                review.CodeSpecification,
                                review.Comments
                                , new SubmissionViewDto(submission, Config), review.User.ContestantId));
                        }
                    }
                }
            }
            return legalReviews;
        }
    }
}