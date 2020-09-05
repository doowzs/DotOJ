using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Worker.Runners;

namespace Worker.Triggers
{
    public sealed class SubmissionRunnerTrigger : TriggerBase<SubmissionRunnerTrigger>
    {
        private readonly IServiceProvider _provider;

        public SubmissionRunnerTrigger(IServiceProvider provider) : base(provider)
        {
            _provider = provider;
        }

        public override async Task CheckAndRunAsync()
        {
            var now = DateTime.Now.ToUniversalTime();
            var contestIds = await Context.Contests
                .Where(c => c.BeginTime <= now && now <= c.EndTime)
                .Select(c => c.Id)
                .ToListAsync();
            var problemIds = await Context.Problems
                .Where(p => contestIds.Contains(p.ContestId))
                .Select(p => p.Id)
                .ToListAsync();

            // Update all new pending submissions' status.
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var pendingSubmissions = await Context.Submissions
                    .Where(s => s.Verdict == Verdict.Pending).ToListAsync();
                foreach (var pendingSubmission in pendingSubmissions)
                {
                    pendingSubmission.Verdict = Verdict.InQueue;
                }

                Context.UpdateRange(pendingSubmissions);
                await Context.SaveChangesAsync();
                scope.Complete();
            }

            Submission submission;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Choose a submission that has not been judged yet.
                // Submissions of a running contest have a higher priority to be judged.
                var queryable = Context.Submissions.Where(s => string.IsNullOrEmpty(s.JudgedBy)).AsQueryable();
                if (await queryable.AnyAsync(s => problemIds.Contains(s.Id)))
                {
                    submission = await queryable.Where(s => problemIds.Contains(s.Id))
                        .OrderBy(s => s.Id).FirstOrDefaultAsync();
                }
                else
                {
                    submission = await queryable.OrderBy(s => s.Id).FirstOrDefaultAsync();
                }

                if (submission != null)
                {
                    submission.JudgedBy = Options.Value.Name;
                    Context.Update(submission);
                }

                await Context.SaveChangesAsync();
                scope.Complete();
            }

            if (submission != null)
            {
                var user = await Context.Users.FindAsync(submission.UserId);
                var problem = await Context.Problems.FindAsync(submission.ProblemId);
                var contest = await Context.Contests.FindAsync(problem.ContestId);

                try
                {
                    Logger.LogInformation($"SubmissionRunner Trigger Id={submission.Id} Problem={problem.Id}");

                    var stopwatch = Stopwatch.StartNew();
                    var runner = new SubmissionRunner(contest, problem, submission, _provider);
                    var result = await runner.RunSubmissionAsync();

                    #region Update judge result of submission

                    submission.Verdict = result.Verdict;
                    submission.Time = result.Time;
                    submission.Memory = result.Memory;
                    submission.FailedOn = result.FailedOn;
                    submission.Score = result.Score;
                    submission.Progress = 100;
                    submission.Message = result.Message;
                    submission.JudgedAt = DateTime.Now.ToUniversalTime();

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        // Validate that the submission is not touched by others since picking up.
                        var judgedBy = (await Context.Submissions.FindAsync(submission.Id)).JudgedBy;
                        if (judgedBy == Options.Value.Name)
                        {
                            Context.Submissions.Update(submission);
                            await Context.SaveChangesAsync();
                        }
                        else
                        {
                            // If the row is touched, revert all changes.
                            await Context.Entry(submission).ReloadAsync();
                        }

                        scope.Complete();
                    }

                    #endregion

                    #region Rebuild statistics of registration

                    var registration = await Context.Registrations.FindAsync(user.Id, contest.Id);
                    if (registration != null)
                    {
                        await registration.RebuildStatisticsAsync(Context);
                        Context.Registrations.Update(registration);
                        await Context.SaveChangesAsync();
                    }

                    #endregion

                    stopwatch.Stop();
                    Logger.LogInformation($"SubmissionRunner Complete Id={submission.Id} Problem={problem.Id}" +
                                          $" TimeElapsed={stopwatch.Elapsed}");
                }
                catch (Exception e)
                {
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
                }
            }
        }
    }
}