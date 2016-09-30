using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kinetix.Account;
using Kinetix.Rules;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.Linq;

namespace Kinetix.Workflow {
    public sealed class WorkflowManager : IWorkflowManager {
        private readonly IWorkflowStorePlugin _workflowStorePlugin;
        private readonly IItemStorePlugin _itemStorePlugin;
        private readonly IRuleManager _ruleManager;

        private static readonly string USER_AUTO = "auto";

        public WorkflowManager(IWorkflowStorePlugin workflowStorePlugin, IItemStorePlugin itemStorePlugin, IRuleManager ruleManager) {
            _workflowStorePlugin = workflowStorePlugin;
            _itemStorePlugin = itemStorePlugin;
            _ruleManager = ruleManager;
        }

        public void AddActivity(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityDefinitionToAdd, int position) {

            WfActivityDefinition wfActivityDefinition = _workflowStorePlugin.FindActivityDefinitionByPosition(wfWorkflowDefinition, position);

            if (wfActivityDefinition == null) {
                // Inserting a activity in trail
                int size = _workflowStorePlugin.CountDefaultTransitions(wfWorkflowDefinition);
                Debug.Assert(size == Math.Max(0, position - 2), "Position is not valid");

                wfActivityDefinitionToAdd.Level = position;

                _workflowStorePlugin.CreateActivityDefinition(wfWorkflowDefinition, wfActivityDefinitionToAdd);

                //Find the previous activity to add a link to the newly created
                if (position == 2) {
                    WfTransitionDefinition wfTransitionDefinition = new WfTransitionBuilder(wfWorkflowDefinition.WfwdId, wfWorkflowDefinition.WfadId, wfActivityDefinitionToAdd.WfadId).Build();
                    _workflowStorePlugin.AddTransition(wfTransitionDefinition);
                } else if (position > 2) {
                    WfActivityDefinition wfActivityDefinitionPrevious = _workflowStorePlugin.FindActivityDefinitionByPosition(wfWorkflowDefinition, position - 2);
                    WfTransitionDefinition wfTransitionDefinition = new WfTransitionBuilder(wfWorkflowDefinition.WfwdId, wfActivityDefinitionPrevious.WfadId, wfActivityDefinitionToAdd.WfadId).Build();
                    _workflowStorePlugin.AddTransition(wfTransitionDefinition);
                } else {
                    //Saving starting activity
                    wfWorkflowDefinition.WfadId = wfActivityDefinitionToAdd.WfadId;
                    _workflowStorePlugin.UpdateWorkflowDefinition(wfWorkflowDefinition);
                }


            } else {
                // Inserting an activity inside the default activities "linked list"
                _workflowStorePlugin.CreateActivityDefinition(wfWorkflowDefinition, wfActivityDefinitionToAdd);
                // Automatically move the next activity after the newly created
                MoveActivity(wfWorkflowDefinition, wfActivityDefinitionToAdd, wfActivityDefinition, false);
            }

        }

        public void AddRule(WfActivityDefinition wfActivity, RuleDefinition ruleDefinition, List<RuleConditionDefinition> conditions) {
            Debug.Assert(wfActivity != null);
            Debug.Assert(ruleDefinition != null);
            Debug.Assert(conditions != null);
            // --
            ruleDefinition.ItemId = wfActivity.WfadId;
            _ruleManager.AddRule(ruleDefinition);

            foreach (RuleConditionDefinition ruleConditionDefinition in conditions) {
                ruleConditionDefinition.RudId = ruleDefinition.Id;
                _ruleManager.AddCondition(ruleConditionDefinition);
            }
        }

        public void AddSelector(WfActivityDefinition wfActivity, SelectorDefinition selector, List<RuleFilterDefinition> filters) {
            Debug.Assert(wfActivity != null);
            Debug.Assert(selector != null);
            Debug.Assert(filters != null);
            // --
            selector.ItemId = wfActivity.WfadId;
            _ruleManager.AddSelector(selector);

            foreach (RuleFilterDefinition ruleFilterDefinition in filters) {
                ruleFilterDefinition.SelId = selector.Id;
                _ruleManager.AddFilter(ruleFilterDefinition);
            }
        }

        public void AutoValidateNextActivities(WfWorkflow wfWorkflow, int wfActivityDefinitionId) {
            WfActivityDefinition activityDefinition = _workflowStorePlugin.ReadActivityDefinition(wfActivityDefinitionId);

            object obj = _itemStorePlugin.ReadItem((int)wfWorkflow.ItemId);
            int? wfCurrentActivityId = null;
            while (CanAutoValidateActivity(activityDefinition, obj)) {
                WfActivity wfActivityCurrent = AutoValidateActivity(activityDefinition);
                wfCurrentActivityId = wfActivityCurrent.WfaId;
                if (_workflowStorePlugin.HasNextActivity(wfActivityCurrent) == false) {
                    break;
                }
                activityDefinition = _workflowStorePlugin.FindNextActivity(wfActivityCurrent);
            }

            // Remove this workflow update ?
            if (wfCurrentActivityId != null) {
                wfWorkflow.WfaId2 = wfCurrentActivityId;
                _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
            }
        }

        private WfActivity AutoValidateActivity(WfActivityDefinition wfNextActivityDefinition) {
            //Automatic validation of this activity
            DateTime now = DateTime.Now;

            WfActivity wfActivityCurrent = new WfActivity();
            wfActivityCurrent.CreationDate = now;
            wfActivityCurrent.WfadId = (int)wfNextActivityDefinition.WfadId;
            wfActivityCurrent.WfwId = wfNextActivityDefinition.WfwdId;

            _workflowStorePlugin.CreateActivity(wfActivityCurrent);

            WfDecision decision = new WfDecision();
            decision.Username = USER_AUTO;
            decision.DecisionDate = now;

            return wfActivityCurrent;
        }


        public bool CanAutoValidateActivity(WfActivityDefinition activityDefinition, object obj) {
            RuleConstants ruleConstants = _ruleManager.GetConstants(activityDefinition.WfwdId);

            bool ruleValid = _ruleManager.IsRuleValid((int)activityDefinition.WfadId, obj, ruleConstants);
            IList<AccountUser> accounts = _ruleManager.SelectAccounts((int)activityDefinition.WfadId, obj, ruleConstants);

            bool atLeastOnePerson = accounts.Count > 0;

            // If no rule is defined for validation or no one can validate this activity, we can autovalidate it.
            return ruleValid == false || atLeastOnePerson == false;
        }

        public void CreateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition) {
            _workflowStorePlugin.CreateWorkflowDefinition(wfWorkflowDefinition);
        }

        public WfWorkflow CreateWorkflowInstance(string definitionName, string username, bool userLogic, int item) {
            Debug.Assert(definitionName != null);
            Debug.Assert(username != null);
            //---
            WfWorkflowDefinition wfWorkflowDefinition = _workflowStorePlugin.ReadWorkflowDefinition(definitionName);
            WfWorkflow wfWorkflow = new WfWorkflow();
            wfWorkflow.CreationDate = DateTime.Now;
            wfWorkflow.ItemId = item;
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Cre.ToString();
            wfWorkflow.WfwdId = wfWorkflowDefinition.WfwdId;
            wfWorkflow.UserLogic = userLogic;
            wfWorkflow.Username = username;

            _workflowStorePlugin.CreateWorkflowInstance(wfWorkflow);

            return wfWorkflow;
        }

        public void EndInstance(WfWorkflow wfWorkflow) {
            Debug.Assert(wfWorkflow != null);
            Debug.Assert(WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode) || WfCodeStatusWorkflow.Pau.ToString().Equals(wfWorkflow.WfsCode), "A workflow must be started or paused before ending");
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.End.ToString();
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
        }

        public IList<WfActivityDefinition> GetActivities(WfWorkflow wfWorkflow) {
            WfWorkflowDefinition wfDefinition = _workflowStorePlugin.ReadWorkflowDefinition((int)wfWorkflow.WfwdId);
            IList<WfActivityDefinition> activities = _workflowStorePlugin.FindAllDefaultActivityDefinitions(wfDefinition);

            object obj = _itemStorePlugin.ReadItem((int)wfWorkflow.ItemId);

            IList<WfActivityDefinition> ret = new List<WfActivityDefinition>();
            foreach (WfActivityDefinition activity in activities)
            {
                if (CanAutoValidateActivity(activity, obj) == false)
                {
                    ret.Add(activity);
                }
            }

            return ret;
        }

        public IList<WfActivityDefinition> GetAllDefaultActivities(WfWorkflowDefinition wfWorkflowDefinition)
        {
            return _workflowStorePlugin.FindAllDefaultActivityDefinitions(wfWorkflowDefinition);
        }


        public WfWorkflow GetWorkflowInstance(int wfwId) {
            return _workflowStorePlugin.ReadWorkflowInstanceById(wfwId);
        }

        public void MoveActivity(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivity, WfActivityDefinition wfActivityReferential, bool after) {
            ///TODO
            throw new NotImplementedException();
        }

        public void MoveActivity(WfWorkflowDefinition wfWorkflowDefinition, int src, int dst, bool after) {
            WfActivityDefinition wfActivityDefinitionFrom = _workflowStorePlugin.FindActivityDefinitionByPosition(wfWorkflowDefinition, src);
            WfActivityDefinition wfActivityDefinitionTo = _workflowStorePlugin.FindActivityDefinitionByPosition(wfWorkflowDefinition, dst);
            MoveActivity(wfWorkflowDefinition, wfActivityDefinitionFrom, wfActivityDefinitionTo, after);
        }

        public void PauseInstance(WfWorkflow wfWorkflow) {
            Debug.Assert(wfWorkflow != null);
            Debug.Assert(WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode), "A workflow must be started before pausing");
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Pau.ToString();
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
        }

        public void RemoveActivity(WfActivityDefinition wfActivityDefinition) {
            _workflowStorePlugin.DeleteActivityDefinition(wfActivityDefinition);
        }

        public void RemoveRule(RuleDefinition rule) {
            _ruleManager.RemoveRule(rule);
        }

        public void RemoveSelector(SelectorDefinition selector) {
            _ruleManager.RemoveSelector(selector);
        }

        public void ResumeInstance(WfWorkflow wfWorkflow) {
            Debug.Assert(wfWorkflow != null);
            Debug.Assert(WfCodeStatusWorkflow.Pau.ToString().Equals(wfWorkflow.WfsCode), "A workflow must be paused before resuming");
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Sta.ToString();
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
        }

        public void SaveDecision(WfWorkflow wfWorkflow, WfDecision wfDecision) {
            Debug.Assert(WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode), "A workflow must be started before saving decision");
            //---
            WfActivity currentActivity = _workflowStorePlugin.ReadActivity((int)wfWorkflow.WfaId2);

            // Attach decision to the activity
            wfDecision.WfaId = (int)currentActivity.WfaId;
            _workflowStorePlugin.CreateDecision(wfDecision);
        }

        public void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, WfDecision wfDecision) {
            SaveDecisionAndGoToNextActivity(wfWorkflow, WfCodeTransition.Default.ToString(), wfDecision);
        }

        public void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, string transitionName, WfDecision wfDecision) {
            Debug.Assert(WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode), "A workflow must be started before saving decision");
            //---
            WfActivity currentActivity = _workflowStorePlugin.ReadActivity((int)wfWorkflow.WfaId2);

            // Updating the decision
            SaveDecision(wfWorkflow, wfDecision);

            WfActivityDefinition currentActivityDefinition = _workflowStorePlugin.ReadActivityDefinition(currentActivity.WfadId);

            WfCodeMultiplicityDefinition wfCodeMultiplicityDefinition = (WfCodeMultiplicityDefinition)Enum.Parse(typeof(WfCodeMultiplicityDefinition), currentActivityDefinition.WfmdCode);

            bool canGoToNextActivity = false;

            if (wfCodeMultiplicityDefinition == WfCodeMultiplicityDefinition.Mul) {
                IList<WfDecision> wfDecisions = _workflowStorePlugin.FindAllDecisionByActivity(currentActivity);
                object obj = _itemStorePlugin.ReadItem((int)wfWorkflow.ItemId);
                RuleConstants ruleConstants = _ruleManager.GetConstants((int)wfWorkflow.WfwdId);
                IList<AccountUser> accounts = _ruleManager.SelectAccounts(currentActivity.WfadId, obj, ruleConstants);

                //TODO : better impl than O(n²)
                int match = 0;
                foreach (AccountUser account in accounts) {
                    foreach (WfDecision decision in wfDecisions) {
                        if (account.Id.Equals(decision.Username)) {
                            match++;
                            break;
                        }
                    }
                }

                if (match == accounts.Count) {
                    canGoToNextActivity = true;
                }

            } else {
                canGoToNextActivity = true;
            }

            if (canGoToNextActivity) {

                if (_workflowStorePlugin.HasNextActivity(currentActivity, transitionName)) {
                    WfActivityDefinition nextActivityDefinition = _workflowStorePlugin.FindNextActivity(currentActivity, transitionName);

                    //Autovalidating next activities
                    AutoValidateNextActivities(wfWorkflow, (int)nextActivityDefinition.WfadId);

                    WfActivity lastAutoValidateActivity = _workflowStorePlugin.ReadActivity((int)wfWorkflow.WfaId2);
                    WfActivityDefinition nextActivityDefinitionPrepare = _workflowStorePlugin.FindNextActivity(lastAutoValidateActivity);

                    DateTime now = DateTime.Now;
                    // Creating the next activity to validate.
                    WfActivity nextActivity = new WfActivity();
                    nextActivity.CreationDate = now;
                    nextActivity.WfadId = (int)nextActivityDefinitionPrepare.WfadId;
                    nextActivity.WfwId = (int)wfWorkflow.WfwId;
                    _workflowStorePlugin.CreateActivity(nextActivity);

                    wfWorkflow.WfaId2 = nextActivity.WfaId;
                    _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
                } else {
                    // No next activity to go. Ending the workflow
                    wfWorkflow.WfsCode = WfCodeStatusWorkflow.End.ToString();
                    _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
                }
            }
        }

        public void StartInstance(WfWorkflow wfWorkflow) {
            Debug.Assert(wfWorkflow != null);
            Debug.Assert(WfCodeStatusWorkflow.Cre.ToString().Equals(wfWorkflow.WfsCode), "A workflow must be created before starting");
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Sta.ToString();

            WfWorkflowDefinition wfWorkflowDefinition = _workflowStorePlugin.ReadWorkflowDefinition((int)wfWorkflow.WfwdId);

            WfActivity wfActivityCurrent = new WfActivity();
            wfActivityCurrent.CreationDate = DateTime.Now;
            wfActivityCurrent.WfadId = (int)wfWorkflowDefinition.WfadId;
            wfActivityCurrent.WfwId = (int)wfWorkflow.WfwId;
            _workflowStorePlugin.CreateActivity(wfActivityCurrent);
            wfWorkflow.WfaId2 = wfActivityCurrent.WfaId;
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);

            AutoValidateNextActivities(wfWorkflow, (int)wfWorkflowDefinition.WfadId);
        }

        /// <summary>
        /// Find the workflow by itemId
        /// </summary>
        /// <param name="criteria"></param>
        public WfWorkflow GetWorkflowInstanceByItemId(int wfwdId, int itemId)
        {
            return _workflowStorePlugin.ReadWorkflowInstanceByItemId(wfwdId, itemId);
        }

        /// <summary>
        /// Find activities matching the criteria in parameters
        /// </summary>
        /// <param name="criteria"></param>
        public IList<WfActivityDefinition> FindActivitiesByCriteria(RuleCriteria criteria)
        {
            WfWorkflowDefinition workflow = new WfWorkflowDefinition() { WfwdId = criteria.WfwdId };

            IList<WfActivityDefinition> activities = GetAllDefaultActivities(workflow);
            IDictionary<int?, WfActivityDefinition> dicAct = activities.ToDictionary<WfActivityDefinition, int?>(a => a.WfadId);
            
            IList<int> matchingActivities = _ruleManager.FindItemsByCriteria(criteria, dicAct.Keys.Cast<int>().ToList());

            return matchingActivities.Select(act => dicAct[act]).ToList();
        }


    }
}
