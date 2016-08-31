using System;
#if NUnit
    using NUnit.Framework; 
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#endif

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Test unitaire de la classe DomainManager.
    /// </summary>
    [TestFixture]
    public class DomainManagerTest {
        /// <summary>
        /// Initialise l'application avec un domaine LIBELLE_COURT.
        /// </summary>
        [SetUp]
        public void Init() {
            DomainManager.Instance.RegisterDomainMetadataType(typeof(TestDomainMetadata));
        }

        /// <summary>
        /// Test la récupération d'un domain existant.
        /// </summary>
        [Test]
        public void GetDomainConstraintLength() {
            IDomain d = DomainManager.Instance.GetDomain("LIBELLE_COURT");
            Assert.AreEqual(typeof(string), d.DataType);
            Assert.AreEqual(30, d.Length);
        }

        /// <summary>
        /// Test la récupération d'un domain existant.
        /// </summary>
        [Test]
        public void GetDomain() {
            IDomain d = DomainManager.Instance.GetDomain("IDENTIFIANT");
            Assert.AreEqual(typeof(int), d.DataType);
            Assert.IsNull(d.Length);
        }

        /// <summary>
        /// Test la récupération d'un domain null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDomainNull() {
            IDomain d = DomainManager.Instance.GetDomain((string)null);
        }

        /// <summary>
        /// Test l'échec de récupération d'un domain absent.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetMissingDomain() {
            IDomain d = DomainManager.Instance.GetDomain("LIBELLE");
        }
    }
}
