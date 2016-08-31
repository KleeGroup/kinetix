using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using Kinetix.Data.SqlClient;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Classe de test du SortParam.
    /// </summary>
    [TestFixture]
    public class SortParamTest {

        /// <summary>
        /// Test le constructeur avec Like.
        /// </summary>
        [Test]
        public void TestConstructorParam() {
            QueryParameter param = new QueryParameter(Bean.Cols.BEA_ID, SortOrder.Desc);
            Assert.AreEqual(1, param.SortedFields.Count);
            Assert.AreEqual(SortOrder.Desc, param["BEA_ID"]);
        }

        /// <summary>
        /// Test le constructeur avec Like.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructorParamNull() {
            QueryParameter param = new QueryParameter(null, SortOrder.Desc);
        }

        /// <summary>
        /// Test l'échec du constructeur vide.
        /// </summary>
        [Test]
        public void TestAddSortParam() {
            QueryParameter param = new QueryParameter(Bean.Cols.BEA_ID, SortOrder.Desc);
            Assert.AreEqual(1, param.SortedFields.Count);
            Assert.AreEqual(SortOrder.Desc, param["BEA_ID"]);
        }

        /// <summary>
        /// Test l'échec du constructeur vide.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestAddSortParamNull() {
            QueryParameter param = new QueryParameter(null, SortOrder.Desc);
        }
    }
}
