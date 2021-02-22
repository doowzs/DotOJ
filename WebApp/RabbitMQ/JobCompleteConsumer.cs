using System;
using System.Collections.Generic;
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
        private readonly ProblemStatisticsService _problemStatisticsService;
        private readonly WorkerStatisticsService _workerStatisticsService;
        private static readonly Dictionary<int, DateTime> Dictionary = new();

        public JobCompleteConsumer(IServiceProvider provider) : base(provider)
        {
            _problemStatisticsService = provider.GetRequiredService<ProblemStatisticsService>();
            _workerStatisticsService = provider.GetRequiredService<WorkerStatisticsService>();
        }

        public override void Start(IConnection connection)
        {
            base.Start(connection);

            var consumer = new AsyncEventingBasicConsumer(Channel);
            Channel.BasicConsume(Queue, false, consumer);
            Channel.BasicQos(0, 1, false);
            consumer.Received += async (ch, ea) =>
            {
                var serialized = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonConvert.DeserializeObject<JobCompleteMessage>(serialized);
                
                switch (message.JobType)
                {
                    case JobType.JudgeSubmission:
                        await _problemStatisticsService.UpdateStatisticsAsync(message);
                        break;
                    case JobType.CheckPlagiarism:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Channel.BasicAck(ea.DeliveryTag, true);
            };
        }
    }
}