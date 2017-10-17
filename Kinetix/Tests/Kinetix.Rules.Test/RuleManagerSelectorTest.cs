using System;
using Kinetix.Test;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Kinetix.Account.Account;
using Kinetix.Account;
using System.Security.Principal;
using System.Configuration;
using Kinetix.Broker;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
namespace Kinetix.Rules.Test
{
    [TestClass]
    public class RuleManagerSelectorTest : UnityBaseTest
    {

        private readonly string DefaultDataSource = "default";

        public override void Register()
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

            var container = GetConfiguredContainer();

            container.RegisterType<Kinetix.Account.IAccountStorePlugin, Kinetix.Account.MemoryAccountStorePlugin>(new ContainerControlledLifetimeManager());
            container.RegisterType<Kinetix.Account.IAccountManager, Kinetix.Account.AccountManager>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Rules.IRuleManager, Kinetix.Rules.RuleManager>();
            //container.RegisterType<Kinetix.Rules.IRuleStorePlugin, Kinetix.Rules.MemoryRuleStorePlugin>(new ContainerControlledLifetimeManager());
            container.RegisterType<Kinetix.Rules.IRuleStorePlugin, Kinetix.Rules.SqlServerRuleStorePlugin>();
            container.RegisterType<Kinetix.Rules.IRuleConstantsStorePlugin, Kinetix.Rules.MemoryRuleConstantsStore>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Rules.IRuleSelectorPlugin, Kinetix.Rules.SimpleRuleSelectorPlugin>();
            container.RegisterType<Kinetix.Rules.IRuleValidatorPlugin, Kinetix.Rules.SimpleRuleValidatorPlugin>();

        }


        /// <summary>
        /// Add/Find Rules for RulesManager
        /// </summary>
        [Test]
        public void TestAddRule()
        {
            int item1 = 10000;
            int item2 = 20000;

            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            DateTime now = DateTime.Now;
            SelectorDefinition rule1 = new SelectorDefinition(null, now, item1, "1");
            SelectorDefinition rule2 = new SelectorDefinition(null, now, item2, "2");
            SelectorDefinition rule3 = new SelectorDefinition(null, now, item2, "3");

            ruleManager.AddSelector(rule1);
            ruleManager.AddSelector(rule2);
            ruleManager.AddSelector(rule3);

            // Only 1 rule
            IList<SelectorDefinition> selectorFetch1 = ruleManager.GetSelectorsForItemId(item1);

            Assert.IsNotNull(selectorFetch1);
            Assert.AreEqual(selectorFetch1.Count, 1);
            Assert.IsTrue(selectorFetch1.SequenceEqual(new List<SelectorDefinition>() { rule1 }, new SelectorEqualityComparer()));

            // 2 rules
            IList<SelectorDefinition> selectorFetch2 = ruleManager.GetSelectorsForItemId(item2);

            Assert.IsNotNull(selectorFetch2);
            Assert.AreEqual(selectorFetch2.Count, 2);
            Assert.IsTrue(selectorFetch2.SequenceEqual(new List<SelectorDefinition>() { rule2, rule3 }, new SelectorEqualityComparer()));
        }


        /// <summary>
        /// Add/Update/Delete Rules for RulesManager
        /// </summary>
        [Test]
        public void TestAddUpdateDelete()
        {
            int item1 = 10000;
            int item2 = 20000;

            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            // Rule created to Item 1
            SelectorDefinition selector = new SelectorDefinition(null, DateTime.Now, item1, "1");
            ruleManager.AddSelector(selector);

            IList<SelectorDefinition> rulesFetch_1_1 = ruleManager.GetSelectorsForItemId(item1);

            Assert.IsNotNull(rulesFetch_1_1);
            Assert.AreEqual(rulesFetch_1_1.Count, 1);
            Assert.IsTrue(rulesFetch_1_1.SequenceEqual(new List<SelectorDefinition>() { selector }, new SelectorEqualityComparer()));

            // Update rule. This is now associated with Item 2
            selector.ItemId = item2;
            ruleManager.UpdateSelector(selector);

            // The rule is not associated to item 1 anymore
            IList<SelectorDefinition> rulesFetch_1_0 = ruleManager.GetSelectorsForItemId(item1);

            Assert.IsNotNull(rulesFetch_1_0);
            Assert.AreEqual(rulesFetch_1_0.Count, 0);

            // The rule should be associated with item 2
            IList<SelectorDefinition> rulesFetch_2_1 = ruleManager.GetSelectorsForItemId(item2);

            Assert.IsNotNull(rulesFetch_2_1);
            Assert.AreEqual(rulesFetch_2_1.Count, 1);
            Assert.IsTrue(rulesFetch_2_1.SequenceEqual(new List<SelectorDefinition>() { selector }, new SelectorEqualityComparer()));

            // Update rule. This is now associated with Item 2
            ruleManager.RemoveSelector(selector);

            // No rule should be associated with item 2
            IList<RuleDefinition> rulesFetch_2_0 = ruleManager.GetRulesForItemId(item2);

            Assert.IsNotNull(rulesFetch_2_0);
            Assert.AreEqual(rulesFetch_2_0.Count, 0);
        }


        /// <summary>
        /// Add/Update/Delete Rules for RulesManager
        /// </summary>
        [Test]
        public void TestDeleteSelectorsByGroupIds()
        {
            string groupIdToDelete = "10000";
            string groupIdToKeep = "20000";

            int item1 = 10000;
            int item2 = 20000;
            int item3 = 30000;

            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();

            // Rule created to Item 1
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, item1, groupIdToDelete);
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, item1, groupIdToDelete);
            SelectorDefinition selector3 = new SelectorDefinition(null, DateTime.Now, item2, groupIdToDelete);
            SelectorDefinition selector4 = new SelectorDefinition(null, DateTime.Now, item1, groupIdToKeep);
            SelectorDefinition selector5 = new SelectorDefinition(null, DateTime.Now, item2, groupIdToKeep);
            SelectorDefinition selector6 = new SelectorDefinition(null, DateTime.Now, item3, groupIdToKeep);
            ruleManager.AddSelector(selector1);
            ruleManager.AddSelector(selector2);
            ruleManager.AddSelector(selector3);
            ruleManager.AddSelector(selector4);
            ruleManager.AddSelector(selector5);
            ruleManager.AddSelector(selector6);

            IList<SelectorDefinition> rulesFetch_1_1 = ruleManager.GetSelectorsForItemId(item1);

            Assert.IsNotNull(rulesFetch_1_1);
            Assert.AreEqual(3, rulesFetch_1_1.Count);
            Assert.IsTrue(rulesFetch_1_1.SequenceEqual(new List<SelectorDefinition>() { selector1, selector2, selector4 }, new SelectorEqualityComparer()));

            IList<SelectorDefinition> rulesFetch_1_2 = ruleManager.GetSelectorsForItemId(item2);

            Assert.IsNotNull(rulesFetch_1_2);
            Assert.AreEqual(2, rulesFetch_1_2.Count);
            Assert.IsTrue(rulesFetch_1_2.SequenceEqual(new List<SelectorDefinition>() { selector3, selector5 }, new SelectorEqualityComparer()));

            IList<SelectorDefinition> rulesFetch_1_3 = ruleManager.GetSelectorsForItemId(item3);

            Assert.IsNotNull(rulesFetch_1_3);
            Assert.AreEqual(1, rulesFetch_1_3.Count);
            Assert.IsTrue(rulesFetch_1_3.SequenceEqual(new List<SelectorDefinition>() { selector6 }, new SelectorEqualityComparer()));

            // Update rule. This is now associated with Item 2
            ruleManager.RemoveSelectorsFiltersByGroupId(groupIdToDelete);

            IList<SelectorDefinition> rulesFetch_2_1 = ruleManager.GetSelectorsForItemId(item1);

            Assert.IsNotNull(rulesFetch_2_1);
            Assert.AreEqual(1, rulesFetch_2_1.Count);
            Assert.IsTrue(rulesFetch_2_1.SequenceEqual(new List<SelectorDefinition>() { selector4 }, new SelectorEqualityComparer()));

            IList<SelectorDefinition> rulesFetch_2_2 = ruleManager.GetSelectorsForItemId(item2);

            Assert.IsNotNull(rulesFetch_2_2);
            Assert.AreEqual(1, rulesFetch_2_2.Count);
            Assert.IsTrue(rulesFetch_2_2.SequenceEqual(new List<SelectorDefinition>() { selector5 }, new SelectorEqualityComparer()));

            IList<SelectorDefinition> rulesFetch_2_3 = ruleManager.GetSelectorsForItemId(item3);

            Assert.IsNotNull(rulesFetch_2_3);
            Assert.AreEqual(1, rulesFetch_2_3.Count);
            Assert.IsTrue(rulesFetch_2_3.SequenceEqual(new List<SelectorDefinition>() { selector6 }, new SelectorEqualityComparer()));

        }

        /// <summary>
        /// One simple selector for RulesManager
        /// </summary>
        [Test]
        public void TestValidationOneSelectorOneFilter()
        {
            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();
            IAccountManager accountManager = container.Resolve<IAccountManager>();

            int actId1 = 10000;

            AccountGroup accountGroup = new AccountGroup("1", "Group activity 1");
            AccountUser account = new AccountUserBuilder("0").WithDisplayName("User 1")
                    .WithEmail("user1@account.vertigo.io")
                    .Build();
            accountManager.GetStore().SaveGroup(accountGroup);
            accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Selector created to Item 1
            SelectorDefinition selector = new SelectorDefinition(null, DateTime.Now, actId1, accountGroup.Id);
            ruleManager.AddSelector(selector);

            RuleFilterDefinition filterDefinition = new RuleFilterDefinition(null, "Division", "=", "BTL", selector.Id);
            ruleManager.AddFilter(filterDefinition);

            MyDummyDtObject myDummyDtObject = new MyDummyDtObject();
            myDummyDtObject.Division = "BTL";
            RuleContext ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            IList<AccountUser> selectedAccounts = ruleManager.SelectAccounts(actId1, ruleContext);

            Assert.IsNotNull(selectedAccounts);
            Assert.AreEqual(1, selectedAccounts.Count);
            Assert.IsTrue(selectedAccounts.SequenceEqual(new List<AccountUser>() { account }, new AccountEqualityComparer()));
        }

        /// <summary>
        /// One simple selector with many filters for RulesManager
        /// </summary>
        [Test]
        public void TestValidationOneSelectorManyFilters()
        {
            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();
            IAccountManager accountManager = container.Resolve<IAccountManager>();

            int actId1 = 10000;

            AccountGroup accountGroup = new AccountGroup("1", "Group activity 1");
            AccountUser account = new AccountUserBuilder("0")
                    .WithDisplayName("User 1")
                    .WithEmail("user1@account.vertigo.io")
                    .Build();

            accountManager.GetStore().SaveGroup(accountGroup);
            accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Selector created to Item 1
            SelectorDefinition selector_1 = new SelectorDefinition(null, DateTime.Now, actId1, accountGroup.Id);
            ruleManager.AddSelector(selector_1);

            RuleFilterDefinition filterDefinition_1_1 = new RuleFilterDefinition(null, "Division", "=", "BTL", selector_1.Id);
            ruleManager.AddFilter(filterDefinition_1_1);

            RuleFilterDefinition filterDefinition_1_2 = new RuleFilterDefinition(null, "Entity", "=", "ENT", selector_1.Id);
            ruleManager.AddFilter(filterDefinition_1_2);

            MyDummyDtObject myDummyDtObject = new MyDummyDtObject();
            myDummyDtObject.Division = "BTL";

            RuleContext ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);

            // The entity is not set to ENT. The selector should not match
            IList<AccountUser> selectedAccounts_1 = ruleManager.SelectAccounts(actId1, ruleContext);

            Assert.IsNotNull(selectedAccounts_1);
            Assert.AreEqual(selectedAccounts_1.Count, 0);

            //We set the entity to 'ENT'
            myDummyDtObject.Entity = "ENT";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            // The selector should match now.
            IList<AccountUser> selectedAccounts_2 = ruleManager.SelectAccounts(actId1, ruleContext);

            Assert.IsNotNull(selectedAccounts_2);
            Assert.AreEqual(selectedAccounts_2.Count, 1);
            Assert.IsTrue(selectedAccounts_2.SequenceEqual(new List<AccountUser>() { account }, new AccountEqualityComparer()));

            //We set the entity to 'XXX'
            myDummyDtObject.Entity = "XXX";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            // The selector should not match .
            IList<AccountUser> selectedAccounts_3 = ruleManager.SelectAccounts(actId1, ruleContext);

            Assert.IsNotNull(selectedAccounts_3);
            Assert.AreEqual(selectedAccounts_3.Count, 0);
        }

        /// <summary>
        /// Many selectors with many filters for RulesManager
        /// </summary>
        [Test]
        public void TestValidationManySelectorsManyFilters()
        {
            var container = GetConfiguredContainer();
            IRuleManager ruleManager = container.Resolve<IRuleManager>();
            IAccountManager accountManager = container.Resolve<IAccountManager>();

            int actId1 = 10000;

            AccountGroup accountGroup_1 = new AccountGroup("1", "Group activity 1");

            AccountUser account_1_1 = new AccountUserBuilder("0")
                    .WithDisplayName("User 1 Group 1")
                    .WithEmail("user1@account.vertigo.io")
                    .Build();

            AccountUser account_1_2 = new AccountUserBuilder("1")
                    .WithDisplayName("User 2 Group 1")
                    .WithEmail("user1@account.vertigo.io")
                    .Build();

            accountManager.GetStore().SaveGroup(accountGroup_1);
            accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account_1_1, account_1_2 });
            accountManager.GetStore().Attach(account_1_1.Id, accountGroup_1.Id);
            accountManager.GetStore().Attach(account_1_2.Id, accountGroup_1.Id);

            AccountGroup accountGroup_2 = new AccountGroup("2", "Group activity 2");

            AccountUser account_2_1 = new AccountUserBuilder("2")
                    .WithDisplayName("User 1 Group 2")
                    .WithEmail("user1@account.vertigo.io")
                    .Build();

            AccountUser account_2_2 = new AccountUserBuilder("3")
                    .WithDisplayName("User 2 Group 2")
                    .WithEmail("user1@account.vertigo.io")
                    .Build();

            accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account_2_1, account_2_2 });
            accountManager.GetStore().SaveGroup(accountGroup_2);
            accountManager.GetStore().Attach(account_2_1.Id, accountGroup_2.Id);
            accountManager.GetStore().Attach(account_2_2.Id, accountGroup_2.Id);

            // Selector 1 created to Item 1
            SelectorDefinition selector_1 = new SelectorDefinition(null, DateTime.Now, actId1, accountGroup_1.Id);
            ruleManager.AddSelector(selector_1);

            RuleFilterDefinition filterDefinition_1_1 = new RuleFilterDefinition(null, "Division", "=", "BTL", selector_1.Id);
            ruleManager.AddFilter(filterDefinition_1_1);

            RuleFilterDefinition filterDefinition_1_2 = new RuleFilterDefinition(null, "Entity", "=", "ENT", selector_1.Id);
            ruleManager.AddFilter(filterDefinition_1_2);

            // Selector 2 created to Item 1
            SelectorDefinition selector_2 = new SelectorDefinition(null, DateTime.Now, actId1, accountGroup_2.Id);
            ruleManager.AddSelector(selector_2);

            RuleFilterDefinition filterDefinition_2_1 = new RuleFilterDefinition(null, "Division", "=", "BTL", selector_2.Id);
            ruleManager.AddFilter(filterDefinition_2_1);

            RuleFilterDefinition filterDefinition_2_2 = new RuleFilterDefinition(null, "Nom", "=", "DONALD", selector_2.Id);
            ruleManager.AddFilter(filterDefinition_2_2);

            // 
            MyDummyDtObject myDummyDtObject = new MyDummyDtObject();
            myDummyDtObject.Division = "BTL";
            RuleContext ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);

            // The entity only has entity set to ENT. No selectors should match
            IList<AccountUser> selectedAccounts_1 = ruleManager.SelectAccounts(actId1, ruleContext);

            Assert.IsNotNull(selectedAccounts_1);
            Assert.AreEqual(selectedAccounts_1.Count, 0);

            // Set entity to ENT
            myDummyDtObject.Entity = "ENT";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            // Only Group 1 should match 
            IList<AccountUser> selectedAccounts_2 = ruleManager.SelectAccounts(actId1, ruleContext);

            Assert.IsNotNull(selectedAccounts_2);
            Assert.AreEqual(selectedAccounts_2.Count, 2);
            Assert.IsTrue(selectedAccounts_2.SequenceEqual(new List<AccountUser>() { account_1_1, account_1_2 }, new AccountEqualityComparer()));

            // Set entity to XXX
            myDummyDtObject.Entity = "XXX";
            myDummyDtObject.Nom = "DONALD";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            // Only Group 2 should match 
            IList<AccountUser> selectedAccounts_3 = ruleManager.SelectAccounts(actId1, ruleContext);

            Assert.IsNotNull(selectedAccounts_3);
            Assert.AreEqual(selectedAccounts_3.Count, 2);
            Assert.IsTrue(selectedAccounts_3.SequenceEqual(new List<AccountUser>() { account_2_1, account_2_2 }, new AccountEqualityComparer()));

            // Set entity to ENT
            myDummyDtObject.Entity = "ENT";
            ruleContext = new RuleContext(myDummyDtObject, RuleConstants.EmptyRuleConstants);
            // Group 1 and Group 2 should match 
            IList<AccountUser> selectedAccounts_4 = ruleManager.SelectAccounts(actId1, ruleContext);

            Assert.IsNotNull(selectedAccounts_4);
            Assert.AreEqual(selectedAccounts_4.Count, 4);
            Assert.IsTrue(selectedAccounts_4.SequenceEqual(new List<AccountUser>() { account_1_1, account_1_2, account_2_1, account_2_2 }, new AccountEqualityComparer()));

        }



        
    }

}
