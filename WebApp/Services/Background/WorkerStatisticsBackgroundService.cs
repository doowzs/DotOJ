using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WebApp.RabbitMQ;
using WebApp.Services.Singleton;

namespace WebApp.Services.Background
{
    public class WorkerStatisticsBackgroundService : CronJobService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JudgeRequestProducer _producer;

        private static readonly Dictionary<string, (string Token, DateTime TimeStamp)> WorkerDictionary = new();
        // private static readonly Dictionary<int, DateTime> SubmissionTimestampDictionary = new();

        public WorkerStatisticsBackgroundService(IServiceProvider provider) : base("* * * * *")
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _producer = provider.GetRequiredService<JudgeRequestProducer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _factory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<WorkerStatisticsService>();
            await service.CheckWorkersAndSubmissionsAsync(stoppingToken);
        }
    }
}