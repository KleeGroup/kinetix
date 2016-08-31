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
#endif

namespace Kinetix.Caching.Test {
    /// <summary>
    /// Test unitaire de la classe CacheException.
    /// </summary>
    [TestFixture]
    public class CacheExceptionTest {
        /// <summary>
        /// Test le fonctionnement du constructeur par défaut.
        /// </summary>
        [Test]
        public void ConstructorDefault() {
            CacheException exception = new CacheException();
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec message.
        /// </summary>
        [Test]
        public void ConstructorMessage() {
            CacheException exception = new CacheException("Message");
            Assert.AreEqual("Message", exception.Message);
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec InnerException.
        /// </summary>
        [Test]
        public void ConstructorInner() {
            Exception e = new Exception();
            CacheException exception = new CacheException("Message", e);
            Assert.AreEqual("Message", exception.Message);
            Assert.AreEqual(e, exception.InnerException);
        }

        /// <summary>
        /// Test le fonctionnement du constructeur de désérialisation.
        /// </summary>
        [Test]
        public void ConstructorDeserialize() {
            CacheException exception = new CacheException("Message");
            BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All));

            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream()) {
                formatter.Serialize(ms, exception);
                buffer = ms.ToArray();
            }

            CacheException deserializeException = null;
            using (MemoryStream ms = new MemoryStream(buffer)) {
                deserializeException = (CacheException)formatter.Deserialize(ms);
            }

            Assert.AreEqual("Message", deserializeException.Message);
        }
    }
}
