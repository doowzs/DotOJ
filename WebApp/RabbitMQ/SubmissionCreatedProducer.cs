using System;
using System.Text;
using System.Threading.Tasks;
using Data.Configs;
using Data.Messages;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace WebApp.RabbitMQ
{
    public class SubmissionCreatedProducer : IRabbitMqAgent
    {
        private readonly ILogger<SubmissionCreatedProducer> _logger;
        private readonly RabbitMQConfig _config;
        private const string Queue = "SubmissionCreated";
        private IConnection _connection;
        private IModel _channel;

        public SubmissionCreatedProducer(ILogger<SubmissionCreatedProducer> logger,
            IOptions<RabbitMQConfig> options, IConnection connection = null)
        {
            _logger = logger;
            _config = options.Value;
            if (_connection is not null)
            {
                _connection = connection;
            }
        }

        public void Start()
        {
            if (_connection is null)
            {
                _logger.LogInformation($"Trying to connect to RabbitMQ at {_config.HostName}:{_config.Port}");
                var factory = new ConnectionFactory
                {
                    HostName = _config.HostName,
                    Port = _config.Port,
                    UserName = _config.UserName,
                    Password = _config.Password
                };
                try
                {
                    _connection = factory.CreateConnection();
                    _logger.LogInformation($"Connected to RabbitMQ instance.");
                } catch (Exception e)
                {
                    _logger.LogCritical($"Cannot create RabbitMQ connection: {e.Message}");
                    Environment.Exit(1);
                }
            }
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(Queue, true);
        }

        public void Stop()
        {
            _channel?.Close();
            _connection?.Close();
            _channel = null;
            _connection = null;
        }

        public void Send(SubmissionCreatedMessage message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _channel.BasicPublish("", Queue, null, body);
            _logger.LogDebug($"Sent RabbitMQ message for submission #{message.Id}");
        }

        public Task SendAsync(SubmissionCreatedMessage message)
        {
            Send(message);
            return Task.CompletedTask;
        }
    }
}
