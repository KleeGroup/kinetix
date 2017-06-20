
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kinetix.Notifications {
    public class MemoryNotificationPlugin : INotificationPlugin
    {
        private IDictionary<string, IList<Notification>> NotificationsByAccountId = new ConcurrentDictionary<string, IList<Notification>>();

        public IList<Notification> GetCurrentNotifications(string accountId)
        {
            IList<Notification> notifications;
            NotificationsByAccountId.TryGetValue(accountId, out notifications);
            if (notifications == null)
            {
                return new List<Notification>();
            }
            return notifications;
        }

        public void Remove(string accountId, Guid notificationGuid)
        {
            IList<Notification> notifications;
            NotificationsByAccountId.TryGetValue(accountId, out notifications);

            if (notifications != null)
            {
                IList<Notification> toRemove = notifications.Where(n => n.Uuid.Equals(notificationGuid)).ToList();
                foreach (Notification rm in toRemove)
                {
                    notifications.Remove(rm);
                }
            }
        }

        public void RemoveAll(string type, string targetUrl)
        {
            foreach (List<Notification> notifications in NotificationsByAccountId.Values)
            {
                IList<Notification> toRemove = notifications.Where(n => n.Type.Equals(type) && n.TargetUrl.Equals(targetUrl)).ToList();
                foreach (Notification rm in toRemove)
                {
                    notifications.Remove(rm);
                }
            }
        }

        public void Send(NotificationEvent notificationEvent)
        {
            Debug.Assert(notificationEvent != null);
            //-----
            //0 - Remplir la pile des événements

            //1 - Dépiler les événemnts en asynchrone FIFO
            foreach (string accountId in notificationEvent.AccountIds)
            {
                ObtainNotifications(accountId).Add(notificationEvent.Notification);
            }

            //2 - gestion globale async des erreurs
        }

        private IList<Notification> ObtainNotifications(string accountId)
        {
            IList<Notification> notifications;
            NotificationsByAccountId.TryGetValue(accountId, out notifications);

            if (notifications == null)
            {
                notifications = new List<Notification>();
                NotificationsByAccountId.Add(accountId, notifications);
            }
            return notifications;
        }
    }
}
