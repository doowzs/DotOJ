using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Notification.Providers;

namespace Notification
{
    public interface INotificationBroadcaster
    {
        public Task SendNotification(bool atAdmins, string title, string message, params object[] args);
    }

    public class NotificationBroadcaster : INotificationBroadcaster
    {
        protected readonly IList<IProvider> Providers;

        public NotificationBroadcaster(IServiceProvider provider)
        {
            Providers = new List<IProvider>();
            Providers.Add((IProvider) provider.GetRequiredService<IDingTalkNotification>());
        }

        public async Task SendNotification(bool atAdmins, string title, string message, params object[] args)
        {
            foreach (var notifier in Providers)
            {
                if (notifier.IsEnabled())
                {
                    await notifier.SendNotification(atAdmins, title, message, args);
                }
            }
        }
    }
}