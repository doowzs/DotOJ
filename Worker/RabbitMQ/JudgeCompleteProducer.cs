using System;
using System.Text;
using System.Threading.Tasks;
using Data.RabbitMQ;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Worker.RabbitMQ
{
    public sealed class JudgeCompleteProducer : RabbitMqQueueBase<JudgeCompleteProducer>
    {
        public JudgeCompleteProducer(IServiceProvider provider) : base(provider)
        {
        }

        public Task SendAsync(int submissionId, int completeVersion)
        {
            var message = new JudgeCompleteMessage
            {
                SubmissionId = submissionId,
                CompleteVersion = completeVersion
            };
            var serialized = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(serialized);
            try
            {
                Channel.BasicPublish("", Queue, null, body);
                Logger.LogDebug(
                    $"SendJudgeCompleteMessage SubmissionId={submissionId} CompleteVersion={completeVersion}");
            }
            catch (Exception e)
            {
                Logger.LogError($"SendJudgeCompleteMessage failed: {e.Message}");
                Logger.LogDebug($"Stacktrace: {e.StackTrace}");
            }

            return Task.CompletedTask;
        }
    }
}