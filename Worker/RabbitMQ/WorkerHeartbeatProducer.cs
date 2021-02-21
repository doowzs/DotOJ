using System;
using System.Text;
using System.Threading.Tasks;
using Data.Configs;
using Data.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Timer = System.Timers.Timer;

namespace Worker.RabbitMQ
{
    public class WorkerHeartbeatProducer : RabbitMqQueueBase<WorkerHeartbeatProducer>
    {
        private readonly byte[] _body;
        private Timer _timer;

        public WorkerHeartbeatProducer(IServiceProvider provider) : base(provider)
        {
            var message = new WorkerHeartbeatMessage
            {
                Name = provider.GetRequiredService<IOptions<JudgingConfig>>().Value.Name,
                Token = Guid.NewGuid().ToString()
            };
            _body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            Logger.LogInformation($"Worker token created: {message.Token}");
        }

        public override void Start(IConnection connection)
        {
            base.Start(connection);

            _timer?.Stop();
            _timer?.Dispose();
            _timer = new Timer(30 * 1000);
            _timer.Elapsed += async (sender, args) => await SendAsync();
            _timer.AutoReset = true;

            Task.Run(async () => await SendAsync());
            _timer.Start();
        }

        public override void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
            base.Stop();
        }

        private Task SendAsync()
        {
            try
            {
                Channel.BasicPublish("", Queue, null, _body);
            }
            catch (Exception e)
            {
                Logger.LogError($"SendWorkerHeartbeatMessage failed: {e.Message}");
                Logger.LogDebug($"Stacktrace: {e.StackTrace}");
            }

            return Task.CompletedTask;
        }
    }
}