using System;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using Kinetix.Test;
using Microsoft.Practices.Unity;
using Kinetix.Workflow.model;
using Kinetix.Rules;
using Kinetix.Workflow.instance;
using System.Collections.Generic;
using Kinetix.Account;
using Kinetix.Account.Account;
using System.Diagnostics;
using System.Security.Principal;
using System.Configuration;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;
using Kinetix.Broker;
using System.Globalization;

namespace Kinetix.Workflow.Test
{
    [TestClass]
    public class WorkflowManagerTest : UnityBaseTest
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
                    // TODO: Change hard coded data-source
                    ConnectionString = "Data Source=carla;Initial Catalog=" + dataBaseName + ";User ID=dianeConnection;Password=Puorgeelk23",
                    ProviderName = "System.Data.SqlClient"
                };

                // Register Domain metadata in Domain manager.
                DomainManager.Instance.RegisterDomainMetadataType(typeof(RuleDomainMetadata));
                DomainManager.Instance.RegisterDomainMetadataType(typeof(WfDomainMetadata));
                SqlServerManager.Instance.RegisterConnectionStringSettings(conn);
                BrokerManager.RegisterDefaultDataSource(DefaultDataSource);
                BrokerManager.Instance.RegisterStore(DefaultDataSource, typeof(SqlServerStore<>));
            }

            var container = GetConfiguredContainer();
            container.RegisterType<Kinetix.Audit.IAuditManager, Kinetix.Audit.AuditManager>();
            container.RegisterType<Kinetix.Audit.IAuditTraceStorePlugin, Kinetix.Audit.MemoryAuditTraceStorePlugin>();

            container.RegisterType<Kinetix.Account.IAccountStorePlugin, Kinetix.Account.MemoryAccountStorePlugin>(new ContainerControlledLifetimeManager());
            container.RegisterType<Kinetix.Account.IAccountManager, Kinetix.Account.AccountManager>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Workflow.IWorkflowManager, Kinetix.Workflow.WorkflowManager>();

            if (SqlServer)
            {
                container.RegisterType<Kinetix.Workflow.IWorkflowStorePlugin, Kinetix.Workflow.SqlServerWorkflowStorePlugin>();
                container.RegisterType<Kinetix.Rules.IRuleStorePlugin, Kinetix.Rules.SqlServerRuleStorePlugin>();
            }
            else
            {
                container.RegisterType<Kinetix.Workflow.IWorkflowStorePlugin, Kinetix.Workflow.MemoryWorkflowStorePlugin>(new ContainerControlledLifetimeManager());
                container.RegisterType<Kinetix.Rules.IRuleStorePlugin, Kinetix.Rules.MemoryRuleStorePlugin>(new ContainerControlledLifetimeManager());
            }

            container.RegisterType<Kinetix.Workflow.IItemStorePlugin, MemoryItemStorePlugin>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Rules.IRuleManager, Kinetix.Rules.RuleManager>();

            container.RegisterType<Kinetix.Rules.IRuleConstantsStorePlugin, Kinetix.Rules.MemoryRuleConstantsStore>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Rules.IRuleSelectorPlugin, Kinetix.Rules.SimpleRuleSelectorPlugin>();
            container.RegisterType<Kinetix.Rules.IRuleValidatorPlugin, Kinetix.Rules.SimpleRuleValidatorPlugin>();
        }

        private MyDummyDtObject createDummyDtObject(int itemId, string entity = "ENT")
        {
            var container = GetConfiguredContainer();
            IItemStorePlugin itemStorePlugin = container.Resolve<IItemStorePlugin>();

            MyDummyDtObject myDummyDtObject = new MyDummyDtObject();
            myDummyDtObject.Id = itemId;
            myDummyDtObject.Division = "DIV";
            myDummyDtObject.Entity = entity;
            myDummyDtObject.Montant = 199.999m;
            itemStorePlugin.AddItem(myDummyDtObject.Id, myDummyDtObject);
            return myDummyDtObject;
        }

        [TestMethod]
        public void TestWorkflowStateChange()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance("WorkflowRules", "JUnit", false, myDummyDtObject.Id);

            Assert.IsNotNull(wfWorkflow);
            Assert.AreEqual(wfWorkflow.WfsCode, WfCodeStatusWorkflow.Cre.ToString());

            try
            {
                _workflowManager.ResumeInstance(wfWorkflow);
                Debug.Fail("Cannot resume an instance that is not started");
            }
            catch (InvalidOperationException)
            {
                // We should enter in this exeption case
            }

            try
            {
                _workflowManager.EndInstance(wfWorkflow);
                Debug.Fail("Cannot end instance that is not started");
            }
            catch (InvalidOperationException)
            {
                // We should enter in this exeption case
            }

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            Assert.AreEqual(wfWorkflow.WfsCode, WfCodeStatusWorkflow.Sta.ToString());

            try
            {
                _workflowManager.ResumeInstance(wfWorkflow);
                Debug.Fail("Cannot resume an instance that is not paused");
            }
            catch (InvalidOperationException)
            {
                // We should enter in this exeption case
            }

            // Pausing the workflow
            _workflowManager.PauseInstance(wfWorkflow);

            Assert.AreEqual(wfWorkflow.WfsCode, WfCodeStatusWorkflow.Pau.ToString());

            WfDecision wfDecision = new WfDecision();
            wfDecision.Choice = 1;
            wfDecision.Username = "junit";
            try
            {
                _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, wfDecision);
                Debug.Fail("Cannot go to next activity while the workflow is paused");
            }
            catch (InvalidOperationException)
            {
                // We should enter in this exeption case
            }

            try
            {
                _workflowManager.StartInstance(wfWorkflow);
                Debug.Fail("Cannot start an already started workflow");
            }
            catch (InvalidOperationException)
            {
                // We should enter in this exeption case
            }

            // A workflow in pause can be resumed
            _workflowManager.ResumeInstance(wfWorkflow);

            Assert.AreEqual(wfWorkflow.WfsCode, WfCodeStatusWorkflow.Sta.ToString());

            // A workflow started can be ended
            _workflowManager.EndInstance(wfWorkflow);

            WfWorkflow wfWorkflow2 = _workflowManager.CreateWorkflowInstance("WorkflowRules", "JUnit", false, myDummyDtObject.Id);

            Assert.IsNotNull(wfWorkflow2);
            Assert.AreEqual(wfWorkflow2.WfsCode, WfCodeStatusWorkflow.Cre.ToString());

            // A workflow created can be started.
            _workflowManager.StartInstance(wfWorkflow2);

            Assert.AreEqual(wfWorkflow2.WfsCode, WfCodeStatusWorkflow.Sta.ToString());

            // A workflow started can be paused.
            _workflowManager.PauseInstance(wfWorkflow2);

            Assert.AreEqual(wfWorkflow2.WfsCode, WfCodeStatusWorkflow.Pau.ToString());

            // A workflow paused can be ended.
            _workflowManager.EndInstance(wfWorkflow2);

            Assert.AreEqual(wfWorkflow2.WfsCode, WfCodeStatusWorkflow.End.ToString());
        }

        private void assertHasOneDecision(WfWorkflowDecision wfWorkflowDecision)
        {
            Assert.IsNotNull(wfWorkflowDecision.Decisions);
            Assert.AreEqual(1, wfWorkflowDecision.Decisions.Count);
        }

        private void assertActivityExist(WfActivityDefinition activityDefinition, WfWorkflowDecision wfWorkflowDecision)
        {
            Assert.AreEqual(activityDefinition.WfadId, wfWorkflowDecision.ActivityDefinition.WfadId);
            Assert.IsNotNull(wfWorkflowDecision.Activity);
            Assert.IsNotNull(wfWorkflowDecision.Activity.WfaId);
            Assert.AreEqual(activityDefinition.WfadId, wfWorkflowDecision.Activity.WfadId);
        }

        private void assertFirstDecisionEquals(WfDecision wfDecisionAct, WfWorkflowDecision wfWorkflowDecision)
        {
            Assert.AreEqual(wfDecisionAct.WfaId, wfWorkflowDecision.Decisions[0].WfaId);
            Assert.AreEqual(wfDecisionAct.Choice, wfWorkflowDecision.Decisions[0].Choice);
            Assert.AreEqual(wfDecisionAct.Comments, wfWorkflowDecision.Decisions[0].Comments);
            Assert.AreEqual(wfDecisionAct.DecisionDate, wfWorkflowDecision.Decisions[0].DecisionDate);
        }

        private void assertHasOneGroup(AccountGroup accountGroup, WfWorkflowDecision wfWorkflowDecision)
        {
            Assert.IsNotNull(wfWorkflowDecision.Groups);
            Assert.AreEqual(1, wfWorkflowDecision.Groups.Count);
            Assert.AreEqual(accountGroup.Id, wfWorkflowDecision.Groups[0].Id);
        }

        [TestMethod]
        public void TestWorkflowRulesManualValidationMulActivities()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value)
                .WithMultiplicity(WfCodeMultiplicityDefinition.Mul.ToString())
                .Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            AccountUser account2 = new AccountUserBuilder("Acc2").Build();
            AccountUser account3 = new AccountUserBuilder("Acc3").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account, account2, account3 });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);
            _accountManager.GetStore().Attach(account2.Id, accountGroup.Id);
            _accountManager.GetStore().Attach(account3.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : No rules/condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, secondActivity.WfadId, accountGroup.Id);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>());

            // Step 3 : 1 rule, 2 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "rule 1");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleConditionDefinition condition2Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3, condition2Rule1Act3 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, DateTime.Now, thirdActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter3 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(thirdActivity, selector3, new List<RuleFilterDefinition>() { filter3 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            IList<WfWorkflowDecision> workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            // 
            WfDecision decision = new WfDecision();
            decision.Choice = 1;
            decision.Comments = "abc";
            decision.Username = "Acc1";
            decision.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, decision);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            // 
            decision = new WfDecision();
            decision.Choice = 1;
            decision.Comments = "abc";
            decision.Username = "Acc2";
            decision.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, decision);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            // 
            decision = new WfDecision();
            decision.Choice = 1;
            decision.Comments = "abc";
            decision.Username = "Acc3";
            decision.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, decision);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, thirdActivity.WfadId);
        }


        [TestMethod]
        public void TestWorkflowRulesForceManualValidationMulActivities()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value)
                .WithMultiplicity(WfCodeMultiplicityDefinition.Mul.ToString())
                .Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            AccountUser account2 = new AccountUserBuilder("Acc2").Build();
            AccountUser account3 = new AccountUserBuilder("Acc3").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account, account2, account3 });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);
            _accountManager.GetStore().Attach(account2.Id, accountGroup.Id);
            _accountManager.GetStore().Attach(account3.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : No rules/condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, secondActivity.WfadId, accountGroup.Id);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>());

            // Step 3 : 1 rule, 2 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "rule 1");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleConditionDefinition condition2Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3, condition2Rule1Act3 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, DateTime.Now, thirdActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter3 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(thirdActivity, selector3, new List<RuleFilterDefinition>() { filter3 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            IList<WfWorkflowDecision> workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            // 
            WfDecision decision = new WfDecision();
            decision.Choice = 1;
            decision.Comments = "abc";
            decision.Username = "Acc1";
            decision.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, decision);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            // 
            decision = new WfDecision();
            decision.Choice = 1;
            decision.Comments = "abc";
            decision.Username = "Acc2";
            decision.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, decision);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            // 
            decision = new WfDecision();
            decision.Choice = 1;
            decision.Comments = "abc";
            decision.Username = "Acc3";
            decision.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, decision);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, thirdActivity.WfadId);
        }

        [TestMethod]
        public void TestWorkflowRemoveWorkflow()
        {
            var container = GetConfiguredContainer();

            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value)
                .Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });


            MyDummyDtObject myDummyDtObject = createDummyDtObject(1, "NoMatch");

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            _workflowManager.GetAllWorkflowDecisions(wfWorkflow.WfwdId.Value);

            MemoryItemStorePlugin memoryItemStorePlugin = (MemoryItemStorePlugin) container.Resolve<IItemStorePlugin>();
            memoryItemStorePlugin.inMemoryItemStore.Clear();

            try
            {
                //No Item is currently linked to this workflow. Item should return null
                _workflowManager.GetAllWorkflowDecisions(wfWorkflow.WfwdId.Value);
            }
            catch (Exception)
            {
                //Nominal case
            }
            
            //We remove the workflow.
            _workflowManager.RemoveWorkflow(wfWorkflow.WfwId.Value);

            //Here GetAllWorkflowDecisions should not throw an Exception if no item are associated to the workflow.
            _workflowManager.GetAllWorkflowDecisions(wfWorkflow.WfwdId.Value);

        }

        [TestMethod]
        public void TestWorkflowItemModifiedWithRecalculation()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value)
                .Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : No rules/condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, secondActivity.WfadId, accountGroup.Id);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>());

            // Step 3 : 1 rule, 2 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "rule 1");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Montant", ">", "10", null);
            RuleConditionDefinition condition2Rule1Act3 = new RuleConditionDefinition(null, "Division", "=", "DIV", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3, condition2Rule1Act3 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, DateTime.Now, thirdActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter3 = new RuleFilterDefinition(null, "Montant", ">", "10", null);
            RuleFilterDefinition filter4 = new RuleFilterDefinition(null, "Division", "=", "DIV", null);
            _workflowManager.AddSelector(thirdActivity, selector3, new List<RuleFilterDefinition>() { filter3, filter4 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1, "NoMatch");

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            IList<WfWorkflowDecision> workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, thirdActivity.WfadId);

            Assert.AreEqual(1, workflowDecisions.Count);
            Assert.IsFalse(workflowDecisions[0].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[0].Activity.IsValid);

            MyDummyDtObject myDummyDtObject2 = createDummyDtObject(1);

            IItemStorePlugin itemStorePlugin = container.Resolve<IItemStorePlugin>();

            itemStorePlugin.AddItem(1, myDummyDtObject2);

            //workflowDecisions = _workflowManager.GetWorkflowDecision(wfWorkflow.WfwId.Value);

            _workflowManager.RecalculateWorkflow(wfWorkflow);

            workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            currentActivityId = wfWorkflowFetched.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            Assert.AreEqual(2, workflowDecisions.Count);
            Assert.IsFalse(workflowDecisions[0].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[0].Activity.IsValid);
            Assert.IsFalse(workflowDecisions[1].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[1].Activity.IsValid);

        }

        [TestMethod]
        public void TestWorkflowRulesRemoveDecisionManualValidationActivities()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value)
                .Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : No rules/condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, secondActivity.WfadId, accountGroup.Id);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>());

            // Step 3 : 1 rule, 2 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "rule 1");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleConditionDefinition condition2Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3, condition2Rule1Act3 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, DateTime.Now, thirdActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter3 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(thirdActivity, selector3, new List<RuleFilterDefinition>() { filter3 });

            // Step 4 : 2 rules, 1 condition
            WfActivityDefinition fourthActivity = new WfActivityDefinitionBuilder("Step 4", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, fourthActivity, 4);
            RuleDefinition rule1Act4 = new RuleDefinition(null, DateTime.Now, fourthActivity.WfadId, "rule 2");
            RuleConditionDefinition condition1Rule1Act4 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleDefinition rule2Act4 = new RuleDefinition(null, DateTime.Now, fourthActivity.WfadId, "rule 3");
            RuleConditionDefinition condition1Rule2Act4 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(fourthActivity, rule1Act4, new List<RuleConditionDefinition>() { condition1Rule1Act4 });
            _workflowManager.AddRule(fourthActivity, rule2Act4, new List<RuleConditionDefinition>() { condition1Rule2Act4 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector41 = new SelectorDefinition(null, DateTime.Now, fourthActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter4 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(fourthActivity, selector41, new List<RuleFilterDefinition>() { filter4 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched);

            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            WfDecision decision = new WfDecision();
            decision.Choice = 1;
            decision.Comments = "abc";
            decision.Username = "AA";
            decision.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecision(wfWorkflow, decision);

            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            bool canGoToNext = _workflowManager.CanGoToNextActivity(wfWorkflow);
            Assert.IsTrue(canGoToNext);
            
            _workflowManager.DeleteDecision(decision);

            canGoToNext = _workflowManager.CanGoToNextActivity(wfWorkflow);
            Assert.IsFalse(canGoToNext);

            WfDecision decision2 = new WfDecision();
            decision2.Choice = 1;
            decision2.Comments = "abc";
            decision2.Username = "AA";
            decision2.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecision(wfWorkflow, decision2);

            canGoToNext = _workflowManager.CanGoToNextActivity(wfWorkflow);
            Assert.IsTrue(canGoToNext);

            _workflowManager.GoToNextActivity(wfWorkflow);

            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, thirdActivity.WfadId);
            

        }


        public void TestWorkflowRulesRemoveDecisionManualValidationMulActivities()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value)
                                .WithMultiplicity(WfCodeMultiplicityDefinition.Mul.ToString())
                                .Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            AccountUser account2 = new AccountUserBuilder("Acc2").Build();
            AccountUser account3 = new AccountUserBuilder("Acc3").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account, account2, account3 });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);
            _accountManager.GetStore().Attach(account2.Id, accountGroup.Id);
            _accountManager.GetStore().Attach(account3.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : No rules/condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, secondActivity.WfadId, accountGroup.Id);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>());

            // Step 3 : 1 rule, 2 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "rule 1");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleConditionDefinition condition2Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3, condition2Rule1Act3 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, DateTime.Now, thirdActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter3 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(thirdActivity, selector3, new List<RuleFilterDefinition>() { filter3 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched);

            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            WfDecision decision = new WfDecision();
            decision.Choice = 1;
            decision.Comments = "abc";
            decision.Username = "Acc1";
            decision.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecision(wfWorkflow, decision);

            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            bool canGoToNext = _workflowManager.CanGoToNextActivity(wfWorkflow);
            Assert.IsFalse(canGoToNext);

            WfDecision decision2 = new WfDecision();
            decision2.Choice = 1;
            decision2.Comments = "abc";
            decision2.Username = "Acc2";
            decision2.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecision(wfWorkflow, decision2);

            canGoToNext = _workflowManager.CanGoToNextActivity(wfWorkflow);
            Assert.IsFalse(canGoToNext);

            WfDecision decision3 = new WfDecision();
            decision3.Choice = 1;
            decision3.Comments = "abc";
            decision3.Username = "Acc3";
            decision3.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecision(wfWorkflow, decision3);

            canGoToNext = _workflowManager.CanGoToNextActivity(wfWorkflow);
            Assert.IsTrue(canGoToNext);

            _workflowManager.DeleteDecision(decision);

            canGoToNext = _workflowManager.CanGoToNextActivity(wfWorkflow);
            Assert.IsFalse(canGoToNext);

            WfDecision newDecision1 = new WfDecision();
            newDecision1.Choice = 1;
            newDecision1.Comments = "abc";
            newDecision1.Username = "Acc1";
            newDecision1.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecision(wfWorkflow, newDecision1);

            canGoToNext = _workflowManager.CanGoToNextActivity(wfWorkflow);
            Assert.IsTrue(canGoToNext);

            _workflowManager.GoToNextActivity(wfWorkflow);

            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, thirdActivity.WfadId);
        }

        [TestMethod]
        public void TestWorkflowRulesManualValidationActivities()
        {

            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : No rules/condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, secondActivity.WfadId, accountGroup.Id);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>());

            // Step 3 : 1 rule, 2 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "rule 1");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleConditionDefinition condition2Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3, condition2Rule1Act3 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, DateTime.Now, thirdActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter3 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(thirdActivity, selector3, new List<RuleFilterDefinition>() { filter3 });

            // Step 4 : 2 rules, 1 condition
            WfActivityDefinition fourthActivity = new WfActivityDefinitionBuilder("Step 4", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, fourthActivity, 4);
            RuleDefinition rule1Act4 = new RuleDefinition(null, DateTime.Now, fourthActivity.WfadId, "rule 2");
            RuleConditionDefinition condition1Rule1Act4 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleDefinition rule2Act4 = new RuleDefinition(null, DateTime.Now, fourthActivity.WfadId, "rule 3");
            RuleConditionDefinition condition1Rule2Act4 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(fourthActivity, rule1Act4, new List<RuleConditionDefinition>() { condition1Rule1Act4 });
            _workflowManager.AddRule(fourthActivity, rule2Act4, new List<RuleConditionDefinition>() { condition1Rule2Act4 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector41 = new SelectorDefinition(null, DateTime.Now, fourthActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter4 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(fourthActivity, selector41, new List<RuleFilterDefinition>() { filter4 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            IList<WfWorkflowDecision> workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            //Step 1,3,4 should be Manual, Step 2 should be auto 
            // No decisons for now
            Assert.IsNotNull(workflowDecisions);
            Assert.AreEqual(workflowDecisions.Count, 3);
            //Check Step 1
            assertActivityExist(firstActivity, workflowDecisions[0]);
            Assert.IsNull(workflowDecisions[0].Decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[0]);
            //Check Step 3
            Assert.AreEqual(thirdActivity.WfadId, workflowDecisions[1].ActivityDefinition.WfadId);
            Assert.IsNull(workflowDecisions[1].Activity);
            Assert.IsNull(workflowDecisions[1].Decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[1]);
            //Check Step 4
            Assert.AreEqual(fourthActivity.WfadId, workflowDecisions[2].ActivityDefinition.WfadId);
            Assert.IsNull(workflowDecisions[2].Activity);
            Assert.IsNull(workflowDecisions[2].Decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[2]);


            // Entry actions should NOT validate all activities.
            int currentActivityId = (int)wfWorkflow.WfaId2;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched);

            currentActivityId = (int)wfWorkflow.WfaId2;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            WfDecision decision = new WfDecision();
            decision.Choice = 1;
            decision.Comments = "abc";
            decision.Username = "AA";
            decision.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, decision);

            workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            // Step 1,3,4 should be Manual, Step 2 should be auto 
            // 1 Decisions for Step 1
            Assert.IsNotNull(workflowDecisions);
            Assert.AreEqual(workflowDecisions.Count, 3);
            //Check Step 1
            assertActivityExist(firstActivity, workflowDecisions[0]);
            // 1 Decision
            assertHasOneDecision(workflowDecisions[0]);
            assertFirstDecisionEquals(decision, workflowDecisions[0]);
            assertHasOneGroup(accountGroup, workflowDecisions[0]);
            //Check Step 3
            assertActivityExist(thirdActivity, workflowDecisions[1]);
            Assert.IsNull(workflowDecisions[1].Decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[1]);
            //Check Step 4
            Assert.AreEqual(fourthActivity.WfadId, workflowDecisions[2].ActivityDefinition.WfadId);
            Assert.IsNull(workflowDecisions[2].Activity);
            Assert.IsNull(workflowDecisions[2].Decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[2]);

            // Activity 1 should now be validated.
            // No rule defined for activity 2. Activity 2 should be autovalidated
            // The current activity should be now activity 3
            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, thirdActivity.WfadId);

            WfWorkflow wfWorkflowFetched2 = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched2);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, thirdActivity.WfadId);


            //Manually validating activity 3
            WfDecision wfDecisionAct3 = new WfDecision();
            wfDecisionAct3.Choice = 1;
            wfDecisionAct3.Username = account.Id;
            wfDecisionAct3.WfaId = currentActivity.WfaId.Value;

            //Using CanGo, SaveDecision and GoToNext
            bool canGo = _workflowManager.CanGoToNextActivity(wfWorkflow);
            Assert.IsFalse(canGo);

            _workflowManager.SaveDecision(wfWorkflow, wfDecisionAct3);
            canGo = _workflowManager.CanGoToNextActivity(wfWorkflow);
            Assert.IsTrue(canGo);
            _workflowManager.CanGoToNextActivity(wfWorkflow);
            _workflowManager.GoToNextActivity(wfWorkflow);

            workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            // Step 1,3,4 should be Manual, Step 2 should be auto 
            // Decisions for Step 1, Step 3
            Assert.IsNotNull(workflowDecisions);
            Assert.AreEqual(workflowDecisions.Count, 3);
            // Check Step 1
            assertActivityExist(firstActivity, workflowDecisions[0]);
            // 1 Decision
            assertHasOneDecision(workflowDecisions[0]);
            assertFirstDecisionEquals(decision, workflowDecisions[0]);
            assertHasOneGroup(accountGroup, workflowDecisions[0]);
            // Check Step 3
            assertActivityExist(thirdActivity, workflowDecisions[1]);
            // Decisions for Step 3
            assertHasOneDecision(workflowDecisions[1]);
            assertFirstDecisionEquals(wfDecisionAct3, workflowDecisions[1]);
            assertHasOneGroup(accountGroup, workflowDecisions[1]);
            // Check Step 4
            Assert.AreEqual(fourthActivity.WfadId, workflowDecisions[2].ActivityDefinition.WfadId);
            Assert.IsNotNull(workflowDecisions[2].Activity);
            Assert.IsNull(workflowDecisions[2].Decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[2]);

            // Activity 3 should now be validated.
            // The current activity should be now activity 4
            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, fourthActivity.WfadId);

            WfWorkflow wfWorkflowFetched3 = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched3);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, fourthActivity.WfadId);

            // Manually validating activity 4
            WfDecision wfDecisionAct4 = new WfDecision();
            wfDecisionAct4.Choice = 1;
            wfDecisionAct4.Username = account.Id;
            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, wfDecisionAct4);

            workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            // Step 1,3,4 should be Manual, Step 2 should be auto 
            // Decisions for Step 1, Step 3
            Assert.IsNotNull(workflowDecisions);
            Assert.AreEqual(workflowDecisions.Count, 3);
            // Check Step 1
            assertActivityExist(firstActivity, workflowDecisions[0]);
            // 1 Decision
            assertHasOneDecision(workflowDecisions[0]);
            assertFirstDecisionEquals(decision, workflowDecisions[0]);
            assertHasOneGroup(accountGroup, workflowDecisions[0]);
            // Check Step 3
            assertActivityExist(thirdActivity, workflowDecisions[1]);
            // Decisions for Step 3
            assertHasOneDecision(workflowDecisions[1]);
            assertFirstDecisionEquals(wfDecisionAct3, workflowDecisions[1]);
            assertHasOneGroup(accountGroup, workflowDecisions[1]);
            // Check Step 4
            assertActivityExist(fourthActivity, workflowDecisions[2]);
            // Decisions for Step 4
            assertHasOneDecision(workflowDecisions[2]);
            assertFirstDecisionEquals(wfDecisionAct4, workflowDecisions[2]);

            Assert.IsNotNull(workflowDecisions[2].Groups);
            Assert.AreEqual(1, workflowDecisions[2].Groups.Count);
            Assert.AreEqual(accountGroup.Id, workflowDecisions[2].Groups[0].Id);

            // Activity 4 should now be validated. The current activity is now activity 4, with the end status
            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, fourthActivity.WfadId);

            // No Automatic ending.
            //Assert.AreEqual(wfWorkflow.WfsCode, WfCodeStatusWorkflow.End.ToString());
            Assert.AreEqual(wfWorkflow.WfsCode, WfCodeStatusWorkflow.Sta.ToString());

            WfWorkflow wfWorkflowFetched5 = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            //Assert.AreEqual(wfWorkflowFetched5.WfsCode, WfCodeStatusWorkflow.End.ToString());
            Assert.AreEqual(wfWorkflowFetched5.WfsCode, WfCodeStatusWorkflow.Sta.ToString());

            IList<WfListWorkflowDecision> allDecisions = _workflowManager.GetAllWorkflowDecisions(wfWorkflow.WfwdId.Value);
            Assert.AreEqual(1, allDecisions.Count);
        }



        [TestMethod]
        public void TestWorkflowRulesAutoValidationNoSelectorAllActivities()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition (NO Selector)
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });

            // Step 2 : No rules/condition (NO Selector)
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);

            // Step 3 : 1 rule, 2 conditions (NO Selector)
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "rule 1");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleConditionDefinition condition2Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3, condition2Rule1Act3 });

            // Step 4 : 2 rules, 1 condition (NO Selector)
            WfActivityDefinition fourthActivity = new WfActivityDefinitionBuilder("Step 4", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, fourthActivity, 4);
            RuleDefinition rule1Act4 = new RuleDefinition(null, DateTime.Now, fourthActivity.WfadId, "rule 2");
            RuleConditionDefinition condition1Rule1Act4 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleDefinition rule2Act4 = new RuleDefinition(null, DateTime.Now, fourthActivity.WfadId, "rule 3");
            RuleConditionDefinition condition1Rule2Act4 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(fourthActivity, rule1Act4, new List<RuleConditionDefinition>() { condition1Rule1Act4 });
            _workflowManager.AddRule(fourthActivity, rule2Act4, new List<RuleConditionDefinition>() { condition1Rule2Act4 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            IList<WfWorkflowDecision> workflowDecisions1 = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            // Entry actions should validate all activities (because no group have been associated).
            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, fourthActivity.WfadId);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched);
            Assert.AreEqual(currentActivity.WfadId, fourthActivity.WfadId);
        }


        [TestMethod]
        public void TestWorkflowRecalculationFirstAndCurrentStepAddRemovingRules()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);
            
            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : No rules/condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, secondActivity.WfadId, accountGroup.Id);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>());

            // Step 3 : 1 rule, 2 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "rule 1");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleConditionDefinition condition2Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3, condition2Rule1Act3 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, DateTime.Now, thirdActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter3 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(thirdActivity, selector3, new List<RuleFilterDefinition>() { filter3 });

            // Step 4 : 2 rules, 1 condition
            WfActivityDefinition fourthActivity = new WfActivityDefinitionBuilder("Step 4", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, fourthActivity, 4);
            RuleDefinition rule1Act4 = new RuleDefinition(null, DateTime.Now, fourthActivity.WfadId, "rule 2");
            RuleConditionDefinition condition1Rule1Act4 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleDefinition rule2Act4 = new RuleDefinition(null, DateTime.Now, fourthActivity.WfadId, "rule 3");
            RuleConditionDefinition condition1Rule2Act4 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(fourthActivity, rule1Act4, new List<RuleConditionDefinition>() { condition1Rule1Act4 });
            _workflowManager.AddRule(fourthActivity, rule2Act4, new List<RuleConditionDefinition>() { condition1Rule2Act4 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector41 = new SelectorDefinition(null, DateTime.Now, fourthActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter4 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(fourthActivity, selector41, new List<RuleFilterDefinition>() { filter4 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(firstActivity.WfadId, currentActivity.WfadId);

            // We are at the first activity 
            // This activity is manual, Activity exist but no decision associated

            // Now let's change the rules associated to the first activity definition.
            // We remove the only rules associated.
            _workflowManager.RemoveRule(rule1Act1);

            //We call the Recalculation of the Workflow
            _workflowManager.RecalculateWorkflowDefinition(wfWorkflowDefinition);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The first activity should be now auto. The Second Activity was auto.
            // The current activity should now be the third activity.
            int recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            WfActivity recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(thirdActivity.WfadId, recalculActivity.WfadId);

            // We add back the rule removed
            rule1Act1.Id = null;
            condition1Rule1Act1.Id = null;
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });

            //We call the Recalculation of the Workflow
            _workflowManager.RecalculateWorkflowDefinition(wfWorkflowDefinition);

            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The first activity should be now manual and be the current activity. 
            recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(firstActivity.WfadId, recalculActivity.WfadId);

        }

        [TestMethod]
        public void TestWorkflowRecalculationAddingNewActivityFirstPosition()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("100").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);


            // The curernt activity is the First activity
            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(firstActivity.WfadId, currentActivity.WfadId);

            // Adding a new Activity with one rule and one selector
            WfActivityDefinition activityZero = new WfActivityDefinitionBuilder("Step 0", wfWorkflowDefinition.WfwdId.Value).Build();

            _workflowManager.AddActivity(wfWorkflowDefinition, activityZero, 1);

            RuleDefinition rule1Act0 = new RuleDefinition(null, DateTime.Now, activityZero.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act0 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(activityZero, rule1Act0, new List<RuleConditionDefinition>() { condition1Rule1Act0 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector0 = new SelectorDefinition(null, DateTime.Now, activityZero.WfadId, accountGroup.Id);
            RuleFilterDefinition filter0 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(activityZero, selector0, new List<RuleFilterDefinition>() { filter0 });

            // We call the Recalculation of the Workflow.
            _workflowManager.RecalculateWorkflowDefinition(wfWorkflowDefinition);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The first activity should be now manual and be the current activity. 
            int recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            WfActivity recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(activityZero.WfadId, recalculActivity.WfadId);

            WfDecision wfDecision = new WfDecision();
            wfDecision.WfaId = recalculActivity.WfaId.Value;
            wfDecision.Username = "100";
            wfDecision.DecisionDate = DateTime.Now;
            wfDecision.Comments = "Test";

            // _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, wfDecision);
            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflowFetched, wfDecision);

            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The first activity should be now manual and be the current activity. 
            recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(firstActivity.WfadId, recalculActivity.WfadId);

            // No Modification of the definition and we call the Recalculation of the Workflow.
            _workflowManager.RecalculateWorkflowDefinition(wfWorkflowDefinition);
            // The workflow must be on the same activity
            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The first activity should be now manual and be the current activity. 
            recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(firstActivity.WfadId, recalculActivity.WfadId);

            // Removing the selector of the activity zero.
            // No selector nor rule. The activity zero should be autovalidated.
            _workflowManager.RemoveRule(rule1Act1);

            // We call the Recalculation
            _workflowManager.RecalculateWorkflowDefinition(wfWorkflowDefinition);

            // Step 0 should be autovalidated, but the current activity must be Step 1
            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The first activity should be now manual and be the current activity. 
            recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(firstActivity.WfadId, recalculActivity.WfadId);
        }


        [TestMethod]
        public void TestWorkflowRecalculationAddingNewActivityLastPosition()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("100").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);


            // The current activity is the First activity
            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(firstActivity.WfadId, currentActivity.WfadId);

            // Adding a new Activity with one rule and one selector
            WfActivityDefinition activityEnd = new WfActivityDefinitionBuilder("Step 2", wfWorkflowDefinition.WfwdId.Value).Build();

            //Adding a new activity at the end
            _workflowManager.AddActivity(wfWorkflowDefinition, activityEnd, 2);

            RuleDefinition rule1Act2 = new RuleDefinition(null, DateTime.Now, activityEnd.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act2 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(activityEnd, rule1Act2, new List<RuleConditionDefinition>() { condition1Rule1Act2 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, activityEnd.WfadId, accountGroup.Id);
            RuleFilterDefinition filter2 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(activityEnd, selector2, new List<RuleFilterDefinition>() { filter2 });

            // We call the Recalculation of the Workflow.
            _workflowManager.RecalculateWorkflowDefinition(wfWorkflowDefinition);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The first activity should be now manual and be the current activity. 
            int recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            WfActivity recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(firstActivity.WfadId, recalculActivity.WfadId);

            WfDecision wfDecision = new WfDecision();
            wfDecision.WfaId = recalculActivity.WfaId.Value;
            wfDecision.Username = "100";
            wfDecision.DecisionDate = DateTime.Now;
            wfDecision.Comments = "Test";

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, wfDecision);

            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The end activity should be now manual and be the current activity. 
            recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(activityEnd.WfadId, recalculActivity.WfadId);

            // No Modification of the definition and we call the Recalculation of the Workflow.
            _workflowManager.RecalculateWorkflowDefinition(wfWorkflowDefinition);
            // The workflow must be on the same activity
            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The end activity should be manual and be the current activity. 
            recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(activityEnd.WfadId, recalculActivity.WfadId);

            // Removing the selector of the first activity.
            // No selector nor rule. The activity zero should be autovalidated.
            _workflowManager.RemoveRule(rule1Act1);

            // We call the Recalculation
            _workflowManager.RecalculateWorkflowDefinition(wfWorkflowDefinition);

            // Step 1 should be autovalidated, but the current activity must be Step 2
            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The first activity should be now manual and be the current activity. 
            recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(activityEnd.WfadId, recalculActivity.WfadId);

        }


        [TestMethod]
        public void TestWorkflowDoubleWithCulture()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("100").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();

            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Montant", "<", "200,001", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });


            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("fr");

            IList<WfWorkflowDecision> workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            Assert.IsNotNull(workflowDecisions);
            Assert.AreEqual(workflowDecisions.Count, 1);

            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");

            workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            Assert.IsNotNull(workflowDecisions);
            Assert.AreEqual(workflowDecisions.Count, 1);

        }

        [TestMethod]
        public void TestWorkflowRecalculationRemovingActivityFirstPosition()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("100").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();

            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : 1 rule, 1 condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", wfWorkflowDefinition.WfwdId.Value).Build();

            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            RuleDefinition rule1Act2 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 2");
            RuleConditionDefinition condition1Rule1Act2 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(secondActivity, rule1Act2, new List<RuleConditionDefinition>() { condition1Rule1Act2 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, secondActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter2 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>() { filter2 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1);

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            // The current activity is the First activity
            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(firstActivity.WfadId, currentActivity.WfadId);

            // Removing the first and Current activity definition
            _workflowManager.RemoveActivity(firstActivity);

            // We call the Recalculation of the Workflow.
            _workflowManager.RecalculateWorkflowDefinition(wfWorkflowDefinition);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The second activity should be manual and be the current activity. 
            int recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            WfActivity recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(secondActivity.WfadId, recalculActivity.WfadId);

            WfDecision wfDecision = new WfDecision();
            wfDecision.WfaId = recalculActivity.WfaId.Value;
            wfDecision.Username = "100";
            wfDecision.DecisionDate = DateTime.Now;
            wfDecision.Comments = "Test";

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflowFetched, wfDecision);

            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The first activity should be now manual and be the current activity. 
            recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(secondActivity.WfadId, recalculActivity.WfadId);

            // No Modification of the definition and we call the Recalculation of the Workflow.
            _workflowManager.RecalculateWorkflowDefinition(wfWorkflowDefinition);
            // The workflow must be on the same activity
            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            // The first activity should be now manual and be the current activity. 
            recalculActivityId = wfWorkflowFetched.WfaId2.Value;
            recalculActivity = _workflowManager.GetActivity(recalculActivityId);
            Assert.AreEqual(secondActivity.WfadId, recalculActivity.WfadId);
        }

        [TestMethod]
        public void TestWorkflowRecalculationMassRecalculation()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : No rules/condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, secondActivity.WfadId, accountGroup.Id);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>());

            // Step 3 : 1 rule, 2 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "rule 1");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleConditionDefinition condition2Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3, condition2Rule1Act3 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, DateTime.Now, thirdActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter3 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(thirdActivity, selector3, new List<RuleFilterDefinition>() { filter3 });

            // Step 4 : 2 rules, 1 condition
            WfActivityDefinition fourthActivity = new WfActivityDefinitionBuilder("Step 4", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, fourthActivity, 4);
            RuleDefinition rule1Act4 = new RuleDefinition(null, DateTime.Now, fourthActivity.WfadId, "rule 2");
            RuleConditionDefinition condition1Rule1Act4 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleDefinition rule2Act4 = new RuleDefinition(null, DateTime.Now, fourthActivity.WfadId, "rule 3");
            RuleConditionDefinition condition1Rule2Act4 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(fourthActivity, rule1Act4, new List<RuleConditionDefinition>() { condition1Rule1Act4 });
            _workflowManager.AddRule(fourthActivity, rule2Act4, new List<RuleConditionDefinition>() { condition1Rule2Act4 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector41 = new SelectorDefinition(null, DateTime.Now, fourthActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter4 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(fourthActivity, selector41, new List<RuleFilterDefinition>() { filter4 });

            IList<WfWorkflow> allWorkflows = new List<WfWorkflow>();

            int nbWf = 2000;

            for (int i = 0; i < nbWf; i++)
            {
                MyDummyDtObject myDummyDtObject = createDummyDtObject(i);

                WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

                allWorkflows.Add(wfWorkflow);

                // Starting the workflow
                _workflowManager.StartInstance(wfWorkflow);

                // We are at the first activity 
                // This activity is manual, Activity exist but no decision associated
            }

            // Now let's change the rules associated to the first activity definition.
            // We remove the only rules associated.
            _workflowManager.RemoveRule(rule1Act1);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            //We call the Recalculation of the Workflow
            WfRecalculationOutput output = _workflowManager.RecalculateWorkflowDefinition(wfWorkflowDefinition, true);

            sw.Stop();

            Trace.WriteLine(sw.ElapsedMilliseconds);
            Assert.IsTrue(sw.ElapsedMilliseconds < 2000);

            sw = new Stopwatch();
            sw.Start();
            //We call the Recalculation of the Workflow
            IList<WfListWorkflowDecision> allDecisions = _workflowManager.GetAllWorkflowDecisions(wfWorkflowDefinition.WfwdId.Value);
            sw.Stop();

            Trace.WriteLine(sw.ElapsedMilliseconds);
            Assert.IsTrue(sw.ElapsedMilliseconds < 5000);

            Assert.AreEqual(output.WfListWorkflowDecision.Count, allDecisions.Count);

            foreach (WfWorkflow wfWorkflow in allWorkflows)
            {
                WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

                // The first activity should be now auto. The Second Activity was auto.
                // The current activity should now be the third activity.
                int recalculActivityId = wfWorkflowFetched.WfaId2.Value;
                WfActivity recalculActivity = _workflowManager.GetActivity(recalculActivityId);
                Assert.AreEqual(thirdActivity.WfadId, recalculActivity.WfadId);
            }

        }


        [TestMethod]
        public void TestWorkflowMove2ActivitiesFirstLastPosition()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("100").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1
            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);

            // Step 2
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);

            IList<WfActivityDefinition> activities = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            Assert.AreEqual(2, activities.Count);
            Assert.AreEqual(firstActivity.WfadId, activities[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities[1].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 2, 1, false);

            IList<WfActivityDefinition> activities2 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            Assert.AreEqual(2, activities2.Count);
            Assert.AreEqual(secondActivity.WfadId, activities2[0].WfadId);
            Assert.AreEqual(firstActivity.WfadId, activities2[1].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 1, 2, true);
            IList<WfActivityDefinition> activities3 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            Assert.AreEqual(2, activities3.Count);
            Assert.AreEqual(firstActivity.WfadId, activities3[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities3[1].WfadId);

        }


        [TestMethod]
        public void TestWorkflowMove3ActivitiesBefore()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("100").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1
            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);

            // Step 2
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);

            // Step 3
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);

            IList<WfActivityDefinition> activities = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            Assert.AreEqual(3, activities.Count);
            Assert.AreEqual(firstActivity.WfadId, activities[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities[1].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities[2].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 2, 1, false);

            IList<WfActivityDefinition> activities2 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            // We should have 2,1,3
            Assert.AreEqual(3, activities2.Count);
            Assert.AreEqual(secondActivity.WfadId, activities2[0].WfadId);
            Assert.AreEqual(firstActivity.WfadId, activities2[1].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities2[2].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 2, 3, false);

            IList<WfActivityDefinition> activities3 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            // We should have 2,1,3 again
            Assert.AreEqual(3, activities3.Count);
            Assert.AreEqual(secondActivity.WfadId, activities3[0].WfadId);
            Assert.AreEqual(firstActivity.WfadId, activities3[1].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities3[2].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 1, 3, false);

            IList<WfActivityDefinition> activities4 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            // We should have 1,2,3 again
            Assert.AreEqual(3, activities4.Count);
            Assert.AreEqual(firstActivity.WfadId, activities4[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities4[1].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities4[2].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 3, 1, false);

            IList<WfActivityDefinition> activities5 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            // We should have 3,1,2
            Assert.AreEqual(3, activities5.Count);
            Assert.AreEqual(thirdActivity.WfadId, activities5[0].WfadId);
            Assert.AreEqual(firstActivity.WfadId, activities5[1].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities5[2].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 3, 2, false);

            IList<WfActivityDefinition> activities6 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            // We should have 3,2,1
            Assert.AreEqual(3, activities6.Count);
            Assert.AreEqual(thirdActivity.WfadId, activities6[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities6[1].WfadId);
            Assert.AreEqual(firstActivity.WfadId, activities6[2].WfadId);
        }


        [TestMethod]
        public void TestWorkflowMove3ActivitiesAfter()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("100").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1
            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);

            // Step 2
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);

            // Step 3
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);

            IList<WfActivityDefinition> activities = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            Assert.AreEqual(3, activities.Count);
            Assert.AreEqual(firstActivity.WfadId, activities[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities[1].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities[2].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 2, 1, true);

            IList<WfActivityDefinition> activities2 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            // We should have 1,2,3 again
            Assert.AreEqual(3, activities2.Count);
            Assert.AreEqual(firstActivity.WfadId, activities2[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities2[1].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities2[2].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 2, 3, true);

            IList<WfActivityDefinition> activities3 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            // We should have 1,3,2 
            Assert.AreEqual(3, activities3.Count);
            Assert.AreEqual(firstActivity.WfadId, activities3[0].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities3[1].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities3[2].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 1, 3, true);

            IList<WfActivityDefinition> activities4 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            // We should have 3,2,1
            Assert.AreEqual(3, activities4.Count);
            Assert.AreEqual(thirdActivity.WfadId, activities4[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities4[1].WfadId);
            Assert.AreEqual(firstActivity.WfadId, activities4[2].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 3, 1, true);

            IList<WfActivityDefinition> activities5 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            // We should have 3,1,2
            Assert.AreEqual(3, activities5.Count);
            Assert.AreEqual(thirdActivity.WfadId, activities5[0].WfadId);
            Assert.AreEqual(firstActivity.WfadId, activities5[1].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities5[2].WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 3, 2, true);

            IList<WfActivityDefinition> activities6 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            // We should have 3,1,2 again
            Assert.AreEqual(3, activities6.Count);
            Assert.AreEqual(thirdActivity.WfadId, activities6[0].WfadId);
            Assert.AreEqual(firstActivity.WfadId, activities6[1].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities6[2].WfadId);

        }



        [TestMethod]
        public void TestWorkflowMoveActivity5ActivitiesWorkflow()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("100").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1
            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);

            // Step 2
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);

            // Step 3
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);

            // Step 4
            WfActivityDefinition fourthActivity = new WfActivityDefinitionBuilder("Step 4", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, fourthActivity, 4);

            // Step 5
            WfActivityDefinition fifthActivity = new WfActivityDefinitionBuilder("Step 5", wfWorkflowDefinition.WfwdId.Value).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, fifthActivity, 5);

            IList<WfActivityDefinition> activities = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);

            // We should have 1,2,3,4,5
            Assert.AreEqual(5, activities.Count);
            Assert.AreEqual(firstActivity.WfadId, activities[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities[1].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities[2].WfadId);
            Assert.AreEqual(fourthActivity.WfadId, activities[3].WfadId);
            Assert.AreEqual(fifthActivity.WfadId, activities[4].WfadId);

            // We move 2 after 4
            _workflowManager.MoveActivity(wfWorkflowDefinition, 2, 4, true);

            IList<WfActivityDefinition> activities2 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);
            // We should have 1,3,4,2,5
            Assert.AreEqual(5, activities2.Count);
            Assert.AreEqual(firstActivity.WfadId, activities2[0].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities2[1].WfadId);
            Assert.AreEqual(fourthActivity.WfadId, activities2[2].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities2[3].WfadId);
            Assert.AreEqual(fifthActivity.WfadId, activities2[4].WfadId);

            // We move 2 before 4
            _workflowManager.MoveActivity(wfWorkflowDefinition, 4, 2, false);

            IList<WfActivityDefinition> activities3 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);
            // We should have 1,2,3,4,5
            Assert.AreEqual(5, activities3.Count);
            Assert.AreEqual(firstActivity.WfadId, activities3[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities3[1].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities3[2].WfadId);
            Assert.AreEqual(fourthActivity.WfadId, activities3[3].WfadId);
            Assert.AreEqual(fifthActivity.WfadId, activities3[4].WfadId);

            // We move 1 before 5
            _workflowManager.MoveActivity(wfWorkflowDefinition, 1, 5, true);

            IList<WfActivityDefinition> activities4 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);
            // We should have 2,3,4,5,1
            Assert.AreEqual(5, activities4.Count);
            Assert.AreEqual(secondActivity.WfadId, activities4[0].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities4[1].WfadId);
            Assert.AreEqual(fourthActivity.WfadId, activities4[2].WfadId);
            Assert.AreEqual(fifthActivity.WfadId, activities4[3].WfadId);
            Assert.AreEqual(firstActivity.WfadId, activities4[4].WfadId);

            // We move 5 before 1
            _workflowManager.MoveActivity(wfWorkflowDefinition, 5, 1, false);

            IList<WfActivityDefinition> activities5 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);
            // We should have 1,2,3,4,5,
            Assert.AreEqual(5, activities5.Count);
            Assert.AreEqual(firstActivity.WfadId, activities5[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities5[1].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities5[2].WfadId);
            Assert.AreEqual(fourthActivity.WfadId, activities5[3].WfadId);
            Assert.AreEqual(fifthActivity.WfadId, activities5[4].WfadId);

            // We move 3 after 4
            _workflowManager.MoveActivity(wfWorkflowDefinition, 3, 4, true);

            IList<WfActivityDefinition> activities6 = _workflowManager.GetAllDefaultActivities(wfWorkflowDefinition);
            // We should have 1,2,4,3,5
            Assert.AreEqual(5, activities6.Count);
            Assert.AreEqual(firstActivity.WfadId, activities6[0].WfadId);
            Assert.AreEqual(secondActivity.WfadId, activities6[1].WfadId);
            Assert.AreEqual(fourthActivity.WfadId, activities6[2].WfadId);
            Assert.AreEqual(thirdActivity.WfadId, activities6[3].WfadId);
            Assert.AreEqual(fifthActivity.WfadId, activities6[4].WfadId);
        }



        [TestMethod]
        public void TestWorkflowChangingManualToAutoToManual()
        {
            var container = GetConfiguredContainer();
            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value)
                .Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : 1 rule, 1 condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            RuleDefinition rule1Act2 = new RuleDefinition(null, DateTime.Now, secondActivity.WfadId, "Règle 2");
            RuleConditionDefinition condition1Rule1Act2 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(secondActivity, rule1Act2, new List<RuleConditionDefinition>() { condition1Rule1Act2 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, secondActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter2 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>() { filter2 });

            // Step 3 : 1 rule, 1 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "Règle 2");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, DateTime.Now, thirdActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter3 = new RuleFilterDefinition(null, "Entity", "IN", "ENT,FED", null);
            _workflowManager.AddSelector(thirdActivity, selector3, new List<RuleFilterDefinition>() { filter3 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1, "ENT");

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            IList<WfWorkflowDecision> workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            Assert.AreEqual(3, workflowDecisions.Count);
            Assert.IsFalse(workflowDecisions[0].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[0].Activity.IsValid);
            Assert.IsNull(workflowDecisions[1].Activity);
            Assert.IsNull(workflowDecisions[2].Activity);

            WfDecision wfDecision = new WfDecision();
            wfDecision.Choice = 1;
            wfDecision.Username = "junit";
            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, wfDecision);

            workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            wfWorkflow = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, secondActivity.WfadId);

            Assert.AreEqual(3, workflowDecisions.Count);
            Assert.IsFalse(workflowDecisions[0].Activity.IsAuto);
            Assert.IsTrue(workflowDecisions[0].Activity.IsValid);
            Assert.IsFalse(workflowDecisions[1].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[1].Activity.IsValid);
            Assert.IsNull(workflowDecisions[2].Activity);

            myDummyDtObject.Entity = "FED";
            _workflowManager.RecalculateWorkflow(wfWorkflow);

            workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            currentActivityId = wfWorkflowFetched.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, thirdActivity.WfadId);

            Assert.AreEqual(1, workflowDecisions.Count);
            Assert.IsFalse(workflowDecisions[0].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[0].Activity.IsValid);

            myDummyDtObject.Entity = "ENT";
            _workflowManager.RecalculateWorkflow(wfWorkflow);

            workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            currentActivityId = wfWorkflowFetched.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, secondActivity.WfadId);

            Assert.IsFalse(workflowDecisions[0].Activity.IsAuto);
            Assert.IsTrue(workflowDecisions[0].Activity.IsValid);
            Assert.IsFalse(workflowDecisions[1].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[1].Activity.IsValid);
            Assert.IsFalse(workflowDecisions[2].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[2].Activity.IsValid);

        }

        [TestMethod]
        public void TestWorkflowChangingManualToAutoToManualWithCustomRecalculation()
        {
            var container = GetConfiguredContainer();
            container.RegisterType<Kinetix.Workflow.IWorkflowRecalculationPlugin, Kinetix.Workflow.ValidateExistingDecisionsRecalculationPlugin>("ValidateExistingDecisionsRecalculationPlugin");

            IWorkflowManager _workflowManager = container.Resolve<IWorkflowManager>();
            IAccountManager _accountManager = container.Resolve<IAccountManager>();

            WfWorkflowDefinition wfWorkflowDefinition = new WfWorkflowDefinitionBuilder("WorkflowRules").Build();
            _workflowManager.CreateWorkflowDefinition(wfWorkflowDefinition);

            WfActivityDefinition firstActivity = new WfActivityDefinitionBuilder("Step 1", wfWorkflowDefinition.WfwdId.Value)
                .Build();

            AccountGroup accountGroup = new AccountGroup("1", "dummy group");
            AccountUser account = new AccountUserBuilder("Acc1").Build();
            _accountManager.GetStore().SaveGroup(accountGroup);
            _accountManager.GetStore().SaveAccounts(new List<AccountUser>() { account });
            _accountManager.GetStore().Attach(account.Id, accountGroup.Id);

            // Step 1 : 1 rule, 1 condition
            _workflowManager.AddActivity(wfWorkflowDefinition, firstActivity, 1);
            RuleDefinition rule1Act1 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 1");
            RuleConditionDefinition condition1Rule1Act1 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(firstActivity, rule1Act1, new List<RuleConditionDefinition>() { condition1Rule1Act1 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector1 = new SelectorDefinition(null, DateTime.Now, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : 1 rule, 1 condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            RuleDefinition rule1Act2 = new RuleDefinition(null, DateTime.Now, secondActivity.WfadId, "Règle 2");
            RuleConditionDefinition condition1Rule1Act2 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(secondActivity, rule1Act2, new List<RuleConditionDefinition>() { condition1Rule1Act2 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, DateTime.Now, secondActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter2 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>() { filter2 });

            // Step 3 : 1 rule, 1 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "Règle 2");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3 });
            //Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, DateTime.Now, thirdActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter3 = new RuleFilterDefinition(null, "Entity", "IN", "ENT", null);
            _workflowManager.AddSelector(thirdActivity, selector3, new List<RuleFilterDefinition>() { filter3 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject(1, "ENT");

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            IList<WfWorkflowDecision> workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            int currentActivityId = wfWorkflow.WfaId2.Value;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            Assert.AreEqual(3, workflowDecisions.Count);
            Assert.IsFalse(workflowDecisions[0].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[0].Activity.IsValid);
            Assert.AreEqual(firstActivity.WfadId, workflowDecisions[0].ActivityDefinition.WfadId);
            Assert.IsNull(workflowDecisions[1].Activity);
            Assert.AreEqual(secondActivity.WfadId, workflowDecisions[1].ActivityDefinition.WfadId);
            Assert.IsNull(workflowDecisions[2].Activity);
            Assert.AreEqual(thirdActivity.WfadId, workflowDecisions[2].ActivityDefinition.WfadId);

            WfDecision wfDecision = new WfDecision();
            wfDecision.Choice = 1;
            wfDecision.Username = "junit";
            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, wfDecision);

            workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            wfWorkflow = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, secondActivity.WfadId);

            Assert.AreEqual(3, workflowDecisions.Count);
            Assert.IsFalse(workflowDecisions[0].Activity.IsAuto);
            Assert.IsTrue(workflowDecisions[0].Activity.IsValid);
            Assert.AreEqual(firstActivity.WfadId, workflowDecisions[0].ActivityDefinition.WfadId);
            Assert.IsFalse(workflowDecisions[1].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[1].Activity.IsValid);
            Assert.AreEqual(secondActivity.WfadId, workflowDecisions[1].ActivityDefinition.WfadId);
            Assert.IsNull(workflowDecisions[2].Activity);
            Assert.AreEqual(thirdActivity.WfadId, workflowDecisions[2].ActivityDefinition.WfadId);

            WfDecision wfDecision2 = new WfDecision();
            wfDecision2.Choice = 1;
            wfDecision2.Username = "junit";
            _workflowManager.SaveDecision(wfWorkflow, wfDecision2);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 2, 1, false);

            _workflowManager.RecalculateWorkflow(wfWorkflow);

            workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            currentActivityId = wfWorkflowFetched.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, thirdActivity.WfadId);

            Assert.AreEqual(3, workflowDecisions.Count);
            Assert.IsFalse(workflowDecisions[0].Activity.IsAuto);
            Assert.IsTrue(workflowDecisions[0].Activity.IsValid);
            Assert.AreEqual(secondActivity.WfadId, workflowDecisions[0].ActivityDefinition.WfadId);
            Assert.IsFalse(workflowDecisions[1].Activity.IsAuto);
            Assert.IsTrue(workflowDecisions[1].Activity.IsValid);
            Assert.AreEqual(firstActivity.WfadId, workflowDecisions[1].ActivityDefinition.WfadId);
            Assert.IsFalse(workflowDecisions[2].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[2].Activity.IsValid);
            Assert.AreEqual(thirdActivity.WfadId, workflowDecisions[2].ActivityDefinition.WfadId);

            _workflowManager.MoveActivity(wfWorkflowDefinition, 1, 2, true);

            _workflowManager.RecalculateWorkflow(wfWorkflowFetched);

            workflowDecisions = _workflowManager.GetWorkflowDecisions(wfWorkflow.WfwId.Value);

            wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);

            currentActivityId = wfWorkflowFetched.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            //Assert.AreEqual(firstActivity.WfadId, currentActivity.WfadId);
            Assert.AreEqual(thirdActivity.WfadId, currentActivity.WfadId);

            Assert.IsFalse(workflowDecisions[0].Activity.IsAuto);
            Assert.IsTrue(workflowDecisions[0].Activity.IsValid);
            Assert.AreEqual(firstActivity.WfadId, workflowDecisions[0].ActivityDefinition.WfadId);
            Assert.IsFalse(workflowDecisions[1].Activity.IsAuto);
            Assert.IsTrue(workflowDecisions[1].Activity.IsValid);
            Assert.AreEqual(secondActivity.WfadId, workflowDecisions[1].ActivityDefinition.WfadId);
            Assert.IsFalse(workflowDecisions[2].Activity.IsAuto);
            Assert.IsFalse(workflowDecisions[2].Activity.IsValid);
            Assert.AreEqual(thirdActivity.WfadId, workflowDecisions[2].ActivityDefinition.WfadId);

        }

    }
}
