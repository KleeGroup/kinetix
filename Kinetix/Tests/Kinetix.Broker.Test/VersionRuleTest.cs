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
    /// Classe de test de VersionRuleTest.
    /// </summary>
    [TestFixture]
    public class VersionRuleTest {
        /// <summary>
        /// Test la propriété FieldName.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullConstructorParam() {
            VersionRule rule = new VersionRule(null);
        }

        /// <summary>
        /// Test la propriété FieldName.
        /// </summary>
        [Test]
        public void TestFieldName() {
            VersionRule rule = new VersionRule("Field");
            Assert.AreEqual("Field", rule.FieldName);
        }

        /// <summary>
        /// Test la valeur d'insertion.
        /// </summary>
        [Test]
        public void TestInsertValue() {
            VersionRule rule = new VersionRule("Field");
            ValueRule val = rule.GetInsertValue(null);
            Assert.AreEqual(1, val.Value);
            Assert.AreEqual(ActionRule.Update, val.Action);
        }

        /// <summary>
        /// Test la valeur de mise à jour.
        /// </summary>
        [Test]
        public void TestUpdateValue() {
            VersionRule rule = new VersionRule("Field");
            ValueRule val = rule.GetUpdateValue(2);
            Assert.AreEqual(1, val.Value);
            Assert.AreEqual(ActionRule.IncrementalUpdate, val.Action);
        }

        /// <summary>
        /// Test la valeur de la clause Where.
        /// </summary>
        [Test]
        public void TestWhereClause() {
            VersionRule rule = new VersionRule("Field");
            ValueRule val = rule.GetWhereClause(3);
            Assert.AreEqual(3, val.Value);
            Assert.AreEqual(ActionRule.Check, val.Action);
        }
    }
}
