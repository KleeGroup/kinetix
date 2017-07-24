using System;
using System.Collections.Generic;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using Kinetix.Rules;

namespace Kinetix.Workflow
{
    public class MemoryWorkflowStorePlugin : IWorkflowStorePlugin
    {

        // WorkflowInstance
        private int memoryWorkflowInstanceSequenceGenerator = 0;
        private IDictionary<int?, WfWorkflow> inMemoryWorkflowInstanceStore = new ConcurrentDictionary<int?, WfWorkflow>();

        // Transition ( idFrom+ "|" +name => WfTransitionDefinition)
        private IDictionary<string, WfTransitionDefinition> transitionsNext = new ConcurrentDictionary<string, WfTransitionDefinition>();

        // Activity
        private int memoryActivitySequenceGenerator = 0;
        private IDictionary<int?, WfActivity> inMemoryActivityStore = new ConcurrentDictionary<int?, WfActivity>();

        // Decision
        private int memoryDecisionSequenceGenerator = 0;
        private IDictionary<int?, WfDecision> inMemoryDecisionStore = new ConcurrentDictionary<int?, WfDecision>();

        // ActivityDefinition
        private int memoryActivityDefinitionSequenceGenerator = 0;
        private IDictionary<int?, WfActivityDefinition> inMemoryActivityDefinitionStore = new ConcurrentDictionary<int?, WfActivityDefinition>();

        // WorkflowDefinition
        private int memoryWorkflowDefinitionSequenceGenerator = 0;
        private IDictionary<long?, WfWorkflowDefinition> inMemoryWorkflowDefinitionStore = new ConcurrentDictionary<long?, WfWorkflowDefinition>();

        public void AddTransition(WfTransitionDefinition transition)
        {
            Debug.Assert(transition != null);
            Debug.Assert(transition.Name != null);
            //--
            transitionsNext.Add(transition.WfadIdFrom + "|" + transition.Name, transition);
        }

        public void RemoveTransition(WfTransitionDefinition transition)
        {
            Debug.Assert(transition != null);
            Debug.Assert(transition.Name != null);
            //--
            transitionsNext.Remove(transition.WfadIdFrom + "|" + transition.Name);
        }

        public int CountDefaultTransitions(WfWorkflowDefinition wfWorkflowDefinition)
        {
            Debug.Assert(wfWorkflowDefinition != null);
            //--
            int? idActivityDefinition = wfWorkflowDefinition.WfadId;
            if (idActivityDefinition == null)
            {
                //The workflow don't have a starting activity
                return 0;
            }
            WfTransitionDefinition transitionNext;
            transitionsNext.TryGetValue(idActivityDefinition + "|" + WfCodeTransition.Default.ToString(), out transitionNext);

            int count = 0;
            while (transitionNext != null)
            {
                WfActivityDefinition wfNextActivityDefinition = inMemoryActivityDefinitionStore[transitionNext.WfadIdTo];
                idActivityDefinition = wfNextActivityDefinition.WfadId;
                transitionsNext.TryGetValue(idActivityDefinition + "|" + WfCodeTransition.Default.ToString(), out transitionNext);
                count++;
            }

            return count;
        }

        public void CreateActivity(WfActivity wfActivity)
        {
            int generatedId = Interlocked.Increment(ref memoryActivitySequenceGenerator);
            wfActivity.WfaId = generatedId;
            inMemoryActivityStore[generatedId] = wfActivity;
        }

        public void DeleteWorkflow(int wfwId)
        {
            IList<int> actIds = new List<int>();

            foreach(WfActivity wfActivity in inMemoryActivityStore.Values)
            {
                IList<int> decIds = inMemoryDecisionStore.Where(wfDec => wfDec.Value.WfaId.Equals(wfActivity.WfaId)).Select(wfDec => wfDec.Key.Value).ToList();

                foreach(int decId in decIds)
                {
                    inMemoryDecisionStore.Remove(decId);
                }
                
                actIds.Add(wfActivity.WfaId.Value);
            }


            foreach (int actId in actIds)
            {
                inMemoryDecisionStore.Remove(actId);
            }

            inMemoryWorkflowInstanceStore.Remove(wfwId);
        }

        public void CreateActivityDefinition(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityDefinition)
        {
            int generatedId = Interlocked.Increment(ref memoryActivityDefinitionSequenceGenerator);
            wfActivityDefinition.WfadId = generatedId;
            inMemoryActivityDefinitionStore[generatedId] = wfActivityDefinition;
        }
        
        public void CreateDecision(WfDecision wfDecision)
        {
            int generatedId = Interlocked.Increment(ref memoryDecisionSequenceGenerator);
            wfDecision.Id = generatedId;
            inMemoryDecisionStore[generatedId] = wfDecision;
        }

        public void DeleteDecision(WfDecision wfDecision)
        {
            inMemoryDecisionStore.Remove(wfDecision.Id);
        }

        public void CreateWorkflowDefinition(WfWorkflowDefinition workflowDefinition)
        {
            int generatedId = Interlocked.Increment(ref memoryWorkflowDefinitionSequenceGenerator);
            workflowDefinition.WfwdId = generatedId;
            inMemoryWorkflowDefinitionStore[generatedId] = workflowDefinition;
        }

        public void CreateWorkflowInstance(WfWorkflow workflow)
        {
            int generatedId = Interlocked.Increment(ref memoryWorkflowInstanceSequenceGenerator);
            workflow.WfwId = generatedId;
            inMemoryWorkflowInstanceStore[generatedId] = workflow;
        }

        public void DeleteActivity(WfActivity wfActivity)
        {
            inMemoryActivityStore.Remove(wfActivity.WfaId);
        }

        public void DeleteActivityDefinition(WfActivityDefinition wfActivityDefinition)
        {
            inMemoryActivityDefinitionStore.Remove(wfActivityDefinition.WfadId);
        }

        public WfActivityDefinition FindActivityDefinitionByPosition(WfWorkflowDefinition wfWorkflowDefinition, int position)
        {
            int? idActivity = wfWorkflowDefinition.WfadId;

            if (idActivity == null)
            {
                //The workflow don't have a starting activity
                return null;
            }

            if (position == 1)
            {
                return ReadActivityDefinition(idActivity.Value);
            }

            WfTransitionDefinition transitionNext;
            transitionsNext.TryGetValue(idActivity + "|" + WfCodeTransition.Default.ToString(), out transitionNext);

            int i = 1;
            while (transitionNext != null && i < (position - 1))
            {
                WfActivityDefinition wfNextActivityDefinition = inMemoryActivityDefinitionStore[transitionNext.WfadIdTo];
                idActivity = wfNextActivityDefinition.WfadId;
                transitionsNext.TryGetValue(wfNextActivityDefinition.WfadId + "|" + WfCodeTransition.Default.ToString(), out transitionNext);
                i++;
            }

            if (transitionNext == null)
            {
                return null;
            }

            return ReadActivityDefinition(transitionNext.WfadIdTo);
        }

        public IList<WfDecision> FindAllDecisionByActivity(WfActivity wfActivity)
        {
            Debug.Assert(wfActivity != null);
            Debug.Assert(wfActivity.WfaId != null);
            //---
            IList<WfDecision> wfDecisions = new List<WfDecision>();
            foreach (WfDecision wfDecision in inMemoryDecisionStore.Values)
            {
                if (wfActivity.WfaId.Equals(wfDecision.WfaId))
                {
                    wfDecisions.Add(wfDecision);
                }
            }

            return wfDecisions;
        }

        public IList<WfActivityDefinition> FindAllDefaultActivityDefinitions(WfWorkflowDefinition wfWorkflowDefinition, int startingPos = 0)
        {
            Debug.Assert(wfWorkflowDefinition != null);
            //---
            int? idStartActivity = wfWorkflowDefinition.WfadId;
            IList<WfActivityDefinition> retAllDefaultActivities = new List<WfActivityDefinition>();
            if (idStartActivity != null)
            {
                WfActivityDefinition first = inMemoryActivityDefinitionStore[idStartActivity];
                retAllDefaultActivities.Add(first);

                WfTransitionDefinition transitionNext;
                transitionsNext.TryGetValue(idStartActivity + "|" + WfCodeTransition.Default.ToString(), out transitionNext);

                while (transitionNext != null)
                {
                    WfActivityDefinition wfNextActivityDefinition = inMemoryActivityDefinitionStore[transitionNext.WfadIdTo];
                    retAllDefaultActivities.Add(wfNextActivityDefinition);
                    transitionsNext.TryGetValue(wfNextActivityDefinition.WfadId + "|" + WfCodeTransition.Default.ToString(), out transitionNext);
                }
            }

            return retAllDefaultActivities;
        }

        public WfActivityDefinition FindNextActivity(int wfadId)
        {
            return FindNextActivity(wfadId, WfCodeTransition.Default.ToString());
        }

        public WfActivityDefinition FindNextActivity(int wfadId, string transitionName)
        {
            WfTransitionDefinition transitionNext = transitionsNext[wfadId + "|" + transitionName];
            return inMemoryActivityDefinitionStore[transitionNext.WfadIdTo];
        }

        public bool HasNextActivity(WfActivity activity)
        {
            return HasNextActivity(activity, WfCodeTransition.Default.ToString());
        }

        public bool HasNextActivity(WfActivity activity, string transitionName)
        {
            return transitionsNext.ContainsKey(activity.WfadId + "|" + transitionName);
        }

        public WfActivity ReadActivity(int wfadId)
        {
            return inMemoryActivityStore[wfadId];
        }

        public WfActivityDefinition ReadActivityDefinition(int wfadId)
        {
            return inMemoryActivityDefinitionStore[wfadId];
        }

        public WfWorkflowDefinition ReadWorkflowDefinition(string definitionName)
        {
            Debug.Assert(definitionName != null);
            //---
            foreach (WfWorkflowDefinition wfWorkflowDefinition in inMemoryWorkflowDefinitionStore.Values)
            {
                if (definitionName.Equals(wfWorkflowDefinition.Name))
                {
                    return wfWorkflowDefinition;
                }
            }

            return null;
        }

        public WfWorkflowDefinition ReadWorkflowDefinition(int wfwdId)
        {
            return inMemoryWorkflowDefinitionStore[wfwdId];
        }

        public WfWorkflow ReadWorkflowInstanceById(int wfwId)
        {
            return inMemoryWorkflowInstanceStore[wfwId];
        }
        public WfWorkflow ReadWorkflowInstanceForUpdateById(int wfwId)
        {
            //No lock for Memory Plugin
            return ReadWorkflowInstanceById(wfwId);
        }

        public IList<WfWorkflow> ReadWorkflowsInstanceForUpdateById(int wfwdId)
        {
            //No lock for Memory Plugin
            return new List<WfWorkflow>(inMemoryWorkflowInstanceStore.Values);
        }

        public WfWorkflow ReadWorkflowInstanceByItemId(int wfwdId, int itemId)
        {
            foreach (WfWorkflow wfWorkflow in inMemoryWorkflowInstanceStore.Values)
            {
                if (itemId.Equals(wfWorkflow.ItemId) && wfwdId.Equals(wfWorkflow.WfwdId))
                {
                    return wfWorkflow;
                }
            }

            return null;
        }

        public void UpdateActivity(WfActivity wfActivity)
        {
            inMemoryActivityStore[wfActivity.WfaId] = wfActivity;
        }

        public void UpdateActivityDefinition(WfActivityDefinition wfActivityDefinition)
        {
            Debug.Assert(wfActivityDefinition != null);
            Debug.Assert(wfActivityDefinition.WfadId != null);
            Debug.Assert(inMemoryActivityDefinitionStore.ContainsKey(wfActivityDefinition.WfadId), "This activity cannot be updated : It does not exist in the store");
            //---
            inMemoryActivityDefinitionStore[wfActivityDefinition.WfadId] = wfActivityDefinition;
        }

        public void RenameActivityDefinition(WfActivityDefinition wfActivityDefinition)
        {
            Debug.Assert(wfActivityDefinition != null);
            Debug.Assert(wfActivityDefinition.WfadId != null);
            //---
            inMemoryActivityDefinitionStore[wfActivityDefinition.WfadId].Name = wfActivityDefinition.Name;
        }


        public void UpdateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition)
        {
            Debug.Assert(wfWorkflowDefinition != null);
            Debug.Assert(wfWorkflowDefinition.WfwdId != null);
            Debug.Assert(inMemoryWorkflowDefinitionStore.ContainsKey(wfWorkflowDefinition.WfwdId), "This activity cannot be updated : It does not exist in the store");
            //---
            inMemoryWorkflowDefinitionStore[wfWorkflowDefinition.WfwdId] = wfWorkflowDefinition;
        }

        public void UpdateWorkflowInstance(WfWorkflow workflow)
        {
            Debug.Assert(workflow != null);
            Debug.Assert(workflow.WfwdId != null);
            Debug.Assert(inMemoryWorkflowInstanceStore.ContainsKey(workflow.WfwId), "This workflow cannot be updated : It does not exist in the store");
            //---
            inMemoryWorkflowInstanceStore[workflow.WfwId] = workflow;
        }

        public IList<WfWorkflow> FindActiveWorkflowInstanceByItemId(int wfwdId, int itemId)
        {
            IList<WfWorkflow> ret = new List<WfWorkflow>();
            List<string> statusActif = new List<string>() { WfCodeStatusWorkflow.Sta.ToString(), WfCodeStatusWorkflow.End.ToString() };
            foreach (WfWorkflow wfWorkflow in inMemoryWorkflowInstanceStore.Values)
            {
                if (itemId.Equals(wfWorkflow.ItemId) && wfwdId.Equals(wfWorkflow.WfwdId) && statusActif.Contains(wfWorkflow.WfsCode))
                {
                    ret.Add(wfWorkflow);
                }
            }

            return ret;
        }

        public IList<WfActivity> FindActivitiesByDefinitionId(WfWorkflow wfWorkflow, IList<int> wfadIds)
        {
            Debug.Assert(wfWorkflow != null);
            Debug.Assert(wfadIds != null);
            //---
            IList<WfActivity> wfActivities = new List<WfActivity>();
            foreach (WfActivity wfActivity in inMemoryActivityStore.Values)
            {
                if (wfadIds.Contains(wfActivity.WfaId.Value) && wfWorkflow.WfwId.Equals(wfActivity.WfwId))
                {
                    wfActivities.Add(wfActivity);
                }
            }

            return wfActivities;
        }

        public IList<WfActivity> FindActivitiesByWorkflowId(WfWorkflow wfWorkflow)
        {
            Debug.Assert(wfWorkflow != null);
            //---
            IList<WfActivity> wfActivities = new List<WfActivity>();
            foreach (WfActivity wfActivity in inMemoryActivityStore.Values)
            {
                if (wfWorkflow.WfwId.Equals(wfActivity.WfwId))
                {
                    wfActivities.Add(wfActivity);
                }
            }

            return wfActivities;
        }

        public IList<WfActivity> FindAllActivitiesByWorkflowDefinitionId(WfWorkflowDefinition wfWorkflowDefinition)
        {
            Debug.Assert(wfWorkflowDefinition != null);
            //---
            IList<WfActivity> wfActivities = new List<WfActivity>();

            IList<WfActivityDefinition> activityDefinitions =  FindAllDefaultActivityDefinitions(wfWorkflowDefinition);

            foreach (WfActivity wfActivity in inMemoryActivityStore.Values)
            {
                foreach (WfActivityDefinition wdActivityDefinition in activityDefinitions)
                {
                    if (wdActivityDefinition.WfadId.Equals(wfActivity.WfadId))
                    {
                        wfActivities.Add(wfActivity);
                    }
                }
            }

            return wfActivities;
        }

        public IList<WfDecision> FindDecisionsByWorkflowId(WfWorkflow wfWorkflow)
        {

            Debug.Assert(wfWorkflow != null);
            Debug.Assert(wfWorkflow.WfwId != null);
            //---

            IList<WfActivity> wfActivities = FindActivitiesByWorkflowId(wfWorkflow);

            IList<int> wfActivitiesId = wfActivities.Select<WfActivity, int>(a => a.WfaId.Value).ToList();

            IList<WfDecision> wfDecisions = new List<WfDecision>();
            foreach (WfDecision wfDecision in inMemoryDecisionStore.Values)
            {
                if (wfActivitiesId.Contains(wfDecision.WfaId))
                {
                    wfDecisions.Add(wfDecision);
                }
            }

            return wfDecisions;
        }

        public void UpdateDecision(WfDecision wfDecision)
        {
            Debug.Assert(wfDecision != null);
            Debug.Assert(wfDecision.Id != null);
            Debug.Assert(inMemoryDecisionStore.ContainsKey(wfDecision.Id), "This workflow cannot be updated : It does not exist in the store");
            //---
            inMemoryDecisionStore[wfDecision.Id] = wfDecision;
        }

        public IList<WfDecision> ReadDecisionsByActivityId(int wfaId)
        {
            IList<WfDecision> collect = new List<WfDecision>();
            foreach (WfDecision wfDecision in inMemoryDecisionStore.Values)
            {
                if (wfaId.Equals(wfDecision.WfaId))
                {
                    collect.Add(wfDecision);
                }
            }

            return collect;
        }

        public WfActivity FindActivityByDefinitionWorkflow(WfWorkflow wfWorkflow, WfActivityDefinition wfActivityDefinition)
        {

            foreach (WfActivity wfActivity in inMemoryActivityStore.Values)
            {
                if (wfActivityDefinition.WfadId.Equals(wfActivity.WfadId))
                {
                    return wfActivity;
                }
            }

            return null;
        }

        public IList<WfWorkflow> FindActiveWorkflows(WfWorkflowDefinition wfWorkflowDefinition, bool isForUpdate)
        {
            IList<WfWorkflow> collect = new List<WfWorkflow>();
            foreach (WfWorkflow wfWorkflow in inMemoryWorkflowInstanceStore.Values)
            {
                WfCodeStatusWorkflow status = (WfCodeStatusWorkflow)Enum.Parse(typeof(WfCodeStatusWorkflow), wfWorkflow.WfsCode, true);

                if (wfWorkflowDefinition.WfwdId.Equals(wfWorkflow.WfwId) && (status == WfCodeStatusWorkflow.Sta || status == WfCodeStatusWorkflow.Pau))
                {
                    collect.Add(wfWorkflow);
                }
            }

            return collect;
        }

        public void UpdateTransition(WfTransitionDefinition transition)
        {
            transitionsNext[transition.WfadIdFrom + "|" + transition.Name] = transition;
        }

        public WfTransitionDefinition FindTransition(WfTransitionCriteria wfTransitionCriteria)
        {
            Debug.Assert(wfTransitionCriteria != null);
            //---

            foreach (WfTransitionDefinition tr in transitionsNext.Values)
            {
                bool matchFrom = wfTransitionCriteria.WfadIdFrom == null || wfTransitionCriteria.WfadIdFrom.Equals(tr.WfadIdFrom);
                bool matchTo = wfTransitionCriteria.WfadIdTo == null || wfTransitionCriteria.WfadIdTo.Equals(tr.WfadIdTo);

                if (wfTransitionCriteria.TransitionName.Equals(tr.Name) && matchFrom && matchTo)
                {
                    return tr;
                }
            }
            return null;
        }

        public void IncrementActivityDefinitionPositionsAfter(int wfwdId, int position)
        {
            ShiftLevelAfter(wfwdId, position, 1);
        }

        public void DecrementActivityDefinitionPositionsAfter(int wfwdId, int position)
        {
            ShiftLevelAfter(wfwdId, position, -1);
        }

        private void ShiftLevelAfter(int wfwdId, int position, int shift)
        {
            foreach (WfActivityDefinition wfActivityDefinition in inMemoryActivityDefinitionStore.Values)
            {
                if (wfwdId.Equals(wfActivityDefinition.WfwdId) && wfActivityDefinition.Level.Value >= position)
                {
                    wfActivityDefinition.Level = wfActivityDefinition.Level.Value + shift;
                }
            }
        }

        public void ShiftActivityDefinitionPositionsBetween(int wfwdId, int posStart, int posEnd, int shift)
        {
            foreach (WfActivityDefinition wfActivityDefinition in inMemoryActivityDefinitionStore.Values)
            {
                if (wfwdId.Equals(wfActivityDefinition.WfwdId) && wfActivityDefinition.Level.Value >= posStart && wfActivityDefinition.Level.Value <= posEnd)
                {
                    wfActivityDefinition.Level = wfActivityDefinition.Level.Value + shift;
                }
            }
        }

        public void DeleteActivities(int wfadId)
        {
            IList<int> wfaIds = new List<int>();

            foreach(WfActivity wfActivity in inMemoryActivityStore.Values)
            {
                if (wfadId.Equals(wfActivity.WfadId))
                {
                    inMemoryActivityStore.Remove(wfActivity.WfaId);
                    wfaIds.Add(wfActivity.WfaId.Value);
                }
            }

            foreach (WfDecision wfDecision in inMemoryDecisionStore.Values)
            {
                if (wfaIds.Contains(wfDecision.WfaId))
                {
                    inMemoryDecisionStore.Remove(wfDecision.Id);
                }
            }
        }

        public void UnsetCurrentActivity(WfActivityDefinition wfActivityDefinition)
        {
            foreach(WfWorkflow wf in inMemoryWorkflowInstanceStore.Values)
            {
                WfActivity currentActivity = ReadActivity(wf.WfaId2.Value);
                if (wf.WfaId2.Equals(currentActivity.WfaId) && wfActivityDefinition.WfadId.Equals(currentActivity.WfadId))
                {
                    wf.WfaId2 = null;
                }
            }
        }

        #region directAccesRules
        public IList<RuleDefinition> FindAllRulesByWorkflowDefinitionId(int wfwdId)
        {
            throw new NotImplementedException();
        }

        public IList<RuleConditionDefinition> FindAllConditionsByWorkflowDefinitionId(int wfwdId)
        {
            throw new NotImplementedException();
        }

        public IList<SelectorDefinition> FindAllSelectorsByWorkflowDefinitionId(int wfwdId)
        {
            throw new NotImplementedException();
        }

        public IList<RuleFilterDefinition> FindAllFiltersByWorkflowDefinitionId(int wfwdId)
        {
            throw new NotImplementedException();
        }

        public IList<WfDecision> FindAllDecisionsByWorkflowDefinitionId(WfWorkflowDefinition wfWorkflowDefinition)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Workflow Mass Update/Create
        public void UpdateActivitiesIsAuto(ICollection<WfActivityUpdate> activities)
        {
            foreach (WfActivityUpdate actUpd in activities)
            {
                inMemoryActivityStore[actUpd.WfaId].IsAuto = actUpd.IsAuto;
            }
        }

        public void CreateActiviesAndUpdateWorkflowCurrentActivities(ICollection<WfActivity> activities)
        {
            foreach (WfActivity activity in activities)
            {
                CreateActivity(activity);
                inMemoryWorkflowInstanceStore[activity.WfwId].WfaId2 = activity.WfaId;
            }

        }

        public void CreateActivies(ICollection<WfActivity> activities)
        {
            foreach(WfActivity activity in activities)
            {
                CreateActivity(activity);
            }
        }

        public void UpdateWorkflowCurrentActivities(ICollection<WfWorkflowUpdate> worfklows)
        {
            foreach(WfWorkflowUpdate wfUpd in worfklows)
            {
                inMemoryWorkflowInstanceStore[wfUpd.WfwId.Value].WfaId2 = wfUpd.WfaId2;
            }
        }

        public void CreateActivityDecision(ICollection<WfActivityDecision> activities)
        {
            foreach (WfActivityDecision activity in activities)
            {
                CreateActivity(activity.Activity);
                activity.Decision.WfaId = activity.Activity.WfaId.Value;
                CreateDecision(activity.Decision);
            }

        }




        #endregion

    }
}
