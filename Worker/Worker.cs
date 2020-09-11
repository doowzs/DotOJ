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
        private readonly IServiceScopeFactory _factory;
        private readonly ILogger<Worker> _logger;

        private readonly IList<ITrigger> _triggers = new List<ITrigger>();

        public Worker(IServiceProvider provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _logger = provider.GetRequiredService<ILogger<Worker>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);

                var continueWorking = true;
                while (continueWorking)
                {
                    continueWorking = false;

                    using var scope = _factory.CreateScope();
                    _triggers.Add(new SubmissionRunnerTrigger(scope.ServiceProvider));
                    foreach (var trigger in _triggers)
                    {
                        try
                        {
                            if (await trigger.CheckAndRunAsync())
                            {
                                continueWorking = true;
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"{nameof(trigger)} error: {e}");
                        }
                    }
                }
            }
        }
    }
}