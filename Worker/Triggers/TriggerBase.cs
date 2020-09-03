using System;
using System.Threading.Tasks;
using Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Worker.Triggers
{
    public interface ITrigger
    {
        public Task CheckAndRunAsync();
    }

    public abstract class TriggerBase<T> : ITrigger where T : class
    {
        protected readonly ApplicationDbContext Context;
        protected readonly ILogger<T> Logger;

        public TriggerBase(IServiceProvider provider)
        {
            Context = provider.GetRequiredService<ApplicationDbContext>();
            Logger = provider.GetRequiredService<ILogger<T>>();
        }

        public virtual Task CheckAndRunAsync()
        {
            return Task.CompletedTask;
        }
    }
}