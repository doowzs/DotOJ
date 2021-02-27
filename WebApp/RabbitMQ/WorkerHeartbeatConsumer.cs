using System;
using System.Text;
using Shared.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebApp.Services.Singleton;

namespace WebApp.RabbitMQ
{
    public class WorkerHeartbeatConsumer : RabbitMqQueueBase<WorkerHeartbeatConsumer>
    {
        private readonly WorkerStatisticsService _service;
        
        public WorkerHeartbeatConsumer(IServiceProvider provider) : base(provider)
        {
            _service = provider.GetRequiredService<WorkerStatisticsService>();
        }

        public override void Start(IConnection connection)
        {
            base.Start(connection);
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.Received += async (ch, ea) =>
            {
                var serialized = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonConvert.DeserializeObject<WorkerHeartbeatMessage>(serialized);
                await _service.HandleWorkerHeartbeatAsync(message);
                Channel.BasicAck(ea.DeliveryTag, false);
            };
            Channel.BasicQos(0, 1, false);
            Channel.BasicConsume(Queue, false, consumer);
        }
    }
}