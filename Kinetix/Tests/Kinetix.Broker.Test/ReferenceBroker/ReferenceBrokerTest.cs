using System.Linq;
using Kinetix.ServiceModel;
using System;
using Microsoft.Practices.Unity;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TestFixtureSetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
#endif

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Classe de test du broker de référence.
    /// </summary>
    [TestFixture]
    public class ReferenceBrokerTest {
        private static Lazy<IUnityContainer> Container = new Lazy<IUnityContainer>(() => {
            return new UnityContainer();
        });

        private ReferenceBrokerMock<ReferenceBean> Broker;

        /// <summary>
        /// Initialise le contexte des tests.
        /// Exécuté une seule fois.
        /// </summary>
        /// <param name="context"></param>
        [TestFixtureSetUp]
        public static void TestFixtureSetUp(TestContext context = null) {
            ServiceManager.Instance.Container = Container.Value;
            ServiceManager.Instance.RegisterLocalService(typeof(IReferenceAccessors), typeof(ReferenceAccessors));

        }

        /// <summary>
        /// Initialise le contexte des tests.
        /// Exécuté pour chaque test.
        /// </summary>
        [SetUp]
        public void SetUp() {
            BrokerManager.Instance.RegisterStore("name", typeof(TestStore<>));
            Broker = new ReferenceBrokerMock<ReferenceBean>("name", new ServiceResourceLoaderMock(), new ServiceResourceWriterMock());
            ReferenceBrokerTestHelper.LangueCode = "EN";
            TestStore<ReferenceBean>.ExceptionOnCall = false;
        }

        /// <summary>
        /// Test le fonctionnement du constructeur.
        /// </summary>
        [Test]
        public void TestConstructor() {
            // Test SetUp.
        }

        /// <summary>
        /// Test le bon fonctionnement du Save.
        /// </summary>
        [Test]
        public void TestSave() {
            Assert.IsNotNull(Broker.Save(new ReferenceBean { Libelle = "A" }, null));
        }

        /// <summary>
        /// Test l'update dans la langue par défaut.
        /// </summary>
        [Test]
        public void TestUpdateOnDefaultLanguage() {
            ReferenceBean bean = new ReferenceBean { Libelle = "A" };
            Assert.IsNotNull(Broker.Save(bean, null));
            bean.Libelle = "B";
            Assert.IsNotNull(Broker.Save(bean, null));

            // Check that translation has been updated.
            Assert.AreEqual("B", ReferenceBrokerTestHelper.Traduction.Last().Value);
            // Check that value in store has been updated.
            Assert.AreEqual("B", TestStore<ReferenceBean>.PutList.Last().Libelle);
        }

        /// <summary>
        /// Test l'update dans une autre langue.
        /// </summary>
        [Test]
        public void TestUpdateOnOtherLanguage() {
            ReferenceBrokerTestHelper.LangueCode = "DE";

            ReferenceBean bean = new ReferenceBean { Libelle = "A" };
            Assert.IsNotNull(Broker.Save(bean, null));
            bean.Libelle = "B";
            Assert.IsNotNull(Broker.Save(bean, null));

            // Check that translation has been updated.
            Assert.AreEqual("B", ReferenceBrokerTestHelper.Traduction.Last().Value);
            // Check that value in store has not been updated.
            Assert.AreNotEqual("B", TestStore<ReferenceBean>.PutList.Last().Libelle);
        }
    }
}
