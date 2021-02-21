using System;
using System.Collections.Generic;
using System.Text;
using Data;
using Data.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebApp.Services.Singleton;

namespace WebApp.RabbitMQ
{
    public class JudgeCompleteConsumer : RabbitMqQueueBase<JudgeCompleteConsumer>
    {
        private readonly ProblemStatisticsService _problemStatisticsService;
        private readonly WorkerStatisticsService _workerStatisticsService;
        private static readonly Dictionary<int, DateTime> Dictionary = new();

        public JudgeCompleteConsumer(IServiceProvider provider) : base(provider)
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
                var message = JsonConvert.DeserializeObject<JudgeCompleteMessage>(serialized);
                await _problemStatisticsService.UpdateStatisticsAsync(message);
                Channel.BasicAck(ea.DeliveryTag, true);
            };
        }
    }
}