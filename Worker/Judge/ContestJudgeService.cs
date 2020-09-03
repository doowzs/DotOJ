using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Transactions;
using Data.Configs;
using Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApp.Notifications;
using WebApp.Services.Judge.Submission;

namespace WebApp.Services.Judge
{
    public interface IContestJudgeService
    {
        public Task JudgeSubmission(int submissionId);
    }

    public class ContestJudgeService : LoggableService<ContestJudgeService>, IContestJudgeService
    {
        protected readonly INotificationBroadcaster Broadcaster;
        protected readonly IOptions<ApplicationConfig> AppOptions;

        public ContestJudgeService(IServiceProvider provider) : base(provider, true)
        {
            Broadcaster = provider.GetRequiredService<INotificationBroadcaster>();
            AppOptions = provider.GetRequiredService<IOptions<ApplicationConfig>>();
        }

        private async Task EnsureUserCanSubmit(ApplicationUser user, Contest contest)
        {
            var begun = DateTime.Now.ToUniversalTime() > contest.BeginTime;
            var ended = DateTime.Now.ToUniversalTime() > contest.EndTime;
            var privileged = await Manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                             await Manager.IsInRoleAsync(user, ApplicationRoles.ContestManager);

            // Only administrators can submit before contest begins.
            if (!begun && !privileged)
            {
                throw new UnauthorizedAccessException("Non-privileged submission before contest begins.");
            }

            // Any user can submit to any contest after contest ends.
            if (ended)
            {
                return;
            }

            // Otherwise, only registered user can submit to a running contest.
            if (contest.IsPublic)
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                var registration = await Context.Registrations.FindAsync(user.Id, contest.Id);
                if (registration == null)
                {
                    await Context.Registrations.AddAsync(new Registration
                    {
                        UserId = user.Id,
                        ContestId = contest.Id,
                        IsParticipant = true,
                        IsContestManager = false,
                        Statistics = new List<ProblemStatistics>()
                    });
                }
            }
            else
            {
                if (await Context.Registrations.FindAsync(user.Id, contest.Id) == null)
                {
                    throw new UnauthorizedAccessException("Not registered to a private running contest.");
                }
            }
        }

        public async Task JudgeSubmission(int submissionId)
        {
            var submission = await Context.Submissions.FindAsync(submissionId);
            if (submission == null)
            {
                throw new ValidationException("Invalid submission ID.");
            }

            try
            {
                await LogInformation($"JudgeSubmission Start Id={submission.Id} Problem={submission.ProblemId}");

                var user = await Manager.FindByIdAsync(submission.UserId);
                var problem = await Context.Problems.FindAsync(submission.ProblemId);
                var contest = await Context.Contests.FindAsync(problem.ContestId);
                await EnsureUserCanSubmit(user, contest);

                ISubmissionJudgeService judgeService;
                switch (contest.Mode)
                {
                    case ContestMode.Practice:
                        judgeService = new PracticeModeJudgeService(Provider);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                #region Update judge result of submission

                var result = await judgeService.Judge(submission, problem);
                submission.Verdict = result.Verdict;
                submission.Time = result.Time;
                submission.Memory = result.Memory;
                submission.FailedOn = result.FailedOn;
                submission.Score = result.Score;
                submission.Progress = 100;
                submission.Message = result.Message;
                submission.JudgedAt = DateTime.Now.ToUniversalTime();
                Context.Submissions.Update(submission);
                await Context.SaveChangesAsync();

                #endregion

                #region Rebuild statistics of registration

                var registration = await Context.Registrations.FindAsync(user.Id, contest.Id);
                await registration.RebuildStatisticsAsync(Context);
                Context.Registrations.Update(registration);
                await Context.SaveChangesAsync();

                #endregion

                await LogInformation($"JudgeSubmission Complete Id={submissionId} Problem={submission.ProblemId} " +
                                     $"Verdict={submission.Verdict} Score={submission.Score} CreatedAt={submission.CreatedAt} JudgedAt={submission.JudgedAt}");
            }
            catch (Exception e)
            {
                submission.Verdict = Verdict.Failed;
                submission.FailedOn = null;
                submission.Score = 0;
                submission.JudgedAt = DateTime.Now.ToUniversalTime();
                Context.Submissions.Update(submission);
                await Context.SaveChangesAsync();
                await LogError($"JudgeSubmission Error Id={submissionId} Error={e.Message}");
                await Broadcaster.SendNotification(true, $"Judge Service Failed on Submission #{submissionId}",
                    $"Contest judge service failed on [submission #{submissionId}]" +
                    $"({AppOptions.Value.Host}/admin/submission/{submissionId}) " +
                    $"with error message **\"{e.Message}\"**.");
            }
        }
    }
}