using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared.Configs;
using Shared.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Worker.Models;
using Worker.RabbitMQ;

namespace Worker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<WorkerConfig> _options;

        public Worker(IServiceProvider provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _logger = provider.GetRequiredService<ILogger<Worker>>();
            _options = provider.GetRequiredService<IOptions<WorkerConfig>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _factory.CreateScope();
            _logger.LogInformation($"Worker {_options.Value.Name} is starting");
            
            #region Initialize Hostname and Box

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/hostname",
                    Arguments = "-i",
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                throw new Exception($"E: Cannot obtain hostname, exit code {process.ExitCode}.");
            }

            var hostname = process.StandardOutput.ReadToEnd().Trim();
            var boxId = hostname.Split('.').ToList().Last();
            _options.Value.Name = _options.Value.Name + '-' + boxId;
            Box.InitBoxAsync(boxId); // Use ip obtained by hostname as the unique ID of worker
            _logger.LogInformation($"Worker works at {hostname}, renaming to {_options.Value.Name}");

            #endregion

            #region Initialize RabbitMq

            var factory = new RabbitMqConnectionFactory(scope.ServiceProvider);
            var requestConsumer = scope.ServiceProvider.GetRequiredService<JobRequestConsumer>();
            var completeProducer = scope.ServiceProvider.GetRequiredService<JobCompleteProducer>();
            var heartbeatProducer = scope.ServiceProvider.GetRequiredService<WorkerHeartbeatProducer>();

            var connection = factory.GetConnection();
            if (!stoppingToken.IsCancellationRequested) // Application may be aborted in GetConnection().
            {
                heartbeatProducer.Start(connection);
                completeProducer.Start(connection);
                requestConsumer.Start(connection);
            }

            #endregion

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation($"Worker {_options.Value.Name} is stopping");
            requestConsumer.Stop();
            completeProducer.Stop();
            heartbeatProducer.Stop();
            factory.CloseConnection();
        }
    }
}