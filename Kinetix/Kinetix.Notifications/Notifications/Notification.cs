
using System;

namespace Kinetix.Notifications {
    public class Notification {
        public Guid Uuid { get; }
        public string Sender { get; }
        public string Type { get; }
        public string Title { get; }
        public string Content { get; }
        public string TargetUrl { get; }
        public DateTime CreationDate { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="sender">Sender name</param>
        /// <param name="type">Sender name</param>
        /// <param name="title">Title</param>
        /// <param name="content">Content</param>
        /// <param name="creationDate">Create date</param>
        /// <param name="targetUrl">Target URL of this notification</param>
        public Notification(Guid guid, string sender, string type, string title, string content, DateTime creationDate, string targetUrl) {
            this.Uuid = guid;
            this.Sender = sender;
            this.Type = type;
            this.Title = title;
            this.Content = content;
            this.CreationDate = creationDate;
            this.TargetUrl = targetUrl;
        }

    }
}
