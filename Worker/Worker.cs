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
        private IOptions<JudgingConfig> _options;

        public Worker(IServiceProvider provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _logger = provider.GetRequiredService<ILogger<Worker>>();
            _options = provider.GetRequiredService<IOptions<JudgingConfig>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _factory.CreateScope();
            _logger.LogInformation($"Worker {_options.Value.Name} is starting");
            
            var factory = new RabbitMqConnectionFactory(scope.ServiceProvider);
            var consumer = new JudgeRequestConsumer(scope.ServiceProvider);
            var connection = factory.GetConnection();
            if (!stoppingToken.IsCancellationRequested) // Application may be aborted in GetConnection().
            {
                consumer.Start(connection);
            }
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Yield();
            }
            
            _logger.LogInformation($"Worker {_options.Value.Name} is stopping");
            consumer.Stop();
            factory.CloseConnection();
        }
    }
}