using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Test unitaire de la classe BrokerException.
    /// </summary>
    [TestFixture]
    public class BrokerExceptionTest {
        /// <summary>
        /// Test le fonctionnement du constructeur par défaut.
        /// </summary>
        [Test]
        public void ConstructorDefault() {
            BrokerException exception = new BrokerException();
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec message.
        /// </summary>
        [Test]
        public void ConstructorMessage() {
            BrokerException exception = new BrokerException("Message");
            Assert.AreEqual("Message", exception.Message);
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec InnerException.
        /// </summary>
        [Test]
        public void ConstructorInner() {
            Exception e = new Exception();
            BrokerException exception = new BrokerException("Message", e);
            Assert.AreEqual("Message", exception.Message);
            Assert.AreEqual(e, exception.InnerException);
        }

        /// <summary>
        /// Test le fonctionnement du constructeur de désérialisation.
        /// </summary>
        [Test]
        public void ConstructorDeserialize() {
            BrokerException exception = new BrokerException("Message");
            BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All));

            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream()) {
                formatter.Serialize(ms, exception);
                buffer = ms.ToArray();
            }

            BrokerException deserializeException = null;
            using (MemoryStream ms = new MemoryStream(buffer)) {
                deserializeException = (BrokerException)formatter.Deserialize(ms);
            }

            Assert.AreEqual("Message", deserializeException.Message);
        }
    }
}
