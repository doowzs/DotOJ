using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Judge1.Notifications
{
    public interface INotificationBroadcaster
    {
        public Task SendNotification(bool atAdmins, string message, params object[] args);
    }

    public class NotificationBroadcaster : INotificationBroadcaster
    {
        protected readonly IList<INotificationBase> Notifiers;

        public NotificationBroadcaster(IServiceProvider provider)
        {
            Notifiers = new List<INotificationBase>();
            Notifiers.Add((INotificationBase) provider.GetRequiredService<IDingTalkNotification>());
        }

        public async Task SendNotification(bool atAdmins, string message, params object[] args)
        {
            foreach (var notifier in Notifiers)
            {
                if (notifier.IsEnabled())
                {
                    await notifier.SendNotification(atAdmins, message, args);
                }
            }
        }
    }
}