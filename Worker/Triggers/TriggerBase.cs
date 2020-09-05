using System;
using System.Threading.Tasks;
using Data;
using Data.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification;

namespace Worker.Triggers
{
    public interface ITrigger
    {
        public Task CheckAndRunAsync();
    }

    public abstract class TriggerBase<T> : ITrigger where T : class
    {
        protected readonly ApplicationDbContext Context;
        protected readonly IOptions<JudgingConfig> Options;
        protected readonly ILogger<T> Logger;
        protected readonly INotificationBroadcaster Broadcaster;

        public TriggerBase(IServiceProvider provider)
        {
            Context = provider.GetRequiredService<ApplicationDbContext>();
            Options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            Logger = provider.GetRequiredService<ILogger<T>>();
            Broadcaster = provider.GetRequiredService<INotificationBroadcaster>();
        }

        public virtual Task CheckAndRunAsync()
        {
            return Task.CompletedTask;
        }
    }
}