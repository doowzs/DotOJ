using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Data;
using Data.Configs;
using Data.Models;
using Data.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Worker.Runners;

namespace Worker.RabbitMQ
{
    public sealed class JudgeRequestConsumer : RabbitMqQueueBase<JudgeRequestConsumer>
    {
        private readonly IServiceProvider _provider;
        private readonly IOptions<JudgingConfig> _options;
        private readonly ApplicationDbContext _context;
        private readonly INotificationBroadcaster _broadcaster;

        public JudgeRequestConsumer(IServiceProvider provider) : base(provider)
        {
            Queue = "JudgeRequest";
            _provider = provider;
            _context = provider.GetRequiredService<ApplicationDbContext>();
            _options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            _broadcaster = provider.GetRequiredService<INotificationBroadcaster>();
        }

        public override void Start(IConnection connection)
        {
            base.Start(connection);
            var consumer = new AsyncEventingBasicConsumer(Channel);
            Channel.BasicConsume(Queue, false, consumer); // disable auto ack for scheduling
            Channel.BasicQos(0, 1, false);
            consumer.Received += async (ch, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                if (int.TryParse(message, out var submissionId))
                {
                    await RunSubmissionAsync(submissionId);
                    Channel.BasicAck(ea.DeliveryTag, true);
                }
                else
                {
                    Logger.LogError($"Invalid judge request message: {message}");
                }
            };
        }

        private async Task RunSubmissionAsync(int submissionId)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission is null)
            {
                Logger.LogError($"Submission with Id={submissionId} not found");
                return;
            }

            var user = await _context.Users.FindAsync(submission.UserId);
            var problem = await _context.Problems.FindAsync(submission.ProblemId);
            var contest = await _context.Contests.FindAsync(problem.ContestId);

            try
            {
                Logger.LogInformation($"RunSubmission Id={submission.Id} Problem={problem.Id}");
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
                    var judgedBy = (await _context.Submissions.FindAsync(submission.Id)).JudgedBy;
                    if (judgedBy == _options.Value.Name)
                    {
                        _context.Submissions.Update(submission);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // If the row is touched, revert all changes.
                        await _context.Entry(submission).ReloadAsync();
                    }

                    scope.Complete();
                }

                #endregion

                #region Rebuild statistics of registration

                if (submission.CreatedAt >= contest.BeginTime && submission.CreatedAt <= contest.EndTime)
                {
                    var registration = await _context.Registrations.FindAsync(user.Id, contest.Id);
                    if (registration != null)
                    {
                        await registration.RebuildStatisticsAsync(_context);
                        _context.Registrations.Update(registration);
                        await _context.SaveChangesAsync();
                    }
                }

                #endregion

                stopwatch.Stop();
                Logger.LogInformation($"RunSubmission Complete Submission={submission.Id} Problem={problem.Id}" +
                                      $" Verdict={submission.Verdict} TimeElapsed={stopwatch.Elapsed}");
            }
            catch (Exception e)
            {
                var message = "Error: " + e.Message + "\n*** Please report this to TA and site administrator ***";
                submission.Verdict = Verdict.Failed;
                submission.FailedOn = null;
                submission.Score = 0;
                submission.Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
                submission.JudgedAt = DateTime.Now.ToUniversalTime();
                _context.Submissions.Update(submission);
                await _context.SaveChangesAsync();
                Logger.LogError($"RunSubmission Error Submission={submissionId} Error={e.Message}");
                await _broadcaster.SendNotification(true, $"Runner failed on Submission #{submissionId}",
                    $"Submission runner \"{_options.Value.Name}\" failed on submission #{submissionId}" +
                    $" with error message **\"{e.Message}\"**.");
            }
        }
    }
}