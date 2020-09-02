using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using IdentityServer4.Extensions;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Judges;
using Judge1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services
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

    public class SubmissionService : ISubmissionService
    {
        private const int PageSize = 50;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        private readonly IHttpContextAccessor _accessor;
        private readonly IContestJudge _judge;
        private readonly ILogger<SubmissionService> _logger;

        public SubmissionService(ApplicationDbContext context, UserManager<ApplicationUser> manager,
            IHttpContextAccessor accessor, IContestJudge judge, ILogger<SubmissionService> logger)
        {
            _context = context;
            _manager = manager;
            _accessor = accessor;
            _judge = judge;
            _logger = logger;
        }

        private async Task EnsureUserCanViewSubmissionAsync(Submission submission)
        {
            var user = await _manager.GetUserAsync(_accessor.HttpContext.User);
            if (await _manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                await _manager.IsInRoleAsync(user, ApplicationRoles.SubmissionManager))
            {
                return;
            }

            var accessible = submission.UserId == user.Id
                             || await _context.Submissions.AnyAsync(s => s.Id == submission.Id
                                                                         && s.UserId == user.Id
                                                                         && s.Verdict == Verdict.Accepted);
            if (!accessible)
            {
                throw new UnauthorizedAccessException("Not allowed to view this submission.");
            }
        }

        private async Task ValidateSubmissionCreateDtoAsync(SubmissionCreateDto dto)
        {
            var problem = await _context.Problems.FindAsync(dto.ProblemId);
            if (problem is null)
            {
                throw new ValidationException("Invalid problem ID.");
            }

            var contest = await _context.Contests.FindAsync(problem.ContestId);
            if (contest.IsPublic)
            {
                if (DateTime.Now.ToUniversalTime() < contest.BeginTime)
                {
                    throw new UnauthorizedAccessException("Cannot submit until contest has begun.");
                }
            }
            else
            {
                var userId = _accessor.HttpContext.User.GetSubjectId();
                var registered = await _context.Registrations
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
            var submissions = _context.Submissions.AsQueryable();

            if (contestId.HasValue)
            {
                var problemIds = await _context.Problems
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
            return await _context.Submissions
                .Where(s => ids.Contains(s.Id))
                .Include(s => s.User)
                .Select(s => new SubmissionInfoDto(s))
                .ToListAsync();
        }

        public async Task<SubmissionInfoDto> GetSubmissionInfoAsync(int id)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
            {
                throw new NotFoundException();
            }

            await _context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionInfoDto(submission);
        }

        public async Task<SubmissionViewDto> GetSubmissionViewAsync(int id)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
            {
                throw new NotFoundException();
            }

            await EnsureUserCanViewSubmissionAsync(submission);
            await _context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionViewDto(submission);
        }

        public async Task<SubmissionInfoDto> CreateSubmissionAsync(SubmissionCreateDto dto)
        {
            await ValidateSubmissionCreateDtoAsync(dto);
            var submission = new Submission()
            {
                UserId = _accessor.HttpContext.User.GetSubjectId(),
                ProblemId = dto.ProblemId.GetValueOrDefault(),
                Program = dto.Program
            };
            await _context.Submissions.AddAsync(submission);
            await _context.SaveChangesAsync();

            BackgroundJob.Enqueue(() => _judge.JudgeSubmission(submission.Id));

            await _context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionInfoDto(submission);
        }
    }
}