using System;
using System.Collections.Generic;
using Kinetix.Account;

namespace Kinetix.Notifications {
    public sealed class NotificationManager : INotificationManager {
        private readonly INotificationPlugin _notificationPlugin;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="accountManager">Account manager</param>
        /// <param name="notificationPlugin">Notifications plugin</param>
        public NotificationManager(INotificationPlugin notificationPlugin) {
            this._notificationPlugin = notificationPlugin;
        }

        public IList<Notification> GetCurrentNotifications(string accountId) {
            return _notificationPlugin.GetCurrentNotifications(accountId);
        }

        public void Remove(string accountId, Guid notificationUUID) {
            _notificationPlugin.Remove(accountId, notificationUUID);
        }

        public void RemoveAll(string type, string targetUrl) {
            _notificationPlugin.RemoveAll(type, targetUrl);
        }

        public void Send(Notification notification, ISet<string> accountId) {
            NotificationEvent notificationEvent = new NotificationEvent(notification, accountId );
            _notificationPlugin.Send(notificationEvent);
        }
    }
}
