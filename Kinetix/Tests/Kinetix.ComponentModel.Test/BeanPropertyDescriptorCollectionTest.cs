using System;
using System.Collections;
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
    /// Test unitaire de la classe BeanPropertyDescriptorCollection.
    /// </summary>
    [TestFixture]
    public class BeanPropertyDescriptorCollectionTest {
        /// <summary>
        /// Initialise l'application avec un domaine LIBELLE_COURT.
        /// </summary>
        [SetUp]
        public void Init() {
            DomainManager.Instance.RegisterDomainMetadataType(typeof(TestDomainMetadata));
        }

        /// <summary>
        /// Test la méthode Contains.
        /// </summary>
        [Test]
        public void Contains() {
            BeanDefinition definition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor primaryKey = definition.PrimaryKey;
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            Assert.IsTrue(coll.Contains(primaryKey));
            Assert.IsTrue(definition.Properties.Contains(primaryKey.PropertyName));
        }

        /// <summary>
        /// Test la méthode Item avec une propriété existante.
        /// </summary>
        [Test]
        public void Item() {
            BeanDefinition definition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor primaryKey = definition.PrimaryKey;
            Assert.AreEqual(primaryKey, definition.Properties[primaryKey.PropertyName]);
        }

        /// <summary>
        /// Test la méthode Item avec une propriété absente.
        /// </summary>
        [Test]
        public void ItemNotFound() {
            BeanDefinition definition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor primaryKey = definition.PrimaryKey;
            try {
                definition.Properties["PasDePropriete"].ToString();
            } catch (Exception e) {
                Assert.IsTrue(e.Message.Contains("Bean"));
            }
        }

        /// <summary>
        /// Test la méthode CopyTo.
        /// </summary>
        [Test]
        public void CopyTo() {
            BeanDefinition definition = BeanDescriptor.GetDefinition(new Bean());
            BeanPropertyDescriptor[] array = new BeanPropertyDescriptor[definition.Properties.Count];
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            coll.CopyTo(array, 0);
            Assert.IsNotNull(array[0]);
        }

        /// <summary>
        /// Test la méthode GetEnumerator.
        /// </summary>
        [Test]
        public void Enumerator() {
            BeanDefinition definition = BeanDescriptor.GetDefinition(new Bean());
            IEnumerable coll = (IEnumerable)definition.Properties;
            int count = 0;
            foreach (object o in coll) {
                count++;
            }
            Assert.AreEqual(definition.Properties.Count, count);
        }

        /// <summary>
        /// Test la valeur de la propriété IsReadOnly.
        /// </summary>
        [Test]
        public void IsReadonly() {
            BeanDefinition definition = BeanDescriptor.GetDefinition(new object());
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            Assert.IsTrue(coll.IsReadOnly);
        }

        /// <summary>
        /// Test la valeur de la propriété Count.
        /// </summary>
        [Test]
        public void Count() {
            BeanDefinition definition = BeanDescriptor.GetDefinition(new object());
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            Assert.AreEqual(0, coll.Count);
        }

        /// <summary>
        /// Test l'échec de la méthode Add.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Add() {
            BeanDefinition definition = BeanDescriptor.GetDefinition(new object());
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            coll.Add(null);
        }

        /// <summary>
        /// Test l'échec de la méthode Clear.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Clear() {
            BeanDefinition definition = BeanDescriptor.GetDefinition(new object());
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            coll.Clear();
        }

        /// <summary>
        /// Test l'échec de la méthode Remove.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Remove() {
            BeanDefinition definition = BeanDescriptor.GetDefinition(new object());
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            coll.Remove(null);
        }
    }
}
