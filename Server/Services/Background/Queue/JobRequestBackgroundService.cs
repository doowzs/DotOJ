using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.RabbitMQ;
using Shared.Generics;
using Shared.RabbitMQ;

namespace Server.Services.Background.Queue
{
    public sealed class JobRequestBackgroundService : BackgroundQueueService<JobRequestMessage>
    {
        private JobRequestProducer _producer { get; set; }

        public JobRequestBackgroundService(IServiceProvider provider) : base(provider)
        {
            _producer = provider.GetRequiredService<JobRequestProducer>();
        }

        protected override async Task ExecuteTaskAsync(JobRequestMessage message, CancellationToken cancellationToken)
        {
            await _producer.SendAsync(message);
        }
    }
}
