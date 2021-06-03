using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Shared.Generics
{
    public interface IBackgroundTaskQueue<T> where T : class
    {
        void EnqueueTask(T taskInfo);
        Task<T> DequeueAsync(CancellationToken cancellationToken);
    }

    public class BackgroundTaskQueue<T> : IBackgroundTaskQueue<T> where T : class
    {
        private ConcurrentQueue<T> _taskInfos { get; set; }
        private SemaphoreSlim _semaphore { get; set; }

        public BackgroundTaskQueue()
        {
            _taskInfos = new ConcurrentQueue<T>();
            _semaphore = new SemaphoreSlim(0);
        }

        public void EnqueueTask(T taskInfo)
        {
            if (taskInfo == null)
            {
                throw new ArgumentNullException(nameof(taskInfo));
            }

            _taskInfos.Enqueue(taskInfo);
            _semaphore.Release();
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            _taskInfos.TryDequeue(out var taskInfo);
            return taskInfo;
        }
    }
}
