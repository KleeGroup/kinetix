using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kinetix.Account;
using Kinetix.Rules;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.Linq;
using Kinetix.Workflow.Workflow;
using System.ServiceModel;


namespace Kinetix.Workflow {
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public sealed class WorkflowManager : IWorkflowManager {
        private readonly IWorkflowStorePlugin _workflowStorePlugin;
        private readonly IItemStorePlugin _itemStorePlugin;
        private readonly IRuleManager _ruleManager;
        private readonly IAccountManager _accountManager;

        private static readonly string USER_AUTO = "auto";

        public WorkflowManager(IWorkflowStorePlugin workflowStorePlugin, IItemStorePlugin itemStorePlugin, IRuleManager ruleManager, IAccountManager accountManager) {
            _workflowStorePlugin = workflowStorePlugin;
            _itemStorePlugin = itemStorePlugin;
            _ruleManager = ruleManager;
            _accountManager = accountManager;
        }

        public void AddActivity(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityDefinitionToAdd, int position) {

            WfActivityDefinition wfActivityDefinition = _workflowStorePlugin.FindActivityDefinitionByPosition(wfWorkflowDefinition, position);

            wfActivityDefinitionToAdd.Level = position;

            if (wfActivityDefinition == null) {
                // Inserting a activity in trail
                int size = _workflowStorePlugin.CountDefaultTransitions(wfWorkflowDefinition);
                Debug.Assert(size == Math.Max(0, position - 2), "Position is not valid");

                _workflowStorePlugin.CreateActivityDefinition(wfWorkflowDefinition, wfActivityDefinitionToAdd);

                //Find the previous activity to add a link to the newly created
                if (position == 2) {
                    WfTransitionDefinition wfTransitionDefinition = new WfTransitionBuilder(wfWorkflowDefinition.WfwdId, wfWorkflowDefinition.WfadId, wfActivityDefinitionToAdd.WfadId).Build();
                    _workflowStorePlugin.AddTransition(wfTransitionDefinition);
                } else if (position > 2) {
                    WfActivityDefinition wfActivityDefinitionPrevious = _workflowStorePlugin.FindActivityDefinitionByPosition(wfWorkflowDefinition, position - 1);
                    WfTransitionDefinition wfTransitionDefinition = new WfTransitionBuilder(wfWorkflowDefinition.WfwdId, wfActivityDefinitionPrevious.WfadId, wfActivityDefinitionToAdd.WfadId).Build();
                    _workflowStorePlugin.AddTransition(wfTransitionDefinition);
                } else {
                    //Saving starting activity
                    wfWorkflowDefinition.WfadId = wfActivityDefinitionToAdd.WfadId;
                    _workflowStorePlugin.UpdateWorkflowDefinition(wfWorkflowDefinition);
                }


            } else {

                _workflowStorePlugin.IncrementActivityDefinitionPositionsAfter(wfWorkflowDefinition.WfwdId.Value, position);

                // Inserting an activity inside the default activities "linked list"
                _workflowStorePlugin.CreateActivityDefinition(wfWorkflowDefinition, wfActivityDefinitionToAdd);
                
                if (position > 1)
                {
                    // Automatically move the next activity after the newly created
                    InsertActivityBefore(wfWorkflowDefinition, wfActivityDefinitionToAdd, wfActivityDefinition);
                }
                else
                {
                    // position == 1
                    WfTransitionDefinition wfTransitionDefinition = new WfTransitionBuilder(wfWorkflowDefinition.WfwdId, wfActivityDefinitionToAdd.WfadId, wfActivityDefinition.WfadId).Build();
                    _workflowStorePlugin.AddTransition(wfTransitionDefinition);
                    wfWorkflowDefinition.WfadId = wfActivityDefinitionToAdd.WfadId;
                    _workflowStorePlugin.UpdateWorkflowDefinition(wfWorkflowDefinition);
                }
                
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

        private WfActivity CreateActivity(WfActivityDefinition activityDefinition, WfWorkflow wfWorkflow, bool isAuto)
        {
            WfActivity wfActivity = new WfActivity();
            wfActivity.CreationDate = DateTime.Now;
            wfActivity.WfadId = activityDefinition.WfadId.Value;
            wfActivity.WfwId = wfWorkflow.WfwId.Value;
            wfActivity.IsAuto = isAuto;

            _workflowStorePlugin.CreateActivity(wfActivity);
            return wfActivity;
        }

        public bool AutoValidateNextActivities(WfWorkflow wfWorkflow, WfActivity currentActivity, int wfActivityDefinitionId)
        {
            WfActivityDefinition activityDefinition = _workflowStorePlugin.ReadActivityDefinition(wfActivityDefinitionId);

            object obj = _itemStorePlugin.ReadItem(wfWorkflow.ItemId.Value);
            int? wfCurrentActivityId = null;
            bool endReached = false;
            WfActivity wfActivityCurrent = currentActivity;
            while (CanAutoValidateActivity(activityDefinition, obj))
            {
                AutoValidateDecision(wfActivityCurrent);

                if (_workflowStorePlugin.HasNextActivity(wfActivityCurrent) == false)
                {
                    endReached = true;
                    break;
                }
                activityDefinition = _workflowStorePlugin.FindNextActivity(wfActivityCurrent.WfadId);

                WfActivity nextActivity = _workflowStorePlugin.FindActivityByDefinitionWorkflow(wfWorkflow, activityDefinition);
                if (nextActivity == null)
                {
                    wfActivityCurrent = CreateActivity(activityDefinition, wfWorkflow, false);
                }
                else
                {
                    wfActivityCurrent = nextActivity;
                }

                wfCurrentActivityId = wfActivityCurrent.WfaId;
            }

            // Remove this workflow update ?
            if (wfCurrentActivityId != null)
            {
                wfWorkflow.WfaId2 = wfCurrentActivityId;
                _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
            }
            return endReached;
        }

        private void AutoValidateDecision(WfActivity wfActivityCurrent)
        {

            wfActivityCurrent.IsAuto = true;
            _workflowStorePlugin.UpdateActivity(wfActivityCurrent);

            WfDecision decision = new WfDecision();
            decision.Username = USER_AUTO;
            decision.DecisionDate = DateTime.Now; 
            decision.WfaId = wfActivityCurrent.WfaId.Value;

            _workflowStorePlugin.CreateDecision(decision);
        }


        public bool CanAutoValidateActivity(WfActivityDefinition activityDefinition, object obj) {
            RuleConstants ruleConstants = _ruleManager.GetConstants(activityDefinition.WfwdId);

            bool ruleValid = _ruleManager.IsRuleValid(activityDefinition.WfadId.Value, obj, ruleConstants);
            IList<AccountUser> accounts = _ruleManager.SelectAccounts(activityDefinition.WfadId.Value, obj, ruleConstants);

            bool atLeastOnePerson = accounts.Count > 0;

            // If no rule is defined for validation or no one can validate this activity, we can autovalidate it.
            return ruleValid == false || atLeastOnePerson == false;
        }


        public void CreateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition) {
            _workflowStorePlugin.CreateWorkflowDefinition(wfWorkflowDefinition);
        }


        public WfWorkflow CreateWorkflowInstance(int wfwdId, string username, bool userLogic, int item)
        {
            WfWorkflow wfWorkflow = new WfWorkflow();
            wfWorkflow.CreationDate = DateTime.Now;
            wfWorkflow.ItemId = item;
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Cre.ToString();
            wfWorkflow.WfwdId = wfwdId;
            wfWorkflow.UserLogic = userLogic;
            wfWorkflow.Username = username;

            _workflowStorePlugin.CreateWorkflowInstance(wfWorkflow);
            return wfWorkflow;
        }

        public WfWorkflow CreateWorkflowInstance(string definitionName, string username, bool userLogic, int item) {
            Debug.Assert(definitionName != null);
            Debug.Assert(username != null);
            //---
            WfWorkflowDefinition wfWorkflowDefinition = _workflowStorePlugin.ReadWorkflowDefinition(definitionName);

            return CreateWorkflowInstance(wfWorkflowDefinition.WfwdId.Value, username, userLogic, item);
        }

        public void EndInstance(WfWorkflow wfWorkflow) {
            Debug.Assert(wfWorkflow != null);
            if (!(WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode) || WfCodeStatusWorkflow.Pau.ToString().Equals(wfWorkflow.WfsCode)))
            {
                throw new System.InvalidOperationException("A workflow must be started or paused before ending");
            }
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.End.ToString();
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
        }

        public IList<WfActivityDefinition> GetActivityDefinitions(WfWorkflow wfWorkflow) {
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


        public IList<WfActivity> GetActivities(WfWorkflow wfWorkflow, IList<int> wfadId)
        {
            return _workflowStorePlugin.FindActivitiesByDefinitionId(wfWorkflow, wfadId);
        }

        public IList<WfActivityDefinition> GetAllDefaultActivities(WfWorkflowDefinition wfWorkflowDefinition)
        {
            return _workflowStorePlugin.FindAllDefaultActivityDefinitions(wfWorkflowDefinition);
        }


        public WfWorkflow GetWorkflowInstance(int wfwId) {
            return _workflowStorePlugin.ReadWorkflowInstanceById(wfwId);
        }

        private void InsertActivityBefore(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityToAdd, WfActivityDefinition wfActivityReferential)
        {
            WfTransitionCriteria wfTransitionCriteria = new WfTransitionCriteria();
            wfTransitionCriteria.TransitionName = WfCodeTransition.Default.ToString();
            wfTransitionCriteria.WfadIdTo = wfActivityReferential.WfadId.Value;

            WfTransitionDefinition transition = _workflowStorePlugin.FindTransition(wfTransitionCriteria);
            transition.WfadIdTo = wfActivityToAdd.WfadId.Value;

            _workflowStorePlugin.UpdateTransition(transition);

            WfTransitionDefinition wfTransitionDefinition = new WfTransitionBuilder(wfWorkflowDefinition.WfwdId, wfActivityToAdd.WfadId, wfActivityReferential.WfadId).Build();
            _workflowStorePlugin.AddTransition(wfTransitionDefinition);
        }

        public void MoveActivity(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityToMove, WfActivityDefinition wfActivityReferential, bool after)
        {
            Debug.Assert(wfActivityToMove.WfadId != null);
            Debug.Assert(wfActivityReferential.WfadId != null);
            Debug.Assert(!wfActivityToMove.WfadId.Equals(wfActivityReferential.WfadId));
            //---
            if (after)
            {
                MoveActivityAfter(wfWorkflowDefinition, wfActivityToMove, wfActivityReferential);
            }
            else
            {
                MoveActivityBefore(wfWorkflowDefinition, wfActivityToMove, wfActivityReferential);
            }
        }

        private void MoveActivityAfter(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityToMove, WfActivityDefinition wfActivityReferential)
        {
            // T1
            WfTransitionCriteria critTrFromRef = new WfTransitionCriteria();
            critTrFromRef.WfadIdFrom = wfActivityReferential.WfadId;
            critTrFromRef.TransitionName = WfCodeTransition.Default.ToString();
            WfTransitionDefinition trFromRef = _workflowStorePlugin.FindTransition(critTrFromRef);

            if (trFromRef != null && trFromRef.WfadIdTo.Equals(wfActivityToMove.WfadId))
            {
                // The activity is already positonned after the ref activity.
                // Nothing to do in that case.
                return;
            }

            // T2
            WfTransitionCriteria critTrFromMove = new WfTransitionCriteria();
            critTrFromMove.WfadIdFrom = wfActivityToMove.WfadId;
            critTrFromMove.TransitionName = WfCodeTransition.Default.ToString();
            WfTransitionDefinition trFromMove = _workflowStorePlugin.FindTransition(critTrFromMove);

            // T3
            WfTransitionCriteria critTrToMove = new WfTransitionCriteria();
            critTrToMove.WfadIdFrom = wfActivityToMove.WfadId;
            critTrToMove.TransitionName = WfCodeTransition.Default.ToString();
            WfTransitionDefinition trToMove = _workflowStorePlugin.FindTransition(critTrToMove);

            // Update T3
            if (trToMove == null)
            {
                //No transition before Move. Move is the first Activity of the WorkflowDefinition
                wfWorkflowDefinition.WfadId = trFromMove.WfadIdTo;
                _workflowStorePlugin.UpdateWorkflowDefinition(wfWorkflowDefinition);
            }
            else
            {
                // Update T3
                trToMove.WfadIdTo = trFromMove.WfadIdTo;
                _workflowStorePlugin.UpdateTransition(trToMove);
            }

            // Update T1/T2
            if (trFromRef == null)
            {
                //No transition after T1. 
                trFromMove.WfadIdFrom = wfActivityReferential.WfadId.Value;
                trFromMove.WfadIdTo = wfActivityToMove.WfadId.Value;
                _workflowStorePlugin.UpdateTransition(trFromMove);
            }
            else
            {
                // Moving T2
                trFromMove.WfadIdTo = trFromRef.WfadIdTo;
                _workflowStorePlugin.UpdateTransition(trFromMove);

                // Moving T1
                trFromRef.WfadIdTo = wfActivityToMove.WfadId.Value;
                _workflowStorePlugin.UpdateTransition(trFromRef);
            }

        }

        private void MoveActivityBefore(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityToMove, WfActivityDefinition wfActivityReferential) {

            // T1
            WfTransitionCriteria critTrToRef = new WfTransitionCriteria();
            critTrToRef.WfadIdTo = wfActivityReferential.WfadId;
            critTrToRef.TransitionName = WfCodeTransition.Default.ToString();
            WfTransitionDefinition trToRef = _workflowStorePlugin.FindTransition(critTrToRef);

            if (trToRef != null && trToRef.WfadIdFrom.Equals(wfActivityToMove.WfadId))
            {
                //The activity is already positonned before the ref activity.
                // Nothing to do in that case.
                return;
            }

            // T2
            WfTransitionCriteria critTrFromMove = new WfTransitionCriteria();
            critTrFromMove.WfadIdFrom = wfActivityToMove.WfadId;
            critTrFromMove.TransitionName = WfCodeTransition.Default.ToString();
            WfTransitionDefinition trFromMove = _workflowStorePlugin.FindTransition(critTrFromMove);

            // T3
            WfTransitionCriteria critTrToMove = new WfTransitionCriteria();
            critTrToMove.WfadIdFrom = wfActivityToMove.WfadId;
            critTrToMove.TransitionName = WfCodeTransition.Default.ToString();
            WfTransitionDefinition trToMove = _workflowStorePlugin.FindTransition(critTrToMove);

            // Update T1
            if (trToRef == null)
            {
                //No transition before Ref. Ref is the first Activity of the WorkflowDefinition
                wfWorkflowDefinition.WfadId = wfActivityToMove.WfadId.Value;
                _workflowStorePlugin.UpdateWorkflowDefinition(wfWorkflowDefinition);
            }
            else
            {
                // Moving T1
                trToRef.WfadIdTo = wfActivityToMove.WfadId.Value;
                _workflowStorePlugin.UpdateTransition(trToRef);
            }

            // Update T2
            //If there is no Activity after the activity to move. No transition should be modified
            if (trFromMove != null)
            {
                // Moving T2
                trFromMove.WfadIdTo = wfActivityReferential.WfadId.Value;
                _workflowStorePlugin.UpdateTransition(trToRef);
            }

            // Update T3
            if (trToMove == null)
            {
                //No transition before T3. Move is the first Activity of the WorkflowDefinition
                wfWorkflowDefinition.WfadId = wfActivityToMove.WfadId.Value;
                _workflowStorePlugin.UpdateWorkflowDefinition(wfWorkflowDefinition);
            }
            else
            {
                // Moving T3
                trToMove.WfadIdTo = trFromMove.WfadIdTo;
                _workflowStorePlugin.UpdateTransition(trToMove);
            }
        }

        public void MoveActivity(WfWorkflowDefinition wfWorkflowDefinition, int src, int dst, bool after) {
            WfActivityDefinition wfActivityDefinitionFrom = _workflowStorePlugin.FindActivityDefinitionByPosition(wfWorkflowDefinition, src);
            WfActivityDefinition wfActivityDefinitionTo = _workflowStorePlugin.FindActivityDefinitionByPosition(wfWorkflowDefinition, dst);

            MoveActivity(wfWorkflowDefinition, wfActivityDefinitionFrom, wfActivityDefinitionTo, after);
        }

        public void PauseInstance(WfWorkflow wfWorkflow) {
            Debug.Assert(wfWorkflow != null);
            if (!WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode))
            {
                throw new System.InvalidOperationException("A workflow must be started before pausing");
            }
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Pau.ToString();
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
        }

        public void RemoveActivity(WfActivityDefinition wfActivityDefinition) {

            WfWorkflowDefinition wfD = _workflowStorePlugin.ReadWorkflowDefinition(wfActivityDefinition.WfwdId);

            IList<RuleDefinition> rules = _ruleManager.GetRulesForItemId(wfActivityDefinition.WfadId.Value);
            IList<SelectorDefinition> selectors = _ruleManager.GetSelectorsForItemId(wfActivityDefinition.WfadId.Value);
            _ruleManager.RemoveRules(rules);
            _ruleManager.RemoveSelectors(selectors);

            _workflowStorePlugin.UnsetCurrentActivity(wfActivityDefinition);

            _workflowStorePlugin.DeleteActivities(wfActivityDefinition.WfadId.Value);

            WfTransitionCriteria critFrom = new WfTransitionCriteria();
            critFrom.WfadIdFrom = wfActivityDefinition.WfadId;
            critFrom.TransitionName = WfCodeTransition.Default.ToString();
            WfTransitionDefinition transitionFrom = _workflowStorePlugin.FindTransition(critFrom);

            if (wfD.WfwdId.Equals(wfActivityDefinition.WfwdId))
            {
                //The Activity Definition to remove is the start activity

                if (transitionFrom != null)
                {
                    // The first activity definition will be the next definition
                    wfD.WfadId = transitionFrom.WfadIdTo;
                    _workflowStorePlugin.UpdateWorkflowDefinition(wfD);
                    _workflowStorePlugin.RemoveTransition(transitionFrom);
                }
            }
            else
            {
                //The Activity Definition to remove is NOT the start activity
                WfTransitionCriteria critTo = new WfTransitionCriteria();
                critTo.WfadIdFrom = wfActivityDefinition.WfadId;
                critTo.TransitionName = WfCodeTransition.Default.ToString();
                WfTransitionDefinition transitionTo = _workflowStorePlugin.FindTransition(critTo);

                if (transitionFrom != null)
                {
                    _workflowStorePlugin.RemoveTransition(transitionFrom);
                    transitionTo.WfadIdTo = transitionFrom.WfadIdTo;
                    _workflowStorePlugin.UpdateTransition(transitionTo);
                }
                else
                {
                    // Last acitivity
                    _workflowStorePlugin.RemoveTransition(transitionFrom);
                    transitionTo.WfadIdTo = transitionFrom.WfadIdTo;
                    _workflowStorePlugin.UpdateTransition(transitionTo);
                }
            }

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
            if (!WfCodeStatusWorkflow.Pau.ToString().Equals(wfWorkflow.WfsCode))
            {
                throw new InvalidOperationException("A workflow must be paused before resuming");
            }
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Sta.ToString();
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
        }

        public void SaveDecision(WfWorkflow wfWorkflow, WfDecision wfDecision) {
            if (!WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode))
            {
                throw new InvalidOperationException("A workflow must be started before saving decision");
            }
            //---
            WfWorkflow wfWorkflowFetch = _workflowStorePlugin.ReadWorkflowInstanceForUpdateById(wfWorkflow.WfwId.Value);

            if (wfWorkflowFetch.WfaId2 != null && !wfWorkflow.WfaId2.Equals(wfWorkflow.WfaId2))
            {
                throw new InvalidOperationException("Concurrent workflow modification");
            }

            WfActivity currentActivity = _workflowStorePlugin.ReadActivity(wfWorkflow.WfaId2.Value);

            // Attach decision to the activity
            currentActivity.IsAuto = false;
            _workflowStorePlugin.UpdateActivity(currentActivity);

            wfDecision.WfaId = currentActivity.WfaId.Value;
            if (wfDecision.Id == null)
            {
                _workflowStorePlugin.CreateDecision(wfDecision);
            }
            else
            {
                _workflowStorePlugin.UpdateDecision(wfDecision);
            }
        }

        public WfDecision GetDecision(WfActivity wfActivity)
        {
            Debug.Assert(wfActivity != null);
            //---
            WfActivityDefinition wfActivityDefinition = _workflowStorePlugin.ReadActivityDefinition(wfActivity.WfadId);
            WfCodeMultiplicityDefinition multiplicity = (WfCodeMultiplicityDefinition) Enum.Parse(typeof(WfCodeMultiplicityDefinition), wfActivityDefinition.WfmdCode, true);

            if (multiplicity != WfCodeMultiplicityDefinition.Sin)
            {
                throw new InvalidOperationException();
            }
            IList<WfDecision> decision = _workflowStorePlugin.ReadDecisionsByActivityId(wfActivity.WfaId.Value);
            if (decision.Count == 0)
            {
                return null;
            }
            else
            {
                return decision[0];
            }
        }

        public IList<WfDecision> GetDecisions(WfActivity wfActivity)
        {
            Debug.Assert(wfActivity != null);
            //---
            WfActivityDefinition wfActivityDefinition = _workflowStorePlugin.ReadActivityDefinition(wfActivity.WfadId);
            WfCodeMultiplicityDefinition multiplicity = (WfCodeMultiplicityDefinition)Enum.Parse(typeof(WfCodeMultiplicityDefinition), wfActivityDefinition.WfmdCode, true);

            if (multiplicity != WfCodeMultiplicityDefinition.Mul)
            {
                throw new InvalidOperationException();
            }
            return _workflowStorePlugin.ReadDecisionsByActivityId(wfActivity.WfaId.Value);
        }

        public void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, WfDecision wfDecision) {
            SaveDecisionAndGoToNextActivity(wfWorkflow, WfCodeTransition.Default.ToString(), wfDecision);
        }

        public void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, string transitionName, WfDecision wfDecision) {
            if (!WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode))
            {
                throw new System.InvalidOperationException("A workflow must be started before saving a decision");
            }
            //---
            WfActivity currentActivity = _workflowStorePlugin.ReadActivity(wfWorkflow.WfaId2.Value);

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
                    WfActivityDefinition nextActivityDefinition = _workflowStorePlugin.FindNextActivity(currentActivity.WfadId, transitionName);

                    WfActivity nextActivity = _workflowStorePlugin.FindActivityByDefinitionWorkflow(wfWorkflow, nextActivityDefinition);
                    if (nextActivity == null)
                    {
                        nextActivity = new WfActivity();
                    }
                    // Creating the next activity to validate.
                    nextActivity.CreationDate = DateTime.Now;
                    nextActivity.WfadId = nextActivityDefinition.WfadId.Value;
                    nextActivity.WfwId = wfWorkflow.WfwId.Value;
                    nextActivity.IsAuto = false;
                    if (nextActivity.WfaId == null)
                    {
                        _workflowStorePlugin.CreateActivity(nextActivity);
                    }
                    else
                    {
                        _workflowStorePlugin.UpdateActivity(nextActivity);
                    }
                    

                    wfWorkflow.WfaId2 = nextActivity.WfaId;
                    _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);

                    //Autovalidating next activities
                    bool endReached = AutoValidateNextActivities(wfWorkflow, nextActivity, nextActivityDefinition.WfadId.Value);

                    if (endReached)
                    {
                        // Stepping back : No Automatic ending. 
                        // TODO: Remove the commented code when the behavior will be validated
                        //EndInstance(wfWorkflow);
                    }
                   
                } else {
                    // No next activity to go. Ending the workflow

                    // Stepping back : No Automatic ending. 
                    // TODO: Remove the commented code when the behavior will be validated
                    //EndInstance(wfWorkflow);
                }
            }
        }

        public void StartInstance(WfWorkflow wfWorkflow)
        {
            Debug.Assert(wfWorkflow != null);
            if (!WfCodeStatusWorkflow.Cre.ToString().Equals(wfWorkflow.WfsCode))
            {
                throw new System.InvalidOperationException("A workflow must be created before starting");
            }
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Sta.ToString();

            WfWorkflowDefinition wfWorkflowDefinition = _workflowStorePlugin.ReadWorkflowDefinition((int)wfWorkflow.WfwdId);

            WfActivity wfActivityCurrent = new WfActivity();
            wfActivityCurrent.CreationDate = DateTime.Now;
            wfActivityCurrent.WfadId = (int)wfWorkflowDefinition.WfadId;
            wfActivityCurrent.WfwId = (int)wfWorkflow.WfwId;
            wfActivityCurrent.IsAuto = false;
            _workflowStorePlugin.CreateActivity(wfActivityCurrent);
            wfWorkflow.WfaId2 = wfActivityCurrent.WfaId;
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);

            bool endReached = AutoValidateNextActivities(wfWorkflow, wfActivityCurrent, (int)wfWorkflowDefinition.WfadId);

            if (endReached)
            {
                // Stepping back : No Automatic ending. 
                // TODO: Remove the commented code when the behavior will be validated
                //EndInstance(wfWorkflow);
            }
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

        public WfActivity GetActivity(int wfaId)
        {
            return _workflowStorePlugin.ReadActivity(wfaId);
        }

        public IList<RuleConditionDefinition> GetConditionsForRuleId(int ruleId)
        {
            return _ruleManager.GetConditionsForRuleId(ruleId);
        }

        public IList<RuleFilterDefinition> GetFiltersForSelectorId(int selectorId)
        {
            return _ruleManager.GetFiltersForSelectorId(selectorId);
        }

        public IList<RuleDefinition> GetRulesForActivityDefinition(int wfadId)
        {
            return _ruleManager.GetRulesForItemId(wfadId);
        }

        public IList<SelectorDefinition> GetSelectorsForActivityDefinition(int wfadId)
        {
            return _ruleManager.GetSelectorsForItemId(wfadId);
        }

        public WfWorkflowDefinition GetWorkflowDefinition(int wfwdId)
        {
            return _workflowStorePlugin.ReadWorkflowDefinition(wfwdId);
        }

        public WfWorkflowDefinition GetWorkflowDefinition(string wfdName)
        {
            return _workflowStorePlugin.ReadWorkflowDefinition(wfdName);
        }

        public void RemoveRules(IList<RuleDefinition> rules)
        {
            _ruleManager.RemoveRules(rules);
        }

        public void RemoveSelectors(IList<SelectorDefinition> selectors)
        {
            _ruleManager.RemoveSelectors(selectors);
        }

        public void DeleteDecision(WfDecision wfDecision)
        {
            Debug.Assert(wfDecision != null);
            Debug.Assert(wfDecision.Id != null);
            //---
            _workflowStorePlugin.DeleteDecision(wfDecision);
        }

        public WfActivity GetActivity(WfWorkflow wfWorkflow, WfActivityDefinition wfActivityDefinition)
        {
            return _workflowStorePlugin.FindActivityByDefinitionWorkflow(wfWorkflow, wfActivityDefinition);
        }


        #region Workflow Recalculation
        public void RecalculateWorkflow(WfWorkflow wfWorkflow)
        {
            Debug.Assert(wfWorkflow != null);
            WfWorkflow wfWorkflowFetched = _workflowStorePlugin.ReadWorkflowInstanceForUpdateById(wfWorkflow.WfwId.Value);
            Debug.Assert(WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflowFetched.WfsCode) || WfCodeStatusWorkflow.Pau.ToString().Equals(wfWorkflowFetched.WfsCode), "A workflow must be started or paused before ending");
            //---
            WfWorkflowDefinition wfWorkflowDefinition = _workflowStorePlugin.ReadWorkflowDefinition(wfWorkflowFetched.WfwdId.Value);
            RecalculateWorkflows(new List<WfWorkflow>() { wfWorkflowFetched}, wfWorkflowDefinition);
        }

        public void RecalculateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition)
        {
            IList<WfWorkflow> workflows = _workflowStorePlugin.FindActiveWorkflows(wfWorkflowDefinition);
            RecalculateWorkflows(workflows, wfWorkflowDefinition);
        }


        private void RecalculateWorkflows(IList<WfWorkflow> wfWorfklows, WfWorkflowDefinition wfWorkflowDefinition)
        {
            IList<WfActivityDefinition> activityDefinitions = _workflowStorePlugin.FindAllDefaultActivityDefinitions(wfWorkflowDefinition);

            RuleConstants ruleConstants = _ruleManager.GetConstants(wfWorkflowDefinition.WfwdId.Value);

            IList<RuleDefinition> rules = _workflowStorePlugin.FindAllRulesByWorkflowDefinitionId(wfWorkflowDefinition.WfwdId.Value);
            IList<RuleConditionDefinition> conditions = _workflowStorePlugin.FindAllConditionsByWorkflowDefinitionId(wfWorkflowDefinition.WfwdId.Value);

            IList<SelectorDefinition> selectors = _workflowStorePlugin.FindAllSelectorsByWorkflowDefinitionId(wfWorkflowDefinition.WfwdId.Value);
            IList<RuleFilterDefinition> filters = _workflowStorePlugin.FindAllFiltersByWorkflowDefinitionId(wfWorkflowDefinition.WfwdId.Value);

            //Build a dictionary from the : WfadId => List<RuleDefinition>
            IDictionary<int, List<RuleDefinition>> dicRules = rules.GroupBy(c => c.ItemId.Value).ToDictionary(d => d.Key, e => e.ToList());

            //Build a dictionary from the : RudId => List<RuleConditionDefinition>
            IDictionary<int, List<RuleConditionDefinition>> dicConditions = conditions.GroupBy(c => c.RudId.Value).ToDictionary(d => d.Key, e => e.ToList());

            //Build a dictionary from the : WfadId => List<SelectorDefinition>
            IDictionary<int, List<SelectorDefinition>> dicSelectors = selectors.GroupBy(c => c.ItemId.Value).ToDictionary(d => d.Key, e => e.ToList());

            //Build a dictionary from the : SelId => List<RuleFilterDefinition>
            IDictionary<int, List<RuleFilterDefinition>> dicFilters = filters.GroupBy(c => c.SelId.Value).ToDictionary(d => d.Key, e => e.ToList());

            foreach (WfWorkflow wfWorfklow in wfWorfklows)
            {
                RecalculateWorkflow(activityDefinitions, ruleConstants, wfWorfklow, dicRules, dicConditions, dicSelectors, dicFilters);
            }
        }


        private void RecalculateWorkflow(IList<WfActivityDefinition> activityDefinitions, RuleConstants ruleConstants, WfWorkflow wf, IDictionary<int, List<RuleDefinition>> dicRules, IDictionary<int, List<RuleConditionDefinition>> dicConditions, IDictionary<int, List<SelectorDefinition>> dicSelectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters)
        {

            if (activityDefinitions.Count == 0)
            {
                // If the workflow don't have any activity definition, no need to recalculate.
                return;
            }

            object obj = _itemStorePlugin.ReadItem(wf.ItemId.Value);
            IList<WfActivity> allActivities = _workflowStorePlugin.FindActivitiesByWorkflowId(wf);
            IDictionary<int, WfActivity> activities = allActivities.ToDictionary(a => a.WfadId);
            WfActivity currentActivity;
            bool isLastPreviousCurrentActivityReached;
            if (wf.WfaId2 == null)
            {
                //If the first(s) manual Activity(ies) has(ve) been deleted, the workflow don't have a current activity.
                currentActivity = null;
                isLastPreviousCurrentActivityReached = true;
            }
            else
            {
                currentActivity = allActivities.Where(a => a.WfaId.Equals(wf.WfaId2.Value)).First();
                isLastPreviousCurrentActivityReached = false;
            }
            
            bool newCurrentActivityFound = false;

            foreach (WfActivityDefinition activityDefinition in activityDefinitions)
            {
                int actDefId = activityDefinition.WfadId.Value;
                WfActivity activity;
                activities.TryGetValue(actDefId, out activity);

                //bool isRuleValid = _ruleManager.IsRuleValid(actDefId, obj, ruleConstants);
                bool isRuleValid = _ruleManager.IsRuleValid(actDefId, obj, ruleConstants, dicRules, dicConditions);

                if (activity != null && currentActivity != null && activityDefinition.WfadId.Equals(currentActivity.WfadId))
                {
                    isLastPreviousCurrentActivityReached = true;
                }

                bool isCurrentActivityAuto = false;

                if (isRuleValid)
                {
                    //This activity need a validation

                    //We need to check if there is at least one user allowed to validate
                    //IList<AccountUser> accounts = _ruleManager.SelectAccounts(actDefId, obj, ruleConstants);
                    IList<AccountUser> accounts = _ruleManager.SelectAccounts(actDefId, obj, ruleConstants);

                    if (accounts.Count > 0)
                    {
                        //There is at least one user allowed to validate.
                        if (activity == null)
                        {
                            // No activity linked to this definition was found. 
                            // 2 possibilities : 
                            // - A new activity definition has been inserted in the workflow.
                            // - The previous current activity has been switched to auto.
                            WfActivity wfActivity = CreateActivity(activityDefinition, wf, false);
                            wf.WfaId2 = wfActivity.WfaId;
                            _workflowStorePlugin.UpdateWorkflowInstance(wf);
                            newCurrentActivityFound = true;
                            break;
                        }
                        else if (activity.IsAuto)
                        {
                            //The previous validation was auto. This activity should be manually validated.
                            activity.IsAuto = false;
                            _workflowStorePlugin.UpdateActivity(activity);
                            wf.WfaId2 = activity.WfaId;
                            _workflowStorePlugin.UpdateWorkflowInstance(wf);
                            newCurrentActivityFound = true;
                            break;
                        }

                        // No new activity. The previous activity was manual too.
                        // We need to check if all the users are still allowed to validate.
                        WfCodeMultiplicityDefinition multiplicity = (WfCodeMultiplicityDefinition)Enum.Parse(typeof(WfCodeMultiplicityDefinition), activityDefinition.WfmdCode, true);
                        if (multiplicity == WfCodeMultiplicityDefinition.Sin)
                        {
                            WfDecision decision = GetDecision(activity);
                            IList<string> accountIds = accounts.Select(a => a.Id).ToList();

                            if (decision == null || !accountIds.Contains(decision.Username))
                            {
                                // The user previously allowed to validate are no longer selected with the new selectors.
                                // This activity must be revalidated
                                wf.WfaId2 = activity.WfaId;
                                _workflowStorePlugin.UpdateWorkflowInstance(wf);
                                newCurrentActivityFound = true;
                                break;
                            }
                        }
                        else
                        {
                            IList<WfDecision> decisions = GetDecisions(activity);
                            IList<string> accountIds = accounts.Select(a => a.Id).ToList();
                            IList<string> decisionUsernames = decisions.Select(d => d.Username).ToList();
                            IList<string> matches = accountIds.Intersect(decisionUsernames).ToList();
                            if (matches.Count < decisionUsernames.Count)
                            {
                                // At least one user previously allowed to validate are no longer selected with the new selectors.
                                // This activity must be revalidated
                                wf.WfaId2 = activity.WfaId;
                                _workflowStorePlugin.UpdateWorkflowInstance(wf);
                                newCurrentActivityFound = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // There is no users allowed to validate.
                        // This activity is now auto.
                        isCurrentActivityAuto = true;
                        activity.IsAuto = true;
                        _workflowStorePlugin.UpdateActivity(activity);
                    }
                }
                else
                {
                    isCurrentActivityAuto = true;
                    if (activity == null)
                    {
                        // No activity linked to this definition was found. 
                        // 2 possibilities : 
                        // - A new activity definition has been inserted in the workflow.
                        // - The previous current activity has been switched to auto.
                        WfActivity wfActivity = CreateActivity(activityDefinition, wf, true);
                    }
                    else if (activity.IsAuto == false)
                    {
                        // The previous activity was manual but now this activity is auto
                        activity.IsAuto = true;
                        _workflowStorePlugin.UpdateActivity(activity);
                    }
                }

                if (isLastPreviousCurrentActivityReached && isCurrentActivityAuto == false)
                {
                    // The last activity has been reached.
                    newCurrentActivityFound = true;
                    break;
                }


            }

            if (newCurrentActivityFound == false)
            {
                // All the definitions have been iterated until the end.
                // The workflow must be ended.

                // Stepping back : No Automatic ending. 
                // TODO: Remove the commented code when the behavior will be validated
                //EndInstance(wf);
            }
        }
        #endregion

        #region Custom Methods

        public IList<WfWorkflowDecision> GetWorkflowDecision(int wfwId)
        {
            //Get the workflow from id
            WfWorkflow wfWorkflow = _workflowStorePlugin.ReadWorkflowInstanceById(wfwId);

            //Get the definition
            WfWorkflowDefinition wfDefinition = _workflowStorePlugin.ReadWorkflowDefinition(wfWorkflow.WfwdId.Value);

            //Get all the activity definitions for the workflow definition
            IList<WfActivityDefinition> activityDefinitions = _workflowStorePlugin.FindAllDefaultActivityDefinitions(wfDefinition);

            //Build a dictionary : WfadId => WfActivity
            IDictionary<int, WfActivity> dicActivities = _workflowStorePlugin.FindActivitiesByWorkflowId(wfWorkflow).ToDictionary<WfActivity, int>(a => a.WfadId);

            //Get all decisions for the workflow instance
            IList<WfDecision> allDecisions = _workflowStorePlugin.FindDecisionsByWorkflowId(wfWorkflow).ToList<WfDecision>();
            //Build a dictionary from the decisions: WfaId => List<WfDecision>
            IDictionary<int, List<WfDecision>> dicDecision = allDecisions.GroupBy(d => d.WfaId).ToDictionary(d => d.Key, e => e.ToList());

            // Fetch the object linked to the workflow instance.
            object obj = _itemStorePlugin.ReadItem(wfWorkflow.ItemId.Value);

            RuleConstants ruleConstants = _ruleManager.GetConstants(wfDefinition.WfwdId.Value);

            IList<WfWorkflowDecision> workflowDecisions = new List<WfWorkflowDecision>();

            foreach (WfActivityDefinition activityDefinition in activityDefinitions)
            {
                bool ruleValid = _ruleManager.IsRuleValid(activityDefinition.WfadId.Value, obj, ruleConstants);

                if (ruleValid)
                {
                    IList<AccountGroup> groups = _ruleManager.SelectGroups(activityDefinition.WfadId.Value, obj, ruleConstants);
                    int nbAccount = 0;
                    bool atLeatOnePerson = false;
                    foreach (AccountGroup accountGroup in groups)
                    {
                        ISet<string> accounts = _accountManager.GetStore().GetAccountIds(accountGroup.Id);
                        nbAccount += accounts.Count;
                        if (nbAccount > 0)
                        {
                            atLeatOnePerson = true;
                            break;
                        }
                    }

                    if (atLeatOnePerson)
                    {
                        WfWorkflowDecision wfWorkflowDecision = new WfWorkflowDecision();
                        wfWorkflowDecision.activityDefinition = activityDefinition;
                        WfActivity wfActivity;
                        dicActivities.TryGetValue(activityDefinition.WfadId.Value, out wfActivity);
                        wfWorkflowDecision.activity = wfActivity;
                        wfWorkflowDecision.groups = groups;
                        List<WfDecision> decisions;
                        if (wfActivity != null)
                        {
                            dicDecision.TryGetValue(wfActivity.WfaId.Value, out decisions);
                            wfWorkflowDecision.decisions = decisions;
                        }
                        workflowDecisions.Add(wfWorkflowDecision);
                    }
                }
            }


            return workflowDecisions;
        }

        #endregion
    }
}
