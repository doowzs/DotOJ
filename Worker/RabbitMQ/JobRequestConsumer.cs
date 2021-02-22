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
using Newtonsoft.Json;
using Notification;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Worker.Runners;

namespace Worker.RabbitMQ
{
    public sealed class JobRequestConsumer : RabbitMqQueueBase<JobRequestConsumer>
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IOptions<JudgingConfig> _options;
        private readonly JobCompleteProducer _producer;

        public JobRequestConsumer(IServiceProvider provider) : base(provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            _producer = provider.GetRequiredService<JobCompleteProducer>();
        }

        public override void Start(IConnection connection)
        {
            base.Start(connection);
            var consumer = new AsyncEventingBasicConsumer(Channel);
            Channel.BasicConsume(Queue, false, consumer); // disable auto ack for scheduling
            Channel.BasicQos(0, 1, false);
            consumer.Received += async (ch, ea) =>
            {
                var serialized = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonConvert.DeserializeObject<JobRequestMessage>(serialized);

                switch (message.JobType)
                {
                    case JobType.JudgeSubmission:
                        var completeVersion = await RunSubmissionAsync(message);
                        if (completeVersion > 0)
                        {
                            await _producer.SendAsync(JobType.JudgeSubmission, message.TargetId, completeVersion);
                        }
                        break;
                    case JobType.CheckPlagiarism:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Channel.BasicAck(ea.DeliveryTag, true);
            };
        }

        private async Task<int> RunSubmissionAsync(JobRequestMessage message)
        {
            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var submission = await context.Submissions.FindAsync(message.TargetId);
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
                submission.JudgedBy = _options.Value.Name;
                submission.RequestVersion = message.RequestVersion;
                await context.SaveChangesAsync();
            }

            var user = await context.Users.FindAsync(submission.UserId);
            var problem = await context.Problems.FindAsync(submission.ProblemId);
            var contest = await context.Contests.FindAsync(problem.ContestId);

            try
            {
                Logger.LogInformation($"RunSubmission Id={submission.Id} Problem={problem.Id}");
                var stopwatch = Stopwatch.StartNew();

                var runner = new SubmissionRunner(contest, problem, submission, scope.ServiceProvider);
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

                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Validate that the submission is not touched by others since picking up.
                    var fetched = await context.Submissions.FindAsync(submission.Id);
                    if (fetched.RequestVersion == message.RequestVersion && fetched.JudgedBy == _options.Value.Name)
                    {
                        context.Submissions.Update(submission);
                        await context.SaveChangesAsync();
                    }

                    transactionScope.Complete();
                }

                #endregion

                #region Rebuild statistics of registration

                // TODO: remove this part to webapp
                if (submission.CreatedAt >= contest.BeginTime && submission.CreatedAt <= contest.EndTime)
                {
                    var registration = await context.Registrations.FindAsync(user.Id, contest.Id);
                    if (registration != null)
                    {
                        await registration.RebuildStatisticsAsync(context);
                        context.Registrations.Update(registration);
                        await context.SaveChangesAsync();
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
                            $"Occurred at {DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC @ {_options.Value.Name}\n" +
                            $"*** Please report this incident to TA and site administrator ***";
                submission.Verdict = Verdict.Failed;
                submission.Time = submission.Memory = null;
                submission.FailedOn = null;
                submission.Score = 0;
                submission.Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(error));
                submission.JudgedAt = DateTime.Now.ToUniversalTime();
                submission.JudgedBy = _options.Value.Name;

                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Validate that the submission is not touched by others since picking up.
                    var fetched = await context.Submissions.FindAsync(submission.Id);
                    if (fetched.RequestVersion == message.RequestVersion && fetched.JudgedBy == _options.Value.Name)
                    {
                        context.Submissions.Update(submission);
                        await context.SaveChangesAsync();
                    }

                    transactionScope.Complete();
                }

                Logger.LogError($"RunSubmission Error Submission={submission.Id} Error={e.Message}\n" +
                                $"Stacktrace of error:\n{e.StackTrace}");
                var broadcaster = scope.ServiceProvider.GetRequiredService<INotificationBroadcaster>();
                await broadcaster.SendNotification(true, $"Runner failed on Submission #{submission.Id}",
                    $"Submission runner \"{_options.Value.Name}\" failed on submission #{submission.Id}" +
                    $" with error message **\"{e.Message}\"**.");
            }

            return submission.RequestVersion;
        }
    }
}