using System;
using System.Threading;
using System.Threading.Tasks;
using Data.Configs;
using Data.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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