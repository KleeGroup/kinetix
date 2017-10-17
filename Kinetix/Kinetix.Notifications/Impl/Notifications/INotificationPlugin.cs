
using System;
using System.Collections.Generic;

namespace Kinetix.Notifications
{
    public interface INotificationPlugin
    {

        /// <summary>
        /// Send a notification
        /// </summary>
        /// <param name="notificationEvent">Notification to send</param>
        void Send(NotificationEvent notificationEvent);

        /// <summary>
        /// Get the Current Notifications
        /// </summary>
        /// <param name="accountId">Accout uri</param>
        /// <returns>All notifications for this account</returns>
        IList<Notification> GetCurrentNotifications(string accountId);


        /// <summary>
        /// Remove one notification
        /// </summary>
        /// <param name="accountId">Account uri</param>
        /// <param name="notificationUUID">Notification uuid</param>
        void Remove(string accountId, Guid notificationUUID);


        /// <summary>
        /// Remove all notification matching filters
        /// </summary>
        /// <param name="type">Notification's type</param>
        /// <param name="targetUrl">Target URL, use to filter all notifications to remove</param>
        void RemoveAll(string type, string targetUrl);

    }
}
