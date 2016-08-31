using System;
using Kinetix.ComponentModel.Formatters;
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
    /// Test du BeanPropertyDescriptor.
    /// </summary>
    [TestFixture]
    public class BeanPropertyDescriptorTest {
        /// <summary>
        /// Initialise l'application avec un domaine LIBELLE_COURT.
        /// </summary>
        [SetUp]
        public void Init() {
            DomainManager.Instance.RegisterDomainMetadataType(typeof(TestDomainMetadata));
        }

        /// <summary>
        /// Test CheckValueType avec un type érroné.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidCastException))]
        public void CheckValueTypeBadType() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor primaryKey = beanDefinition.PrimaryKey;
            primaryKey.CheckValueType(String.Empty);
        }

        /// <summary>
        /// Test CheckValueType avec un type générique érroné.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidCastException))]
        public void CheckValueTypeBadGenericType() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor primaryKey = beanDefinition.PrimaryKey;
            Nullable<double> d = new Nullable<double>();
            d = 3;
            primaryKey.CheckValueType(d);
        }

        /// <summary>
        /// Test CheckValueType avec un type générique non supporté.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void CheckValueTypeUnsupportedGenericType() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor primaryKey = beanDefinition.PrimaryKey;
            primaryKey.CheckValueType(new Generic<Bean>());
        }

        /// <summary>
        /// Test de la convertion en string.
        /// </summary>
        [Test]
        public void ConvertToString() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor primaryKey = beanDefinition.PrimaryKey;
            string text = primaryKey.ConvertToString(3);
            Assert.AreEqual("3", text);
        }

        /// <summary>
        /// Test de la convertion en string avec un formateur.
        /// </summary>
        [Test]
        public void ConvertToStringFormatteur() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor propertyDate = beanDefinition.Properties["Date"];
            propertyDate.ConvertToString(DateTime.Now);
        }

        /// <summary>
        /// Test de la convertion depuis string.
        /// </summary>
        [Test]
        public void ConvertFromString() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor primaryKey = beanDefinition.PrimaryKey;
            object o = primaryKey.ConvertFromString("3");
            Assert.AreEqual(3, o);
        }

        /// <summary>
        /// Test de la convertion depuis string.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ConstraintException))]
        public void ConvertFromStringBadType() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor primaryKey = beanDefinition.PrimaryKey;
            primaryKey.ConvertFromString("aaa");
        }

        /// <summary>
        /// Test de la lecture des unités.
        /// </summary>
        [Test]
        public void GetUnit() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor propertyPourcentage = beanDefinition.Properties["Pourcentage"];
            Assert.AreEqual(FormatterPercent.UnitPercentage, propertyPourcentage.Unit);
        }

        /// <summary>
        /// Test de la vérification d'un champ en forçant le champ à obligatoire.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ConstraintException))]
        public void ValidConstraintsForceMandatory() {
            Bean b = new Bean();
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(b);
            BeanPropertyDescriptor propertyPourcentage = beanDefinition.Properties["Pourcentage"];
            try {
                propertyPourcentage.ValidConstraints(b.Pourcentage, null);
            } catch (Exception e) {
                throw new Exception(e.Message, e);
            }
            propertyPourcentage.ValidConstraints(b.Pourcentage, true);
        }

        /// <summary>
        /// Test de la convertion depuis string avec un formateur.
        /// </summary>
        [Test]
        public void ConvertFromStringFormatteur() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor propertyDate = beanDefinition.Properties["Date"];
            propertyDate.ConvertFromString("02/01/2007");
        }

        /// <summary>
        /// Test l'affectation d'une valeur.
        /// </summary>
        [Test]
        public void SetValue() {
            Bean b = new Bean();
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(b);
            BeanPropertyDescriptor primaryKey = beanDefinition.PrimaryKey;
            primaryKey.SetValue(b, 2);
            Assert.AreEqual(2, b.Id);
        }

        /// <summary>
        /// Test l'affectation d'une valeur d'un mauvais type.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SetValueBadType() {
            Bean b = new Bean();
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(b);
            BeanPropertyDescriptor primaryKey = beanDefinition.PrimaryKey;
            primaryKey.SetValue(b, "test");
        }

        /// <summary>
        /// Test la vérification d'une contrainte.
        /// </summary>
        [Test]
        public void ValidConstraints() {
            Bean b = new Bean();
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(b);
            BeanPropertyDescriptor primaryKey = beanDefinition.PrimaryKey;
            primaryKey.ValidConstraints(2, null);
        }

        /// <summary>
        /// Test la présence d'une erreur en cas de violation de contrainte.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ConstraintException))]
        public void ValidConstraintsError() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor property = beanDefinition.Properties["Libelle"];
            property.ValidConstraints("0123456789012345678901234567890123456789", null);
        }

        /// <summary>
        /// Test la présence d'une erreur en cas de violation de contrainte.
        /// </summary>
        [Test]
        public void GetObjectUnit() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor property = beanDefinition.Properties["Child"];
            Assert.IsNull(property.Unit);
        }

        /// <summary>
        /// Test la présence d'une erreur en cas de violation de contrainte.
        /// </summary>
        [Test]
        public void GetCollectionUnit() {
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor property = beanDefinition.Properties["RoleList"];
            Assert.IsNull(property.Unit);
        }
    }
}
