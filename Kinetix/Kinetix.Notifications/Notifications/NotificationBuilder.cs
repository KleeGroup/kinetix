
using System;
using System.Diagnostics;

namespace Kinetix.Notifications {
    public class NotificationBuilder {
        private string myType;
        private string myTitle;
        private string myContent;
        private string mySender;
        private DateTime? myCreationDate = null;
        private string myTargetUrl;
        private Guid? myGuid = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public NotificationBuilder() {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Sender's name</param>
        /// <returns></returns>
        public NotificationBuilder WithGuid(string guid) {
            Debug.Assert(myGuid == null, "guid already set");
            Debug.Assert(string.IsNullOrEmpty(guid) == false);
            //-----
            myGuid = new Guid(guid);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Sender's name</param>
        /// <returns></returns>
        public NotificationBuilder WithSender(string sender) {
            Debug.Assert(mySender == null, "sender already set");
            Debug.Assert(string.IsNullOrEmpty(sender) == false);
            //-----
            mySender = sender;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Notification's type</param>
        /// <returns>this builder</returns>
        public NotificationBuilder WithType(string type) {
            Debug.Assert(myType == null, "type already set");
            //type is nullable
            //-----
            myType = type;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="creationDate">Creation date</param>
        /// <returns>this builder</returns>
        public NotificationBuilder WithCreationDate(DateTime creationDate) {
            Debug.Assert(myCreationDate == null, "creationDate already set");
            Debug.Assert(creationDate != null);
            //-----
            myCreationDate = creationDate;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title">Notification's title</param>
        /// <returns>this builder</returns>
        public NotificationBuilder WithTitle(string title) {
            Debug.Assert(myTitle == null, "title already set");
            Debug.Assert(title != null);
            //-----
            myTitle = title;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content">Notification's content</param>
        /// <returns>this builder</returns>
        public NotificationBuilder WithContent(string content) {
            Debug.Assert(myContent == null, "content already set");
            Debug.Assert(string.IsNullOrEmpty(content) == false);
            //-----
            myContent = content;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetUrl">Notification's target url</param>
        /// <returns>this builder</returns>
        public NotificationBuilder WithTargetUrl(string targetUrl) {
            Debug.Assert(myTargetUrl == null, "targetUrl already set");
            Debug.Assert(string.IsNullOrEmpty(targetUrl) == false);
            //-----
            myTargetUrl = targetUrl;
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>the built object</returns>
        public Notification Build() {

            if (myGuid == null) {
                myGuid = Guid.NewGuid();
            }

            if (myCreationDate == null) {
                myCreationDate = DateTime.UtcNow;
            }

            if (myContent == null) {
                myContent = string.Empty;
            }

            return new Notification(myGuid.Value, mySender, myType, myTitle, myContent, myCreationDate.Value, myTargetUrl);
        }

    }
}
