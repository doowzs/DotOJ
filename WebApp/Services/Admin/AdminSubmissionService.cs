using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Data.DTOs;
using Data.Generics;
using Data.Models;
using IdentityServer4.Extensions;
using Microsoft.EntityFrameworkCore;
using WebApp.Exceptions;

namespace WebApp.Services.Admin
{
    public interface IAdminSubmissionService
    {
        public Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionInfosAsync
            (int? contestId, int? problemId, string userId, Verdict? verdict, int? pageIndex);

        public Task<List<SubmissionInfoDto>> GetBatchSubmissionInfosAsync(IEnumerable<int> ids);
        public Task<SubmissionEditDto> GetSubmissionEditAsync(int id);
        public Task<SubmissionEditDto> UpdateSubmissionAsync(int id, SubmissionEditDto dto);
        public Task DeleteSubmissionAsync(int id);
        public Task<List<SubmissionInfoDto>> RejudgeSubmissionsAsync(int? contestId, int? problemId, int? submissionId);
    }

    public class AdminSubmissionService : LoggableService<AdminSubmissionService>, IAdminSubmissionService
    {
        private const int PageSize = 50;

        public AdminSubmissionService(IServiceProvider provider) : base(provider)
        {
        }

        private async Task EnsureSubmissionExists(int id)
        {
            var submission = await Context.Submissions.FindAsync(id);
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

        public async Task<SubmissionEditDto> GetSubmissionEditAsync(int id)
        {
            await EnsureSubmissionExists(id);
            var submission = await Context.Submissions.FindAsync(id);
            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionEditDto(submission);
        }

        public async Task<SubmissionEditDto> UpdateSubmissionAsync(int id, SubmissionEditDto dto)
        {
            await EnsureSubmissionExists(id);
            await ValidateSubmissionEditDto(dto);

            var user = await Manager.FindByIdAsync(Accessor.HttpContext.User.GetSubjectId());
            var submission = await Context.Submissions.FindAsync(id);
            submission.Verdict = dto.Verdict.GetValueOrDefault();
            submission.Time = submission.Memory = null;
            submission.FailedOn = -1;
            submission.Score = submission.Verdict == Verdict.Accepted ? 100 : 0;
            submission.Message = dto.Message;
            submission.JudgedBy = "[manual] " + user.ContestantId;
            submission.JudgedAt = DateTime.Now.ToUniversalTime();
            Context.Update(submission);
            await Context.SaveChangesAsync();

            var problem = await Context.Problems.FindAsync(submission.ProblemId);
            var registration = await Context.Registrations.FindAsync(submission.UserId, problem.ContestId);
            await registration.RebuildStatisticsAsync(Context);
            await Context.SaveChangesAsync();

            await LogInformation($"UpdateSubmission Id={submission.Id} Verdict={submission.Verdict}");
            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionEditDto(submission);
        }

        public async Task DeleteSubmissionAsync(int id)
        {
            await EnsureSubmissionExists(id);
            var submission = new Submission {Id = id};
            Context.Submissions.Attach(submission);
            Context.Submissions.Remove(submission);
            await Context.SaveChangesAsync();

            var problem = await Context.Problems.FindAsync(submission.ProblemId);
            var registration = await Context.Registrations.FindAsync(submission.UserId, problem.ContestId);
            await registration.RebuildStatisticsAsync(Context);
            await Context.SaveChangesAsync();
            await LogInformation($"DeleteSubmission Id={id}");
        }

        public async Task<List<SubmissionInfoDto>> RejudgeSubmissionsAsync
            (int? contestId, int? problemId, int? submissionId)
        {
            if (!contestId.HasValue && !problemId.HasValue && !submissionId.HasValue)
            {
                throw new ValidationException("At lease one parameter is required.");
            }

            var queryable = Context.Submissions.AsQueryable();
            if (contestId.HasValue)
            {
                var problemIds = await Context.Problems
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
                submission.Score = submission.Progress = null;
                submission.Message = null;
                submission.JudgedBy = null;
                submission.JudgedAt = null;
            }

            Context.UpdateRange(submissions);
            await Context.SaveChangesAsync();
            await LogInformation($"RejudgeSubmissions ContestId={contestId} " +
                                 $"ProblemId={problemId} SubmissionId={submissionId}");

            var infos = new List<SubmissionInfoDto>();
            foreach (var submission in submissions)
            {
                infos.Add(new SubmissionInfoDto(submission));
            }

            return infos;
        }
    }
}