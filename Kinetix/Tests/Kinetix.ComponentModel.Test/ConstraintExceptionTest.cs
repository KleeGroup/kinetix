using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#endif

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Test unitaire de la classe ConstraintException.
    /// </summary>
    [TestFixture]
    public class ConstraintExceptionTest {
        /// <summary>
        /// Test le fonctionnement du constructeur par défaut.
        /// </summary>
        [Test]
        public void ConstructorDefault() {
            ConstraintException exception = new ConstraintException();
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec message.
        /// </summary>
        [Test]
        public void ConstructorMessage() {
            ConstraintException exception = new ConstraintException("Message");
            Assert.AreEqual("Message", exception.Message);
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec message.
        /// </summary>
        [Test]
        public void ConstructorMessageInner() {
            Exception inner = new Exception();
            ConstraintException exception = new ConstraintException("Message", inner);
            Assert.AreEqual("Message", exception.Message);
            Assert.AreEqual(inner, exception.InnerException);
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec message.
        /// </summary>
        [Test]
        public void ConstructorPropertyMessage() {
            ConstraintException exception = new ConstraintException((BeanPropertyDescriptor)null, "Message");
            Assert.AreEqual("Message", exception.Message);
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec InnerException.
        /// </summary>
        [Test]
        public void ConstructorInner() {
            Exception e = new Exception();
            ConstraintException exception = new ConstraintException(string.Empty, "Message", e);
            Assert.AreEqual("Message", exception.Code);
            Assert.AreEqual(e, exception.InnerException);
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec une ErrorStack.
        /// </summary>
        [Test]
        public void ConstructorErrorStack() {
            ErrorMessageCollection stack = new ErrorMessageCollection();
            ConstraintException exception = new ConstraintException(stack);
            Assert.AreEqual(stack, exception.Errors);
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec une ErrorStack.
        /// </summary>
        [Test]
        public void ConstructorFieldMessage() {
            ConstraintException exception = new ConstraintException("BEA_ID", "Message");
            Assert.IsNotNull(exception.Errors);
            Assert.AreEqual(1, ((ICollection<ErrorMessage>)exception.Errors).Count);
            foreach (ErrorMessage entry in exception.Errors) {
                Assert.AreEqual("BEA_ID", entry.FieldName);
                Assert.AreEqual("Message", entry.Message);
            }
        }

        /// <summary>
        /// Test le fonctionnement du constructeur de désérialisation.
        /// </summary>
        [Test]
        public void ConstructorDeserialize() {
            ConstraintException exception = new ConstraintException((BeanPropertyDescriptor)null, "Message");
            BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All));

            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream()) {
                formatter.Serialize(ms, exception);
                buffer = ms.ToArray();
            }

            ConstraintException deserializeException = null;
            using (MemoryStream ms = new MemoryStream(buffer)) {
                deserializeException = (ConstraintException)formatter.Deserialize(ms);
            }

            Assert.AreEqual("Message", deserializeException.Message);
        }
    }
}
