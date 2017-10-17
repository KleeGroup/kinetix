
using System.Collections.Generic;
using System.Diagnostics;

namespace Kinetix.Notifications
{
    public class NotificationEvent
    {
        public Notification Notification { get; }
	    public ISet<string> AccountIds { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="notification">Notification</param>
        /// <param name="accountIds">To accounts uri</param>
        public NotificationEvent(Notification notification, ISet<string> accountIds)
        {
            Debug.Assert(notification != null);
            Debug.Assert(accountIds != null);
            //-----
            this.Notification = notification;
            this.AccountIds = accountIds;
        }

    }
}
