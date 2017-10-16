using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using Kinetix.Account;
using Kinetix.Rules;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;

namespace Kinetix.Workflow {
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public sealed class WorkflowManager : IWorkflowManager {
        private readonly IWorkflowStorePlugin _workflowStorePlugin;
        private readonly IItemStorePlugin _itemStorePlugin;
        private readonly IRuleManager _ruleManager;
        private readonly IAccountManager _accountManager;
        private readonly IWorkflowPredicateAutoValidatePlugin _selectorRuleWorkflowPredicateAutoValidatePlugin;
        private readonly IList<IWorkflowRecalculationPlugin> _customsRecalculations;

        public static readonly string USER_AUTO = "<AUTO>";

        public WorkflowManager(IWorkflowStorePlugin workflowStorePlugin, IItemStorePlugin itemStorePlugin, IRuleManager ruleManager, IAccountManager accountManager, IWorkflowPredicateAutoValidatePlugin workflowPredicateAutoValidatePlugin, IWorkflowRecalculationPlugin[] customsRecalculations) {
            _workflowStorePlugin = workflowStorePlugin;
            _itemStorePlugin = itemStorePlugin;
            _ruleManager = ruleManager;
            _accountManager = accountManager;
            _selectorRuleWorkflowPredicateAutoValidatePlugin = workflowPredicateAutoValidatePlugin;
            _customsRecalculations = customsRecalculations;
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

                if (position > 1) {
                    // Automatically move the next activity after the newly created
                    InsertActivityBefore(wfWorkflowDefinition, wfActivityDefinitionToAdd, wfActivityDefinition);
                } else {
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

        private WfActivity GetNewActivity(WfActivityDefinition activityDefinition, WfWorkflow wfWorkflow, bool isAuto, bool isValid) {
            WfActivity wfActivity = new WfActivity();
            wfActivity.CreationDate = DateTime.Now;
            wfActivity.WfadId = activityDefinition.WfadId.Value;
            wfActivity.WfwId = wfWorkflow.WfwId.Value;
            wfActivity.IsAuto = isAuto;
            wfActivity.IsValid = isValid;
            return wfActivity;
        }


        private WfActivity CreateActivity(WfActivityDefinition activityDefinition, WfWorkflow wfWorkflow, bool isAuto, bool isValid) {
            WfActivity wfActivity = GetNewActivity(activityDefinition, wfWorkflow, isAuto, isValid);
            _workflowStorePlugin.CreateActivity(wfActivity);
            return wfActivity;
        }


        #region AutovalidateMass

        /// <summary>
        /// Version ensembliste pour pouvoir autovalider 1400 activitées ...
        /// </summary>
        /// <param name="wfWorkflow"></param>
        /// <param name="currentActivity"></param>
        /// <param name="wfActivityDefinitionId"></param>
        /// <returns></returns>
        public bool AutoValidateNextActivitiesMass(WfWorkflow wfWorkflow, WfActivity currentActivity, int wfActivityDefinitionId) {
            int wfwdId = wfWorkflow.WfwdId.Value;

            WfActivityDefinition wfActivityDefinition = _workflowStorePlugin.ReadActivityDefinition(wfActivityDefinitionId);
            WfWorkflowDefinition wfWorkflowDefinition = _workflowStorePlugin.ReadWorkflowDefinition(wfWorkflow.WfwdId.Value);

            IList<WfActivityDefinition> activityDefinitions = _workflowStorePlugin.FindAllDefaultActivityDefinitions(wfWorkflowDefinition, wfActivityDefinition.Level.Value);

            IList<WfActivity> allActivities = _workflowStorePlugin.FindAllActivitiesByWorkflowDefinitionId(wfWorkflowDefinition);
            IList<WfDecision> allDecisions = _workflowStorePlugin.FindAllDecisionsByWorkflowDefinitionId(wfWorkflowDefinition);

            //Build a dictionary from the rules: WfadId => List<RuleDefinition>
            IDictionary<int, List<RuleDefinition>> dicRules = constructDicRulesForWorkflowDefinition(wfwdId);

            //Build a dictionary from the conditions: RudId => List<RuleConditionDefinition>
            IDictionary<int, List<RuleConditionDefinition>> dicConditions = constructDicConditionsForWorkflowDefinition(wfwdId);

            //Build a dictionary from the selectors: WfadId => List<SelectorDefinition>
            IDictionary<int, List<SelectorDefinition>> dicSelectors = constructDicSelectorsForWorkflowDefinition(wfwdId);

            //Build a dictionary from the filters: SelId => List<RuleFilterDefinition>
            IDictionary<int, List<RuleFilterDefinition>> dicFilters = constructDicFiltersForWorkflowDefinition(wfwdId);

            //Build a dictionnary from the workflows: WfadId => WfActivity
            IDictionary<int, WfActivity> dicActivities = allActivities.ToDictionary(a => a.WfadId);

            //Build a dictionnary from the activities: WfaId => List<WfDecision>
            IDictionary<int, List<WfDecision>> dicDecision = allDecisions.GroupBy(c => c.WfaId).ToDictionary(d => d.Key, e => e.ToList());

            object obj = _itemStorePlugin.ReadItem(wfWorkflow.ItemId.Value);
            int? wfCurrentActivityId = null;
            WfActivity wfActivityCurrent = currentActivity;
            RuleConstants ruleConstants = _ruleManager.GetConstants(wfActivityDefinition.WfwdId);
            RuleContext ruleContext = new RuleContext(obj, ruleConstants);
            WfMassValidation validation = new WfMassValidation();
            int i;
            for (i = 0; i < activityDefinitions.Count; i++) {
                WfActivityDefinition activityDefinition = activityDefinitions[i];

                if (CanAutoValidateActivity(activityDefinition, ruleContext, dicRules, dicConditions, dicSelectors, dicFilters)) {
                    if (i > 0) {
                        dicActivities.TryGetValue(activityDefinition.WfadId.Value, out wfActivityCurrent);

                        if (wfActivityCurrent == null) {
                            wfActivityCurrent = GetNewActivity(activityDefinition, wfWorkflow, true, false);
                        } else {
                            wfActivityCurrent.IsAuto = true;
                            // We keep the previous value of IsValid
                            //wfActivityCurrent.IsValid = true;
                        }
                    }

                    WfDecision decision = new WfDecision();
                    decision.Username = USER_AUTO;
                    decision.DecisionDate = DateTime.Now;
                    decision.WfaId = wfActivityCurrent.WfaId.Value;

                    validation.AddActivityDecision(wfActivityCurrent, decision);

                    wfCurrentActivityId = wfActivityCurrent.WfaId;
                } else {
                    break;
                }
            }

            //Updating current workflow activities (no new activities)
            if (validation.ActivitiesDecisions.Count > 0) {
                _workflowStorePlugin.CreateActivityDecision(validation.ActivitiesDecisions);
            }

            if (wfCurrentActivityId != null) {
                wfWorkflow.WfaId2 = wfCurrentActivityId;
                _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
            }

            return i == activityDefinitions.Count;
        }

        public bool CanAutoValidateActivity(WfActivityDefinition activityDefinition, RuleContext ruleContext,
            IDictionary<int, List<RuleDefinition>> dicRules, IDictionary<int, List<RuleConditionDefinition>> dicConditions,
            IDictionary<int, List<SelectorDefinition>> dicSelectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters) {
            return _selectorRuleWorkflowPredicateAutoValidatePlugin.CanAutoValidateActivity(activityDefinition, ruleContext, dicRules, dicConditions, dicSelectors, dicFilters);
        }

        #endregion

        public bool AutoValidateNextActivities(WfWorkflow wfWorkflow, WfActivity currentActivity, int wfActivityDefinitionId) {
            WfActivityDefinition activityDefinition = _workflowStorePlugin.ReadActivityDefinition(wfActivityDefinitionId);

            object obj = _itemStorePlugin.ReadItem(wfWorkflow.ItemId.Value);
            int? wfCurrentActivityId = null;
            bool endReached = false;
            WfActivity wfActivityCurrent = currentActivity;
            while (CanAutoValidateActivity(activityDefinition, obj)) {
                AutoValidateDecision(wfActivityCurrent);

                if (_workflowStorePlugin.HasNextActivity(wfActivityCurrent) == false) {
                    endReached = true;
                    break;
                }
                activityDefinition = _workflowStorePlugin.FindNextActivity(wfActivityCurrent.WfadId);

                WfActivity nextActivity = _workflowStorePlugin.FindActivityByDefinitionWorkflow(wfWorkflow, activityDefinition);
                if (nextActivity == null) {
                    wfActivityCurrent = CreateActivity(activityDefinition, wfWorkflow, false, false);
                } else {
                    wfActivityCurrent = nextActivity;
                }

                wfCurrentActivityId = wfActivityCurrent.WfaId;
            }

            if (wfCurrentActivityId != null) {
                wfWorkflow.WfaId2 = wfCurrentActivityId;
                _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
            }
            return endReached;
        }

        private void AutoValidateDecision(WfActivity wfActivityCurrent) {

            wfActivityCurrent.IsAuto = true;
            // We keep the previous value of IsValid
            _workflowStorePlugin.UpdateActivity(wfActivityCurrent);

            WfDecision decision = new WfDecision();
            decision.Username = USER_AUTO;
            decision.DecisionDate = DateTime.Now;
            decision.WfaId = wfActivityCurrent.WfaId.Value;

            _workflowStorePlugin.CreateDecision(decision);
        }


        public bool CanAutoValidateActivity(WfActivityDefinition activityDefinition, object obj) {
            return _selectorRuleWorkflowPredicateAutoValidatePlugin.CanAutoValidateActivity(activityDefinition, obj);
        }

        public void CreateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition) {
            _workflowStorePlugin.CreateWorkflowDefinition(wfWorkflowDefinition);
        }


        public WfWorkflow CreateWorkflowInstance(int wfwdId, string username, bool userLogic, int item) {
            WfWorkflow wfWorkflow = new WfWorkflow();
            wfWorkflow.CreationDate = DateTime.Now;
            wfWorkflow.ItemId = item;
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Cre.ToString();
            wfWorkflow.WfwdId = wfwdId;
            wfWorkflow.UserLogic = userLogic;
            wfWorkflow.Username = username;

            IList<WfWorkflow> wfActiveWorkflow = _workflowStorePlugin.FindActiveWorkflowInstanceByItemId(wfwdId, item);
            if (wfActiveWorkflow.Count > 0) {
                throw new System.InvalidOperationException("Only one active workflow must exist for this Definition and Item Id");
            }

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
            if (!(WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode) || WfCodeStatusWorkflow.Pau.ToString().Equals(wfWorkflow.WfsCode))) {
                throw new System.InvalidOperationException("A workflow must be started or paused before ending");
            }
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.End.ToString();
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
        }

        public string GetUserAuto() {
            return USER_AUTO;
        }

        public IList<WfActivityDefinition> GetActivityDefinitions(WfWorkflow wfWorkflow) {
            WfWorkflowDefinition wfDefinition = _workflowStorePlugin.ReadWorkflowDefinition(wfWorkflow.WfwdId.Value);
            IList<WfActivityDefinition> activities = _workflowStorePlugin.FindAllDefaultActivityDefinitions(wfDefinition);

            object obj = _itemStorePlugin.ReadItem((int)wfWorkflow.ItemId);

            IList<WfActivityDefinition> ret = new List<WfActivityDefinition>();
            foreach (WfActivityDefinition activity in activities) {
                if (CanAutoValidateActivity(activity, obj) == false) {
                    ret.Add(activity);
                }
            }

            return ret;
        }


        public IList<WfActivity> GetActivities(WfWorkflow wfWorkflow, IList<int> wfadId) {
            return _workflowStorePlugin.FindActivitiesByDefinitionId(wfWorkflow, wfadId);
        }

        public IList<WfActivityDefinition> GetAllDefaultActivities(WfWorkflowDefinition wfWorkflowDefinition) {
            return _workflowStorePlugin.FindAllDefaultActivityDefinitions(wfWorkflowDefinition);
        }


        public WfWorkflow GetWorkflowInstance(int wfwId) {
            return _workflowStorePlugin.ReadWorkflowInstanceById(wfwId);
        }

        private void InsertActivityBefore(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityToAdd, WfActivityDefinition wfActivityReferential) {
            WfTransitionCriteria wfTransitionCriteria = new WfTransitionCriteria();
            wfTransitionCriteria.TransitionName = WfCodeTransition.Default.ToString();
            wfTransitionCriteria.WfadIdTo = wfActivityReferential.WfadId.Value;

            WfTransitionDefinition transition = _workflowStorePlugin.FindTransition(wfTransitionCriteria);
            transition.WfadIdTo = wfActivityToAdd.WfadId.Value;

            _workflowStorePlugin.UpdateTransition(transition);

            WfTransitionDefinition wfTransitionDefinition = new WfTransitionBuilder(wfWorkflowDefinition.WfwdId, wfActivityToAdd.WfadId, wfActivityReferential.WfadId).Build();
            _workflowStorePlugin.AddTransition(wfTransitionDefinition);
        }

        public void MoveActivity(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityToMove, WfActivityDefinition wfActivityReferential, bool after) {
            Debug.Assert(wfActivityToMove?.WfadId != null);
            Debug.Assert(wfActivityReferential?.WfadId != null);
            Debug.Assert(!wfActivityToMove.WfadId.Equals(wfActivityReferential.WfadId));
            //---
            if (after) {
                MoveActivityAfter(wfWorkflowDefinition, wfActivityToMove, wfActivityReferential);
            } else {
                MoveActivityBefore(wfWorkflowDefinition, wfActivityToMove, wfActivityReferential);
            }

            //Shifting position number
            int shift;
            int posStart;
            int posEnd;
            if (wfActivityToMove.Level.Value < wfActivityReferential.Level.Value) {
                shift = -1;
                posStart = wfActivityToMove.Level.Value;
                if (after) {
                    posEnd = wfActivityReferential.Level.Value;
                } else {
                    posEnd = wfActivityReferential.Level.Value - 1;
                }
                wfActivityToMove.Level = posEnd;
            } else {
                shift = 1;
                posEnd = wfActivityToMove.Level.Value - 1;
                if (after) {
                    posStart = wfActivityReferential.Level.Value + 1;
                } else {
                    posStart = wfActivityReferential.Level.Value;
                }
                wfActivityToMove.Level = posStart;
            }

            _workflowStorePlugin.ShiftActivityDefinitionPositionsBetween(wfWorkflowDefinition.WfwdId.Value, posStart, posEnd, shift);
            _workflowStorePlugin.UpdateActivityDefinition(wfActivityToMove);
        }

        private void MoveActivityAfter(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityToMove, WfActivityDefinition wfActivityReferential) {
            // T1
            WfTransitionCriteria critTrFromRef = new WfTransitionCriteria();
            critTrFromRef.WfadIdFrom = wfActivityReferential.WfadId;
            critTrFromRef.TransitionName = WfCodeTransition.Default.ToString();
            WfTransitionDefinition trFromRef = _workflowStorePlugin.FindTransition(critTrFromRef);

            if (trFromRef != null && trFromRef.WfadIdTo.Equals(wfActivityToMove.WfadId)) {
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
            critTrToMove.WfadIdTo = wfActivityToMove.WfadId;
            critTrToMove.TransitionName = WfCodeTransition.Default.ToString();
            WfTransitionDefinition trToMove = _workflowStorePlugin.FindTransition(critTrToMove);

            // Update T3
            if (trToMove == null) {
                //No transition before Move. Move is the first Activity of the WorkflowDefinition
                wfWorkflowDefinition.WfadId = trFromMove.WfadIdTo;
                _workflowStorePlugin.UpdateWorkflowDefinition(wfWorkflowDefinition);
            } else {
                // Update T3
                if (trFromMove == null) {
                    trToMove.WfadIdFrom = wfActivityToMove.WfadId.Value;
                    trToMove.WfadIdTo = trFromRef.WfadIdTo;
                } else {
                    trToMove.WfadIdTo = trFromMove.WfadIdTo;
                }
                // Moving T3
                _workflowStorePlugin.UpdateTransition(trToMove);
            }

            // Update T1/T2
            if (trFromRef == null) {
                //No transition after T1. 
                trFromMove.WfadIdFrom = wfActivityReferential.WfadId.Value;
                trFromMove.WfadIdTo = wfActivityToMove.WfadId.Value;
                _workflowStorePlugin.UpdateTransition(trFromMove);
            } else {
                // Moving T2
                //If there is no Activity after the activity to move. No transition should be modified
                if (trFromMove != null) {
                    trFromMove.WfadIdTo = trFromRef.WfadIdTo;
                    _workflowStorePlugin.UpdateTransition(trFromMove);
                }

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

            if (trToRef != null && trToRef.WfadIdFrom.Equals(wfActivityToMove.WfadId)) {
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
            critTrToMove.WfadIdTo = wfActivityToMove.WfadId;
            critTrToMove.TransitionName = WfCodeTransition.Default.ToString();
            WfTransitionDefinition trToMove = _workflowStorePlugin.FindTransition(critTrToMove);

            // Update T1
            if (trToRef == null) {
                //No transition before Ref. Ref is the first Activity of the WorkflowDefinition
                wfWorkflowDefinition.WfadId = wfActivityToMove.WfadId.Value;
                _workflowStorePlugin.UpdateWorkflowDefinition(wfWorkflowDefinition);
            } else {
                // Moving T1
                trToRef.WfadIdTo = wfActivityToMove.WfadId.Value;
                _workflowStorePlugin.UpdateTransition(trToRef);
            }

            // Update T3
            if (trToMove == null) {
                //No transition before T3. Move is the first Activity of the WorkflowDefinition
                //wfWorkflowDefinition.WfadId = wfActivityToMove.WfadId.Value;
                wfWorkflowDefinition.WfadId = trFromMove.WfadIdTo;
                _workflowStorePlugin.UpdateWorkflowDefinition(wfWorkflowDefinition);
            } else {
                // Moving T3
                if (trFromMove == null) {
                    trToMove.WfadIdFrom = wfActivityToMove.WfadId.Value;
                    trToMove.WfadIdTo = wfActivityReferential.WfadId.Value;
                } else {
                    trToMove.WfadIdTo = trFromMove.WfadIdTo;
                }

                _workflowStorePlugin.UpdateTransition(trToMove);
            }

            // Update T2
            //If there is no Activity after the activity to move. No transition should be modified
            if (trFromMove != null) {
                // Moving T2
                trFromMove.WfadIdTo = wfActivityReferential.WfadId.Value;
                _workflowStorePlugin.UpdateTransition(trFromMove);
            }

        }

        public void MoveActivity(WfWorkflowDefinition wfWorkflowDefinition, int src, int dst, bool after) {
            WfActivityDefinition wfActivityDefinitionFrom = _workflowStorePlugin.FindActivityDefinitionByPosition(wfWorkflowDefinition, src);
            WfActivityDefinition wfActivityDefinitionTo = _workflowStorePlugin.FindActivityDefinitionByPosition(wfWorkflowDefinition, dst);

            MoveActivity(wfWorkflowDefinition, wfActivityDefinitionFrom, wfActivityDefinitionTo, after);
        }


        public void RenameActivity(WfActivityDefinition wfActivityDefinition) {
            Debug.Assert(wfActivityDefinition != null);
            Debug.Assert(wfActivityDefinition.WfadId != null);
            ///---
            _workflowStorePlugin.RenameActivityDefinition(wfActivityDefinition);
        }

        public void PauseInstance(WfWorkflow wfWorkflow) {
            Debug.Assert(wfWorkflow != null);
            if (!WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode)) {
                throw new System.InvalidOperationException("A workflow must be started before pausing");
            }
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Pau.ToString();
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
        }

        public void RemoveWorkflow(int wfwId) {
            _workflowStorePlugin.DeleteWorkflow(wfwId);
        }

        public void RemoveActivity(WfActivityDefinition wfActivityDefinition) {

            WfWorkflowDefinition wfD = _workflowStorePlugin.ReadWorkflowDefinition(wfActivityDefinition.WfwdId);

            IList<RuleDefinition> rules = _ruleManager.GetRulesForItemId(wfActivityDefinition.WfadId.Value);
            IList<SelectorDefinition> selectors = _ruleManager.GetSelectorsForItemId(wfActivityDefinition.WfadId.Value);
            _ruleManager.RemoveRules(rules);
            _ruleManager.RemoveSelectors(selectors);

            //The current activity will be unset. The workflow recalculation will correct the current activity
            _workflowStorePlugin.UnsetCurrentActivity(wfActivityDefinition);

            _workflowStorePlugin.DeleteActivities(wfActivityDefinition.WfadId.Value);

            WfTransitionCriteria critFrom = new WfTransitionCriteria();
            critFrom.WfadIdFrom = wfActivityDefinition.WfadId;
            critFrom.TransitionName = WfCodeTransition.Default.ToString();
            WfTransitionDefinition transitionFrom = _workflowStorePlugin.FindTransition(critFrom);

            if (wfD.WfadId.Equals(wfActivityDefinition.WfadId)) {
                //The Activity Definition to remove is the start activity

                if (transitionFrom != null) {
                    // The first activity definition will be the next definition
                    wfD.WfadId = transitionFrom.WfadIdTo;
                    _workflowStorePlugin.UpdateWorkflowDefinition(wfD);
                    _workflowStorePlugin.RemoveTransition(transitionFrom);
                }
            } else {
                //The Activity Definition to remove is NOT the start activity
                WfTransitionCriteria critTo = new WfTransitionCriteria();
                critTo.WfadIdTo = wfActivityDefinition.WfadId;
                critTo.TransitionName = WfCodeTransition.Default.ToString();
                WfTransitionDefinition transitionTo = _workflowStorePlugin.FindTransition(critTo);

                if (transitionFrom != null) {
                    _workflowStorePlugin.RemoveTransition(transitionFrom);
                    transitionTo.WfadIdTo = transitionFrom.WfadIdTo;
                    _workflowStorePlugin.UpdateTransition(transitionTo);
                } else {
                    // Last activity
                    _workflowStorePlugin.RemoveTransition(transitionTo);
                }
            }

            _workflowStorePlugin.DeleteActivityDefinition(wfActivityDefinition);
            _workflowStorePlugin.DecrementActivityDefinitionPositionsAfter(wfD.WfwdId.Value, wfActivityDefinition.Level.Value);
        }

        public void RemoveRule(RuleDefinition rule) {
            _ruleManager.RemoveRule(rule);
        }

        public void RemoveSelector(SelectorDefinition selector) {
            _ruleManager.RemoveSelector(selector);
        }

        public void ResumeInstance(WfWorkflow wfWorkflow) {
            Debug.Assert(wfWorkflow != null);
            if (!WfCodeStatusWorkflow.Pau.ToString().Equals(wfWorkflow.WfsCode)) {
                throw new InvalidOperationException("A workflow must be paused before resuming");
            }
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Sta.ToString();
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);
        }

        public void SaveDecision(WfWorkflow wfWorkflow, WfDecision wfDecision) {
            SaveDecision(wfWorkflow, wfDecision, false);
        }

        public void SaveDecision(WfWorkflow wfWorkflow, WfDecision wfDecision, bool forceValid) {
            if (!WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode)) {
                throw new InvalidOperationException("A workflow must be started before saving decision");
            }
            //---
            WfWorkflow wfWorkflowFetch = _workflowStorePlugin.ReadWorkflowInstanceForUpdateById(wfWorkflow.WfwId.Value);

            if (wfWorkflowFetch.WfaId2 != null && !wfWorkflow.WfaId2.Equals(wfWorkflow.WfaId2)) {
                throw new InvalidOperationException("Concurrent workflow modification");
            }

            WfActivity currentActivity = _workflowStorePlugin.ReadActivity(wfWorkflow.WfaId2.Value);

            wfDecision.WfaId = currentActivity.WfaId.Value;
            if (wfDecision.Id == null) {
                _workflowStorePlugin.CreateDecision(wfDecision);
            } else {
                _workflowStorePlugin.UpdateDecision(wfDecision);
            }

            // Attach decision to the activity
            currentActivity.IsAuto = false;

            if (IsActivityValid(wfWorkflow, currentActivity, false, forceValid)) {
                currentActivity.IsValid = true;
            }

            _workflowStorePlugin.UpdateActivity(currentActivity);

        }

        private bool IsActivityValid(WfWorkflow wfWorkflow, WfActivity currentActivity, bool checkDecisionSingle, bool forceValid) {
            WfActivityDefinition currentActivityDefinition = _workflowStorePlugin.ReadActivityDefinition(currentActivity.WfadId);

            WfCodeMultiplicityDefinition wfCodeMultiplicityDefinition = (WfCodeMultiplicityDefinition)Enum.Parse(typeof(WfCodeMultiplicityDefinition), currentActivityDefinition.WfmdCode);

            bool activityValid = false;

            if (wfCodeMultiplicityDefinition == WfCodeMultiplicityDefinition.Mul) {
                IList<WfDecision> wfDecisions = _workflowStorePlugin.FindAllDecisionByActivity(currentActivity);

                if (forceValid) {
                    activityValid = wfDecisions.Count > 0;
                } else {
                    object obj = _itemStorePlugin.ReadItem(wfWorkflow.ItemId.Value);
                    RuleConstants ruleConstants = _ruleManager.GetConstants(wfWorkflow.WfwdId.Value);
                    RuleContext ruleContext = new RuleContext(obj, ruleConstants);
                    IList<AccountUser> accounts = _ruleManager.SelectAccounts(currentActivity.WfadId, ruleContext);

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
                        activityValid = true;
                    }

                }

            } else {
                if (checkDecisionSingle) {
                    WfDecision wfDecision = GetDecision(currentActivity);
                    if (wfDecision != null) {
                        activityValid = true;
                    }
                } else {
                    activityValid = true;
                }

            }

            return activityValid;
        }

        public WfDecision GetDecision(WfActivity wfActivity) {
            Debug.Assert(wfActivity != null);
            //---
            WfActivityDefinition wfActivityDefinition = _workflowStorePlugin.ReadActivityDefinition(wfActivity.WfadId);
            WfCodeMultiplicityDefinition multiplicity = (WfCodeMultiplicityDefinition)Enum.Parse(typeof(WfCodeMultiplicityDefinition), wfActivityDefinition.WfmdCode, true);

            if (multiplicity != WfCodeMultiplicityDefinition.Sin) {
                throw new InvalidOperationException();
            }
            IList<WfDecision> decision = _workflowStorePlugin.ReadDecisionsByActivityId(wfActivity.WfaId.Value);
            if (decision.Count == 0) {
                return null;
            } else {
                return decision[0];
            }
        }

        public IList<WfDecision> GetDecisions(WfActivity wfActivity) {
            Debug.Assert(wfActivity != null);
            //---
            WfActivityDefinition wfActivityDefinition = _workflowStorePlugin.ReadActivityDefinition(wfActivity.WfadId);
            WfCodeMultiplicityDefinition multiplicity = (WfCodeMultiplicityDefinition)Enum.Parse(typeof(WfCodeMultiplicityDefinition), wfActivityDefinition.WfmdCode, true);

            if (multiplicity != WfCodeMultiplicityDefinition.Mul) {
                throw new InvalidOperationException();
            }
            return _workflowStorePlugin.ReadDecisionsByActivityId(wfActivity.WfaId.Value);
        }


        public void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, WfDecision wfDecision) {
            SaveDecisionAndGoToNextActivity(wfWorkflow, wfDecision, false);
        }

        public void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, WfDecision wfDecision, bool forceValid) {
            SaveDecisionAndGoToNextActivity(wfWorkflow, WfCodeTransition.Default.ToString(), wfDecision, forceValid);
        }

        public void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, string transitionName, WfDecision wfDecision) {
            SaveDecisionAndGoToNextActivity(wfWorkflow, transitionName, wfDecision, false);
        }

        public bool CanGoToNextActivity(WfWorkflow wfWorkflow) {
            WfActivity currentActivity = _workflowStorePlugin.ReadActivity(wfWorkflow.WfaId2.Value);

            return CanGoToNextActivity(currentActivity);
        }

        private bool CanGoToNextActivity(WfActivity currentActivity) {
            if (!currentActivity.IsValid) {
                return false;
            }

            return _workflowStorePlugin.HasNextActivity(currentActivity);
        }


        public void GoToNextActivity(WfWorkflow wfWorkflow) {
            WfActivity currentActivity = _workflowStorePlugin.ReadActivity(wfWorkflow.WfaId2.Value);

            bool canGoToNext = CanGoToNextActivity(wfWorkflow);
            if (!canGoToNext) {
                throw new System.InvalidOperationException("Can't go to the next activity");
            }

            GoToNextActivity(wfWorkflow, currentActivity, WfCodeTransition.Default.ToString());
        }


        private void GoToNextActivity(WfWorkflow wfWorkflow, WfActivity currentActivity, string transitionName) {
            if (_workflowStorePlugin.HasNextActivity(currentActivity, transitionName)) {
                WfActivityDefinition nextActivityDefinition = _workflowStorePlugin.FindNextActivity(currentActivity.WfadId, transitionName);

                WfActivity nextActivity = _workflowStorePlugin.FindActivityByDefinitionWorkflow(wfWorkflow, nextActivityDefinition);
                if (nextActivity == null) {
                    nextActivity = new WfActivity();
                }
                // Creating the next activity to validate.
                nextActivity.CreationDate = DateTime.Now;
                nextActivity.WfadId = nextActivityDefinition.WfadId.Value;
                nextActivity.WfwId = wfWorkflow.WfwId.Value;
                nextActivity.IsAuto = false;
                if (nextActivity.WfaId == null) {
                    nextActivity.IsValid = false;
                    _workflowStorePlugin.CreateActivity(nextActivity);
                } else {
                    _workflowStorePlugin.UpdateActivity(nextActivity);
                }


                wfWorkflow.WfaId2 = nextActivity.WfaId;
                _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);

                //Autovalidating next activities
                bool endReached = AutoValidateNextActivities(wfWorkflow, nextActivity, nextActivityDefinition.WfadId.Value);
                //bool endReached = AutoValidateNextActivities(wfWorkflow, nextActivity, nextActivityDefinition.WfadId.Value);

                if (endReached) {
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


        public void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, string transitionName, WfDecision wfDecision, bool forceValid) {
            if (!WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflow.WfsCode)) {
                throw new System.InvalidOperationException("A workflow must be started before saving a decision");
            }
            //---
            WfActivity currentActivity = _workflowStorePlugin.ReadActivity(wfWorkflow.WfaId2.Value);

            // Updating the decision
            SaveDecision(wfWorkflow, wfDecision, forceValid);

            currentActivity = _workflowStorePlugin.ReadActivity(wfWorkflow.WfaId2.Value);

            bool canGoToNextActivity = CanGoToNextActivity(currentActivity);

            if (canGoToNextActivity) {
                GoToNextActivity(wfWorkflow, currentActivity, transitionName);
            }
        }

        public void StartInstance(WfWorkflow wfWorkflow) {
            Debug.Assert(wfWorkflow != null);
            if (!WfCodeStatusWorkflow.Cre.ToString().Equals(wfWorkflow.WfsCode)) {
                throw new System.InvalidOperationException("A workflow must be created before starting");
            }
            Debug.Assert(wfWorkflow.WfwdId != null);
            Debug.Assert(wfWorkflow.ItemId != null);
            IList<WfWorkflow> wfActiveWorkflow = _workflowStorePlugin.FindActiveWorkflowInstanceByItemId(wfWorkflow.WfwdId.Value, wfWorkflow.ItemId.Value);
            if (wfActiveWorkflow.Count > 0) {
                throw new System.InvalidOperationException("Only one active workflow must exist for this Definition and Item Id");
            }
            //---
            wfWorkflow.WfsCode = WfCodeStatusWorkflow.Sta.ToString();

            WfWorkflowDefinition wfWorkflowDefinition = _workflowStorePlugin.ReadWorkflowDefinition(wfWorkflow.WfwdId.Value);

            WfActivity wfActivityCurrent = new WfActivity();
            wfActivityCurrent.CreationDate = DateTime.Now;
            wfActivityCurrent.WfadId = wfWorkflowDefinition.WfadId.Value;
            wfActivityCurrent.WfwId = wfWorkflow.WfwId.Value;
            wfActivityCurrent.IsAuto = false;
            wfActivityCurrent.IsValid = false;
            _workflowStorePlugin.CreateActivity(wfActivityCurrent);
            wfWorkflow.WfaId2 = wfActivityCurrent.WfaId;
            _workflowStorePlugin.UpdateWorkflowInstance(wfWorkflow);

            bool endReached = AutoValidateNextActivities(wfWorkflow, wfActivityCurrent, wfWorkflowDefinition.WfadId.Value);

            if (endReached) {
                // Stepping back : No Automatic ending. 
                // TODO: Remove the commented code when the behavior will be validated
                //EndInstance(wfWorkflow);
            }
        }

        /// <summary>
        /// Find the workflow by itemId
        /// </summary>
        /// <param name="criteria"></param>
        public WfWorkflow GetWorkflowInstanceByItemId(int wfwdId, int itemId) {
            return _workflowStorePlugin.ReadWorkflowInstanceByItemId(wfwdId, itemId);
        }

        /// <summary>
        /// Find activities matching the criteria in parameters
        /// </summary>
        /// <param name="criteria"></param>
        public IList<WfActivityDefinition> FindActivitiesByCriteria(RuleCriteria criteria) {
            WfWorkflowDefinition workflow = new WfWorkflowDefinition() { WfwdId = criteria.WfwdId };

            IList<WfActivityDefinition> activities = GetAllDefaultActivities(workflow);
            IDictionary<int?, WfActivityDefinition> dicAct = activities.ToDictionary<WfActivityDefinition, int?>(a => a.WfadId);

            IList<int> matchingActivities = _ruleManager.FindItemsByCriteria(criteria, dicAct.Keys.Cast<int>().ToList());

            return matchingActivities.Select(act => dicAct[act]).ToList();
        }

        public WfActivity GetActivity(int wfaId) {
            return _workflowStorePlugin.ReadActivity(wfaId);
        }

        public IList<RuleConditionDefinition> GetConditionsForRuleId(int ruleId) {
            return _ruleManager.GetConditionsForRuleId(ruleId);
        }

        public IList<RuleFilterDefinition> GetFiltersForSelectorId(int selectorId) {
            return _ruleManager.GetFiltersForSelectorId(selectorId);
        }

        public IList<RuleDefinition> GetRulesForActivityDefinition(int wfadId) {
            return _ruleManager.GetRulesForItemId(wfadId);
        }

        public IList<SelectorDefinition> GetSelectorsForActivityDefinition(int wfadId) {
            return _ruleManager.GetSelectorsForItemId(wfadId);
        }

        public WfWorkflowDefinition GetWorkflowDefinition(int wfwdId) {
            return _workflowStorePlugin.ReadWorkflowDefinition(wfwdId);
        }

        public WfWorkflowDefinition GetWorkflowDefinition(string wfdName) {
            return _workflowStorePlugin.ReadWorkflowDefinition(wfdName);
        }

        public void RemoveRules(IList<RuleDefinition> rules) {
            _ruleManager.RemoveRules(rules);
        }

        public void RemoveSelectors(IList<SelectorDefinition> selectors) {
            _ruleManager.RemoveSelectors(selectors);
        }

        public void DeleteDecision(WfDecision wfDecision) {
            Debug.Assert(wfDecision != null);
            Debug.Assert(wfDecision.Id != null);
            //---
            WfActivity wfActivity = _workflowStorePlugin.ReadActivity(wfDecision.WfaId);
            _workflowStorePlugin.DeleteDecision(wfDecision);
            wfActivity.IsValid = false;
            _workflowStorePlugin.UpdateActivity(wfActivity);

        }

        public WfActivity GetActivity(WfWorkflow wfWorkflow, WfActivityDefinition wfActivityDefinition) {
            return _workflowStorePlugin.FindActivityByDefinitionWorkflow(wfWorkflow, wfActivityDefinition);
        }


        #region Workflow Recalculation
        public WfRecalculationOutput RecalculateWorkflow(WfWorkflow wfWorkflow, bool fetchWorkflowDecisions = false) {
            Debug.Assert(wfWorkflow != null);
            WfWorkflow wfWorkflowFetched = _workflowStorePlugin.ReadWorkflowInstanceForUpdateById(wfWorkflow.WfwId.Value);
            Debug.Assert(WfCodeStatusWorkflow.Sta.ToString().Equals(wfWorkflowFetched.WfsCode) || WfCodeStatusWorkflow.Pau.ToString().Equals(wfWorkflowFetched.WfsCode), "A workflow must be started or paused before ending");
            //---
            WfWorkflowDefinition wfWorkflowDefinition = _workflowStorePlugin.ReadWorkflowDefinition(wfWorkflowFetched.WfwdId.Value);

            IList<WfActivity> activities = _workflowStorePlugin.FindActivitiesByWorkflowId(wfWorkflow);
            IList<WfDecision> decisions = _workflowStorePlugin.FindDecisionsByWorkflowId(wfWorkflow);

            return RecalculateWorkflows(new List<WfWorkflow>() { wfWorkflowFetched }, wfWorkflowDefinition, activities, decisions, fetchWorkflowDecisions);
        }

        public WfRecalculationOutput RecalculateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition, bool fetchWorkflowDecisions = false) {
            IList<WfWorkflow> workflows = _workflowStorePlugin.FindActiveWorkflows(wfWorkflowDefinition, true);
            IList<WfActivity> allActivities = _workflowStorePlugin.FindAllActivitiesByWorkflowDefinitionId(wfWorkflowDefinition);
            IList<WfDecision> allDecisions = _workflowStorePlugin.FindAllDecisionsByWorkflowDefinitionId(wfWorkflowDefinition);
            return RecalculateWorkflows(workflows, wfWorkflowDefinition, allActivities, allDecisions, fetchWorkflowDecisions);
        }


        private WfRecalculationOutput RecalculateWorkflows(IList<WfWorkflow> wfWorfklows, WfWorkflowDefinition wfWorkflowDefinition, IList<WfActivity> activities, IList<WfDecision> decisions, bool fetchWorkflowDecisions) {
            IList<WfActivityDefinition> activityDefinitions = _workflowStorePlugin.FindAllDefaultActivityDefinitions(wfWorkflowDefinition);

            int wfwdId = wfWorkflowDefinition.WfwdId.Value;

            RuleConstants ruleConstants = _ruleManager.GetConstants(wfwdId);

            //Build a dictionary from the rules: WfadId => List<RuleDefinition>
            IDictionary<int, List<RuleDefinition>> dicRules = constructDicRulesForWorkflowDefinition(wfwdId);

            //Build a dictionary from the conditions: RudId => List<RuleConditionDefinition>
            IDictionary<int, List<RuleConditionDefinition>> dicConditions = constructDicConditionsForWorkflowDefinition(wfwdId);

            //Build a dictionary from the selectors: WfadId => List<SelectorDefinition>
            IDictionary<int, List<SelectorDefinition>> dicSelectors = constructDicSelectorsForWorkflowDefinition(wfwdId);

            //Build a dictionary from the filters: SelId => List<RuleFilterDefinition>
            IDictionary<int, List<RuleFilterDefinition>> dicFilters = constructDicFiltersForWorkflowDefinition(wfwdId);

            //Build a dictionnary from the workflows: WfwId => List<WfActivity>
            IDictionary<int, List<WfActivity>> dicActivities = activities.GroupBy(c => c.WfwId).ToDictionary(d => d.Key, e => e.ToList());

            //Build a dictionnary from the activities: WfaId => List<WfDecision>
            IDictionary<int, List<WfDecision>> dicDecision = decisions.GroupBy(c => c.WfaId).ToDictionary(d => d.Key, e => e.ToList());

            //Build a list of items Ids 
            List<int> itemIds = wfWorfklows.Select(w => w.ItemId.Value).ToList();
            IDictionary<int, object> dicObjects = _itemStorePlugin.ReadItems(itemIds);

            WfRecalculationOutput output = new WfRecalculationOutput();
            foreach (WfWorkflow wfWorfklow in wfWorfklows) {
                RecalculateWorkflow(activityDefinitions, ruleConstants, wfWorfklow, dicRules, dicConditions, dicSelectors, dicFilters, dicActivities, dicDecision, dicObjects, output);

                foreach (IWorkflowRecalculationPlugin customRecalculation in _customsRecalculations) {
                    customRecalculation.CustomRecalculation(activityDefinitions, ruleConstants, wfWorfklow, dicRules, dicConditions, dicSelectors, dicFilters, dicActivities, dicDecision, dicObjects, output);
                }

                if (fetchWorkflowDecisions) {
                    WfListWorkflowDecision wfListWorkflowDecision = GetWorkflowDecisions(activityDefinitions, ruleConstants, wfWorfklow, dicRules, dicConditions, dicSelectors, dicFilters, dicActivities, dicDecision, dicObjects);
                    output.AddWfListWorkflowDecision(wfListWorkflowDecision);
                }
            }

            UpdateWorkflows(output);
            return output;
        }

        private void UpdateWorkflows(WfRecalculationOutput output) {
            //Updating current workflow activities (no new activities)
            if (output.WorkflowsUpdateCurrentActivity.Count > 0) {
                _workflowStorePlugin.UpdateWorkflowCurrentActivities(output.WorkflowsUpdateCurrentActivity.Values);
            }

            //Updating IsAuto flag on activities
            if (output.ActivitiesUpdateIsAuto.Count > 0) {
                _workflowStorePlugin.UpdateActivitiesIsAuto(output.ActivitiesUpdateIsAuto.Values);
            }

            //Creating new activities
            if (output.ActivitiesCreate.Count > 0) {
                _workflowStorePlugin.CreateActivies(output.ActivitiesCreate.Values);
            }

            //Creating new activities and flaging them as current activity
            if (output.ActivitiesCreateUpdateCurrentActivity.Count > 0) {
                _workflowStorePlugin.CreateActiviesAndUpdateWorkflowCurrentActivities(output.ActivitiesCreateUpdateCurrentActivity.Values);
            }
        }

        private void RecalculateWorkflow(IList<WfActivityDefinition> activityDefinitions, RuleConstants ruleConstants, WfWorkflow wf, IDictionary<int, List<RuleDefinition>> dicRules, IDictionary<int, List<RuleConditionDefinition>> dicConditions, IDictionary<int, List<SelectorDefinition>> dicSelectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters, IDictionary<int, List<WfActivity>> dicActivities, IDictionary<int, List<WfDecision>> dicDecision, IDictionary<int, object> dicObjects, WfRecalculationOutput output) {

            if (activityDefinitions.Count == 0) {
                // If the workflow don't have any activity definition, no need to recalculate.
                return;
            }

            object obj;
            dicObjects.TryGetValue(wf.ItemId.Value, out obj);

            if (obj == null) {
                // No item associated to this workflow.
                return;
            }

            List<WfActivity> allActivities;
            dicActivities.TryGetValue(wf.WfwId.Value, out allActivities);

            if (allActivities == null) {
                // No activity for this workflow.
                allActivities = new List<WfActivity>();
            }

            IDictionary<int, WfActivity> activities = allActivities.ToDictionary(a => a.WfadId);

            WfActivity currentActivity;
            bool isLastPreviousCurrentActivityReached = false;
            if (wf.WfaId2 == null) {
                //If the first(s) manual Activity(ies) has(ve) been deleted, the workflow doesn't have a current activity.
                currentActivity = null;
            } else {
                currentActivity = allActivities.Where(a => a.WfaId.Equals(wf.WfaId2.Value)).First();
            }

            bool newCurrentActivityFound = false;

            RuleContext ruleContext = new RuleContext(obj, ruleConstants);

            foreach (WfActivityDefinition activityDefinition in activityDefinitions) {
                int actDefId = activityDefinition.WfadId.Value;
                WfActivity activity;
                activities.TryGetValue(actDefId, out activity);

                bool isRuleValid = _ruleManager.IsRuleValid(actDefId, ruleContext, dicRules, dicConditions);

                if (activity != null && currentActivity != null && activityDefinition.WfadId.Equals(currentActivity.WfadId)) {
                    isLastPreviousCurrentActivityReached = true;
                }

                bool isCurrentActivityAuto = false;

                if (isRuleValid) {
                    //This activity need a validation

                    //We need to check if there is at least one user allowed to validate
                    IList<AccountUser> accounts = _ruleManager.SelectAccounts(actDefId, ruleContext, dicSelectors, dicFilters);

                    if (accounts.Count > 0) {
                        //There is at least one user allowed to validate.
                        if (activity == null) {
                            // No activity linked to this definition was found. 
                            // 2 possibilities : 
                            // - A new activity definition has been inserted in the workflow.
                            // - The previous current activity has been switched to auto.

                            WfActivity wfActivity = GetNewActivity(activityDefinition, wf, false, false);
                            output.AddActivitiesCreateUpdateCurrentActivity(wfActivity);

                            newCurrentActivityFound = true;
                            break;
                        } else if (activity.IsAuto) {
                            //The previous validation was auto. This activity should be manually validated.
                            activity.IsAuto = false;
                            output.AddActivitiesUpdateIsAuto(activity);
                        }

                        // No new activity. The previous activity was manual too.
                        if (activity.IsValid == false) {
                            // This activity must be revalidated
                            wf.WfaId2 = activity.WfaId;
                            output.AddWorkflowsUpdateCurrentActivity(wf);
                            newCurrentActivityFound = true;
                            break;
                        }

                    } else {
                        // There is no users allowed to validate.
                        // This activity is now auto.
                        isCurrentActivityAuto = true;
                        if (activity == null) {
                            // No activity linked to this definition was found. 
                            // 2 possibilities : 
                            // - A new activity definition has been inserted in the workflow.
                            // - The previous current activity has been switched to auto.
                            WfActivity wfActivity = GetNewActivity(activityDefinition, wf, true, false);
                            output.AddActivitiesCreate(wfActivity);
                        } else {
                            activity.IsAuto = true;
                            output.AddActivitiesUpdateIsAuto(activity);
                        }
                    }
                } else {
                    isCurrentActivityAuto = true;
                    if (activity == null) {
                        // No activity linked to this definition was found. 
                        // 2 possibilities : 
                        // - A new activity definition has been inserted in the workflow.
                        // - The previous current activity has been switched to auto.

                        WfActivity wfActivity = GetNewActivity(activityDefinition, wf, true, false);
                        output.AddActivitiesCreate(wfActivity);
                    } else if (activity.IsAuto == false) {
                        // The previous activity was manual but now this activity is auto
                        activity.IsAuto = true;
                        output.AddActivitiesUpdateIsAuto(activity);
                    }
                }

                if (isLastPreviousCurrentActivityReached && isCurrentActivityAuto == false) {
                    // The last activity has been reached.
                    newCurrentActivityFound = true;
                    break;
                }
            }

            if (newCurrentActivityFound == false) {
                // All the definitions have been iterated until the end.
                // The workflow must be ended.

                // Stepping back : No Automatic ending. 
                // TODO: Remove the commented code when the behavior will be validated
                //EndInstance(wf);
            }
        }

        private IDictionary<int, List<RuleDefinition>> constructDicRulesForWorkflowDefinition(int wfwdId) {
            IList<RuleDefinition> rules = _workflowStorePlugin.FindAllRulesByWorkflowDefinitionId(wfwdId);
            //Build a dictionary from the rules: WfadId => List<RuleDefinition>
            IDictionary<int, List<RuleDefinition>> dicRules = rules.GroupBy(c => c.ItemId.Value).ToDictionary(d => d.Key, e => e.ToList());
            return dicRules;
        }

        private IDictionary<int, List<RuleConditionDefinition>> constructDicConditionsForWorkflowDefinition(int wfwdId) {
            IList<RuleConditionDefinition> conditions = _workflowStorePlugin.FindAllConditionsByWorkflowDefinitionId(wfwdId);
            //Build a dictionary from the conditions: RudId => List<RuleConditionDefinition>
            IDictionary<int, List<RuleConditionDefinition>> dicConditions = conditions.GroupBy(c => c.RudId.Value).ToDictionary(d => d.Key, e => e.ToList());
            return dicConditions;
        }

        private IDictionary<int, List<SelectorDefinition>> constructDicSelectorsForWorkflowDefinition(int wfwdId) {
            IList<SelectorDefinition> selectors = _workflowStorePlugin.FindAllSelectorsByWorkflowDefinitionId(wfwdId);
            //Build a dictionary from the selectors: WfadId => List<SelectorDefinition>
            IDictionary<int, List<SelectorDefinition>> dicSelectors = selectors.GroupBy(c => c.ItemId.Value).ToDictionary(d => d.Key, e => e.ToList());
            return dicSelectors;
        }

        private IDictionary<int, List<RuleFilterDefinition>> constructDicFiltersForWorkflowDefinition(int wfwdId) {
            IList<RuleFilterDefinition> filters = _workflowStorePlugin.FindAllFiltersByWorkflowDefinitionId(wfwdId);
            //Build a dictionary from the filters: SelId => List<RuleFilterDefinition>
            IDictionary<int, List<RuleFilterDefinition>> dicFilters = filters.GroupBy(c => c.SelId.Value).ToDictionary(d => d.Key, e => e.ToList());
            return dicFilters;
        }
        #endregion

        #region Custom Methods

        public IList<WfWorkflowDecision> GetWorkflowDecisions(int wfwId) {
            //Get the workflow from id
            WfWorkflow wfWorkflow = _workflowStorePlugin.ReadWorkflowInstanceById(wfwId);

            int wfwdId = wfWorkflow.WfwdId.Value;
            //Get the definition
            WfWorkflowDefinition wfDefinition = _workflowStorePlugin.ReadWorkflowDefinition(wfwdId);

            //Get all the activity definitions for the workflow definition
            IList<WfActivityDefinition> activityDefinitions = _workflowStorePlugin.FindAllDefaultActivityDefinitions(wfDefinition);

            //Build a dictionary : WfadId => WfActivity
            IDictionary<int, WfActivity> dicActivities = _workflowStorePlugin.FindActivitiesByWorkflowId(wfWorkflow).ToDictionary<WfActivity, int>(a => a.WfadId);

            //Get all decisions for the workflow instance
            IList<WfDecision> allDecisions = _workflowStorePlugin.FindDecisionsByWorkflowId(wfWorkflow).ToList<WfDecision>();
            //Build a dictionary from the decisions: WfaId => List<WfDecision>
            IDictionary<int, List<WfDecision>> dicDecision = allDecisions.GroupBy(d => d.WfaId).ToDictionary(d => d.Key, e => e.ToList());

            IDictionary<int, List<RuleDefinition>> dicRules = constructDicRulesForWorkflowDefinition(wfwdId);
            IDictionary<int, List<RuleConditionDefinition>> dicConditions = constructDicConditionsForWorkflowDefinition(wfwdId);
            IDictionary<int, List<SelectorDefinition>> dicSelectors = constructDicSelectorsForWorkflowDefinition(wfwdId);
            IDictionary<int, List<RuleFilterDefinition>> dicFilters = constructDicFiltersForWorkflowDefinition(wfwdId);

            // Fetch the object linked to the workflow instance.
            object obj = _itemStorePlugin.ReadItem(wfWorkflow.ItemId.Value);

            RuleConstants ruleConstants = _ruleManager.GetConstants(wfwdId);
            RuleContext ruleContext = new RuleContext(obj, ruleConstants);

            IList<WfWorkflowDecision> workflowDecisions = new List<WfWorkflowDecision>();

            foreach (WfActivityDefinition activityDefinition in activityDefinitions) {
                int actDefId = activityDefinition.WfadId.Value;
                bool ruleValid = _ruleManager.IsRuleValid(actDefId, ruleContext, dicRules, dicConditions);

                if (ruleValid) {
                    IList<AccountGroup> groups = _ruleManager.SelectGroups(actDefId, ruleContext, dicSelectors, dicFilters);
                    int nbAccount = 0;
                    bool atLeatOnePerson = false;
                    foreach (AccountGroup accountGroup in groups) {
                        ISet<string> accounts = _accountManager.GetStore().GetAccountIds(accountGroup.Id);
                        nbAccount += accounts.Count;
                        if (nbAccount > 0) {
                            atLeatOnePerson = true;
                            break;
                        }
                    }

                    if (atLeatOnePerson) {
                        WfWorkflowDecision wfWorkflowDecision = new WfWorkflowDecision();
                        wfWorkflowDecision.ActivityDefinition = activityDefinition;
                        WfActivity wfActivity;
                        dicActivities.TryGetValue(activityDefinition.WfadId.Value, out wfActivity);
                        wfWorkflowDecision.Activity = wfActivity;
                        wfWorkflowDecision.Groups = groups;
                        List<WfDecision> decisions;
                        if (wfActivity != null) {
                            dicDecision.TryGetValue(wfActivity.WfaId.Value, out decisions);
                            wfWorkflowDecision.Decisions = decisions;
                        }
                        workflowDecisions.Add(wfWorkflowDecision);
                    }
                }
            }


            return workflowDecisions;
        }


        private WfListWorkflowDecision GetWorkflowDecisions(IList<WfActivityDefinition> activityDefinitions, RuleConstants ruleConstants, WfWorkflow wfWorkflow, IDictionary<int, List<RuleDefinition>> dicRules, IDictionary<int, List<RuleConditionDefinition>> dicConditions, IDictionary<int, List<SelectorDefinition>> dicSelectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters, IDictionary<int, List<WfActivity>> dicAllActivities, IDictionary<int, List<WfDecision>> dicDecision, IDictionary<int, object> dicObjects) {
            IList<WfWorkflowDecision> workflowDecisions = new List<WfWorkflowDecision>();

            object obj;
            dicObjects.TryGetValue(wfWorkflow.ItemId.Value, out obj);

            List<WfActivity> activities;
            dicAllActivities.TryGetValue(wfWorkflow.WfwId.Value, out activities);

            if (activities == null) {
                // No activity for this workflow.
                activities = new List<WfActivity>();
            }

            IDictionary<int, WfActivity> dicActivities = activities.ToDictionary(a => a.WfadId);

            RuleContext ruleContext = new RuleContext(obj, ruleConstants);

            foreach (WfActivityDefinition activityDefinition in activityDefinitions) {
                int actDefId = activityDefinition.WfadId.Value;
                bool ruleValid = _ruleManager.IsRuleValid(actDefId, ruleContext, dicRules, dicConditions);

                if (ruleValid) {
                    IList<AccountGroup> groups = _ruleManager.SelectGroups(actDefId, ruleContext, dicSelectors, dicFilters);
                    int nbAccount = 0;
                    bool atLeatOnePerson = false;
                    foreach (AccountGroup accountGroup in groups) {
                        ISet<string> accounts = _accountManager.GetStore().GetAccountIds(accountGroup.Id);
                        nbAccount += accounts.Count;
                        if (nbAccount > 0) {
                            atLeatOnePerson = true;
                            break;
                        }
                    }

                    if (atLeatOnePerson) {
                        WfWorkflowDecision wfWorkflowDecision = new WfWorkflowDecision();
                        wfWorkflowDecision.ActivityDefinition = activityDefinition;
                        WfActivity wfActivity;
                        dicActivities.TryGetValue(activityDefinition.WfadId.Value, out wfActivity);
                        wfWorkflowDecision.Activity = wfActivity;
                        wfWorkflowDecision.Groups = groups;
                        List<WfDecision> decisions;
                        if (wfActivity != null) {
                            dicDecision.TryGetValue(wfActivity.WfaId.Value, out decisions);
                            wfWorkflowDecision.Decisions = decisions;
                        }
                        workflowDecisions.Add(wfWorkflowDecision);
                    }
                }
            }

            WfListWorkflowDecision ret = new WfListWorkflowDecision();
            ret.WfWorkflow = wfWorkflow;
            ret.WorkflowDecisions = workflowDecisions;

            return ret;
        }

        public IList<WfListWorkflowDecision> GetAllWorkflowDecisions(int wfwdId) {
            //Get the definition
            WfWorkflowDefinition wfWorkflowDefinition = _workflowStorePlugin.ReadWorkflowDefinition(wfwdId);

            //Get all the activity definitions for the workflow definition
            IList<WfActivityDefinition> activityDefinitions = _workflowStorePlugin.FindAllDefaultActivityDefinitions(wfWorkflowDefinition);

            IList<WfActivity> allActivities = _workflowStorePlugin.FindAllActivitiesByWorkflowDefinitionId(wfWorkflowDefinition);
            IList<WfDecision> allDecisions = _workflowStorePlugin.FindAllDecisionsByWorkflowDefinitionId(wfWorkflowDefinition);

            //Build a dictionnary from the workflows: WfwId => List<WfActivity>
            IDictionary<int, List<WfActivity>> dicAllActivities = allActivities.GroupBy(c => c.WfwId).ToDictionary(d => d.Key, e => e.ToList());

            //Build a dictionary from the decisions: WfaId => List<WfDecision>
            IDictionary<int, List<WfDecision>> dicDecision = allDecisions.GroupBy(d => d.WfaId).ToDictionary(d => d.Key, e => e.ToList());

            IDictionary<int, List<RuleDefinition>> dicRules = constructDicRulesForWorkflowDefinition(wfwdId);
            IDictionary<int, List<RuleConditionDefinition>> dicConditions = constructDicConditionsForWorkflowDefinition(wfwdId);
            IDictionary<int, List<SelectorDefinition>> dicSelectors = constructDicSelectorsForWorkflowDefinition(wfwdId);
            IDictionary<int, List<RuleFilterDefinition>> dicFilters = constructDicFiltersForWorkflowDefinition(wfwdId);

            IList<WfWorkflow> workflows = _workflowStorePlugin.FindActiveWorkflows(wfWorkflowDefinition, false);

            List<int> itemIds = workflows.Select(w => w.ItemId.Value).ToList();
            IDictionary<int, object> dicObjects = _itemStorePlugin.ReadItems(itemIds);

            RuleConstants ruleConstants = _ruleManager.GetConstants(wfwdId);

            IList<WfListWorkflowDecision> ret = new List<WfListWorkflowDecision>();

            foreach (WfWorkflow wfWorkflow in workflows) {
                WfListWorkflowDecision wfListWorkflowDecision = GetWorkflowDecisions(activityDefinitions, ruleConstants, wfWorkflow, dicRules, dicConditions, dicSelectors, dicFilters, dicAllActivities, dicDecision, dicObjects);
                ret.Add(wfListWorkflowDecision);
            }

            return ret;
        }

        public void RemoveSelectorsFiltersByGroupId(string groupId) {
            Debug.Assert(groupId != null);
            //---
            _ruleManager.RemoveSelectorsFiltersByGroupId(groupId);
        }

        #endregion

        #region FindAll*ByWorkflowDefinition methods

        public IList<WfActivity> FindAllActivitiesByWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition) {
            return _workflowStorePlugin.FindAllActivitiesByWorkflowDefinitionId(wfWorkflowDefinition);
        }

        public IList<WfDecision> FindAllDecisionsByWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition) {
            return _workflowStorePlugin.FindAllDecisionsByWorkflowDefinitionId(wfWorkflowDefinition);
        }

        public IList<RuleDefinition> FindAllRulesDecisionsByWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition) {
            return _workflowStorePlugin.FindAllRulesByWorkflowDefinitionId(wfWorkflowDefinition.WfwdId.Value);
        }

        public IList<RuleConditionDefinition> FindAllConditionsDecisionsByWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition) {
            return _workflowStorePlugin.FindAllConditionsByWorkflowDefinitionId(wfWorkflowDefinition.WfwdId.Value);
        }

        public IList<SelectorDefinition> FindAllSelectorsDecisionsByWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition) {
            return _workflowStorePlugin.FindAllSelectorsByWorkflowDefinitionId(wfWorkflowDefinition.WfwdId.Value);
        }

        public IList<RuleFilterDefinition> FindAllFiltersByWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition) {
            return _workflowStorePlugin.FindAllFiltersByWorkflowDefinitionId(wfWorkflowDefinition.WfwdId.Value);
        }

        #endregion
    }
}