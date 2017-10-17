
using System.Collections.Generic;
using Kinetix.Account;
using Kinetix.Connectors;
using Kinetix.Test;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinetix.Notifications {
    [TestClass]
    public class NotificationManagerTest : UnityBaseTest {

        private const string HOST_NAME = "diane-rec-esr";
        private const int PORT_NUMBER = 10001;
        private readonly int DB_ID = 10;

        private readonly bool Redis = true;

        private string accountId0;
        private string accountId1;
        private string accountId2;


        [TestInitialize]
        public void InitTest() {
            accountId0 = "0";
            accountId1 = "1";
            accountId2 = "2";

            var container = GetConfiguredContainer();
            IAccountManager accountManager = container.Resolve<IAccountManager>();

            Accounts.InitData(accountManager);
        }

        [TestCleanup]
        public new void TestCleanup() {
            var container = GetConfiguredContainer();
            if (Redis) {
                RedisConnector redisConnector = container.Resolve<RedisConnector>();
                var redis = redisConnector.GetMultiplexer();
                foreach (var endpoint in redis.GetEndPoints()) {
                    var server = redis.GetServer(endpoint);
                    server.FlushDatabase(DB_ID);
                }
            }
        }

        public override void Register() {
            var container = GetConfiguredContainer();
            container.RegisterType<INotificationManager, NotificationManager>();

            container.RegisterType<IAccountStorePlugin, MemoryAccountStorePlugin>(new ContainerControlledLifetimeManager());
            container.RegisterType<IAccountManager, AccountManager>(new ContainerControlledLifetimeManager());

            if (Redis) {
                container.RegisterType<Kinetix.Notifications.INotificationPlugin, Kinetix.Notifications.RedisNotificationPlugin>(new ContainerControlledLifetimeManager());
                container.RegisterType<Kinetix.Connectors.RedisConnector>(new ContainerControlledLifetimeManager(), new InjectionConstructor(HOST_NAME, PORT_NUMBER, DB_ID, true, new InjectionParameter<string>(null)));
            } else {
                container.RegisterType<INotificationPlugin, MemoryNotificationPlugin>();
            }
        }

        [TestMethod]
        public void TestNotifications() {
            var container = GetConfiguredContainer();
            INotificationManager notificationManager = container.Resolve<INotificationManager>();


            for (int i = 0; i < 10; i++) {
                Notification notification = new NotificationBuilder()
                    .WithSender(accountId0)
                    .WithType("Test")
                    .WithTitle("news")
                    .WithContent("discover this amazing app !!" + i.ToString())
                    .WithTargetUrl("#keyConcept@2")
                    .Build();

                notificationManager.Send(notification, new HashSet<string>() { accountId1, accountId2 });
            }

            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId0).Count);
            Assert.AreEqual(10, notificationManager.GetCurrentNotifications(accountId1).Count);
            Assert.AreEqual(10, notificationManager.GetCurrentNotifications(accountId2).Count);
        }

        [TestMethod]
        public void TestNotificationsWithRemove() {

            var container = GetConfiguredContainer();
            INotificationManager notificationManager = container.Resolve<INotificationManager>();

            Notification notification = new NotificationBuilder()
                    .WithSender(accountId0)
                    .WithType("Test")
                    .WithTitle("news")
                    .WithTargetUrl("#keyConcept@2")
                    .WithContent("discover this amazing app !!")
                    .Build();

            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId0).Count);
            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId1).Count);
            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId2).Count);

            notificationManager.Send(notification, new HashSet<string>() { accountId1, accountId2 });

            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId0).Count);
            Assert.AreEqual(1, notificationManager.GetCurrentNotifications(accountId1).Count);
            Assert.AreEqual(1, notificationManager.GetCurrentNotifications(accountId2).Count);

            notificationManager.Remove(accountId1, notificationManager.GetCurrentNotifications(accountId1)[0].Uuid);

            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId0).Count);
            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId1).Count);
            Assert.AreEqual(1, notificationManager.GetCurrentNotifications(accountId2).Count);

        }

        [TestMethod]
        public void TestNotificationsWithRemoveFromTargetUrl() {
            var container = GetConfiguredContainer();
            INotificationManager notificationManager = container.Resolve<INotificationManager>();

            Notification notification = new NotificationBuilder()
                    .WithSender(accountId0)
                    .WithType("Test")
                    .WithTitle("news")
                    .WithTargetUrl("#keyConcept@2")
                    .WithContent("discover this amazing app !!")
                    .Build();

            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId0).Count);
            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId1).Count);
            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId2).Count);

            notificationManager.Send(notification, new HashSet<string>() { accountId1, accountId2 });

            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId0).Count);
            Assert.AreEqual(1, notificationManager.GetCurrentNotifications(accountId1).Count);
            Assert.AreEqual(1, notificationManager.GetCurrentNotifications(accountId2).Count);

            notificationManager.RemoveAll("Test", "#keyConcept@2");

            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId0).Count);
            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId1).Count);
            Assert.AreEqual(0, notificationManager.GetCurrentNotifications(accountId2).Count);

        }
    }
}
