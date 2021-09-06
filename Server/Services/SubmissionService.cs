using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Generics;
using Shared.RabbitMQ;
using Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Exceptions;
using Server.Services.Singleton;

namespace Server.Services
{
    public interface ISubmissionService
    {
        public Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionsAsync(int? contestId, string userId,
            string contestantId, int? problemId, Verdict? verdict, int? pageSize, int? pageIndex);

        public Task<List<SubmissionInfoDto>> GetBatchSubmissionInfosAsync(IEnumerable<int> ids);
        public Task<SubmissionInfoDto> GetSubmissionInfoAsync(int id);
        public Task<SubmissionViewDto> GetSubmissionViewAsync(int id);
        public Task<(byte[], string)> DownloadSubmissionAsync(int id);
        public Task<SubmissionInfoDto> CreateSubmissionAsync(SubmissionCreateDto dto);
        public Task<string> GetTestKitLabSubmitTokenAsync(int problemId);
        public Task<string> CreateTestKitLabSubmissionAsync(string token, IFormFile file);
    }

    public class SubmissionService : LoggableService<SubmissionService>, ISubmissionService
    {
        private const int PageSize = 50;
        private readonly BackgroundTaskQueue<JobRequestMessage> _queue;
        private readonly TestKitLabSubmitTokenService _token;

        public SubmissionService(IServiceProvider provider) : base(provider)
        {
            _queue = provider.GetRequiredService<BackgroundTaskQueue<JobRequestMessage>>();
            _token = provider.GetRequiredService<TestKitLabSubmitTokenService>();
        }

        private async Task<Boolean> IsSubmissionViewableAsync(Submission submission)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (submission.UserId == user.Id)
            {
                return true;
            }

            if (await Manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                await Manager.IsInRoleAsync(user, ApplicationRoles.ContestManager) ||
                await Manager.IsInRoleAsync(user, ApplicationRoles.SubmissionManager))
            {
                return true;
            }

            if (Config.Value.ExamId.HasValue)
            {
                return false;
            }

            var problem = await Context.Problems.FindAsync(submission.ProblemId);
            if (problem.Type != ProblemType.Ordinary)
            {
                return false; // Cannot view lab submissions
            }

            var contest = await Context.Contests.FindAsync(problem.ContestId);
            if (DateTime.Now.ToUniversalTime() > contest.EndTime)
            {
                return true;
            }

            return await Context.Submissions.AnyAsync(s =>
                s.UserId == user.Id && s.ProblemId == problem.Id && s.Verdict == Verdict.Accepted);
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
                        Statistics = new List<RegistrationProblemStatistics>()
                    };
                    await Context.Registrations.AddAsync(registration);
                    await Context.SaveChangesAsync();
                }
                else
                {
                    throw new UnauthorizedAccessException("Unregistered user cannot submit until contest has ended.");
                }
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

            return contest;
        }

        public async Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionsAsync(int? contestId, string userId,
            string contestantId, int? problemId, Verdict? verdict, int? pageSize, int? pageIndex)
        {
            var submissions = Context.Submissions.AsQueryable();

            var currentUser = await Manager.GetUserAsync(Accessor.HttpContext.User);
            bool canViewHiddenSubmissions = await Manager.IsInRoleAsync(currentUser, ApplicationRoles.Administrator) ||
                                            await Manager.IsInRoleAsync(currentUser, ApplicationRoles.ContestManager) ||
                                            await Manager.IsInRoleAsync(currentUser,
                                                ApplicationRoles.SubmissionManager);
            if (!canViewHiddenSubmissions)
            {
                submissions = submissions.Where(s => !s.Hidden);
            }

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

            return new PaginatedList<SubmissionInfoDto>
                (paginated.TotalItems, paginated.PageIndex, paginated.PageSize, infos);
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

            await Context.Entry(submission).Reference(s => s.Problem).LoadAsync();
            await EnsureUserCanViewSubmissionAsync(submission);
            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            return new SubmissionViewDto(submission, Config);
        }

        public async Task<(byte[], string)> DownloadSubmissionAsync(int id)
        {
            var submission = await Context.Submissions.FindAsync(id);
            if (submission == null)
            {
                throw new NotFoundException();
            }

            await Context.Entry(submission).Reference(s => s.Problem).LoadAsync();
            await EnsureUserCanViewSubmissionAsync(submission);
            await Context.Entry(submission).Reference(s => s.User).LoadAsync();

            var filename = submission.User.ContestantId + '-' + submission.Id +
                           submission.Program.GetSourceFileExtension();
            switch (submission.Problem.Type)
            {
                case ProblemType.Ordinary:
                    return (Convert.FromBase64String(submission.Program.Code), filename);
                case ProblemType.TestKitLab:
                    var path = Path.Combine(Config.Value.DataPath, "submissions", submission.Id.ToString());
                    var file = Encoding.UTF8.GetString(Convert.FromBase64String(submission.Program.Code));
                    return (await File.ReadAllBytesAsync(Path.Combine(path, file)), filename);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<SubmissionInfoDto> CreateSubmissionAsync(SubmissionCreateDto dto)
        {
            var contest = await ValidateSubmissionCreateDtoAsync(dto);

            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            var lastSubmission = await Context.Submissions
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();
            if (lastSubmission != null &&
                (DateTime.Now.ToUniversalTime() - lastSubmission.CreatedAt).TotalSeconds < 5)
            {
                throw new TooManyRequestsException("Cannot submit twice between 5 seconds.");
            }

            var submission = new Submission
            {
                UserId = Accessor.HttpContext.User.Identity.Name,
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
            _queue.EnqueueTask(new JobRequestMessage(JobType.JudgeSubmission, submission.Id, 1));

            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            var result = new SubmissionInfoDto(submission, true);
            await LogInformation($"CreateSubmission Id={result.Id} ProblemId={result.ProblemId}" +
                                 $" ContestantId={user.ContestantId} Language={result.Language}");
            return result;
        }

        public async Task<string> GetTestKitLabSubmitTokenAsync(int problemId)
        {
            var problem = await Context.Problems.FindAsync(problemId);
            if (problem is null || problem.Type != ProblemType.TestKitLab)
            {
                throw new BadHttpRequestException("Invalid problem ID or problem does not allow token submit.");
            }

            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            return await _token.GetOrGenerateToken(user.Id, problemId);
        }

        public async Task<string> CreateTestKitLabSubmissionAsync(string token, IFormFile file)
        {
            var (userId, problemId) = await _token.ConsumeTokenAsync(token);
            if (userId is null)
            {
                throw new UnauthorizedAccessException("Unauthorized: Invalid submit token.");
            }

            try
            {
                var stream = file.OpenReadStream();
                var archive = new ZipArchive(stream, ZipArchiveMode.Read);
                if (archive.Entries.Count == 0)
                {
                    throw new BadHttpRequestException("BadRequest: Zip archive is empty.");
                }
            }
            catch (Exception)
            {
                throw new BadHttpRequestException($"BadRequest: File is not a valid zip archive.");
            }

            var user = await Context.Users.FindAsync(userId);
            if (user is null)
            {
                throw new Exception("User does not exist.");
            }

            var problem = await Context.Problems.FindAsync(problemId);
            if (problem is null)
            {
                throw new Exception("Problem does not exist.");
            }
            else if (problem.Type != ProblemType.TestKitLab)
            {
                throw new BadHttpRequestException("BadRequest: Problem does not accept submission through this API.");
            }

            var contest = await Context.Contests.FindAsync(problem.ContestId);
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
                UserId = userId,
                ProblemId = problemId,
                Program = new Shared.Models.Program
                {
                    Language = Language.LabArchive,
                    Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(file.FileName))
                },
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


            var folder = Path.Combine(Config.Value.DataPath, "submissions", submission.Id.ToString());
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            await using (var stream = new FileStream(Path.Combine(folder, file.FileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _queue.EnqueueTask(new JobRequestMessage(JobType.JudgeSubmission, submission.Id, 1));
            
            await LogInformation($"CreateSubmission Id={submission.Id} ProblemId={problem.Id}" +
                                 $" ContestantId={user.ContestantId} Language={Language.LabArchive}");
            return $"Accepted submission #{submission.Id} for contestant {user.ContestantId}.";
        }
    }
}