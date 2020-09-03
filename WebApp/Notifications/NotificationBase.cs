using System;
using System.Net.Http;
using System.Threading.Tasks;
using Judge1.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Judge1.Notifications
{
    public interface INotificationBase
    {
        public bool IsEnabled();
        public Task SendNotification(bool atAdmins, string title, string message, params object[] args);
    }

    public abstract class NotificationBase<TL> : INotificationBase where TL : class
    {
        protected IHttpClientFactory Factory;
        protected IOptions<NotificationConfig> Options;
        protected ILogger<TL> Logger;

        public NotificationBase(IServiceProvider provider)
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