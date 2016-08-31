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
    /// Classe de test de ModificationDateRuleTest.
    /// </summary>
    [TestFixture]
    public class ModificationDateRuleTest {
        /// <summary>
        /// Test la propriété FieldName.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullConstructorParam() {
            ModificationDateRule rule = new ModificationDateRule(null);
        }

        /// <summary>
        /// Test la propriété FieldName.
        /// </summary>
        [Test]
        public void TestFieldName() {
            ModificationDateRule rule = new ModificationDateRule("Field");
            Assert.AreEqual("Field", rule.FieldName);
        }

        /// <summary>
        /// Test la valeur d'insertion.
        /// </summary>
        [Test]
        public void TestInsertValue() {
            ModificationDateRule rule = new ModificationDateRule("Field");
            ValueRule val = rule.GetInsertValue(null);
            Assert.IsNotNull(val.Value);
            Assert.AreEqual(ActionRule.Update, val.Action);
        }

        /// <summary>
        /// Test la valeur de mise à jour.
        /// </summary>
        [Test]
        public void TestUpdateValue() {
            ModificationDateRule rule = new ModificationDateRule("Field");
            ValueRule val = rule.GetUpdateValue(null);
            Assert.IsNotNull(val.Value);
            Assert.AreEqual(ActionRule.Update, val.Action);
        }

        /// <summary>
        /// Test la valeur de la clause Where.
        /// </summary>
        [Test]
        public void TestWhereClause() {
            ModificationDateRule rule = new ModificationDateRule("Field");
            ValueRule val = rule.GetWhereClause(null);
            Assert.IsNull(val.Value);
            Assert.AreEqual(ActionRule.DoNothing, val.Action);
        }
    }
}
