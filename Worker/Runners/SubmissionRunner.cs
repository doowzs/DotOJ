using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Data;
using Data.Configs;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification;
using Worker.Runners.Modes;

namespace Worker.Runners
{
    public interface ISubmissionRunner
    {
        public Task RunSubmissionAsync(Submission submission);
    }

    public class SubmissionRunner : ISubmissionRunner
    {
        protected readonly ApplicationDbContext Context;
        protected readonly INotificationBroadcaster Broadcaster;
        protected readonly IOptions<JudgingConfig> Options;
        protected readonly ILogger<SubmissionRunner> Logger;
        protected readonly IServiceProvider Provider;

        public SubmissionRunner(IServiceProvider provider)
        {
            Context = provider.GetRequiredService<ApplicationDbContext>();
            Broadcaster = provider.GetRequiredService<INotificationBroadcaster>();
            Options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            Logger = provider.GetRequiredService<ILogger<SubmissionRunner>>();
            Provider = provider;
        }

        private async Task EnsureRegistrationExists(ApplicationUser user, Contest contest)
        {
            var begun = DateTime.Now.ToUniversalTime() > contest.BeginTime;
            var ended = DateTime.Now.ToUniversalTime() > contest.EndTime;

            var roleIds = await Context.Roles.Where(r =>
                    r.Name == ApplicationRoles.Administrator || r.Name == ApplicationRoles.ContestManager)
                .Select(r => r.Id)
                .ToListAsync();
            var privileged = await Context.UserRoles
                .AnyAsync(ur => ur.UserId == user.Id && roleIds.Contains(ur.RoleId));

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
            var user = await Context.Users.FindAsync(submission.UserId);
            var problem = await Context.Problems.FindAsync(submission.ProblemId);
            var contest = await Context.Contests.FindAsync(problem.ContestId);
            await EnsureRegistrationExists(user, contest);

            {
                Logger.LogInformation($"JudgeSubmission Start Id={submission.Id} Problem={submission.ProblemId}");

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

                var result = await submissionRunner.RunSubmissionAsync(submission, problem);
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

                Logger.LogInformation($"JudgeSubmission Complete Id={submission.Id} Problem={submission.ProblemId} " +
                                      $"Verdict={submission.Verdict} Score={submission.Score} CreatedAt={submission.CreatedAt} JudgedAt={submission.JudgedAt}");
            }
            /*
                submission.Verdict = Verdict.Failed;
                submission.FailedOn = null;
                submission.Score = 0;
                submission.JudgedAt = DateTime.Now.ToUniversalTime();
                Context.Submissions.Update(submission);
                await Context.SaveChangesAsync();
                Logger.LogError($"RunSubmission Error Id={submission.Id} Error={e.Message}");
                await Broadcaster.SendNotification(true, $"Runner failed on Submission #{submission.Id}",
                    $"Submission runner \"{Options.Value.Name}\" failed on submission #{submission.Id}" +
                    $" with error message **\"{e.Message}\"**.");
            }*/
        }
    }
}