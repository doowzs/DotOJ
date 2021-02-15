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
using WebApp.Exceptions;

namespace WebApp.Services
{
    public interface ISubmissionService
    {
        public Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionsAsync(int? contestId, string userId,
            string contestantId, int? problemId, Verdict? verdict, int? pageSize, int? pageIndex);

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

        private async Task<Boolean> IsSubmissionViewableAsync(Submission submission)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (submission.UserId == user.Id)
            {
                return true;
            }

            var problem = await Context.Problems.FindAsync(submission.ProblemId);
            var contest = await Context.Contests.FindAsync(problem.ContestId);
            if (DateTime.Now.ToUniversalTime() > contest.EndTime)
            {
                return true;
            }

            return await Context.Submissions.AnyAsync(s => s.UserId == user.Id && s.ProblemId == problem.Id && s.Verdict == Verdict.Accepted);
        }

        private async Task EnsureUserCanViewSubmissionAsync(Submission submission)
        {
            var accessible = await IsSubmissionViewableAsync(submission);
            if (!accessible)
            {
                throw new UnauthorizedAccessException("Not allowed to view this submission.");
            }
        }

        private async Task<Contest> ValidateSubmissionCreateDtoAsync(SubmissionCreateDto dto)
        {
            var problem = await Context.Problems.FindAsync(dto.ProblemId);
            if (problem is null)
            {
                throw new ValidationException("Invalid problem ID.");
            }

            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            var contest = await Context.Contests.FindAsync(problem.ContestId);
            var registered = await Context.Registrations
                .AnyAsync(r => r.ContestId == contest.Id && r.UserId == user.Id);
            if (await Manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                await Manager.IsInRoleAsync(user, ApplicationRoles.ContestManager))
            {
                // Administrator and contest manager can submit to any problem.
            }
            else if (DateTime.Now.ToUniversalTime() < contest.BeginTime)
            {
                throw new UnauthorizedAccessException("Cannot submit until contest has begun.");
            }
            else if (DateTime.Now.ToUniversalTime() < contest.EndTime && !registered)
            {
                if (contest.IsPublic)
                {
                    // Automatically register for user if it is submitting during contest.
                    var registration = new Registration
                    {
                        UserId = user.Id,
                        ContestId = contest.Id,
                        IsParticipant = true,
                        IsContestManager = false,
                        Statistics = new List<ProblemStatistics>()
                    };
                    await Context.Registrations.AddAsync(registration);
                    await Context.SaveChangesAsync();
                }
                else
                {
                    throw new UnauthorizedAccessException("Unregistered user cannot submit until contest has ended.");
                }
            }

            if (!Regex.IsMatch(dto.Program.Code, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
            {
                throw new ValidationException("Invalid program code.");
            }

            return contest;
        }

        public async Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionsAsync(int? contestId, string userId,
            string contestantId, int? problemId, Verdict? verdict, int? pageSize, int? pageIndex)
        {
            var submissions = Context.Submissions.Where(s => !s.Hidden);

            if (contestId.HasValue)
            {
                var problemIds = await Context.Problems
                    .Where(p => p.ContestId == contestId.GetValueOrDefault())
                    .Select(p => p.Id)
                    .ToListAsync();
                submissions = submissions.Where(s => problemIds.Contains(s.ProblemId));
            }

            if (!string.IsNullOrEmpty(userId))
            {
                submissions = submissions.Where(s => s.UserId == userId);
            }

            if (!string.IsNullOrEmpty(contestantId))
            {
                var user = await Context.Users.Where(u => u.ContestantId == contestantId).FirstOrDefaultAsync();
                if (user == null)
                {
                    return new PaginatedList<SubmissionInfoDto>(0, 1,
                        pageSize ?? PageSize, new List<SubmissionInfoDto>());
                }
                else
                {
                    submissions = submissions.Where(s => s.UserId == user.Id);
                }
            }

            if (problemId.HasValue)
            {
                submissions = submissions.Where(s => s.ProblemId == problemId.GetValueOrDefault());
            }

            if (verdict.HasValue)
            {
                submissions = submissions.Where(s => s.Verdict == verdict.GetValueOrDefault());
            }
            
            var paginated = await submissions.OrderByDescending(s => s.Id)
                .PaginateAsync(s => s.User, s => s, pageIndex ?? 1, pageSize ?? PageSize);
            var dict = new Dictionary<int, bool>();
            var infos = new List<SubmissionInfoDto>();
            foreach (var submission in paginated.Items)
            {
                var viewable = false;
                if (!dict.TryGetValue(submission.Id, out viewable))
                {
                    viewable = await IsSubmissionViewableAsync(submission);
                    dict.Add(submission.Id, viewable);
                }
                infos.Add(new SubmissionInfoDto(submission, viewable));
            }

            return new PaginatedList<SubmissionInfoDto>(paginated.TotalItems, paginated.PageIndex, paginated.PageSize, infos);
        }

        public async Task<List<SubmissionInfoDto>> GetBatchSubmissionInfosAsync(IEnumerable<int> ids)
        {
            var dict = new Dictionary<int, bool>();
            var submissions = await Context.Submissions
                .Where(s => ids.Contains(s.Id))
                .Include(s => s.User)
                .ToListAsync();
            
            var infos = new List<SubmissionInfoDto>();
            foreach (var submission in submissions)
            {
                var viewable = false;
                if (!dict.TryGetValue(submission.Id, out viewable))
                {
                    viewable = await IsSubmissionViewableAsync(submission);
                    dict.Add(submission.Id, viewable);
                }
                infos.Add(new SubmissionInfoDto(submission, viewable));
            }

            return infos;
        }

        public async Task<SubmissionInfoDto> GetSubmissionInfoAsync(int id)
        {
            var submission = await Context.Submissions.FindAsync(id);
            if (submission == null)
            {
                throw new NotFoundException();
            }

            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionInfoDto(submission, await IsSubmissionViewableAsync(submission));
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
            var contest = await ValidateSubmissionCreateDtoAsync(dto);

            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            var lastSubmission = await Context.Submissions
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();
            if (lastSubmission != null && (DateTime.Now.ToUniversalTime() - lastSubmission.CreatedAt).TotalSeconds < 5)
            {
                throw new TooManyRequestsException("Cannot submit twice between 5 seconds.");
            }

            var submission = new Submission
            {
                UserId = Accessor.HttpContext.User.GetSubjectId(),
                ProblemId = dto.ProblemId.GetValueOrDefault(),
                Program = dto.Program,
                Verdict = Verdict.Pending,
                Hidden = DateTime.Now.ToUniversalTime() < contest.BeginTime,
                Time = null,
                Memory = null,
                FailedOn = null,
                Score = null,
                Progress = null,
                Message = null,
                JudgedBy = null,
                JudgedAt = null
            };
            await Context.Submissions.AddAsync(submission);
            await Context.SaveChangesAsync();

            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            var result = new SubmissionInfoDto(submission, true);
            await LogInformation($"CreateSubmission ProblemId={result.ProblemId} " +
                                 $"Language={result.Language} Length={result.CodeBytes}");
            return result;
        }
    }
}