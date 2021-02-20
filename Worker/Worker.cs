using System;
using System.Threading;
using System.Threading.Tasks;
using Data.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Worker.RabbitMQ;

namespace Worker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly ILogger<Worker> _logger;

        public Worker(IServiceProvider provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _logger = provider.GetRequiredService<ILogger<Worker>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _factory.CreateScope();
            var factory = new RabbitMqConnectionFactory(scope.ServiceProvider);
            var consumer = new JudgeRequestConsumer(scope.ServiceProvider);
            
            // Application may be aborted in GetConnection().
            var connection = factory.GetConnection();
            if (!stoppingToken.IsCancellationRequested)
            {
                consumer.Start(connection);
            }
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            consumer.Stop();
            factory.CloseConnection();
        }
    }
}