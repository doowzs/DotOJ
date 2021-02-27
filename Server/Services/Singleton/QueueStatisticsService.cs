using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace Server.Services.Singleton
{
    public class QueueStatisticsService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly ILogger<QueueStatisticsService> _logger;

        private const int Capacity = 100000;
        private const double Alpha = 0.7;
        private const double Delta = 10.0;
        private double _expectedValue = 0.0;

        private readonly AsyncReaderWriterLock _lock = new();
        private readonly Dictionary<Tuple<JobType, int>, DateTime> _dictionary = new();

        public QueueStatisticsService(IServiceProvider provider)
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _logger = provider.GetRequiredService<ILogger<QueueStatisticsService>>();
        }

        public async Task<int> GetAverageWaitingSecondsAsync()
        {
            var newValue = await CalculateCurrentWaitingSecondsAsync();
            _expectedValue = newValue * Alpha + _expectedValue * (1.0 - Alpha);
            return (int) _expectedValue;
        }

        private async Task<double> CalculateCurrentWaitingSecondsAsync()
        {
            var workerCount = 0;
            using (var scope = _factory.CreateScope())
            {
                var workerStatisticsService = scope.ServiceProvider.GetRequiredService<WorkerStatisticsService>();
                workerCount = await workerStatisticsService.GetAvailableWorkerCountAsync();
                if (workerCount == 0) return -1;
            }

            using var locked = await _lock.ReaderLockAsync();
            var now = DateTime.Now.ToUniversalTime();
            var result = _dictionary
                .Select(p => now.Subtract(p.Value).TotalSeconds)
                .Aggregate(0.0, (sum, wait) => sum + (sum / workerCount) + wait + Delta);
            if (_dictionary.Count > 0)
            {
                result /= _dictionary.Count;
            }
            return result;
        }

        public async Task AddJobRequestAsync(JobRequestMessage message)
        {
            using var locked = await _lock.WriterLockAsync();
            if (_dictionary.Count <= Capacity)
            {
                _ = _dictionary.TryAdd(Tuple.Create(message.JobType, message.TargetId), DateTime.Now.ToUniversalTime());
            }
        }

        public async Task RemoveJobRequestAsync(JobCompleteMessage message)
        {
            using var locked = await _lock.WriterLockAsync();
            _ = _dictionary.Remove(Tuple.Create(message.JobType, message.TargetId), out _);
        }
    }
}