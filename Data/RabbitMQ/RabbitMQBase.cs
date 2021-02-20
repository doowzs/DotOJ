using System;
using System.Diagnostics.CodeAnalysis;
using Data.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Data.RabbitMQ
{

    public class RabbitMqConnectionFactory
    {
        private readonly RabbitMqConfig _config;
        private readonly ILogger<RabbitMqConnectionFactory> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private static IConnection _connection;

        public RabbitMqConnectionFactory(IServiceProvider provider)
        {
            _config = provider.GetRequiredService<IOptions<RabbitMqConfig>>().Value;
            _logger = provider.GetRequiredService<ILogger<RabbitMqConnectionFactory>>();
            _lifetime = provider.GetRequiredService<IHostApplicationLifetime>();
        }

        public IConnection GetConnection()
        {
            // Only one thread in startup will call this function.
            // There is no need to use a lock for single thread.
            if (_connection is null)
            {
                _logger.LogInformation($"Connecting to RabbitMQ instance at {_config.HostName}:{_config.Port}");
                try
                {
                    var factory = new ConnectionFactory()
                    {
                        HostName = _config.HostName,
                        Port = _config.Port,
                        UserName = _config.UserName,
                        Password = _config.Password,
                        DispatchConsumersAsync = true
                    };
                    _connection = factory.CreateConnection();
                    _logger.LogInformation($"Connected to RabbitMQ instance");
                }
                catch (Exception e)
                {
                    Environment.ExitCode = 1;
                    _logger.LogCritical($"Cannot connect to RabbitMQ instance: {e.Message}");
                    _lifetime.StopApplication(); // Gracefully shutdown
                }
            }

            return _connection;
        }

        public void CloseConnection()
        {
            _logger.LogInformation("Closing RabbitMQ connection");
            _connection?.Close();
            _connection = null;
        }
    }

    public abstract class RabbitMqQueueBase<T> where T : class
    {
        protected readonly ILogger<T> Logger;
        protected string Queue;
        protected IModel Channel;
        
        protected RabbitMqQueueBase(IServiceProvider provider)
        {
            Logger = provider.GetRequiredService<ILogger<T>>();
        }

        public virtual void Start([NotNull] IConnection connection)
        {
            Channel = connection.CreateModel();
            Channel.QueueDeclare(Queue, durable:true, exclusive:false, autoDelete:false);
        }

        public void Stop()
        {
            Channel?.Close();
            Channel = null;
        }
    }
}
