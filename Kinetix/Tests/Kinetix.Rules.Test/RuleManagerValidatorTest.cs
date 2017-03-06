using Kinetix.Test;
using System.Collections.Generic;
using System.Security.Principal;
using System.Configuration;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;
using Kinetix.Broker;
using System.Linq;
#if NUnit
    using NUnit.Framework; 
#else

using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using Microsoft.Practices.Unity;

using System;

namespace Kinetix.Rules.Test
{
    [TestClass]
    public class RuleManagerValidatorTest : UnityBaseTest
    {
        private readonly string DefaultDataSource = "default";

        private readonly bool SqlServer = true;

        public override void Register()
        {

            if (SqlServer)
            {
                /* Utilise une base de données spécifique si on est dans la build TFS. */
                WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
                bool isTfs = currentUser.Name == @"KLEE\TFSBUILD";
                string dataBaseName = isTfs ? "DianeTfs" : "DianeDev";

                // Connection string.
                ConnectionStringSettings conn = new ConnectionStringSettings
                {
                    Name = DefaultDataSource,
                    ConnectionString = "Data Source=carla;Initial Catalog=" + dataBaseName + ";User ID=dianeConnection;Password=Puorgeelk23",
                    ProviderName = "System.Data.SqlClient"
                };

                // Register Domain metadata in Domain manager.
                DomainManager.Instance.RegisterDomainMetadataType(typeof(RuleDomainMetadata));
                SqlServerManager.Instance.RegisterConnectionStringSettings(conn);
                BrokerManager.RegisterDefaultDataSource(DefaultDataSource);
                BrokerManager.Instance.RegisterStore(DefaultDataSource, typeof(SqlServerStore<>));
            }
            var container = GetConfiguredContainer();

            container.RegisterType<Kinetix.Account.IAccountStorePlugin, Kinetix.Account.MemoryAccountStorePlugin>(new ContainerControlledLifetimeManager());
            container.RegisterType<Kinetix.Account.IAccountManager, Kinetix.Account.AccountManager>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Rules.IRuleManager, Kinetix.Rules.RuleManager>();

            if (SqlServer)
            {
                container.RegisterType<Kinetix.Rules.IRuleStorePlugin, Kinetix.Rules.SqlServerRuleStorePlugin>();
            } else
            {
                container.RegisterType<Kinetix.Rules.IRuleStorePlugin, Kinetix.Rules.MemoryRuleStorePlugin>(new ContainerControlledLifetimeManager());
            }

            container.RegisterType<Kinetix.Rules.IRuleConstantsStorePlugin, Kinetix.Rules.MemoryRuleConstantsStore>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Rules.IRuleSelectorPlugin, Kinetix.Rules.SimpleRuleSelectorPlugin>();
            container.RegisterType<Kinetix.Rules.IRuleValidatorPlugin, Kinetix.Rules.SimpleRuleValidatorPlugin>();
        }

        [TestMethod]
        public void TestAddRule()
        {
            int item1 = 10000;
            int item2 = 20000;

            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            DateTime now = DateTime.Now;
            RuleDefinition rule1 = new RuleDefinition(null, now, item1, "My Rule 1");
            RuleDefinition rule2 = new RuleDefinition(null, now, item2, "My Rule 2");
            RuleDefinition rule3 = new RuleDefinition(null, now, item2, "My Rule 3");

            ruleManager.AddRule(rule1);
            ruleManager.AddRule(rule2);
            ruleManager.AddRule(rule3);

            // Only 1 rule
            IList<RuleDefinition> rulesFetch1 = ruleManager.GetRulesForItemId(item1);

            Assert.IsNotNull(rulesFetch1);
            Assert.AreEqual(rulesFetch1.Count, 1);

            Assert.IsTrue(rulesFetch1.SequenceEqual(new List<RuleDefinition>() { rule1 }, new RuleEqualityComparer()));

            // 2 rules
            IList<RuleDefinition> rulesFetch2 = ruleManager.GetRulesForItemId(item2);

            Assert.IsNotNull(rulesFetch2);
            Assert.AreEqual(rulesFetch2.Count, 2);
            Assert.IsTrue(rulesFetch2.SequenceEqual(new List<RuleDefinition>() { rule2, rule3 }, new RuleEqualityComparer()));
        }

        [TestMethod]
        public void TestAddUpdateDelete()
        {
            int item1 = 10000;
            int item2 = 20000;
            
            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            // Rule created to Item 1
            RuleDefinition rule = new RuleDefinition(null, null, item1, "My Rule 1");
            ruleManager.AddRule(rule);

            IList<RuleDefinition> rulesFetch_1_1 = ruleManager.GetRulesForItemId(item1);

            Assert.IsNotNull(rulesFetch_1_1);
            Assert.AreEqual(1, rulesFetch_1_1.Count);
            Assert.IsTrue(rulesFetch_1_1.SequenceEqual(new List<RuleDefinition>() { rule }, new RuleEqualityComparer()));

            // Update rule. This is now associated with Item 2
            rule.ItemId = item2;
            ruleManager.UpdateRule(rule);

            // The rule is not associated to item 1 anymore
            IList<RuleDefinition> rulesFetch_1_0 = ruleManager.GetRulesForItemId(item1);

            Assert.IsNotNull(rulesFetch_1_0);
            Assert.AreEqual(0, rulesFetch_1_0.Count);

            // The rule should be associated with item 2
            IList<RuleDefinition> rulesFetch_2_1 = ruleManager.GetRulesForItemId(item2);

            Assert.IsNotNull(rulesFetch_2_1);
            Assert.AreEqual(1, rulesFetch_2_1.Count);
            Assert.IsTrue(rulesFetch_2_1.SequenceEqual(new List<RuleDefinition>() { rule }, new RuleEqualityComparer()));

            // Update rule. This is now associated with Item 2
            ruleManager.RemoveRule(rule);

            // No rule should be associated with item 2
            IList<RuleDefinition> rulesFetch_2_0 = ruleManager.GetRulesForItemId(item2);

            Assert.IsNotNull(rulesFetch_2_0);
            Assert.AreEqual(0, rulesFetch_2_0.Count);
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
            RuleContext ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);

            //The division field is null here
            bool isValid = ruleManager.IsRuleValid(1, ruleContext);
            //The rule should NOT be valid here.
            Assert.IsFalse(isValid);

            //The division is set to "BTL" here
            myDummyDtObject.Division = "BTL";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should be valid now
            isValid = ruleManager.IsRuleValid(1, ruleContext);
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
            RuleContext ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);

            //The division field is null here
            bool isValid = ruleManager.IsRuleValid(1, ruleContext);
            //The rule should NOT be valid here.
            Assert.IsFalse(isValid);

            //The division is set to "BTL" here
            myDummyDtObject.Division = "BTL";
            //The rule should NOT be valid too
            isValid = ruleManager.IsRuleValid(1, ruleContext);
            Assert.IsFalse(isValid);

            //The division is set to "ABC" here
            myDummyDtObject.Division = "ABC";
            //The rule should be valid too
            isValid = ruleManager.IsRuleValid(1, ruleContext);
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
            RuleContext ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);

            //The division field is null here
            bool isValid = ruleManager.IsRuleValid(1, ruleContext);
            //The rule should NOT be valid here.
            Assert.IsFalse(isValid);

            //The division is set to "BTL" here
            myDummyDtObject.Division = "BTL";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should NOT be valid (no entity defined)
            isValid = ruleManager.IsRuleValid(1, ruleContext);
            Assert.IsFalse(isValid);

            //The entity is set to "ENT_1" here
            myDummyDtObject.Entity = "ENT_1";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should be valid now
            isValid = ruleManager.IsRuleValid(1, ruleContext);
            Assert.IsTrue(isValid);

            //The division is set to "UNKNOWN_ENT" here
            myDummyDtObject.Entity = "UNKNOWN_ENT";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should NOT be valid anymore
            isValid = ruleManager.IsRuleValid(1, ruleContext);
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
            RuleContext ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);

            //The division and entity field are null here
            bool isValid = ruleManager.IsRuleValid(1, ruleContext);
            //The rule should NOT be valid here.
            Assert.IsFalse(isValid);

            //The division is set to "BTL" here
            myDummyDtObject.Division = "BTL";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should be valid as it match 1 rule
            isValid = ruleManager.IsRuleValid(1, ruleContext);
            Assert.IsTrue(isValid);

            //The entity is set to "ENT_1" here
            myDummyDtObject.Entity = "ENT_1";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should be valid now (2 rules valid)
            isValid = ruleManager.IsRuleValid(1, ruleContext);
            Assert.IsTrue(isValid);

            //The division is set to "UNKNOWN_ENT" here
            myDummyDtObject.Entity = "UNKNOWN_ENT";
            myDummyDtObject.Division = "DIV";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should NOT be valid anymore
            isValid = ruleManager.IsRuleValid(1, ruleContext);
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
            RuleContext ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);

            //The division and entity field are null here
            bool isValid = ruleManager.IsRuleValid(1, ruleContext);
            //The rule should be valid here.
            Assert.IsFalse(isValid);

            //The division is set to "BTL" here
            myDummyDtObject.Division = "BTL";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should NOT be valid as no rule match
            isValid = ruleManager.IsRuleValid(1, ruleContext);
            Assert.IsFalse(isValid);

            //The entity is set to "MAR" here
            myDummyDtObject.Entity = "MAR";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should NOT be valid (only one condition in each rules is valid)
            isValid = ruleManager.IsRuleValid(1, ruleContext);
            Assert.IsFalse(isValid);

            //The entity is set to "ENT" here
            myDummyDtObject.Entity = "ENT";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should be valid (match all conditions of rule 1)
            isValid = ruleManager.IsRuleValid(1, ruleContext);
            Assert.IsTrue(isValid);

            //The division is set to "UNKNOWN_ENT" here
            myDummyDtObject.Entity = "UNKNOWN_ENT";
            myDummyDtObject.Division = "DIV";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            //The rule should NOT be valid anymore
            isValid = ruleManager.IsRuleValid(1, ruleContext);
            Assert.IsFalse(isValid);
        }

    }
}
