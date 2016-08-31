using System;
using System.Collections.Generic;
using Kinetix.ComponentModel.Test.Contract;
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
    /// Test de BeanFactory.
    /// </summary>
    [TestFixture]
    public class BeanFactoryTest {
        /// <summary>
        /// Initialise l'application avec un domaine LIBELLE_COURT.
        /// </summary>
        [SetUp]
        public void Init() {
            DomainManager.Instance.RegisterDomainMetadataType(typeof(TestDomainMetadata));
        }

        /// <summary>
        /// Clone un bean.
        /// </summary>
        [Test]
        public void CloneBeanTest() {
            Bean b = new Bean();
            b.Id = 2;
            Bean bean = new BeanFactory<Bean>().CloneBean(b);
            Assert.IsNotNull(bean.RoleList);
            Assert.IsNotNull(bean.Child);
            Assert.AreEqual(2, bean.Id);
        }

        /// <summary>
        /// Clone un bean.
        /// </summary>
        [Test]
        public void CloneBeanRecursiveTest() {
            BeanRecursive b = new BeanRecursive();
            b.Child = new BeanRecursive();
            BeanRecursive bean = new BeanFactory<BeanRecursive>().CloneBean(b);
            Assert.IsNotNull(bean.Child);
            Assert.IsNotNull(bean.Child.Child);
            Assert.IsNull(bean.Child.Child.Child);
        }

        /// <summary>
        /// Clone un bean.
        /// </summary>
        [Test]
        public void CreateInheritBeanTest() {
            Bean b = new Bean();
            b.Id = 2;
            BeanInherit bean = new BeanFactory<Bean, BeanInherit>().CreateBean(b);
            Assert.AreEqual(2, bean.Id);
        }

        /// <summary>
        /// Clone un bean.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateInheritBeanNullTest() {
            new BeanFactory<Bean, BeanInherit>().CreateBean(null);
        }

        /// <summary>
        /// Clone un bean.
        /// </summary>
        [Test]
        public void CreateInheritBeanCollectionTest() {
            List<Bean> list = new List<Bean>();
            Bean b = new Bean();
            b.Id = 2;
            list.Add(b);

            IList<BeanInherit> newList = (IList<BeanInherit>)new BeanFactory<Bean, BeanInherit>().CreateCollection(list);
            Assert.AreEqual(1, newList.Count);
            Assert.AreEqual(2, newList[0].Id);
        }

        /// <summary>
        /// Clone un bean.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateInheritBeanCollectionNullTest() {
            new BeanFactory<Bean, BeanInherit>().CreateCollection(null);
        }
    }
}
