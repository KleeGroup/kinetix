using System;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Classe de test du broker manager.
    /// </summary>
    [TestFixture]
    public class BrokerManagerTest {
        /// <summary>
        /// Test le constructeur.
        /// </summary>
        [Test]
        public void TestConstructor() {
            BrokerManager broker = BrokerManager.Instance;
            Assert.IsNotNull(broker);
        }

        /// <summary>
        /// Test l'appel à la méthode RegisterStore avec un type nul.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestRegisterNullStore() {
            BrokerManager.Instance.RegisterStore("test", null);
        }

        /// <summary>
        /// Test l'appel à la méthode RegisterStore avec une datasource nulle.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestRegisterNullDs() {
            BrokerManager.Instance.RegisterStore(null, typeof(TestStore<>));
        }
    }
}
