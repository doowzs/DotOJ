using System;
using System.Text;
using System.Threading.Tasks;
using Data.RabbitMQ;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace WebApp.RabbitMQ
{
    public class JudgeRequestProducer : RabbitMqQueueBase<JudgeRequestProducer>
    {
        public JudgeRequestProducer(IServiceProvider provider) : base(provider)
        {
        }

        public Task<bool> SendAsync(int submissionId, int requestVersion)
        {
            var message = new JudgeRequestMessage
            {
                SubmissionId = submissionId,
                RequestVersion = requestVersion
            };
            var serialized = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(serialized);
            try
            {
                Channel.BasicPublish("", Queue, null, body);
                Logger.LogDebug($"SendJudgeRequestMessage SubmissionId={submissionId} RequestVersion={requestVersion}");
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Logger.LogError($"SendJudgeRequestMessage failed: {e.Message}");
                Logger.LogError($"Stacktrace: {e.StackTrace}");
                return Task.FromResult(false);
            }
        }
    }
}