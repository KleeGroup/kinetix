using System;
#if NUnit
    using NUnit.Framework; 
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#endif

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Test unitaire de la classe Domain.
    /// </summary>
    [TestFixture]
    public class DomainTest {
        /// <summary>
        /// Test la création d'un domain de type BYTE.
        /// </summary>
        [Test]
        public void ConstructorByte() {
            IDomain d = new Domain<byte?>("DOMAIN", null, null);
        }

        /// <summary>
        /// Test la création d'un domain de type SHORT.
        /// </summary>
        [Test]
        public void ConstructorShort() {
            IDomain d = new Domain<short?>("DOMAIN", null, null);
        }

        /// <summary>
        /// Test la création d'un domain de type INT.
        /// </summary>
        [Test]
        public void ConstructorInt() {
            IDomain d = new Domain<int?>("DOMAIN", null, null);
        }

        /// <summary>
        /// Test la création d'un domain de type string.
        /// </summary>
        [Test]
        public void ConstructorString() {
            IDomain d = new Domain<string>("DOMAIN", null, null);
        }

        /// <summary>
        /// Test la création d'un domain de type long.
        /// </summary>
        [Test]
        public void ConstructorLong() {
            IDomain d = new Domain<long?>("DOMAIN", null, null);
        }

        /// <summary>
        /// Test l'échec de création d'un domain de type object.
        /// </summary>
        [Test]
        public void ConstructorGuid() {
            IDomain d = new Domain<Guid?>("DOMAIN", null, null);
        }

        /// <summary>
        /// Test l'échec de création d'un domain de type int non nullable.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorNullableInt() {
            IDomain d = new Domain<int>("DOMAIN", null, null);
        }

        /// <summary>
        /// Test l'échec de création d'un domain de type int non nullable.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorObject() {
            IDomain d = new Domain<object>("DOMAIN", null, null);
        }

        /// <summary>
        /// Test l'échec de création d'un domain sans nom.
        /// </summary>
        [Test]
        public void ConstructorParameterNameNull() {
            try {
                IDomain d = new Domain<string>(null, null, null);
                Assert.Fail();
            } catch (ArgumentException ae) {
                Assert.AreEqual(ae.ParamName, "name");
            }
        }
    }
}
