using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Shared.Models;
using Shared.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notification;
using Worker.Models;
using Worker.Runners.JudgeSubmission.ContestModes;

namespace Worker.Runners.JudgeSubmission
{
    public sealed class SubmissionRunner : JobRunnerBase<SubmissionRunner>
    {
        public SubmissionRunner(IServiceProvider provider) : base(provider)
        {
        }

        public override async Task<int> HandleJobRequest(JobRequestMessage message)
        {
            var submission = await Context.Submissions.FindAsync(message.TargetId);
            if (submission is null || submission.RequestVersion >= message.RequestVersion)
            {
                Logger.LogDebug($"IgnoreJudgeRequestMessage" +
                                $" SubmissionId={message.TargetId}" +
                                $" RequestVersion={message.RequestVersion}");
                return 0;
            }
            else
            {
                submission.Verdict = Verdict.Running;
                submission.Time = null;
                submission.Memory = null;
                submission.FailedOn = null;
                submission.Score = null;
                submission.Progress = 0;
                submission.JudgedBy = Options.Value.Name;
                submission.RequestVersion = message.RequestVersion;
                await Context.SaveChangesAsync();
            }

            await Context.Entry(submission).Reference(s => s.User).LoadAsync();
            var problem = await Context.Problems.FindAsync(submission.ProblemId);
            var contest = await Context.Contests.FindAsync(problem.ContestId);
            var user = submission.User;

            try
            {
                Logger.LogInformation($"RunSubmission Id={submission.Id} Problem={problem.Id}");
                var stopwatch = Stopwatch.StartNew();

                JudgeResult result;
                await using (var box = await Box.GetBoxAsync())
                {
                    result = await this.RunSubmissionAsync(contest, problem, submission, box);
                }

                #region Update score of result with bonus and decay

                if (contest.HasScoreBonus &&
                    contest.ScoreBonusTime.HasValue &&
                    submission.CreatedAt <= contest.ScoreBonusTime.Value)
                {
                    result.Score = result.Score * contest.ScoreBonusPercentage.GetValueOrDefault(100) / 100;
                }
                else if (contest.HasScoreDecay &&
                         contest.ScoreDecayTime.HasValue &&
                         submission.CreatedAt > contest.ScoreDecayTime.Value)
                {
                    var decayPercentage = contest.ScoreDecayPercentage.GetValueOrDefault(100);
                    if (contest.IsScoreDecayLinear.GetValueOrDefault(false))
                    {
                        var progress = contest.EndTime.Subtract(contest.ScoreDecayTime.Value).TotalSeconds /
                                       submission.CreatedAt.Subtract(contest.ScoreDecayTime.Value).TotalSeconds;
                        decayPercentage = (int) (progress * (decayPercentage - 100) + 100);
                        decayPercentage = Math.Max(0, Math.Min(decayPercentage, 100));
                    }

                    result.Score = result.Score * decayPercentage / 100;
                }

                #endregion

                #region Update judge result of submission

                submission.IsValid = result.IsValid;
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
                    var fetched = await Context.Submissions.FindAsync(submission.Id);
                    if (fetched.RequestVersion == message.RequestVersion && fetched.JudgedBy == Options.Value.Name)
                    {
                        Context.Submissions.Update(submission);
                        await Context.SaveChangesAsync();
                    }

                    scope.Complete();
                }

                #endregion

                #region Rebuild statistics of registration

                // TODO: remove this part to webapp
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
                Logger.LogInformation($"RunSubmission Complete Submission={submission.Id} Problem={problem.Id}" +
                                      $" Verdict={submission.Verdict} TimeElapsed={stopwatch.Elapsed}");
            }
            catch (Exception e)
            {
                var error = $"Internal error: {e.Message}\n" +
                            $"Occurred at {DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC @ {Options.Value.Name}\n" +
                            $"*** Please report this incident to TA and site administrator ***";
                submission.IsValid = false;
                submission.Verdict = Verdict.Failed;
                submission.Time = submission.Memory = null;
                submission.FailedOn = null;
                submission.Score = 0;
                submission.Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(error));
                submission.JudgedAt = DateTime.Now.ToUniversalTime();
                submission.JudgedBy = Options.Value.Name;

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Validate that the submission is not touched by others since picking up.
                    var fetched = await Context.Submissions.FindAsync(submission.Id);
                    if (fetched.RequestVersion == message.RequestVersion && fetched.JudgedBy == Options.Value.Name)
                    {
                        Context.Submissions.Update(submission);
                        await Context.SaveChangesAsync();
                    }

                    scope.Complete();
                }

                Logger.LogError($"RunSubmission Error Submission={submission.Id} Error={e.Message}\n" +
                                $"Stacktrace of error:\n{e.StackTrace}");
                var broadcaster = Provider.GetRequiredService<INotificationBroadcaster>();
                await broadcaster.SendNotification(true, $"Runner failed on Submission #{submission.Id}",
                    $"Submission runner \"{Options.Value.Name}\" failed on submission #{submission.Id}" +
                    $" with error message **\"{e.Message}\"**.");
            }

            return submission.RequestVersion;
        }

        private async Task<JudgeResult> RunSubmissionAsync
            (Contest contest, Problem problem, Submission submission, Box box)
        {
            ContestRunnerBase runner;
            switch (contest.Mode)
            {
                case ContestMode.Practice:
                    runner = new PracticeRunner(contest, problem, submission, box, Provider);
                    break;
                case ContestMode.OneShot:
                    runner = new OneShotRunner(contest, problem, submission, box, Provider);
                    break;
                case ContestMode.UntilFail:
                    runner = new UntilFailRunner(contest, problem, submission, box, Provider);
                    break;
                case ContestMode.SampleOnly:
                    runner = new SampleOnlyRunner(contest, problem, submission, box, Provider);
                    break;
                default:
                    throw new Exception($"Unknown contest mode ${contest.Mode}");
            }

            return await runner.RunSubmissionAsync();
        }
    }
}