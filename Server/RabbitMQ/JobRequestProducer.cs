using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Shared.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Server.Services.Singleton;

namespace Server.RabbitMQ
{
    public class JobRequestProducer : RabbitMqQueueBase<JobRequestProducer>
    {
        private readonly IServiceScopeFactory _factory;

        public JobRequestProducer(IServiceProvider provider) : base(provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
        }

        public async Task<bool> SendAsync(JobType jobType, int targetId, int requestVersion)
        {
            var message = new JobRequestMessage(jobType, targetId, requestVersion);
            var serialized = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(serialized);

            using var scope = _factory.CreateScope();
            var queueStatisticsService = scope.ServiceProvider.GetRequiredService<QueueStatisticsService>();
            try
            {
                Channel.BasicPublish("", Queue, null, body);
                await queueStatisticsService.AddJobRequestAsync(message);
                Logger.LogDebug($"SendJobRequestMessage JobType={jobType}" +
                                $" TargetId={targetId} RequestVersion={requestVersion}");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"SendJobRequestMessage failed: {e.Message}");
                Logger.LogError($"Stacktrace: {e.StackTrace}");
                return false;
            }
        }
    }
}