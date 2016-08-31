using Kinetix.Monitoring.Manager;
#if NUnit
    using NUnit.Framework; 
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;

#endif

namespace Kinetix.Data.SqlClient.Test {
    /// <summary>
    /// Classe de test du sql server manager.
    /// </summary>
    [TestFixture]
    public class SqlServerManagerTest {
        /// <summary>
        /// Test le bon fonctionnement du constructeur.
        /// </summary>
        [Test]
        public void Constructor() {
            SqlServerManager manager = SqlServerManager.Instance;
        }

        /// <summary>
        /// Récupération du nom.
        /// </summary>
        [Test]
        public void DescriptionName() {
            SqlServerManager manager = SqlServerManager.Instance;
            string name = ((IManager)manager).Description.Name;
            Assert.IsNotNull(name);
            Assert.IsTrue(!string.IsNullOrEmpty(name));
        }

        /// <summary>
        /// Récupération du type mime.
        /// </summary>
        [Test]
        public void DescriptionImageMimeType() {
            SqlServerManager manager = SqlServerManager.Instance;
            string mineType = ((IManager)manager).Description.ImageMimeType;
            Assert.IsNotNull(mineType);
            Assert.IsTrue(!string.IsNullOrEmpty(mineType));
        }

        /// <summary>
        /// Récupération du nom de l'image.
        /// </summary>
        [Test]
        public void DescriptionImage() {
            SqlServerManager manager = SqlServerManager.Instance;
            string image = ((IManager)manager).Description.Image;
            Assert.IsNotNull(image);
            Assert.IsTrue(!string.IsNullOrEmpty(image));
        }

        /// <summary>
        /// Récupération des données de l'image.
        /// </summary>
        [Test]
        public void DescriptionImageData() {
            SqlServerManager manager = SqlServerManager.Instance;
            Assert.IsNotNull(((IManager)manager).Description.ImageData);
        }

        /// <summary>
        /// Récupération la priorité.
        /// </summary>
        [Test]
        public void DescriptionPriority() {
            SqlServerManager manager = SqlServerManager.Instance;
            Assert.IsNotNull(((IManager)manager).Description.Priority);
        }

        /// <summary>
        /// Cloture du service.
        /// </summary>
        [Test]
        public void ManagerClose() {
            SqlServerManager manager = SqlServerManager.Instance;
            ((IManager)manager).Close();
        }
    }
}
