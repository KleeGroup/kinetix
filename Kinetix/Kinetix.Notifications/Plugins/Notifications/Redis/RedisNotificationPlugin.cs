using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kinetix.Connectors;
using StackExchange.Redis;

namespace Kinetix.Notifications {

    /// <summary>
    /// Plugin Redis to request/send notifications.
    /// </summary>
    public class RedisNotificationPlugin : INotificationPlugin {

        private const string CODEC_DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:ss";
        private readonly RedisConnector redisConnector;

        /// <summary>
        /// Constructor.
        /// </summary>
        public RedisNotificationPlugin(RedisConnector redisConnector)
        {
            this.redisConnector = redisConnector;
        }

        /// <inheritDoc cref="INotificationPlugin.Send" />
        public void Send(NotificationEvent notificationEvent) {
            var redis = redisConnector.GetResource();
            Notification notification = notificationEvent.Notification;
            string guid = notification.Uuid.ToString();
            string typedTarget = "type:" + notification.Type + ";target:" + notification.TargetUrl;

            ITransaction tx = redis.CreateTransaction();
            tx.HashSetAsync("notif:" + guid, NotificationToHashEntry(notification));
            tx.StringSetAsync(typedTarget + ";uuid", guid);
            foreach (string accountURI in notificationEvent.AccountIds) {
                String notifiedAccount = "notifs:" + accountURI;
                //On publie la notif (the last wins)
                tx.ListRemoveAsync(notifiedAccount, guid);
                tx.ListLeftPushAsync(notifiedAccount, guid);
                tx.ListRemoveAsync(typedTarget, notifiedAccount);
                tx.ListLeftPushAsync(typedTarget, notifiedAccount);
            }

            tx.Execute();
        }

        /// <inheritDoc cref="INotificationPlugin.GetCurrentNotifications" />
        public IList<Notification> GetCurrentNotifications(string accountId) {
            IList<Task<HashEntry[]>> responses = new List<Task<HashEntry[]>>();

            var redis = redisConnector.GetResource();
            var guids = redis.ListRange("notifs:" + accountId, 0, -1);

            //----- we are using batch to avoid roundtrips
            IBatch batch = redis.CreateBatch();

            foreach (string guid in guids) {
                var hgetPending = batch.HashGetAllAsync("notif:" + guid);
                responses.Add(hgetPending);
            }

            batch.Execute();
            batch.WaitAll(responses.ToArray());

            List<Notification> notifications = new List<Notification>();
            foreach (Task<HashEntry[]> response in responses)
            {
                if (response.IsCompleted && response.Result.Any())
                {
                    notifications.Add(HashEntryToNotification(response.Result));
                }
            }

            return notifications;
        }

        /// <inheritDoc cref="INotificationPlugin.Remove" />
        public void Remove(string accountId, Guid notificationGuid) {
            var redis = redisConnector.GetResource();
            redis.ListRemove("notifs:" + accountId, notificationGuid.ToString(), - 1);
        }

        /// <inheritDoc cref="INotificationPlugin.RemoveAll" />
        public void RemoveAll(string type, string targetUrl) {
            var redis = redisConnector.GetResource();
            string uuid = redis.StringGet("type:" + type + ";target:" + targetUrl + ";uuid");
            var userNotifsKeys = redis.ListRange("type:" + type + ";target:" + targetUrl, 0, -1);
            foreach (var userNotifsKey in userNotifsKeys) {
                redis.ListRemove(userNotifsKey.ToString(), uuid, - 1);
            }
        }

        /// <summary>
        /// Fill an HashEntry from a Notification.
        /// </summary>
        /// <param name="notification">Notification.</param>
        /// <returns>Map key/value.</returns>
        private HashEntry[] NotificationToHashEntry(Notification notification) {
            ICollection<HashEntry> hashEntryList = new List<HashEntry>();
            String creationDate = notification.CreationDate.ToString(CODEC_DATE_FORMAT);
            hashEntryList.Add(new HashEntry("uuid", notification.Uuid.ToString()));
            hashEntryList.Add(new HashEntry("sender", notification.Sender));
            hashEntryList.Add(new HashEntry("type", notification.Type));
            hashEntryList.Add(new HashEntry("title", notification.Title));
            hashEntryList.Add(new HashEntry("content", notification.Content));
            hashEntryList.Add(new HashEntry("creationDate", creationDate));
            hashEntryList.Add(new HashEntry("targetUrl", notification.TargetUrl ?? string.Empty));
            return hashEntryList.ToArray();
        }

        /// <summary>
        /// Fill a notification from an HashEntry.
        /// </summary>
        /// <param name="data">Map key/value.</param>
        /// <returns>Notification.</returns>
        private static Notification HashEntryToNotification(HashEntry[] data) {
            var creationDate = data.FirstOrDefault(d => d.Name == "creationDate");

            return new NotificationBuilder()
                    .WithGuid(data.FirstOrDefault(d => d.Name == "uuid").Value)
                    .WithSender(data.FirstOrDefault(d => d.Name == "sender").Value)
                    .WithType(data.FirstOrDefault(d => d.Name == "type").Value)
                    .WithTitle(data.FirstOrDefault(d => d.Name == "title").Value)
                    .WithContent(data.FirstOrDefault(d => d.Name == "content").Value)
                    .WithCreationDate(Convert.ToDateTime(creationDate.Value.ToString()))
                    .WithTargetUrl(data.FirstOrDefault(d => d.Name == "targetUrl").Value)
                    .Build();
        }
    }
}
