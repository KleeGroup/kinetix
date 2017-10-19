
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Kinetix.Account;

namespace Kinetix.Notifications {
    [RoutePrefix("x/notification")]
    public class NotificationController : ApiController {
        private static string API_VERSION = "0.1.0";
        private static string IMPL_VERSION = "0.0.0.1";

        private INotificationManager _notificationManager;
        private IAccountManager _accountManager;

        public NotificationController(INotificationManager notificationManager, IAccountManager accountManager) {
            _notificationManager = notificationManager;
            _accountManager = accountManager;
        }

        /// <summary>
        /// Get messages for logged user.
        /// </summary>
        /// <returns>messages for logged user</returns>
        [HttpGet]
        [Route("api/messages")]
        public IList<Notification> GetMessages() {
            string loggedAccountId = _accountManager.GetLoggedAccount();
            return _notificationManager.GetCurrentNotifications(loggedAccountId);
        }

        [HttpDelete]
        [Route("api/messages/{messageGuid}")]
        public HttpResponseMessage RemoveMessage(string messageGuid) {
            string loggedAccountId = _accountManager.GetLoggedAccount();
            _notificationManager.Remove(loggedAccountId, Guid.Parse(messageGuid));
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Remove a list of messages.
        /// </summary>
        /// <param name="messageUuids">messages id</param>
        [HttpDelete]
        [Route("api/messages")]
        public HttpResponseMessage removeMessage(string[] messageUuids) {
            string loggedAccountIds = _accountManager.GetLoggedAccount();
            foreach (string messageUuid in messageUuids) {
                _notificationManager.Remove(loggedAccountIds, Guid.Parse(messageUuid));
            }
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Extension status (code 200 or 500)
        /// </summary>
        /// <returns>"OK" or error message</returns>
        [HttpGet]
        [Route("status")]
        public string GetStatus() {
            return "OK";
        }

        /// <summary>
        /// Extension stats.
        /// </summary>
        /// <returns>"OK" or error message</returns>
        [HttpGet]
        [Route("stats")]
        public IDictionary<string, object> GetStats() {
            IDictionary<string, object> stats = new Dictionary<string, object>();
            IDictionary<string, object> sizeStats = new Dictionary<string, object>();
            sizeStats.Add("notifications", "not yet");
            stats.Add("size", sizeStats);
            return stats;
        }

        /// <summary>
        /// Extension config.
        /// </summary>
        /// <returns>Config object</returns>
        [HttpGet]
        [Route("config")]
        public IDictionary<string, object> GetConfig() {
            return new Dictionary<string, object>()
            {
                { "api-version", API_VERSION },
                { "impl-version", IMPL_VERSION }
            };
        }

        /// <summary>
        /// Extension help.
        /// </summary>
        /// <returns>Help object</returns>
        [HttpGet]
        [Route("help")]
        public string GetHelp() {
            return "##Notification extension"
                    + "\n This extension manage the notification center.";
        }
    }
}
