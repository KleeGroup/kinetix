
using System;
using System.Collections.Generic;

namespace Kinetix.Notifications {

    public interface INotificationManager {

        /// <summary>
        /// Send a notification to a user.
        /// </summary>
        /// <param name="notification">Notification.</param>
        /// <param name="accountId">Destination user.</param>
        void Send(Notification notification, ISet<string> accountId);

        /// <summary>
        /// Retrieve all notification for one account
        /// </summary>
        /// <param name="accountId">Account</param>
        /// <returns>List notifications</returns>
        IList<Notification> GetCurrentNotifications(string accountId);

        /// <summary>
        /// Remove one notification.
        /// </summary>
        /// <param name="accountId">User account</param>
        /// <param name="notificationUUID">Notification uid</param>
        void Remove(string accountId, Guid notificationUUID);

        /// <summary>
        /// Remove all notifications by type and targetUrl.
        /// Could be use when a business module need to revoke its notifications.
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="targetUrl">Notification's target Url</param>
        void RemoveAll(string type, string targetUrl);
    }
}
