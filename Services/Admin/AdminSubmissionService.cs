using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Judges;
using Judge1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services.Admin
{
    public interface IAdminSubmissionService
    {
        public Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionInfosAsync
            (int? contestId, int? problemId, string userId, Verdict? verdict, int? pageIndex);

        public Task<SubmissionEditDto> GetSubmissionEditAsync(int id);
        public Task<SubmissionEditDto> UpdateSubmissionAsync(int id, SubmissionEditDto dto);
        public Task DeleteSubmissionAsync(int id);
        public Task<List<SubmissionInfoDto>> RejudgeSubmissionsAsync(int? contestId, int? problemId, int? submissionId);
    }

    public class AdminSubmissionService : IAdminSubmissionService
    {
        private const int PageSize = 50;

        private readonly ApplicationDbContext _context;
        private readonly IContestJudge _judge;
        private readonly ILogger<AdminSubmissionService> _logger;

        public AdminSubmissionService(ApplicationDbContext context, IContestJudge judge,
            ILogger<AdminSubmissionService> logger)
        {
            _context = context;
            _judge = judge;
            _logger = logger;
        }

        private async Task EnsureSubmissionExists(int id)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
            {
                throw new NotFoundException();
            }
        }

        private Task ValidateSubmissionEditDto(SubmissionEditDto dto)
        {
            if (!Enum.IsDefined(typeof(Verdict), dto.Verdict.GetValueOrDefault()))
            {
                throw new ValidationException("Invalid verdict.");
            }

            return Task.CompletedTask;
        }

        public async Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionInfosAsync
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

        public async Task<SubmissionEditDto> GetSubmissionEditAsync(int id)
        {
            await EnsureSubmissionExists(id);
            var submission = await _context.Submissions.FindAsync(id);
            await _context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionEditDto(submission);
        }

        public async Task<SubmissionEditDto> UpdateSubmissionAsync(int id, SubmissionEditDto dto)
        {
            await EnsureSubmissionExists(id);
            await ValidateSubmissionEditDto(dto);
            var submission = await _context.Submissions.FindAsync(id);
            submission.Verdict = dto.Verdict.GetValueOrDefault();
            submission.Time = submission.Memory = null;
            submission.FailedOn = -1;
            submission.Score = submission.Verdict == Verdict.Accepted ? 100 : 0;
            submission.Message = dto.Message;
            submission.JudgedAt = DateTime.Now.ToUniversalTime();
            _context.Update(submission);
            await _context.SaveChangesAsync();

            var problem = await _context.Problems.FindAsync(submission.ProblemId);
            var registration = await _context.Registrations.FindAsync(submission.UserId, problem.ContestId);
            await registration.RebuildStatisticsAsync(_context);
            await _context.SaveChangesAsync();

            await _context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionEditDto(submission);
        }

        public async Task DeleteSubmissionAsync(int id)
        {
            await EnsureSubmissionExists(id);
            var submission = new Submission {Id = id};
            _context.Submissions.Attach(submission);
            _context.Submissions.Remove(submission);
            await _context.SaveChangesAsync();

            var problem = await _context.Problems.FindAsync(submission.ProblemId);
            var registration = await _context.Registrations.FindAsync(submission.UserId, problem.ContestId);
            await registration.RebuildStatisticsAsync(_context);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SubmissionInfoDto>> RejudgeSubmissionsAsync
            (int? contestId, int? problemId, int? submissionId)
        {
            if (!contestId.HasValue && !problemId.HasValue && !submissionId.HasValue)
            {
                throw new ValidationException("At lease one parameter is required.");
            }

            var queryable = _context.Submissions.AsQueryable();
            if (contestId.HasValue)
            {
                var problemIds = await _context.Problems
                    .Where(p => p.ContestId == contestId.Value)
                    .Select(p => p.Id)
                    .ToListAsync();
                queryable = queryable.Where(s => problemIds.Contains(s.ProblemId));
            }

            if (problemId.HasValue)
            {
                queryable = queryable.Where(s => s.ProblemId == problemId.Value);
            }

            if (submissionId.HasValue)
            {
                queryable = queryable.Where(s => s.Id == submissionId.Value);
            }

            var submissions = await queryable.Include(s => s.User).ToListAsync();
            foreach (var submission in submissions)
            {
                submission.Verdict = Verdict.Pending;
                submission.Time = submission.Memory = null;
                submission.FailedOn = submission.FailedOn = null;
                submission.Message = null;
                submission.JudgedAt = null;
            }

            _context.UpdateRange(submissions);
            await _context.SaveChangesAsync();

            var infos = new List<SubmissionInfoDto>();
            foreach (var submission in submissions)
            {
                infos.Add(new SubmissionInfoDto(submission));
                BackgroundJob.Enqueue(() => _judge.JudgeSubmission(submission.Id));
            }

            return infos;
        }
    }
}