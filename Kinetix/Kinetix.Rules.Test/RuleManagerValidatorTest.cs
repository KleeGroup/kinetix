using Kinetix.Test;
using System.Collections.Generic;
#if NUnit
    using NUnit.Framework; 
#else

using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using Microsoft.Practices.Unity;

namespace Kinetix.Rules.Test
{
    [TestClass]
    public class RuleManagerValidatorTest : MemoryBaseTest
    {

        public override void Register()
        {
            var container = GetConfiguredContainer();

            container.RegisterType<Kinetix.Account.IAccountStorePlugin, Kinetix.Account.MemoryAccountStorePlugin>(new ContainerControlledLifetimeManager());
            container.RegisterType<Kinetix.Account.IAccountManager, Kinetix.Account.AccountManager>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Rules.IRuleManager, Kinetix.Rules.RuleManager>();
            container.RegisterType<Kinetix.Rules.IRuleStorePlugin, Kinetix.Rules.MemoryRuleStorePlugin>(new ContainerControlledLifetimeManager());
            //container.RegisterType<Kinetix.Rules.IRuleStorePlugin, Kinetix.Rules.SqlServerRuleStorePlugin>();
            container.RegisterType<Kinetix.Rules.IRuleConstantsStorePlugin, Kinetix.Rules.MemoryRuleConstantsStore>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Rules.IRuleSelectorPlugin, Kinetix.Rules.SimpleRuleSelectorPlugin>();
            container.RegisterType<Kinetix.Rules.IRuleValidatorPlugin, Kinetix.Rules.SimpleRuleValidatorPlugin>();
        }

        [TestMethod]
        public void TestAddRule()
        {
            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            RuleDefinition rule1 = new RuleDefinition(null, null, 1, "My Rule 1");
            RuleDefinition rule2 = new RuleDefinition(null, null, 2, "My Rule 2");
            RuleDefinition rule3 = new RuleDefinition(null, null, 2, "My Rule 3");

            ruleManager.AddRule(rule1);
            ruleManager.AddRule(rule2);
            ruleManager.AddRule(rule3);

            // Only 1 rule
            IList<RuleDefinition> rulesFetch1 = ruleManager.GetRulesForItemId(1);

            Assert.IsNotNull(rulesFetch1);
            Assert.AreEqual(rulesFetch1.Count, 1);
            Assert.IsTrue(rulesFetch1.Contains(rule1));

            // 2 rules
            IList<RuleDefinition> rulesFetch2 = ruleManager.GetRulesForItemId(2);

            Assert.IsNotNull(rulesFetch2);
            Assert.AreEqual(rulesFetch2.Count, 2);
            CollectionAssert.AreEquivalent(new List<RuleDefinition>() { rule2, rule3 }, (List<RuleDefinition>)rulesFetch2);
        }

        [TestMethod]
        public void TestAddUpdateDelete()
        {
            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            // Rule created to Item 1
            RuleDefinition rule = new RuleDefinition(null, null, 1, "My Rule 1");
            ruleManager.AddRule(rule);

            IList<RuleDefinition> rulesFetch_1_1 = ruleManager.GetRulesForItemId(1);

            Assert.IsNotNull(rulesFetch_1_1);
            Assert.AreEqual(rulesFetch_1_1.Count, 1);
            CollectionAssert.AreEquivalent(new List<RuleDefinition>() { rule }, (List <RuleDefinition>) rulesFetch_1_1);

            // Update rule. This is now associated with Item 2
            rule.ItemId = 2;
            ruleManager.UpdateRule(rule);

            // The rule is not associated to item 1 anymore
            IList<RuleDefinition> rulesFetch_1_0 = ruleManager.GetRulesForItemId(1);

            Assert.IsNotNull(rulesFetch_1_0);
            Assert.AreEqual(rulesFetch_1_0.Count, 0);

            // The rule should be associated with item 2
            IList<RuleDefinition> rulesFetch_2_1 = ruleManager.GetRulesForItemId(2);

            Assert.IsNotNull(rulesFetch_2_1);
            Assert.AreEqual(rulesFetch_2_1.Count, 1);
            CollectionAssert.AreEquivalent(new List<RuleDefinition>() { rule }, (List<RuleDefinition>) rulesFetch_2_1);

            // Update rule. This is now associated with Item 2
            ruleManager.RemoveRule(rule);

            // No rule should be associated with item 2
            IList<RuleDefinition> rulesFetch_2_0 = ruleManager.GetRulesForItemId(2);

            Assert.IsNotNull(rulesFetch_2_0);
            Assert.AreEqual(rulesFetch_2_0.Count, 0);
        }

        [TestMethod]
        public void TestValidationOneRuleOneCondition()
        {
            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            // Rule created to Item 1
            RuleDefinition rule = new RuleDefinition(null, null, 1, "My Rule 1");
            ruleManager.AddRule(rule);

            RuleConditionDefinition condition = new RuleConditionDefinition(null, "Division", "=", "BTL", rule.Id);
            ruleManager.AddCondition(condition);

            MyDummyDtObject myDummyDtObject = new MyDummyDtObject();

            //The division field is null here
            bool isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should NOT be valid here.
            Assert.IsFalse(isValid);

            //The division is set to "BTL" here
            myDummyDtObject.Division = "BTL";
            //The rule should be valid now
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsTrue(isValid);
        }

        /// <summary>
        /// No rule for RulesManager
        /// </summary>
        [TestMethod]
        public void TestValidationNoRuleNoCondition()
        {
            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            // Rule created to Item 1
            MyDummyDtObject myDummyDtObject = new MyDummyDtObject();

            //The division field is null here
            bool isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should NOT be valid here.
            Assert.IsFalse(isValid);

            //The division is set to "BTL" here
            myDummyDtObject.Division = "BTL";
            //The rule should NOT be valid too
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsFalse(isValid);

            //The division is set to "ABC" here
            myDummyDtObject.Division = "ABC";
            //The rule should be valid too
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsFalse(isValid);
        }

        /// <summary>
        /// Combining many condition in one rule for RulesManager
        /// </summary>
        [TestMethod]
        public void TestValidationOneRuleManyCondition()
        {
            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            // Rule created to Item 1
            RuleDefinition rule = new RuleDefinition(null, null, 1, "My Rule 1");
            ruleManager.AddRule(rule);

            RuleConditionDefinition condition1 = new RuleConditionDefinition(null, "Division", "=", "BTL", rule.Id);
            ruleManager.AddCondition(condition1);

            RuleConditionDefinition condition2 = new RuleConditionDefinition(null, "Entity", "=", "ENT_1", rule.Id);
            ruleManager.AddCondition(condition2);

            MyDummyDtObject myDummyDtObject = new MyDummyDtObject();

            //The division field is null here
            bool isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should NOT be valid here.
            Assert.IsFalse(isValid);

            //The division is set to "BTL" here
            myDummyDtObject.Division = "BTL";
            //The rule should NOT be valid (no entity defined)
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsFalse(isValid);

            //The entity is set to "ENT_1" here
            myDummyDtObject.Entity = "ENT_1";
            //The rule should be valid now
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsTrue(isValid);

            //The division is set to "UNKNOWN_ENT" here
            myDummyDtObject.Entity = "UNKNOWN_ENT";
            //The rule should NOT be valid anymore
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsFalse(isValid);
        }

        /// <summary>
        /// Combining many rules with one condition for RulesManager
        /// </summary>
        [TestMethod]
        public void TestValidationManyRulesOneCondition()
        {
            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            // Rule created to Item 1
            RuleDefinition rule_1 = new RuleDefinition(null, null, 1, "My Rule 1");
            ruleManager.AddRule(rule_1);

            RuleConditionDefinition condition_1 = new RuleConditionDefinition(null, "Division", "=", "BTL", rule_1.Id);
            ruleManager.AddCondition(condition_1);

            RuleDefinition rule_2 = new RuleDefinition(null, null, 1, "My Rule 2");
            ruleManager.AddRule(rule_2);

            RuleConditionDefinition condition_2 = new RuleConditionDefinition(null, "Entity", "=", "ENT_1", rule_2.Id);
            ruleManager.AddCondition(condition_2);

            MyDummyDtObject myDummyDtObject = new MyDummyDtObject();

            //The division and entity field are null here
            bool isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should NOT be valid here.
            Assert.IsFalse(isValid);

            //The division is set to "BTL" here
            myDummyDtObject.Division = "BTL";
            //The rule should be valid as it match 1 rule
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsTrue(isValid);

            //The entity is set to "ENT_1" here
            myDummyDtObject.Entity = "ENT_1";
            //The rule should be valid now (2 rules valid)
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsTrue(isValid);

            //The division is set to "UNKNOWN_ENT" here
            myDummyDtObject.Entity = "UNKNOWN_ENT";
            myDummyDtObject.Division = "DIV";
            //The rule should NOT be valid anymore
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsFalse(isValid);
        }

        /// <summary>
        /// Combining many rules with many rules for RulesManager
        /// </summary>
        [TestMethod]
        public void TestValidationManyRulesManyCondition()
        {
            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            // Rule created to Item 1
            RuleDefinition rule_1 = new RuleDefinition(null, null, 1, "My Rule 1");
            ruleManager.AddRule(rule_1);

            RuleConditionDefinition condition_1_1 = new RuleConditionDefinition(null, "Division", "=", "BTL", rule_1.Id);
            ruleManager.AddCondition(condition_1_1);

            RuleConditionDefinition condition_2_1 = new RuleConditionDefinition(null, "Entity", "=", "ENT", rule_1.Id);
            ruleManager.AddCondition(condition_2_1);

            RuleDefinition rule_2 = new RuleDefinition(null, null, 1, "My Rule 2");
            ruleManager.AddRule(rule_2);

            RuleConditionDefinition condition_1_2 = new RuleConditionDefinition(null, "Division", "=", "DIV", rule_2.Id);
            ruleManager.AddCondition(condition_1_2);

            RuleConditionDefinition condition_2_2 = new RuleConditionDefinition(null, "Entity", "=", "MAR", rule_2.Id);
            ruleManager.AddCondition(condition_2_2);

            MyDummyDtObject myDummyDtObject = new MyDummyDtObject();

            //The division and entity field are null here
            bool isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should be valid here.
            Assert.IsFalse(isValid);

            //The division is set to "BTL" here
            myDummyDtObject.Division = "BTL";
            //The rule should NOT be valid as no rule match
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsFalse(isValid);

            //The entity is set to "MAR" here
            myDummyDtObject.Entity = "MAR";
            //The rule should NOT be valid (only one condition in each rules is valid)
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsFalse(isValid);

            //The entity is set to "ENT" here
            myDummyDtObject.Entity = "ENT";
            //The rule should be valid (match all conditions of rule 1)
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsTrue(isValid);

            //The division is set to "UNKNOWN_ENT" here
            myDummyDtObject.Entity = "UNKNOWN_ENT";
            myDummyDtObject.Division = "DIV";
            //The rule should NOT be valid anymore
            isValid = ruleManager.IsRuleValid(1, myDummyDtObject, RuleConstants.EmptyRuleConstants);
            Assert.IsFalse(isValid);
        }

    }
}
