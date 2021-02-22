using System;
using System.Text;
using System.Threading.Tasks;
using Data.RabbitMQ;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Worker.RabbitMQ
{
    public sealed class JobCompleteProducer : RabbitMqQueueBase<JobCompleteProducer>
    {
        public JobCompleteProducer(IServiceProvider provider) : base(provider)
        {
        }

        public Task SendAsync(JobType jobType, int targetId, int completeVersion)
        {
            var message = new JobCompleteMessage(jobType, targetId, completeVersion);
            var serialized = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(serialized);
            try
            {
                Channel.BasicPublish("", Queue, null, body);
                Logger.LogDebug($"SendJobCompleteMessage JobType={jobType}" +
                                $" TargetId={targetId} CompleteVersion={completeVersion}");
            }
            catch (Exception e)
            {
                Logger.LogError($"SendJobCompleteMessage failed: {e.Message}");
                Logger.LogDebug($"Stacktrace: {e.StackTrace}");
            }

            return Task.CompletedTask;
        }
    }
}