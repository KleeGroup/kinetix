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

namespace Kinetix.Data.SqlClient.Test {
    /// <summary>
    /// Test unitaire de la classe SqlServerException.
    /// </summary>
    [TestFixture]
    public class SqlServerExceptionTest {
        /// <summary>
        /// Test le fonctionnement du constructeur par défaut.
        /// </summary>
        [Test]
        public void ConstructorDefault() {
            SqlServerException exception = new SqlServerException();
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec message.
        /// </summary>
        [Test]
        public void ConstructorMessage() {
            SqlServerException exception = new SqlServerException("Message");
            Assert.AreEqual("Message", exception.Message);
        }

        /// <summary>
        /// Test le fonctionnement du constructeur avec InnerException.
        /// </summary>
        [Test]
        public void ConstructorInner() {
            Exception e = new Exception();
            SqlServerException exception = new SqlServerException("Message", e);
            Assert.AreEqual("Message", exception.Message);
            Assert.AreEqual(e, exception.InnerException);
        }

        /// <summary>
        /// Test le fonctionnement du constructeur de désérialisation.
        /// </summary>
        [Test]
        public void ConstructorDeserialize() {
            SqlServerException exception = new SqlServerException("Message");
            BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All));

            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream()) {
                formatter.Serialize(ms, exception);
                buffer = ms.ToArray();
            }

            SqlServerException deserializeException = null;
            using (MemoryStream ms = new MemoryStream(buffer)) {
                deserializeException = (SqlServerException)formatter.Deserialize(ms);
            }

            Assert.AreEqual("Message", deserializeException.Message);
        }
    }
}
