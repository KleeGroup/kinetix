using System;
using System.Collections.Generic;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.ServiceModel;
using Kinetix.Broker;
using Kinetix.Data.SqlClient;

namespace Kinetix.Workflow
{
    public class SqlServerWorkflowStorePlugin : IWorkflowStorePlugin
    {
        [OperationContract]
        public void AddTransition(WfTransitionDefinition transition)
        {
            BrokerManager.GetBroker<WfTransitionDefinition>().Save(transition);
        }

        /// <summary>
        /// Retourne la commande SQL Server associée au script.
        /// </summary>
        /// <param name="script">Nom du script.</param>
        /// <param name="dataSource">Datasource.</param>
        /// <param name="disableCheckTransCtx">Indique si la vérification de la présence d'un contexte transactionnel doit être désactivée.</param>
        /// <returns>Commande.</returns>
        public SqlServerCommand GetSqlServerCommand(string script, string dataSource = "default", bool disableCheckTransCtx = false)
        {
            if (string.IsNullOrEmpty(script))
            {
                throw new ArgumentNullException("script");
            }
            if (string.IsNullOrEmpty(dataSource))
            {
                throw new ArgumentNullException("dataSource");
            }
            return new SqlServerCommand(dataSource, GetType().Assembly, string.Concat(GetType().Namespace + ".SQLResources.", script), false);
        }

        [OperationContract]
        public int CountDefaultTransitions(WfWorkflowDefinition wfWorkflowDefinition)
        {
            var cmd = GetSqlServerCommand("CountDefaultTransactions.sql");
            cmd.Parameters.AddWithValue(WfWorkflowDefinition.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId);
            return cmd.ReadScalar<int>();
        }

        [OperationContract]
        public void CreateActivity(WfActivity wfActivity)
        {
            int id = (int) BrokerManager.GetBroker<WfActivity>().Save(wfActivity);
            wfActivity.WfaId = id;
        }

        [OperationContract]
        public void CreateActivityDefinition(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityDefinition)
        {
            wfActivityDefinition.WfwdId = (int) wfWorkflowDefinition.WfwdId;
            int id = (int) BrokerManager.GetBroker<WfActivityDefinition>().Save(wfActivityDefinition);
            wfActivityDefinition.WfadId = id;
        }

        [OperationContract]
        public void CreateDecision(WfDecision wfDecision)
        {
            int id = (int) BrokerManager.GetBroker<WfDecision>().Save(wfDecision);
            wfDecision.Id = id;
        }

        [OperationContract]
        public void CreateWorkflowDefinition(WfWorkflowDefinition workflowDefinition)
        {
            int id = (int) BrokerManager.GetBroker<WfWorkflowDefinition>().Save(workflowDefinition);
            workflowDefinition.WfwdId = id;
        }

        [OperationContract]
        public void CreateWorkflowInstance(WfWorkflow workflow)
        {
            int id = (int) BrokerManager.GetBroker<WfWorkflow>().Save(workflow);
            workflow.WfwId = id;
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
            var cmd = GetSqlServerCommand("FindActivityDefinitionByPosition.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId);
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.LEVEL, position);
            WfActivityDefinition activity = cmd.ReadScalar<WfActivityDefinition>();
            return activity;
        }

        [OperationContract]
        public IList<WfActivityDefinition> FindActivityMatchingRules()
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        public IList<WfDecision> FindAllDecisionByActivity(WfActivity wfActivity)
        {
            IList<WfDecision> ret;
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfDecision.Cols.WFA_ID, wfActivity.WfaId);
            ret = new List<WfDecision>(BrokerManager.GetBroker<WfDecision>().GetAllByCriteria(filterCriteria));
            return ret;
        }

        [OperationContract]
        public IList<WfActivityDefinition> FindAllDefaultActivityDefinitions(WfWorkflowDefinition wfWorkflowDefinition)
        {
            var cmd = GetSqlServerCommand("FindAllDefaultActivityDefinitions.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId);
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.NAME, WfCodeTransition.Default.ToString());
            IList<WfActivityDefinition> activities = new List<WfActivityDefinition>(cmd.ReadList<WfActivityDefinition>());
            return activities;
        }

        [OperationContract]
        public WfActivityDefinition FindNextActivity(WfActivity activity)
        {
            return FindNextActivity(activity, WfCodeTransition.Default.ToString());
        }

        [OperationContract]
        public WfActivityDefinition FindNextActivity(WfActivity activity, string transitionName)
        {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfTransitionDefinition.Cols.WFAD_ID_FROM, activity.WfadId);
            filterCriteria.Equals(WfTransitionDefinition.Cols.NAME, transitionName);
            WfTransitionDefinition transition = BrokerManager.GetBroker<WfTransitionDefinition>().GetByCriteria(filterCriteria);

            return BrokerManager.GetBroker<WfActivityDefinition>().Get(transition.WfadIdTo);
        }

        [OperationContract]
        public bool HasNextActivity(WfActivity activity)
        {
            return HasNextActivity(activity, WfCodeTransition.Default.ToString());
        }

        [OperationContract]
        public bool HasNextActivity(WfActivity activity, string transitionName)
        {
            //TODO: remove this method and use FindNextActivity
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfTransitionDefinition.Cols.WFAD_ID_FROM, activity.WfadId);
            filterCriteria.Equals(WfTransitionDefinition.Cols.NAME, transitionName);
            WfTransitionDefinition transition = BrokerManager.GetBroker<WfTransitionDefinition>().GetByCriteria(filterCriteria);
            return transition != null;
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
