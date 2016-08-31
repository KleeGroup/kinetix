using System;
using System.Web.UI;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Html;
using Kinetix.Monitoring.Manager;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#endif

namespace Kinetix.Monitoring.Test {
    /// <summary>
    /// Classe de test du monitoring.
    /// </summary>
    [TestFixture]
    public class AnalyticsTest : IManagerDescription {

        /// <summary>
        /// Nom du manager.
        /// </summary>
        string IManagerDescription.Name {
            get {
                return null;
            }
        }

        /// <summary>
        /// Image du manager.
        /// </summary>
        string IManagerDescription.Image {
            get {
                return null;
            }
        }

        /// <summary>
        /// Type mime de l'image.
        /// </summary>
        string IManagerDescription.ImageMimeType {
            get {
                return null;
            }
        }

        /// <summary>
        /// Image.
        /// </summary>
        byte[] IManagerDescription.ImageData {
            get {
                return null;
            }
        }

        /// <summary>
        /// Priorité d'affichage du manager.
        /// </summary>
        int IManagerDescription.Priority {
            get {
                return 0;
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        public void MonitoringTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.IsEnabled = true;
                instance.OpenDataBase("TESTDB", null);
                instance.CreateCounter("Compteur de test", "TST", 0, 0, 10);

                instance.StartProcess("Processus1");
                instance.IncValue("TST", 100);
                instance.StopProcess("TESTDB");
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MonitoringCreateProcessNull() {
            using (Analytics instance = Analytics.Instance) {
                instance.IsEnabled = true;
                instance.OpenDataBase("TESTDB", null);
                instance.StartProcess(null);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        public void MonitoringCreateCounter() {
            using (Analytics instance = Analytics.Instance) {
                instance.IsEnabled = true;
                instance.OpenDataBase("TESTDB", null);
                instance.CreateCounter("Compteur de test", "TST", 0, 0, 10);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void MonitoringCreateCounterTwiss() {
            using (Analytics instance = Analytics.Instance) {
                instance.IsEnabled = true;
                instance.OpenDataBase("TESTDB", null);
                instance.CreateCounter("Compteur de test", "TST", 0, 0, 10);
                instance.CreateCounter("Compteur de test", "TST", 0, 0, 10);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MonitoringCreateCounterLabelNull() {
            using (Analytics instance = Analytics.Instance) {
                instance.IsEnabled = true;
                instance.OpenDataBase("TESTDB", null);
                instance.CreateCounter(null, "TST", 0, 0, 10);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MonitoringCreateCounterCodeNull() {
            using (Analytics instance = Analytics.Instance) {
                instance.IsEnabled = true;
                instance.OpenDataBase("TESTDB", null);
                instance.CreateCounter("Compteur de test", null, 0, 0, 10);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MonitoringCreateCounterLabelEmpty() {
            using (Analytics instance = Analytics.Instance) {
                instance.IsEnabled = true;
                instance.OpenDataBase("TESTDB", null);
                instance.CreateCounter(String.Empty, "TST", 0, 0, 10);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MonitoringCreateCounterCodeEmpty() {
            using (Analytics instance = Analytics.Instance) {
                instance.IsEnabled = true;
                instance.OpenDataBase("TESTDB", null);
                instance.CreateCounter("Compteur de test", String.Empty, 0, 0, 10);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MonitoringOpenDatabaseNullTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.IsEnabled = true;
                instance.OpenDataBase(null, null);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void MonitoringOpenDatabaseTwissTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.IsEnabled = true;
                instance.OpenDataBase("TESTDB", null);
                instance.OpenDataBase("TESTDB", null);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MonitoringAddStoreNullTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.IsEnabled = true;
                instance.AddMonitoringStore(null);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MonitoringIncNullCounterTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.IncValue(null, 0);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void MonitoringIncUndefineCounterTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.IncValue("TEST", 0);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MonitoringStopProcessNullTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.StopProcess(null);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void MonitoringStopProcessUnknownDbTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.StartProcess("Process");
                instance.StopProcess("TESTDB");
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        public void MonitoringStopProcessUnstartedTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.OpenDataBase("TESTDB", null);
                instance.StopProcess("TESTDB");
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        public void MonitoringStorePreLoadedTest() {
            using (Analytics instance = Analytics.Instance) {
                MonitoringStoreTest store = new MonitoringStoreTest();
                instance.AddMonitoringStore(store);
                instance.OpenDataBase("TESTDB", this);
                Assert.AreEqual("TESTDB", store.LastDatabaseName);
                instance.CreateCounter("Compteur de test", "TST", 0, 0, 10);
                Assert.AreEqual("TST", store.LastCounterCode);

                instance.StartProcess("Processus1");
                instance.IncValue("TST", 100);
                instance.StopProcess("TESTDB");
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        public void MonitoringStoreExceptionTest() {
            using (Analytics instance = Analytics.Instance) {
                MonitoringStoreException store = new MonitoringStoreException();
                instance.AddMonitoringStore(store);
                instance.OpenDataBase("TESTDB", this);
                instance.CreateCounter("Compteur de test", "TST", 0, 0, 10);

                instance.StartProcess("Processus1");
                instance.IncValue("TST", 100);
                instance.StopProcess("TESTDB");

                instance.HandleException(new Exception());
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        public void MonitoringStoreHandleExceptionTest() {
            using (Analytics instance = Analytics.Instance) {
                MonitoringStoreTest store = new MonitoringStoreTest();
                instance.AddMonitoringStore(store);
                Exception e = new Exception();
                instance.HandleException(e);
                Assert.AreEqual(e, store.LastException);
            }
        }

        /// <summary>
        /// Test du monitoring.
        /// </summary>
        [Test]
        public void MonitoringStorePostLoadedTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.OpenDataBase("TESTDB", this);
                instance.CreateCounter("Compteur de test", "TST", 0, 0, 10);

                MonitoringStoreTest store = new MonitoringStoreTest();
                instance.AddMonitoringStore(store);
                Assert.AreEqual("TESTDB", store.LastDatabaseName);
                Assert.AreEqual("TST", store.LastCounterCode);

                instance.StartProcess("Processus1");
                instance.IncValue("TST", 100);
                instance.StopProcess("TESTDB");
            }
        }

        /// <summary>
        /// Vérifie la persistance des données à l'arrêt du monitoring.
        /// </summary>
        [Test]
        public void MonitoringDisposeDataTest() {
            MonitoringStoreTest store = new MonitoringStoreTest();
            using (Analytics instance = Analytics.Instance) {
                instance.OpenDataBase("TESTDB", this);
                instance.CreateCounter("Compteur de test", "TST", 0, 0, 10);

                instance.AddMonitoringStore(store);
                Assert.AreEqual("TESTDB", store.LastDatabaseName);
                Assert.AreEqual("TST", store.LastCounterCode);

                instance.StartProcess("Processus1");
                instance.IncValue("TST", 100);
                instance.StopProcess("TESTDB");
            }
            Assert.IsTrue(store.HasCounterData);
        }

        /// <summary>
        /// Vérifie la persistance des données à l'arrêt du monitoring.
        /// </summary>
        [Test]
        public void MonitoringSubProcessTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.OpenDataBase("TESTDB", this);
                instance.CreateCounter("Compteur de test", "TST", 0, 0, 10);

                instance.StartProcess("Processus1");
                instance.IncValue("TST", 100);
                instance.StartProcess("Processus2");
                instance.IncValue("TST", 100);
                instance.StopProcess("TESTDB");
                instance.StopProcess("TESTDB");
            }
        }

        /// <summary>
        /// Vérifie la persistance des données à l'arrêt du monitoring.
        /// </summary>
        [Test]
        public void MonitoringHandlerTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.OpenDataBase("TESTDB", this);
                instance.CreateCounter("Compteur de test", "TST", 0, 0, 10);

                instance.StartProcess("Processus1");
                instance.IncValue("TST", 100);
                instance.StopProcess("TESTDB");

                AnalyticsHandler handler = new AnalyticsHandler();
                handler.ProcessRequest(null);
            }
        }

        /// <summary>
        /// Vérifie la persistance des données à l'arrêt du monitoring.
        /// </summary>
        [Test]
        public void MonitoringResetTest() {
            using (Analytics instance = Analytics.Instance) {
                instance.OpenDataBase("TESTDB", this);
                instance.CreateCounter("Compteur de test", "TST", 0, 0, 10);

                instance.StartProcess("Processus1");
                instance.IncValue("TST", 100);
                instance.StopProcess("TESTDB");

                instance.Reset("TESTDB");
            }
        }

        /// <summary>
        /// Extension de la méthode toString().
        /// Permet à chaque Manager de présenter son propre état.
        /// </summary>
        /// <param name="writer">Writer HTML.</param>
        void IManagerDescription.ToHtml(HtmlTextWriter writer) {
            return;
        }
    }
}
