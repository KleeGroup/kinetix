using System;
using System.Collections.Generic;
using System.Data.Linq;
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
    /// Test du BeanDescriptor.
    /// </summary>
    [TestFixture]
    public class BeanDescriptorTest {
        /// <summary>
        /// Initialise l'application avec un domaine LIBELLE_COURT.
        /// </summary>
        [SetUp]
        public void Init() {
            DomainManager.Instance.RegisterDomainMetadataType(typeof(TestDomainMetadata));
        }

        /// <summary>
        /// Test la récupération de la propriété par défaut d'un bean.
        /// </summary>
        [Test]
        public void TestDefaultProperty() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            Assert.AreEqual("Libelle", beanDefinition.DefaultProperty.PropertyName);
        }

        /// <summary>
        /// Test la récupération des propriétés d'un type implémentant ICustomTypeDescriptor.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetDefinitionByTypeCustom() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(typeof(BeanDynamic));
        }

        /// <summary>
        /// Test la récupération des propriétés d'un type null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDefinitionByTypeNull() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition((Type)null);
        }

        /// <summary>
        /// Test la récupération des propriétés d'une collection de bean nulle.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDefinitionCollectionNull() {
            BeanDefinition beanDefinition = BeanDescriptor.GetCollectionDefinition(null);
        }

        /// <summary>
        /// Test la récupération des propriétés d'une collection de bean non générique.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetDefinitionCollectionNotGenericCollection() {
            Generic<Bean> generic = new Generic<Bean>();
            BeanDefinition beanDefinition = BeanDescriptor.GetCollectionDefinition(generic);
        }

        /// <summary>
        /// Test la récupération de la clef primaire.
        /// </summary>
        [Test]
        public void GetDefinitionPrimaryKey() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            Assert.IsNotNull(beanDefinition.PrimaryKey);
            Assert.AreEqual("Id", beanDefinition.PrimaryKey.PropertyName);
        }

        /// <summary>
        /// Test la récupération du nom de contrat.
        /// </summary>
        [Test]
        public void GetDefinitionContract() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            Assert.IsNotNull(beanDefinition.ContractName);
            Assert.AreEqual("BEAN", beanDefinition.ContractName);
        }

        /// <summary>
        /// Test la récupération des propriétés d'un bean valide.
        /// </summary>
        [Test]
        public void GetDefinitionDynamic() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new BeanDynamic());
            BeanPropertyDescriptorCollection coll = beanDefinition.Properties;
            bool idOk = false;
            bool otherIdOk = false;
            foreach (BeanPropertyDescriptor prop in coll) {
                if ("Id".Equals(prop.PropertyName)) {
                    Assert.AreEqual("IDENTIFIANT", prop.DomainName);
                    Assert.AreEqual("BEA_ID", prop.MemberName);
                    Assert.IsTrue(prop.IsRequired);
                    idOk = true;
                } else if ("OtherId".Equals(prop.PropertyName)) {
                    Assert.AreEqual("IDENTIFIANT", prop.DomainName);
                    Assert.AreEqual("OTH_ID", prop.MemberName);
                    Assert.IsFalse(prop.IsRequired);
                    otherIdOk = true;
                }
            }
            Assert.IsTrue(idOk);
            Assert.IsTrue(otherIdOk);
        }

        /// <summary>
        /// Test la récupération des propriétés avec une valeur nulle.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDefinitionNull() {
            BeanDescriptor.GetDefinition((object)null);
        }

        /// <summary>
        /// Test la récupération des propriétés pour un type de clef.
        /// étrangère invalide.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetDefinitionInvalidReferenceType() {
            BeanDescriptor.GetDefinition(new BeanInvalidReferenceType());
        }

        /// <summary>
        /// Test la récupération des propriétés pour un domain invalide.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetDefinitionInvalidDomainType() {
            BeanDescriptor.GetDefinition(new BeanInvalidDomainType());
        }

        /// <summary>
        /// Test la récupération des propriétés pour un domain invalide.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetDefinitionOtherInvalidDomainType() {
            BeanDescriptor.GetDefinition(new BeanOtherInvalidDomainType());
        }

        /// <summary>
        /// Test la récupération des propriétés pour un type invalide.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetDefinitionInvalidPropertyType() {
            BeanDescriptor.GetDefinition(new BeanInvalidPropertyType());
        }

        /// <summary>
        /// Test la récupération des propriétés pour un type généric invalide.
        /// </summary>
        [Test]
        public void GetDefinitionInvalidGenericType() {
            BeanDescriptor.GetDefinition(new BeanInvalidGenericType());
        }

        /// <summary>
        /// Test la récupération des propriétés pour un type non supporté.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetDefinitionUnsupportedType() {
            BeanDefinition definition = BeanDescriptor.GetDefinition(new BeanInvalidUnsupportedType());
            BeanPropertyDescriptor property = definition.Properties["OtherId"];
            property.ValidConstraints(3, null);
        }

        /// <summary>
        /// Vérifie le contenu d'un bean : la clef primaire peut être nulle.
        /// </summary>
        [Test]
        public void CheckInsert() {
            Bean b = new Bean();
            b.LibelleNotNull = "libelle";
            BeanDescriptor.Check(b, true);
        }

        /// <summary>
        /// Vérifie le contenu d'un bean : la clef primaire ne peut pas être nulle.
        /// </summary>
        [Test]
        public void CheckUpdate() {
            Bean b = new Bean();
            b.Id = 3;
            b.LibelleNotNull = "libelle";
            BeanDescriptor.Check(b, false);
        }

        /// <summary>
        /// Vérifie le contenu d'un bean : libellé vide == null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ConstraintException))]
        public void CheckUpdateStringEmpty() {
            Bean b = new Bean();
            b.Id = 3;
            b.LibelleNotNull = string.Empty;
            BeanDescriptor.Check(b, false);
        }

        /// <summary>
        /// Test CheckAll : paramètre collection d'objets invalides supprimés.
        /// </summary>
        [Test]
        public void TestCheckAllObjectCollection() {
            List<IBeanState> coll = new List<IBeanState>();

            StateBean bean = new StateBean();
            bean.State = ChangeAction.Delete;
            coll.Add(bean);

            bean = new StateBean();
            bean.LibelleNotNull = "libelle";
            bean.State = ChangeAction.Insert;
            coll.Add(bean);

            BeanDescriptor.CheckAll(coll, true);
        }

        /// <summary>
        /// Test CheckAll : paramètre collection null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCheckAllNullCollection() {
            BeanDescriptor.CheckAll<int>(null, false);
        }
    }
}
