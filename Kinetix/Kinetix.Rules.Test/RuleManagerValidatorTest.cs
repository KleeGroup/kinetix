using Kinetix.Test;
#if NUnit
    using NUnit.Framework; 
#else

using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Rules.Test
{
    [TestClass]
    class RuleManagerValidatorTest : MemoryBaseTest
    {

        [TestMethod]
        public void TestAddRule()
        {

        }

        [TestMethod]
        public void TestAddUpdateDelete()
        {

        }

        [TestMethod]
        public void TestValidationOneRuleOneCondition()
        {

        }

        /// <summary>
        /// No rule for RulesManager
        /// </summary>
        [TestMethod]
        public void TestValidationNoRuleNoCondition()
        {

        }

        /// <summary>
        /// Combining many condition in one rule for RulesManager
        /// </summary>
        [TestMethod]
        public void TestValidationOneRuleManyCondition()
        {

        }

        /// <summary>
        /// Combining many rules with one condition for RulesManager
        /// </summary>
        [TestMethod]
        public void TestValidationManyRulesOneCondition()
        {

        }

        /// <summary>
        /// Combining many rules with many rules for RulesManager
        /// </summary>
        [TestMethod]
        public void TestValidationManyRulesManyCondition()
        {

        }

    }
}
