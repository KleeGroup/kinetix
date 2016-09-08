using System;
using System.Collections.Generic;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Kinetix.Workflow
{
    public class MemoryWorkflowStorePlugin : IWorkflowStorePlugin
    {

        // WorkflowInstance
        private int memoryWorkflowInstanceSequenceGenerator = 0;
        private IDictionary<long?, WfWorkflow> inMemoryWorkflowInstanceStore = new ConcurrentDictionary<long?, WfWorkflow>();

        // Transition
        private IDictionary<string, WfTransitionDefinition> transitionsNext = new ConcurrentDictionary<string, WfTransitionDefinition>();

        // Activity
        private int memoryActivitySequenceGenerator = 0;
        private IDictionary<long?, WfActivity> inMemoryActivityStore = new ConcurrentDictionary<long?, WfActivity>();

        // Decision
        private int memoryDecisionSequenceGenerator = 0;
        private IDictionary<long?, WfDecision> inMemoryDecisionStore = new ConcurrentDictionary<long?, WfDecision>();

        // ActivityDefinition
        private int memoryActivityDefinitionSequenceGenerator = 0;
        private IDictionary<long?, WfActivityDefinition> inMemoryActivityDefinitionStore = new ConcurrentDictionary<long?, WfActivityDefinition>();

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
            long? idActivity = wfWorkflowDefinition.WfadId;
            if (idActivity == null)
            {
                //The workflow don't have a starting activity
                return 0;
            }

            WfTransitionDefinition transitionNext = transitionsNext[idActivity + "|" + WfCodeTransition.Default.ToString()];

            int count = 0;
            while (transitionNext != null)
            {
                WfActivityDefinition wfNextActivityDefinition = inMemoryActivityDefinitionStore[transitionNext.WfadIdTo];
                idActivity = wfNextActivityDefinition.WfadId;
                transitionNext = transitionsNext[wfNextActivityDefinition.WfadId + "|" + WfCodeTransition.Default.ToString()];
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
            WfTransitionDefinition transitionNext = transitionsNext[idActivity + "|" + WfCodeTransition.Default.ToString()];

            int i = 1;
            while (transitionNext != null && i < position)
            {
                WfActivityDefinition wfNextActivityDefinition = inMemoryActivityDefinitionStore[transitionNext.WfadIdTo];
                idActivity = wfNextActivityDefinition.WfadId;
                transitionNext = transitionsNext[wfNextActivityDefinition.WfadId + "|" + WfCodeTransition.Default.ToString()];
                i++;
            }

            if (transitionNext == null)
            {
                return null;
            }

            return ReadActivityDefinition(transitionNext.WfadIdTo);
        }

        public IList<WfActivityDefinition> FindActivityMatchingRules()
        {
            throw new NotImplementedException();
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

        public IList<WfActivityDefinition> FindAllDefaultActivityDefinitions(WfWorkflowDefinition wfWorkflowDefinition)
        {
            Debug.Assert(wfWorkflowDefinition != null);
            //---
            long? idStartActivity = wfWorkflowDefinition.WfadId;
            IList<WfActivityDefinition> retAllDefaultActivities = new List<WfActivityDefinition>();

            WfTransitionDefinition transitionNext = transitionsNext[idStartActivity + "|" + WfCodeTransition.Default.ToString()];

            while (transitionNext != null)
            {
                WfActivityDefinition wfNextActivityDefinition = inMemoryActivityDefinitionStore[transitionNext.WfadIdTo];
                retAllDefaultActivities.Add(wfNextActivityDefinition);
                transitionNext = transitionsNext[wfNextActivityDefinition.WfadId + "|" + WfCodeTransition.Default.ToString()];
            }

            return retAllDefaultActivities;
        }

        public WfActivityDefinition FindNextActivity(WfActivity activity)
        {
            return FindNextActivity(activity, WfCodeTransition.Default.ToString());
        }

        public WfActivityDefinition FindNextActivity(WfActivity activity, string transitionName)
        {
            WfTransitionDefinition transitionNext = transitionsNext[activity.WfaId + "|" + transitionName];
            return inMemoryActivityDefinitionStore[transitionNext.WfadIdTo];
        }

        public bool HasNextActivity(WfActivity activity)
        {
            return HasNextActivity(activity, WfCodeTransition.Default.ToString());
        }

        public bool HasNextActivity(WfActivity activity, string transitionName)
        {
            return transitionsNext.ContainsKey(activity.WfaId + "|" + transitionName);
        }

        public WfActivity ReadActivity(long wfadId)
        {
            return inMemoryActivityStore[wfadId];
        }

        public WfActivityDefinition ReadActivityDefinition(long wfadId)
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

        public WfWorkflowDefinition ReadWorkflowDefinition(long wfwdId)
        {
            return inMemoryWorkflowDefinitionStore[wfwdId];
        }

        public WfWorkflow ReadWorkflowInstanceById(long wfwId)
        {
            return inMemoryWorkflowInstanceStore[wfwId];
        }

        public WfWorkflow ReadWorkflowInstanceByItemId(long itemId)
        {
            foreach (WfWorkflow wfWorkflow in inMemoryWorkflowInstanceStore.Values)
            {
                if (itemId.Equals(wfWorkflow.ItemId))
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
    }
}
