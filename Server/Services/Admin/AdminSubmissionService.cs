using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shared;
using Shared.DTOs;
using Shared.Generics;
using Shared.Models;
using Shared.RabbitMQ;
using IdentityServer4.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Exceptions;
using Server.RabbitMQ;
using Server.Services.Singleton;

namespace Server.Services.Admin
{
    public interface IAdminSubmissionService
    {
        public Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionInfosAsync
            (int? contestId, int? problemId, string userId, Verdict? verdict, int? pageIndex);

        public Task<List<SubmissionInfoDto>> GetBatchSubmissionInfosAsync(IEnumerable<int> ids);
        public Task<SubmissionEditDto> GetSubmissionEditAsync(int id);
        public Task<SubmissionInfoDto> CreateSubmissionAsync(SubmissionCreateDto dto);
        public Task<SubmissionEditDto> UpdateSubmissionAsync(int id, SubmissionEditDto dto);
        public Task DeleteSubmissionAsync(int id);
        public Task<List<SubmissionInfoDto>> RejudgeSubmissionsAsync(int? contestId, int? problemId, int? submissionId);
    }

    public class AdminSubmissionService : LoggableService<AdminSubmissionService>, IAdminSubmissionService
    {
        private const int PageSize = 50;
        private readonly BackgroundTaskQueue<JobRequestMessage> _queue;
        private readonly ProblemStatisticsService _statistics;

        public AdminSubmissionService(IServiceProvider provider) : base(provider)
        {
            _queue = provider.GetRequiredService<BackgroundTaskQueue<JobRequestMessage>>();
            _statistics = provider.GetRequiredService<ProblemStatisticsService>();
        }

        private async Task EnsureSubmissionExists(int id)
        {
            if (!await Context.Submissions.AnyAsync(s => s.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private async Task ValidateSubmissionCreateDtoAsync(SubmissionCreateDto dto)
        {
            var problem = await Context.Problems.FindAsync(dto.ProblemId);
            if (problem is null)
            {
                throw new ValidationException("Invalid problem ID.");
            }

            switch (problem.Type)
            {
                case ProblemType.Ordinary:
                    if (dto.Program.Language == Language.LabArchive)
                    {
                        throw new ValidationException("Ordinary problem does not accept this language..");
                    }
                    else if (!Regex.IsMatch(dto.Program.Code, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                    {
                        throw new ValidationException("Invalid program code.");
                    }
                    break;
                case ProblemType.TestKitLab:
                    throw new ValidationException("Cannot submit testkit lab problems through this API.");
                default:
                    throw new ArgumentOutOfRangeException();
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
                .PaginateAsync(s => s.User, s => new SubmissionInfoDto(s, true), pageIndex ?? 1, PageSize);
        }

        public async Task<List<SubmissionInfoDto>> GetBatchSubmissionInfosAsync(IEnumerable<int> ids)
        {
            return await Context.Submissions
                .Where(s => ids.Contains(s.Id))
                .Include(s => s.User)
                .Select(s => new SubmissionInfoDto(s, true))
                .ToListAsync();
        }

        public async Task<SubmissionEditDto> GetSubmissionEditAsync(int id)
        {
            await EnsureSubmissionExists(id);
            var submission = await Context.Submissions.FindAsync(id);
            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionEditDto(submission);
        }

        public async Task<SubmissionInfoDto> CreateSubmissionAsync(SubmissionCreateDto dto)
        {
            await ValidateSubmissionCreateDtoAsync(dto);

            var submission = new Submission
            {
                UserId = Accessor.HttpContext.User.GetSubjectId(),
                ProblemId = dto.ProblemId.GetValueOrDefault(),
                Program = dto.Program,
                Hidden = true,
                Verdict = Verdict.Pending,
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
            _queue.EnqueueTask(new JobRequestMessage(JobType.JudgeSubmission, submission.Id, 1));

            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            var result = new SubmissionInfoDto(submission, true);
            await LogInformation($"CreateSubmission [Admin] ProblemId={result.ProblemId} " +
                                 $"Language={result.Language} Length={result.CodeBytes}");
            return result;
        }

        public async Task<SubmissionEditDto> UpdateSubmissionAsync(int id, SubmissionEditDto dto)
        {
            await EnsureSubmissionExists(id);
            await ValidateSubmissionEditDto(dto);

            var user = await Manager.FindByIdAsync(Accessor.HttpContext.User.GetSubjectId());
            var submission = await Context.Submissions.FindAsync(id);
            submission.Verdict = dto.Verdict.GetValueOrDefault();
            submission.Time = submission.Memory = null;
            submission.FailedOn = null;
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

            await _statistics.InvalidStatisticsAsync(submission.ProblemId);
            await LogInformation($"UpdateSubmission Id={submission.Id} Verdict={submission.Verdict}");
            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionEditDto(submission);
        }

        public async Task DeleteSubmissionAsync(int id)
        {
            await EnsureSubmissionExists(id);
            var submission = await Context.Submissions.FindAsync(id);
            var userId = submission.UserId;
            var problemId = submission.ProblemId;
            Context.Submissions.Remove(submission);
            await Context.SaveChangesAsync();

            var problem = await Context.Problems.FindAsync(problemId);
            var registration = await Context.Registrations.FindAsync(userId, problem.ContestId);
            if (registration != null)
            {
                await registration.RebuildStatisticsAsync(Context);
                await Context.SaveChangesAsync();
            }

            await _statistics.InvalidStatisticsAsync(submission.ProblemId);
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
                submission.ResetVerdictFields();
                await _statistics.InvalidStatisticsAsync(submission.ProblemId);
            }

            Context.UpdateRange(submissions);
            await Context.SaveChangesAsync();
            foreach (var submission in submissions)
            {
                var reloadedSubmission = await Context.Submissions.FindAsync(submission.Id);
                _queue.EnqueueTask(new JobRequestMessage(JobType.JudgeSubmission,
                        reloadedSubmission.Id, reloadedSubmission.RequestVersion + 1));
            }

            await LogInformation($"RejudgeSubmissions ContestId={contestId} " +
                                 $"ProblemId={problemId} SubmissionId={submissionId}");

            var infos = new List<SubmissionInfoDto>();
            foreach (var submission in submissions)
            {
                infos.Add(new SubmissionInfoDto(submission, true));
            }

            return infos;
        }
    }
}