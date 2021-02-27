using System;
using System.Text;
using Shared.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Worker.Runners;
using Worker.Runners.CheckPlagiarism;
using Worker.Runners.JudgeSubmission;

namespace Worker.RabbitMQ
{
    public sealed class JobRequestConsumer : RabbitMqQueueBase<JobRequestConsumer>
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JobCompleteProducer _producer;

        public JobRequestConsumer(IServiceProvider provider) : base(provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _producer = provider.GetRequiredService<JobCompleteProducer>();
        }

        public override void Start(IConnection connection)
        {
            base.Start(connection);
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.Received += async (ch, ea) =>
            {
                using var scope = _factory.CreateScope();
                var serialized = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonConvert.DeserializeObject<JobRequestMessage>(serialized);

                IJobRunner runner = null;
                switch (message.JobType)
                {
                    case JobType.JudgeSubmission:
                        runner = new SubmissionRunner(scope.ServiceProvider);
                        break;
                    case JobType.CheckPlagiarism:
                        runner = new PlagiarismChecker(scope.ServiceProvider);
                        break;
                    default:
                        Logger.LogError($"Unknown job type JobType={message.JobType}");
                        break;
                }
                if (runner is not null)
                {
                    var completeVersion = await runner.HandleJobRequest(message);
                    if (completeVersion > 0)
                    {
                        await _producer.SendAsync(message.JobType, message.TargetId, completeVersion);
                    }
                }
                Channel.BasicAck(ea.DeliveryTag, false);
            };
            Channel.BasicQos(0, 1, false);
            Channel.BasicConsume(Queue, false, consumer); // disable auto ack for work scheduling
        }
    }
}