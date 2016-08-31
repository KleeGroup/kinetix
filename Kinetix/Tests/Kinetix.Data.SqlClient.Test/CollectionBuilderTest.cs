using System.Collections.Generic;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif

namespace Kinetix.Data.SqlClient.Test {
    /// <summary>
    /// Classe de test du collection builder.
    /// </summary>
    [TestFixture]
    public class CollectionBuilderTest {
        /// <summary>
        /// Construction d'un collection à partir d'une commande.
        /// </summary>
        [Test]
        public void ParseCommandTest() {
            BeanTest BeanTest = null;
            List<BeanTest> list = new List<BeanTest>();
            BeanTest = new BeanTest();
            BeanTest.Id = 1;
            BeanTest.Name = "Name1";
            list.Add(BeanTest);

            BeanTest = new BeanTest();
            BeanTest.Id = 2;
            BeanTest.Name = "Name2";
            list.Add(BeanTest);

            ICollection<BeanTest> result = CollectionBuilder<BeanTest>.ParseCommand(new TestDbCommand(list));

            bool ok1 = false;
            bool ok2 = false;
            Assert.AreEqual(2, result.Count);
            foreach (BeanTest b in result) {
                if (b.Id == 1) {
                    ok1 = true;
                    Assert.AreEqual("Name1", b.Name);
                } else if (b.Id == 2) {
                    ok2 = true;
                    Assert.AreEqual("Name2", b.Name);
                }
                Assert.IsNull(b.OtherAttribut);
            }
            Assert.IsTrue(ok1);
            Assert.IsTrue(ok2);
        }

        /// <summary>
        /// Construction d'un collection à partir d'une commande.
        /// </summary>
        [Test]
        public void ParseCommandFromCollectionTest() {
            BeanTest BeanTest = null;
            List<BeanTest> list = new List<BeanTest>();
            BeanTest = new BeanTest();
            BeanTest.Id = 1;
            BeanTest.Name = "Name1";
            list.Add(BeanTest);

            BeanTest = new BeanTest();
            BeanTest.Id = 2;
            BeanTest.Name = "Name2";
            list.Add(BeanTest);

            ICollection<BeanTest> result = CollectionBuilder<BeanTest, BeanTest>.ParseCommand(
                    null, new Kinetix.Data.SqlClient.Test.TestDbCommand(list));

            bool ok1 = false;
            bool ok2 = false;
            Assert.AreEqual(2, result.Count);
            foreach (BeanTest b in result) {
                if (b.Id == 1) {
                    ok1 = true;
                    Assert.AreEqual("Name1", b.Name);
                } else if (b.Id == 2) {
                    ok2 = true;
                    Assert.AreEqual("Name2", b.Name);
                }
                Assert.IsNull(b.OtherAttribut);
            }
            Assert.IsTrue(ok1);
            Assert.IsTrue(ok2);
        }

        /// <summary>
        /// Construction d'une object à partir d'une commande.
        /// </summary>
        [Test]
        public void ParseCommandForSingleObjectTest() {
            BeanTest BeanTest = null;
            List<BeanTest> list = new List<BeanTest>();
            BeanTest = new BeanTest();
            BeanTest.Id = 1;
            BeanTest.Name = "Name1";
            list.Add(BeanTest);

            BeanTest = CollectionBuilder<BeanTest>.ParseCommandForSingleObject(
                null,
                    new Kinetix.Data.SqlClient.Test.TestDbCommand(list));

            Assert.AreEqual(1, BeanTest.Id);
            Assert.AreEqual("Name1", BeanTest.Name);
            Assert.IsNull(BeanTest.OtherAttribut);
        }

        /// <summary>
        /// Construction d'une object à partir d'une commande retournant plusieurs lignes.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CollectionBuilderException))]
        public void ParseCommandForSingleObjectFailTest() {
            BeanTest BeanTest = null;
            List<BeanTest> list = new List<BeanTest>();
            BeanTest = new BeanTest();
            BeanTest.Id = 1;
            BeanTest.Name = "Name1";
            list.Add(BeanTest);

            BeanTest = new BeanTest();
            BeanTest.Id = 2;
            BeanTest.Name = "Name2";
            list.Add(BeanTest);

            CollectionBuilder<BeanTest>.ParseCommandForSingleObject(null, new TestDbCommand(list));
        }

        /// <summary>
        /// Construction d'une object à partir d'une commande retournant 0 lignes.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CollectionBuilderException))]
        public void ParseCommandForSingleObjectEmptyFailTest() {
            CollectionBuilder<BeanTest>.ParseCommandForSingleObject(null, new TestDbCommand(new List<BeanTest>()));
        }
    }
}
