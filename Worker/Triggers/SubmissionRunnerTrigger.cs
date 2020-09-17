using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        public override async Task<bool> CheckAndRunAsync()
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
                    // Must reset all fields to clear cache in DBContext.
                    pendingSubmission.Verdict = Verdict.InQueue;
                    pendingSubmission.Time = null;
                    pendingSubmission.Memory = null;
                    pendingSubmission.Score = null;
                    pendingSubmission.FailedOn = null;
                    pendingSubmission.Progress = null;
                    pendingSubmission.Message = null;
                    pendingSubmission.JudgedBy = null;
                    pendingSubmission.JudgedAt = null;
                }

                Context.UpdateRange(pendingSubmissions);
                await Context.SaveChangesAsync();
                scope.Complete();
            }

            Submission submission;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Choose a submission that has not been judged yet, with a favor for contest submissions.
                // To prevent hunger, only choose contest submission from the oldest 10 submissions if available.
                var queryable = Context.Submissions.Where(s => string.IsNullOrEmpty(s.JudgedBy)).AsQueryable();
                submission = await queryable.OrderBy(s => s.Id).FirstOrDefaultAsync();

                if (submission != null && !problemIds.Contains(submission.ProblemId) &&
                    await queryable.AnyAsync(s => s.Id < submission.Id + 10 && problemIds.Contains(s.ProblemId)))
                {
                    var idLimit = submission.Id + 10;
                    submission = await queryable.Where(s => s.Id < idLimit && problemIds.Contains(s.ProblemId))
                        .OrderBy(s => s.Id).FirstAsync();
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
                    submission.Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(result.Message));
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

                    if (submission.CreatedAt >= contest.BeginTime && submission.CreatedAt <= contest.EndTime)
                    {
                        var registration = await Context.Registrations.FindAsync(user.Id, contest.Id);
                        if (registration != null)
                        {
                            await registration.RebuildStatisticsAsync(Context);
                            Context.Registrations.Update(registration);
                            await Context.SaveChangesAsync();
                        }
                    }

                    #endregion

                    stopwatch.Stop();
                    Logger.LogInformation($"SubmissionRunner Complete Submission={submission.Id} Problem={problem.Id}" +
                                          $" Verdict={submission.Verdict} TimeElapsed={stopwatch.Elapsed}");
                }
                catch (Exception e)
                {
                    submission.Verdict = Verdict.Failed;
                    submission.FailedOn = null;
                    submission.Score = 0;
                    submission.Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(e.Message));
                    submission.JudgedAt = DateTime.Now.ToUniversalTime();
                    Context.Submissions.Update(submission);
                    await Context.SaveChangesAsync();
                    Logger.LogError($"RunSubmission Error Submission={submission.Id} Error={e.Message}");
                    await Broadcaster.SendNotification(true, $"Runner failed on Submission #{submission.Id}",
                        $"Submission runner \"{Options.Value.Name}\" failed on submission #{submission.Id}" +
                        $" with error message **\"{e.Message}\"**.");
                }
            }

            return submission != null;
        }
    }
}