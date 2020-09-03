using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.DTOs;
using Data.Generics;
using Data.Models;
using IdentityServer4.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Exceptions;

namespace WebApp.Services
{
    public interface ISubmissionService
    {
        public Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionsAsync
            (int? contestId, int? problemId, string userId, Verdict? verdict, int? pageIndex);

        public Task<List<SubmissionInfoDto>> GetBatchSubmissionInfosAsync(IEnumerable<int> ids);
        public Task<SubmissionInfoDto> GetSubmissionInfoAsync(int id);
        public Task<SubmissionViewDto> GetSubmissionViewAsync(int id);
        public Task<SubmissionInfoDto> CreateSubmissionAsync(SubmissionCreateDto dto);
    }

    public class SubmissionService : LoggableService<SubmissionService>, ISubmissionService
    {
        private const int PageSize = 50;

        public SubmissionService(IServiceProvider provider) : base(provider)
        {
        }

        private async Task EnsureUserCanViewSubmissionAsync(Submission submission)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (await Manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                await Manager.IsInRoleAsync(user, ApplicationRoles.SubmissionManager))
            {
                return;
            }

            var accessible = submission.UserId == user.Id
                             || await Context.Submissions.AnyAsync(s => s.Id == submission.Id
                                                                        && s.UserId == user.Id
                                                                        && s.Verdict == Verdict.Accepted);
            if (!accessible)
            {
                throw new UnauthorizedAccessException("Not allowed to view this submission.");
            }
        }

        private async Task ValidateSubmissionCreateDtoAsync(SubmissionCreateDto dto)
        {
            var problem = await Context.Problems.FindAsync(dto.ProblemId);
            if (problem is null)
            {
                throw new ValidationException("Invalid problem ID.");
            }

            var contest = await Context.Contests.FindAsync(problem.ContestId);
            if (contest.IsPublic)
            {
                if (DateTime.Now.ToUniversalTime() < contest.BeginTime)
                {
                    throw new UnauthorizedAccessException("Cannot submit until contest has begun.");
                }
            }
            else
            {
                var userId = Accessor.HttpContext.User.GetSubjectId();
                var registered = await Context.Registrations
                    .AnyAsync(r => r.ContestId == contest.Id && r.UserId == userId);
                if (DateTime.Now.ToUniversalTime() < contest.BeginTime ||
                    (!registered && DateTime.Now.ToUniversalTime() < contest.EndTime))
                {
                    throw new UnauthorizedAccessException("Cannot submit until contest has begun.");
                }
            }

            if (!Regex.IsMatch(dto.Program.Code, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
            {
                throw new ValidationException("Invalid program code.");
            }
        }

        public async Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionsAsync
            (int? contestId, int? problemId, string userId, Verdict? verdict, int? pageIndex)
        {
            var submissions = Context.Submissions.AsQueryable();

            if (contestId.HasValue)
            {
                var problemIds = await Context.Problems
                    .Where(p => p.ContestId == contestId.GetValueOrDefault())
                    .Select(p => p.Id)
                    .ToListAsync();
                submissions = submissions.Where(s => problemIds.Contains(s.ProblemId));
            }

            if (problemId.HasValue)
            {
                submissions = submissions.Where(s => s.ProblemId == problemId.GetValueOrDefault());
            }

            if (!string.IsNullOrEmpty(userId))
            {
                submissions = submissions.Where(s => s.UserId == userId);
            }

            if (verdict.HasValue)
            {
                submissions = submissions.Where(s => s.Verdict == verdict.GetValueOrDefault());
            }

            return await submissions.OrderByDescending(s => s.Id)
                .PaginateAsync(s => s.User, s => new SubmissionInfoDto(s), pageIndex ?? 1, PageSize);
        }

        public async Task<List<SubmissionInfoDto>> GetBatchSubmissionInfosAsync(IEnumerable<int> ids)
        {
            return await Context.Submissions
                .Where(s => ids.Contains(s.Id))
                .Include(s => s.User)
                .Select(s => new SubmissionInfoDto(s))
                .ToListAsync();
        }

        public async Task<SubmissionInfoDto> GetSubmissionInfoAsync(int id)
        {
            var submission = await Context.Submissions.FindAsync(id);
            if (submission == null)
            {
                throw new NotFoundException();
            }

            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionInfoDto(submission);
        }

        public async Task<SubmissionViewDto> GetSubmissionViewAsync(int id)
        {
            var submission = await Context.Submissions.FindAsync(id);
            if (submission == null)
            {
                throw new NotFoundException();
            }

            await EnsureUserCanViewSubmissionAsync(submission);
            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionViewDto(submission);
        }

        public async Task<SubmissionInfoDto> CreateSubmissionAsync(SubmissionCreateDto dto)
        {
            await ValidateSubmissionCreateDtoAsync(dto);

            var lastSubmission = await Context.Submissions.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
            if (lastSubmission != null && (DateTime.Now.ToUniversalTime() - lastSubmission.CreatedAt).TotalSeconds < 15)
            {
                throw new TooManyRequestsException("Cannot submit twice between 15 seconds.");
            }

            var submission = new Submission
            {
                UserId = Accessor.HttpContext.User.GetSubjectId(),
                ProblemId = dto.ProblemId.GetValueOrDefault(),
                Program = dto.Program
            };
            await Context.Submissions.AddAsync(submission);
            await Context.SaveChangesAsync();

            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            var result = new SubmissionInfoDto(submission);
            await LogInformation($"CreateSubmission ProblemId={result.ProblemId} " +
                                 $"Langauge={result.Language} Length={result.CodeBytes}");
            return result;
        }
    }
}