using System;
using System.Text;
using System.Threading.Tasks;
using Data.RabbitMQ;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace WebApp.RabbitMQ
{
    public class JobRequestProducer : RabbitMqQueueBase<JobRequestProducer>
    {
        public JobRequestProducer(IServiceProvider provider) : base(provider)
        {
        }

        public Task<bool> SendAsync(JobType jobType, int targetId, int requestVersion)
        {
            var message = new JobRequestMessage(jobType, targetId, requestVersion);
            var serialized = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(serialized);
            try
            {
                Channel.BasicPublish("", Queue, null, body);
                Logger.LogDebug($"SendJobRequestMessage JobType={jobType}" +
                                $" TargetId={targetId} RequestVersion={requestVersion}");
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Logger.LogError($"SendJobRequestMessage failed: {e.Message}");
                Logger.LogError($"Stacktrace: {e.StackTrace}");
                return Task.FromResult(false);
            }
        }
    }
}