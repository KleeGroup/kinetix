using System;
#if NUnit
    using NUnit.Framework; 
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;

#endif

namespace Kinetix.ServiceModel.Test {
    /// <summary>
    /// Classe de test du service manager.
    /// </summary>
    [TestFixture]
    public class ServiceManagerTest {
        /// <summary>
        /// Test le bon fonctionnement du constructeur.
        /// </summary>
        [Test]
        public void Constructor() {
            ServiceManager manager = ServiceManager.Instance;
        }

        /// <summary>
        /// Test l'enregistrement d'un service déjà enregistré.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void RegisterServiceAlreadyRegistered() {
            ServiceManager manager = ServiceManager.Instance;
            if (!manager.ContainsLocalService(typeof(ISampleContract))) {
                manager.RegisterLocalService(typeof(ISampleContract), typeof(SampleService));
            }
            manager.RegisterLocalService(typeof(ISampleContract), typeof(SampleService));
        }

        /// <summary>
        /// Test l'enregistrement d'un service : contract null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterServiceContractNull() {
            ServiceManager manager = ServiceManager.Instance;
            manager.RegisterLocalService(null, typeof(SampleService));
        }

        /// <summary>
        /// Test l'enregistrement d'un service : service null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterServiceServiceNull() {
            ServiceManager manager = ServiceManager.Instance;
            manager.RegisterLocalService(typeof(ISampleContract), null);
        }

        /// <summary>
        /// Test l'enregistrement d'un service : contract représente un class.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterServiceContractClassIsType() {
            ServiceManager manager = ServiceManager.Instance;
            manager.RegisterLocalService(typeof(ServiceManager), typeof(SampleService));
        }

        /// <summary>
        /// Test l'enregistrement d'un service : service n'implémente pas contract.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterServiceBadContractImplementation() {
            ServiceManager manager = ServiceManager.Instance;
            manager.RegisterLocalService(typeof(ISampleContract), typeof(object));
        }

        /// <summary>
        /// Cloture du service.
        /// </summary>
        [Test]
        public void ManagerClose() {
            ServiceManager manager = ServiceManager.Instance;
            manager.Close();
        }

        /// <summary>
        /// Test la présence d'un service avec un paramètre null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsServiceNull() {
            ServiceManager manager = ServiceManager.Instance;
            manager.ContainsLocalService(null);
        }

        /// <summary>
        /// Test l'enregistrement d'un contrat érroné.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void RegisterServiceBadContract() {
            ServiceManager manager = ServiceManager.Instance;
            manager.RegisterLocalService(typeof(IBadContract), typeof(BadService));
        }

        /// <summary>
        /// Test l'enregistrement d'un contrat érroné.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void RegisterServiceBadContractDictionary() {
            ServiceManager manager = ServiceManager.Instance;
            manager.RegisterLocalService(typeof(IBadContractDictionary), typeof(BadServiceDictionary));
        }

        /// <summary>
        /// Test l'enregistrement d'un contrat érroné.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void RegisterServiceBadContractMethod() {
            ServiceManager manager = ServiceManager.Instance;
            manager.RegisterLocalService(typeof(IBadContractMethod), typeof(BadServiceMethod));
        }

        /// <summary>
        /// Test la récupération d'un contrat érroné.
        /// Le contrat érroné ne doit pas être enregistré.
        /// </summary>
        [Test]
        public void RegisterServiceGetBadContractReference() {
            ServiceManager manager = ServiceManager.Instance;
            try {
                manager.RegisterLocalService(typeof(IBadContract), typeof(BadService));
            } catch {
                Assert.IsFalse(manager.ContainsLocalService(typeof(IBadContract)));
            }
        }
    }
}
