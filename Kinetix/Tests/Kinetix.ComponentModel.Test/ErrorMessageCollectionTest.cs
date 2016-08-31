using System;
using System.Collections;
using System.Collections.Generic;
using Kinetix.ComponentModel.Test.Contract;
#if NUnit
    using NUnit.Framework; 
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#endif

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Test de la collection de message.
    /// </summary>
    [TestFixture]
    public class ErrorMessageCollectionTest {
        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        public void TestConstructor() {
            ErrorMessageCollection collection = new ErrorMessageCollection();
            Assert.IsFalse(collection.HasError);
            collection.Throw();
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ConstraintException))]
        public void TestAddEntry() {
            ErrorMessageCollection collection = new ErrorMessageCollection();
            collection.AddEntry("Field", "Erreur");
            Assert.IsTrue(collection.HasError);
            collection.Throw();
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ConstraintException))]
        public void TestAddEntryWithRowNum() {
            ErrorMessageCollection collection = new ErrorMessageCollection();
            collection.AddEntry(0, "Field", "Erreur");
            Assert.IsTrue(collection.HasError);
            collection.Throw();
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        public void TestAddErrorStack() {
            ErrorMessageCollection collection = new ErrorMessageCollection();
            collection.AddErrorStack("Prefix", new ErrorMessageCollection());
            Assert.IsFalse(collection.HasError);
            collection.Throw();
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ConstraintException))]
        public void TestAddErrorStackWithError() {
            ErrorMessageCollection collection = new ErrorMessageCollection();
            ErrorMessageCollection innerCollection = new ErrorMessageCollection();
            innerCollection.AddEntry("Field", "Erreur");
            collection.AddErrorStack("Prefix", innerCollection);
            Assert.IsTrue(collection.HasError);
            collection.Throw();
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ConstraintException))]
        public void TestAddConstraint() {
            ErrorMessageCollection collection = new ErrorMessageCollection();
            collection.AddConstraintException(new ConstraintException("Erreur"));
            Assert.IsTrue(collection.HasError);
            collection.Throw();
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ConstraintException))]
        public void TestAddConstraintWithErrors() {
            ErrorMessageCollection innerCollection = new ErrorMessageCollection();
            innerCollection.AddEntry("Field", "Erreur");

            ErrorMessageCollection collection = new ErrorMessageCollection();
            collection.AddConstraintException(new ConstraintException(innerCollection));
            Assert.IsTrue(collection.HasError);
            collection.Throw();
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ConstraintException))]
        public void TestAddConstraintWithField() {
            Bean b = new Bean();
            ErrorMessageCollection collection = new ErrorMessageCollection();
            collection.AddConstraintException(new ConstraintException(BeanDescriptor.GetDefinition(b).PrimaryKey, "Erreur"));
            Assert.IsTrue(collection.HasError);
            collection.Throw();
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestClear() {
            ICollection<ErrorMessage> collection = new ErrorMessageCollection();
            collection.Clear();
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestAdd() {
            ErrorMessageCollection initialCollection = new ErrorMessageCollection();
            initialCollection.AddEntry("Field", "Erreur");
            IEnumerator enumerator = ((IEnumerable)initialCollection).GetEnumerator();
            enumerator.MoveNext();

            ICollection<ErrorMessage> collection = new ErrorMessageCollection();
            collection.Add((ErrorMessage)enumerator.Current);
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        public void TestIsReadonly() {
            ICollection<ErrorMessage> collection = new ErrorMessageCollection();
            Assert.IsTrue(collection.IsReadOnly);
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestRemove() {
            ICollection<ErrorMessage> collection = new ErrorMessageCollection();
            collection.Remove(null);
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestContains() {
            ICollection<ErrorMessage> collection = new ErrorMessageCollection();
            collection.Contains(null);
        }

        /// <summary>
        /// Test de la collection de message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestCopyTo() {
            ICollection<ErrorMessage> collection = new ErrorMessageCollection();
            collection.CopyTo(null, 0);
        }
    }
}
