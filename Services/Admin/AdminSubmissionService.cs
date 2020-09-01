using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Exceptions;
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
    }

    public class AdminSubmissionService : IAdminSubmissionService
    {
        private const int PageSize = 50;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminSubmissionService> _logger;

        public AdminSubmissionService(ApplicationDbContext context, ILogger<AdminSubmissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task EnsureSubmissionExists(int id)
        {
            var submission = _context.Submissions.FindAsync(id);
            if (submission == null)
            {
                throw new NotFoundException();
            }
        }

        private async Task ValidateSubmissionEditDto(SubmissionEditDto dto)
        {
            if (!Enum.IsDefined(typeof(Verdict), dto.Verdict.GetValueOrDefault()))
            {
                throw new ValidationException("Invalid verdict.");
            }
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
                .PaginateAsync(s => new SubmissionInfoDto(s), pageIndex ?? 1, PageSize);
        }

        public async Task<SubmissionEditDto> GetSubmissionEditAsync(int id)
        {
            await EnsureSubmissionExists(id);
            return new SubmissionEditDto(await _context.Submissions.FindAsync(id));
        }

        public async Task<SubmissionEditDto> UpdateSubmissionAsync(int id, SubmissionEditDto dto)
        {
            await EnsureSubmissionExists(id);
            await ValidateSubmissionEditDto(dto);
            var submission = await _context.Submissions.FindAsync(id);
            submission.Verdict = dto.Verdict.GetValueOrDefault();
            submission.JudgedAt = DateTime.Now.ToUniversalTime();
            _context.Update(submission);
            await _context.SaveChangesAsync();
            return new SubmissionEditDto(submission);
        }

        public async Task DeleteSubmissionAsync(int id)
        {
            await EnsureSubmissionExists(id);
            var submission = new Submission {Id = id};
            _context.Submissions.Attach(submission);
            _context.Submissions.Remove(submission);
            await _context.SaveChangesAsync();
        }
    }
}