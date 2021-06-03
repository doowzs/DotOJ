using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Generics;

namespace Server.Services.Background.Queue
{
    public abstract class BackgroundQueueService<T> : BackgroundService where T : class
    {
        protected readonly IServiceProvider Provider;
        private readonly BackgroundTaskQueue<T> _queue;

        public BackgroundQueueService(IServiceProvider provider)
        {
            Provider = provider;
            _queue = provider.GetRequiredService<BackgroundTaskQueue<T>>();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var taskInfo = await _queue.DequeueAsync(cancellationToken);
                await ExecuteTaskAsync(taskInfo, cancellationToken);
            }
        }

        protected abstract Task ExecuteTaskAsync(T taskInfo, CancellationToken cancellationToken);
    }
}