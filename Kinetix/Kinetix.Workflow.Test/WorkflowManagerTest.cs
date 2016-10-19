using System;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using Kinetix.Test;
using Microsoft.Practices.Unity;
using Kinetix.Workflow.model;
using Kinetix.Workflow.Workflow;
using Kinetix.Rules;
using Kinetix.Workflow.instance;
using System.Collections.Generic;
using Kinetix.Account;
using Kinetix.Account.Account;
using System.Diagnostics;

namespace Kinetix.Workflow.Test
{
    [TestClass]
    public class WorkflowManagerTest : MemoryBaseTest
    {

        public override void Register()
        {
            var container = GetConfiguredContainer();
            container.RegisterType<Kinetix.Audit.IAuditManager, Kinetix.Audit.AuditManager>();
            container.RegisterType<Kinetix.Audit.IAuditTraceStorePlugin, Kinetix.Audit.MemoryAuditTraceStorePlugin>();

            container.RegisterType<Kinetix.Account.IAccountStorePlugin, Kinetix.Account.MemoryAccountStorePlugin>(new ContainerControlledLifetimeManager());
            container.RegisterType<Kinetix.Account.IAccountManager, Kinetix.Account.AccountManager>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Workflow.IWorkflowManager, Kinetix.Workflow.WorkflowManager>();
            container.RegisterType<Kinetix.Workflow.IWorkflowStorePlugin, Kinetix.Workflow.MemoryWorkflowStorePlugin>(new ContainerControlledLifetimeManager());
            //container.RegisterType<Kinetix.Workflow.IWorkflowStorePlugin, Kinetix.Workflow.SqlServerWorkflowStorePlugin>();
            container.RegisterType<Kinetix.Workflow.IItemStorePlugin, MemoryItemStorePlugin>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Rules.IRuleManager, Kinetix.Rules.RuleManager>();
            container.RegisterType<Kinetix.Rules.IRuleStorePlugin, Kinetix.Rules.MemoryRuleStorePlugin>(new ContainerControlledLifetimeManager());
            //container.RegisterType<Kinetix.Rules.IRuleStorePlugin, Kinetix.Rules.SqlServerRuleStorePlugin>();
            container.RegisterType<Kinetix.Rules.IRuleConstantsStorePlugin, Kinetix.Rules.MemoryRuleConstantsStore>(new ContainerControlledLifetimeManager());

            container.RegisterType<Kinetix.Rules.IRuleSelectorPlugin, Kinetix.Rules.SimpleRuleSelectorPlugin>();
            container.RegisterType<Kinetix.Rules.IRuleValidatorPlugin, Kinetix.Rules.SimpleRuleValidatorPlugin>();
        }

        private MyDummyDtObject createDummyDtObject()
        {
            var container = GetConfiguredContainer();
            IItemStorePlugin itemStorePlugin = container.Resolve<IItemStorePlugin>();

            MyDummyDtObject myDummyDtObject = new MyDummyDtObject();
            myDummyDtObject.Id = 1;
            myDummyDtObject.Division = "DIV";
            myDummyDtObject.Entity = "ENT";
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
            SelectorDefinition selector1 = new SelectorDefinition(null, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject();

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
            Assert.IsNotNull(wfWorkflowDecision.decisions);
            Assert.AreEqual(1, wfWorkflowDecision.decisions.Count);
        }

        private void assertActivityExist(WfActivityDefinition activityDefinition, WfWorkflowDecision wfWorkflowDecision)
        {
            Assert.AreEqual(activityDefinition.WfadId, wfWorkflowDecision.activityDefinition.WfadId);
            Assert.IsNotNull(wfWorkflowDecision.activity);
            Assert.IsNotNull(wfWorkflowDecision.activity.WfaId);
            Assert.AreEqual(activityDefinition.WfadId, wfWorkflowDecision.activity.WfadId);
        }

        private void assertFirstDecisionEquals(WfDecision wfDecisionAct, WfWorkflowDecision wfWorkflowDecision)
        {
            Assert.AreEqual(wfDecisionAct.WfaId, wfWorkflowDecision.decisions[0].WfaId);
            Assert.AreEqual(wfDecisionAct.Choice, wfWorkflowDecision.decisions[0].Choice);
            Assert.AreEqual(wfDecisionAct.Comments, wfWorkflowDecision.decisions[0].Comments);
            Assert.AreEqual(wfDecisionAct.DecisionDate, wfWorkflowDecision.decisions[0].DecisionDate);
        }

        private void assertHasOneGroup(AccountGroup accountGroup, WfWorkflowDecision wfWorkflowDecision)
        {
            Assert.IsNotNull(wfWorkflowDecision.groups);
            Assert.AreEqual(1, wfWorkflowDecision.groups.Count);
            Assert.AreEqual(accountGroup.Id, wfWorkflowDecision.groups[0].Id);
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
            SelectorDefinition selector1 = new SelectorDefinition(null, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : No rules/condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, secondActivity.WfadId, accountGroup.Id);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>());

            // Step 3 : 1 rule, 2 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "rule 1");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleConditionDefinition condition2Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3, condition2Rule1Act3 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, thirdActivity.WfadId, accountGroup.Id);
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
            SelectorDefinition selector41 = new SelectorDefinition(null, fourthActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter4 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(fourthActivity, selector41, new List<RuleFilterDefinition>() { filter4 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject();

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            IList<WfWorkflowDecision> workflowDecisions = _workflowManager.GetWorkflowDecision(wfWorkflow.WfwId.Value);

            //Step 1,3,4 should be Manual, Step 2 should be auto 
            // No decisons for now
            Assert.IsNotNull(workflowDecisions);
            Assert.AreEqual(workflowDecisions.Count, 3);
            //Check Step 1
            assertActivityExist(firstActivity, workflowDecisions[0]);
            Assert.IsNull(workflowDecisions[0].decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[0]);
            //Check Step 3
            Assert.AreEqual(thirdActivity.WfadId, workflowDecisions[1].activityDefinition.WfadId);
            Assert.IsNull(workflowDecisions[1].activity);
            Assert.IsNull(workflowDecisions[1].decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[1]);
            //Check Step 4
            Assert.AreEqual(fourthActivity.WfadId, workflowDecisions[2].activityDefinition.WfadId);
            Assert.IsNull(workflowDecisions[2].activity);
            Assert.IsNull(workflowDecisions[2].decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[2]);


            // Entry actions should NOT validate all activities.
            int currentActivityId = (int)wfWorkflow.WfaId2;
            WfActivity currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, firstActivity.WfadId);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched);
            Assert.AreEqual(wfWorkflowFetched.WfaId2, firstActivity.WfadId);

            WfDecision decision = new WfDecision();
            decision.Choice = 1;
            decision.Comments = "abc";
            decision.Username = "AA";
            decision.DecisionDate = DateTime.Now;

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, decision);

            workflowDecisions = _workflowManager.GetWorkflowDecision(wfWorkflow.WfwId.Value);

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
            Assert.IsNull(workflowDecisions[1].decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[1]);
            //Check Step 4
            Assert.AreEqual(fourthActivity.WfadId, workflowDecisions[2].activityDefinition.WfadId);
            Assert.IsNull(workflowDecisions[2].activity);
            Assert.IsNull(workflowDecisions[2].decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[2]);

            // Activity 1 should now be validated.
            // No rule defined for activity 2. Activity 2 should be autovalidated
            // The current activity should be now activity 3
            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, thirdActivity.WfadId);

            WfWorkflow wfWorkflowFetched2 = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched2);
            Assert.AreEqual(wfWorkflowFetched2.WfaId2, thirdActivity.WfadId);

            //Manually validating activity 3
            WfDecision wfDecisionAct3 = new WfDecision();
            wfDecisionAct3.Choice = 1;
            wfDecisionAct3.Username = account.Id;
            wfDecisionAct3.WfaId = currentActivity.WfaId.Value;

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, wfDecisionAct3);

            workflowDecisions = _workflowManager.GetWorkflowDecision(wfWorkflow.WfwId.Value);

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
            Assert.AreEqual(fourthActivity.WfadId, workflowDecisions[2].activityDefinition.WfadId);
            Assert.IsNotNull(workflowDecisions[2].activity);
            Assert.IsNull(workflowDecisions[2].decisions);
            assertHasOneGroup(accountGroup, workflowDecisions[2]);

            // Activity 3 should now be validated.
            // The current activity should be now activity 4
            currentActivityId = wfWorkflow.WfaId2.Value;
            currentActivity = _workflowManager.GetActivity(currentActivityId);
            Assert.AreEqual(currentActivity.WfadId, fourthActivity.WfadId);

            WfWorkflow wfWorkflowFetched3 = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched3);
            Assert.AreEqual(wfWorkflowFetched3.WfaId2, fourthActivity.WfadId);

            // Manually validating activity 4
            WfDecision wfDecisionAct4 = new WfDecision();
            wfDecisionAct4.Choice = 1;
            wfDecisionAct4.Username = account.Id;
            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, wfDecisionAct4);

            workflowDecisions = _workflowManager.GetWorkflowDecision(wfWorkflow.WfwId.Value);

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

            Assert.IsNotNull(workflowDecisions[2].groups);
            Assert.AreEqual(1, workflowDecisions[2].groups.Count);
            Assert.AreEqual(accountGroup.Id, workflowDecisions[2].groups[0].Id);

            // Activity 4 should now be validated. The current activity is now activity 4, with the end status
            currentActivityId = wfWorkflow.WfaId2.Value;
            Assert.AreEqual(currentActivityId, fourthActivity.WfadId);

            // No Automatic ending.
            //Assert.AreEqual(wfWorkflow.WfsCode, WfCodeStatusWorkflow.End.ToString());
            Assert.AreEqual(wfWorkflow.WfsCode, WfCodeStatusWorkflow.Sta.ToString());

            WfWorkflow wfWorkflowFetched5 = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            //Assert.AreEqual(wfWorkflowFetched5.WfsCode, WfCodeStatusWorkflow.End.ToString());
            Assert.AreEqual(wfWorkflowFetched5.WfsCode, WfCodeStatusWorkflow.Sta.ToString());
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

            MyDummyDtObject myDummyDtObject = createDummyDtObject();

            WfWorkflow wfWorkflow = _workflowManager.CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, "JUnit", false, myDummyDtObject.Id);

            // Starting the workflow
            _workflowManager.StartInstance(wfWorkflow);

            IList<WfWorkflowDecision> workflowDecisions1 = _workflowManager.GetWorkflowDecision(wfWorkflow.WfwId.Value);

            // Entry actions should validate all activities (because no group have been associated).
            int currentActivity = wfWorkflow.WfaId2.Value;
            Assert.AreEqual(currentActivity, fourthActivity.WfadId);

            WfWorkflow wfWorkflowFetched = _workflowManager.GetWorkflowInstance(wfWorkflow.WfwId.Value);
            Assert.IsNotNull(wfWorkflowFetched);
            Assert.AreEqual(currentActivity, fourthActivity.WfadId);
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
            SelectorDefinition selector1 = new SelectorDefinition(null, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : No rules/condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, secondActivity.WfadId, accountGroup.Id);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>());

            // Step 3 : 1 rule, 2 conditions
            WfActivityDefinition thirdActivity = new WfActivityDefinitionBuilder("Step 3", (int)wfWorkflowDefinition.WfwdId).Build();
            _workflowManager.AddActivity(wfWorkflowDefinition, thirdActivity, 3);
            RuleDefinition rule1Act3 = new RuleDefinition(null, DateTime.Now, thirdActivity.WfadId, "rule 1");
            RuleConditionDefinition condition1Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            RuleConditionDefinition condition2Rule1Act3 = new RuleConditionDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddRule(thirdActivity, rule1Act3, new List<RuleConditionDefinition>() { condition1Rule1Act3, condition2Rule1Act3 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector3 = new SelectorDefinition(null, thirdActivity.WfadId, accountGroup.Id);
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
            SelectorDefinition selector41 = new SelectorDefinition(null, fourthActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter4 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(fourthActivity, selector41, new List<RuleFilterDefinition>() { filter4 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject();

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
            SelectorDefinition selector1 = new SelectorDefinition(null, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject();

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
            SelectorDefinition selector0 = new SelectorDefinition(null, activityZero.WfadId, accountGroup.Id);
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

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, wfDecision);

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
            SelectorDefinition selector1 = new SelectorDefinition(null, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject();

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
            SelectorDefinition selector2 = new SelectorDefinition(null, activityEnd.WfadId, accountGroup.Id);
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
            SelectorDefinition selector1 = new SelectorDefinition(null, firstActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter1 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(firstActivity, selector1, new List<RuleFilterDefinition>() { filter1 });

            // Step 2 : 1 rule, 1 condition
            WfActivityDefinition secondActivity = new WfActivityDefinitionBuilder("Step 2", wfWorkflowDefinition.WfwdId.Value).Build();

            _workflowManager.AddActivity(wfWorkflowDefinition, secondActivity, 2);
            RuleDefinition rule1Act2 = new RuleDefinition(null, DateTime.Now, firstActivity.WfadId, "Règle 2");
            RuleConditionDefinition condition1Rule1Act2 = new RuleConditionDefinition(null, "Entity", "IN", "ENT,FED,GFE", null);
            _workflowManager.AddRule(secondActivity, rule1Act2, new List<RuleConditionDefinition>() { condition1Rule1Act2 });
            // Selector/filter to validate the activity (preventing auto validation when no one is linked to an activity)
            SelectorDefinition selector2 = new SelectorDefinition(null, secondActivity.WfadId, accountGroup.Id);
            RuleFilterDefinition filter2 = new RuleFilterDefinition(null, "Entity", "=", "ENT", null);
            _workflowManager.AddSelector(secondActivity, selector2, new List<RuleFilterDefinition>() { filter2 });

            MyDummyDtObject myDummyDtObject = createDummyDtObject();

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

            _workflowManager.SaveDecisionAndGoToNextActivity(wfWorkflow, wfDecision);

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
    }
}
