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
    /// Test unitaire de la classe FmkBroker.
    /// </summary>
    [TestFixture]
    public class FmkBrokerTest {
        /// <summary>
        /// Initialise le contexte des tests.
        /// </summary>
        [SetUp]
        public void SetUp() {
            BrokerManager.Instance.RegisterStore("nodatasource", typeof(TestStore<>));
        }

        /// <summary>
        /// Test l'appel du constructeur sans optimistic locking.
        /// </summary>
        [Test]
        public void TestConstructorWithoutLocking() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("nodatasource");
            Assert.AreEqual(0, broker.StoreRules.Count);
        }
    }
}
