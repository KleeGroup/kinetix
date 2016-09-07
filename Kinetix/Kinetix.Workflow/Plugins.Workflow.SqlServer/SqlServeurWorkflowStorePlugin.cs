using System;
using System.Collections.Generic;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.ServiceModel;
using Kinetix.Broker;

namespace Kinetix.Workflow.Plugins.Workflow.SqlServer
{
    public class SqlServeurWorkflowStorePlugin : IWorkflowStorePlugin
    {
        [OperationContract]
        public void AddTransition(WfTransitionDefinition transition)
        {
            BrokerManager.GetBroker<WfTransitionDefinition>().Save(transition);
        }

        [OperationContract]
        public int CountDefaultTransitions(WfWorkflowDefinition wfWorkflowDefinition)
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        public void CreateActivity(WfActivity wfActivity)
        {
            BrokerManager.GetBroker<WfActivity>().Save(wfActivity);
        }

        [OperationContract]
        public void CreateActivityDefinition(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityDefinition)
        {
            wfActivityDefinition.WfwdId = (long) wfWorkflowDefinition.WfwdId;
            BrokerManager.GetBroker<WfActivityDefinition>().Save(wfActivityDefinition);
        }

        [OperationContract]
        public void CreateDecision(WfDecision wfDecision)
        {
            BrokerManager.GetBroker<WfDecision>().Save(wfDecision);
        }

        [OperationContract]
        public void CreateWorkflowDefinition(WfWorkflowDefinition workflowDefinition)
        {
            BrokerManager.GetBroker<WfWorkflowDefinition>().Save(workflowDefinition);
        }

        [OperationContract]
        public void CreateWorkflowInstance(WfWorkflow workflow)
        {
            BrokerManager.GetBroker<WfWorkflow>().Save(workflow);
        }

        [OperationContract]
        public void DeleteActivity(WfActivity wfActivity)
        {
            BrokerManager.GetBroker<WfWorkflow>().Delete(wfActivity);
        }

        [OperationContract]
        public void DeleteActivityDefinition(WfActivityDefinition wfActivityDefinition)
        {
            BrokerManager.GetBroker<WfActivityDefinition>().Delete(wfActivityDefinition);
        }

        [OperationContract]
        public WfActivityDefinition FindActivityDefinitionByPosition(WfWorkflowDefinition wfWorkflowDefinition, int position)
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        public IList<WfActivityDefinition> FindActivityMatchingRules()
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        public IList<WfDecision> FindAllDecisionByActivity(WfActivity wfActivity)
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        public IList<WfActivityDefinition> FindAllDefaultActivityDefinitions(WfWorkflowDefinition wfWorkflowDefinition)
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        public WfActivityDefinition FindNextActivity(WfActivity activity)
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        public WfActivityDefinition FindNextActivity(WfActivity activity, string transitionName)
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        public bool HasNextActivity(WfActivity activity)
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        public bool HasNextActivity(WfActivity activity, string transitionName)
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        public WfActivity ReadActivity(long wfadId)
        {
            return BrokerManager.GetBroker<WfActivity>().Get(wfadId);
        }

        [OperationContract]
        public WfActivityDefinition ReadActivityDefinition(long wfadId)
        {
            return BrokerManager.GetBroker<WfActivityDefinition>().Get(wfadId);
        }

        [OperationContract]
        public WfWorkflowDefinition ReadWorkflowDefinition(string definitionName)
        {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfWorkflowDefinition.Cols.NAME, definitionName);
            return BrokerManager.GetBroker<WfWorkflowDefinition>().GetByCriteria(filterCriteria);
        }

        [OperationContract]
        public WfWorkflowDefinition ReadWorkflowDefinition(long wfwdId)
        {
            return BrokerManager.GetBroker<WfWorkflowDefinition>().Get(wfwdId);
        }

        [OperationContract]
        public WfWorkflow ReadWorkflowInstanceById(long wfwId)
        {
            return BrokerManager.GetBroker<WfWorkflow>().Get(wfwId);
        }

        [OperationContract]
        public WfWorkflow ReadWorkflowInstanceByItemId(long itemId)
        {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfWorkflow.Cols.ITEM_ID, itemId);
            return BrokerManager.GetBroker<WfWorkflow>().GetByCriteria(filterCriteria);
        }

        [OperationContract]
        public void RemoveTransition(WfTransitionDefinition transition)
        {
            BrokerManager.GetBroker<WfTransitionDefinition>().Delete(transition);
        }

        [OperationContract]
        public void UpdateActivity(WfActivity wfActivity)
        {
            BrokerManager.GetBroker<WfActivity>().Save(wfActivity);
        }

        [OperationContract]
        public void UpdateActivityDefinition(WfActivityDefinition wfActivityDefinition)
        {
            BrokerManager.GetBroker<WfActivityDefinition>().Save(wfActivityDefinition);
        }

        [OperationContract]
        public void UpdateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition)
        {
            BrokerManager.GetBroker<WfWorkflowDefinition>().Save(wfWorkflowDefinition);
        }

        [OperationContract]
        public void UpdateWorkflowInstance(WfWorkflow workflow)
        {
            BrokerManager.GetBroker<WfWorkflow>().Save(workflow);
        }
    }
}
