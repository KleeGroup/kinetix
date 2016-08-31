using System;
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
    /// Classe de test du FilterCriteria.
    /// </summary>
    [TestFixture]
    public class FilterCriteriaTest {
        /// <summary>
        /// Test le constructeur par défaut.
        /// </summary>
        [Test]
        public void TestConstructorDefault() {
            FilterCriteria criteria = new FilterCriteria();
        }

        /// <summary>
        /// Test l'échec du constructeur vide.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructorCriteriaNull() {
            FilterCriteria criteria = new FilterCriteria((object)null);
        }

        /// <summary>
        /// Test l'échec du constructeur critèria non vide.
        /// </summary>
        [Test]
        public void TestConstructorCriteria() {
            Bean bean = new Bean();
            bean.Id = 2;
            bean.Name = "123";

            bool hasParameterId = false;
            bool hasParameterName = false;
            object beaIdValue = null;
            object beaNameValue = null;

            FilterCriteria criteria = new FilterCriteria(bean);

            foreach (FilterCriteriaParam parameter in criteria.Parameters) {
                if ("BEA_ID".Equals(parameter.ColumnName)) {
                    hasParameterId = true;
                    beaIdValue = parameter.Value;
                } else if ("BEA_NAME".Equals(parameter.ColumnName)) {
                    hasParameterName = true;
                    beaNameValue = parameter.Value;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsTrue(hasParameterId);
            Assert.IsTrue(hasParameterName);

            Assert.AreEqual(bean.Id, beaIdValue);
            Assert.AreEqual(bean.Name, beaNameValue);
        }

        /// <summary>
        /// Test l'échec du constructeur avec un critère.
        /// </summary>
        [Test]
        public void TestConstructorOneCriteria() {
            FilterCriteria criteria = new FilterCriteria().Equals(Bean.Cols.BEA_ID, 2);
            FilterCriteriaParam param = null;
            foreach (FilterCriteriaParam parameter in criteria.Parameters) {
                if ("BEA_ID".Equals(parameter.ColumnName)) {
                    param = parameter;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsNotNull(param);
            Assert.AreEqual(2, param.Value);
        }

        /// <summary>
        /// Test l'échec du constructeur avec un critère avec like.
        /// </summary>
        [Test]
        public void TestConstructorOneCriteriaWithLike() {
            FilterCriteria criteria = new FilterCriteria().Equals(Bean.Cols.BEA_ID, 2);

            FilterCriteriaParam param = null;
            foreach (FilterCriteriaParam parameter in criteria.Parameters) {
                if ("BEA_ID".Equals(parameter.ColumnName)) {
                    param = parameter;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsNotNull(param);
            Assert.AreEqual(2, param.Value);
        }

        /// <summary>
        /// Test l'ajout de paramètres.
        /// </summary>
        [Test]
        public void TestAddCriteria() {
            FilterCriteria criteria = new FilterCriteria().Equals(Bean.Cols.BEA_ID, 2);

            FilterCriteriaParam param = null;
            foreach (FilterCriteriaParam parameter in criteria.Parameters) {
                if ("BEA_ID".Equals(parameter.ColumnName)) {
                    param = parameter;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsNotNull(param);
            Assert.AreEqual(2, param.Value);
        }

        /// <summary>
        /// Test l'échec de l'ajout d'un paramètre avec une valeur nulle.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestAddCriteriaValueNull() {
            FilterCriteria criteria = new FilterCriteria().Equals(Bean.Cols.BEA_ID, null);
        }
    }
}
