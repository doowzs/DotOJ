using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Worker.Triggers;

namespace Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IList<ITrigger> _triggers;

        public Worker(IServiceProvider provider)
        {
            _logger = provider.GetRequiredService<ILogger<Worker>>();
            _triggers = new List<ITrigger>
            {
                new SubmissionRunnerTrigger(provider)
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                foreach (var trigger in _triggers)
                {
                    try
                    {
                        await trigger.CheckAndRunAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"{nameof(trigger)} error: {e.ToString()}");
                    }
                }
            }
        }
    }
}