using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Services.Singleton;

namespace WebApp.Services.Background
{
    public class WorkerStatisticsBackgroundService : CronJobService
    {
        private readonly IServiceScopeFactory _factory;
        public WorkerStatisticsBackgroundService(IServiceProvider provider) : base("* * * * *")
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _factory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<WorkerStatisticsService>();
            await service.CheckWorkersAndSubmissionsAsync(stoppingToken);
        }
    }
}