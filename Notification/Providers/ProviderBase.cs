using System;
using System.Net.Http;
using System.Threading.Tasks;
using Shared.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Notification.Providers
{
    public interface IProvider
    {
        public bool IsEnabled();
        public Task SendNotification(bool atAdmins, string title, string message, params object[] args);
    }

    public abstract class ProviderBase<TL> : IProvider where TL : class
    {
        protected IHttpClientFactory Factory;
        protected IOptions<NotificationConfig> Options;
        protected ILogger<TL> Logger;

        public ProviderBase(IServiceProvider provider)
        {
            Factory = provider.GetRequiredService<IHttpClientFactory>();
            Options = provider.GetRequiredService<IOptions<NotificationConfig>>();
            Logger = provider.GetRequiredService<ILogger<TL>>();
        }

        public virtual bool IsEnabled()
        {
            return false;
        }

        public virtual Task SendNotification(bool atAdmins, string title, string message, params object[] args)
        {
            return Task.CompletedTask;
        }
    }
}