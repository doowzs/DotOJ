using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Transactions;
using Data.Configs;
using Data.Generics;
using Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notification;
using Worker.Runners.Modes;

namespace Worker.Runners
{
    public interface ISubmissionRunner
    {
        public Task RunSubmissionAsync(Submission submission);
    }

    public class SubmissionRunner : LoggableService<SubmissionRunner>, ISubmissionRunner
    {
        protected readonly INotificationBroadcaster Broadcaster;
        protected readonly IOptions<ApplicationConfig> AppOptions;

        public SubmissionRunner(IServiceProvider provider) : base(provider, true)
        {
            Broadcaster = provider.GetRequiredService<INotificationBroadcaster>();
            AppOptions = provider.GetRequiredService<IOptions<ApplicationConfig>>();
        }

        private async Task EnsureRegistrationExists(ApplicationUser user, Contest contest)
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

        public async Task RunSubmissionAsync(Submission submission)
        {
            var user = await Manager.FindByIdAsync(submission.UserId);
            var problem = await Context.Problems.FindAsync(submission.ProblemId);
            var contest = await Context.Contests.FindAsync(problem.ContestId);
            await EnsureRegistrationExists(user, contest);

            try
            {
                await LogInformation($"JudgeSubmission Start Id={submission.Id} Problem={submission.ProblemId}");

                IModeSubmissionRunner submissionRunner;
                switch (contest.Mode)
                {
                    case ContestMode.Practice:
                        submissionRunner = new PracticeModeSubmissionRunner(Provider);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                #region Update judge result of submission

                var result = await submissionRunner.RunAsync(submission, problem);
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

                await LogInformation($"JudgeSubmission Complete Id={submission.Id} Problem={submission.ProblemId} " +
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
                await LogError($"RunSubmission Error Id={submission.Id} Error={e.Message}");
                await Broadcaster.SendNotification(true, $"Runner failed on Submission #{submission.Id}",
                    $"Submission runner failed on [submission #{submission.Id}]" +
                    $"({AppOptions.Value.Host}/admin/submission/{submission.Id}) " +
                    $"with error message **\"{e.Message}\"**.");
            }
        }
    }
}