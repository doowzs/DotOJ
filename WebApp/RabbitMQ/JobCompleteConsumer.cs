using System;
using System.Text;
using Data.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebApp.Services.Singleton;

namespace WebApp.RabbitMQ
{
    public class JobCompleteConsumer : RabbitMqQueueBase<JobCompleteConsumer>
    {
        private readonly IServiceScopeFactory _factory;

        public JobCompleteConsumer(IServiceProvider provider) : base(provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
        }

        public override void Start(IConnection connection)
        {
            base.Start(connection);
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.Received += async (ch, ea) =>
            {
                var serialized = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonConvert.DeserializeObject<JobCompleteMessage>(serialized);

                using var scope = _factory.CreateScope();
                var problemStatisticsService = scope.ServiceProvider.GetRequiredService<ProblemStatisticsService>();
                var queueStatisticsService = scope.ServiceProvider.GetRequiredService<QueueStatisticsService>();
                switch (message.JobType)
                {
                    case JobType.JudgeSubmission:
                        await problemStatisticsService.UpdateStatisticsAsync(message);
                        break;
                    case JobType.CheckPlagiarism:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                await queueStatisticsService.RemoveJobRequestAsync(message);

                Channel.BasicAck(ea.DeliveryTag, false);
            };
            Channel.BasicQos(0, 1, false);
            Channel.BasicConsume(Queue, false, consumer);
        }
    }
}