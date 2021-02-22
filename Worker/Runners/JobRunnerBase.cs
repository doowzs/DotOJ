using System;
using System.Threading.Tasks;
using Data;
using Data.Configs;
using Data.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Worker.Runners
{
    public interface IJobRunner
    {
        public Task<int> HandleJobRequest(JobRequestMessage message);
    }

    public abstract class JobRunnerBase<T> : IJobRunner where T : class
    {
        protected readonly ILogger<T> Logger;
        protected readonly IOptions<WorkerConfig> Options;
        protected readonly ApplicationDbContext Context;
        protected readonly IServiceProvider Provider;

        protected JobRunnerBase(IServiceProvider provider)
        {
            Logger = provider.GetRequiredService<ILogger<T>>();
            Options = provider.GetRequiredService<IOptions<WorkerConfig>>();
            Context = provider.GetRequiredService<ApplicationDbContext>();
            Provider = provider;
        }

        public abstract Task<int> HandleJobRequest(JobRequestMessage message);
    }
}