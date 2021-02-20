using System;
using System.Text;
using System.Threading.Tasks;
using Data.Models;
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
            Queue = "JudgeRequest";
        }

        public void Send(Submission submission)
        {
            var body = Encoding.UTF8.GetBytes(submission.Id.ToString());
            Channel.BasicPublish("", Queue, null, body);
            Logger.LogDebug($"Sent RabbitMQ message for submission #{submission.Id}");
        }

        public Task SendAsync(Submission submission)
        {
            Send(submission);
            return Task.CompletedTask;
        }
    }
}
