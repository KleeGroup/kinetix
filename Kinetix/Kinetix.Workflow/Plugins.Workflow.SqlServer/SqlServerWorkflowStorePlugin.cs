using System;
using System.Collections.Generic;
using System.ServiceModel;
using Kinetix.Broker;
using Kinetix.Data.SqlClient;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.Diagnostics;
using Kinetix.Workflow.Workflow;

namespace Kinetix.Workflow {
    public class SqlServerWorkflowStorePlugin : IWorkflowStorePlugin {

        private static string ACT_DEF_ID = "ACT_DEF_ID";

        [OperationContract]
        public void AddTransition(WfTransitionDefinition transition) {
            BrokerManager.GetBroker<WfTransitionDefinition>().Save(transition);
        }

        /// <summary>
        /// Retourne la commande SQL Server associée au script.
        /// </summary>
        /// <param name="script">Nom du script.</param>
        /// <param name="dataSource">Datasource.</param>
        /// <param name="disableCheckTransCtx">Indique si la vérification de la présence d'un contexte transactionnel doit être désactivée.</param>
        /// <returns>Commande.</returns>
        public SqlServerCommand GetSqlServerCommand(string script, string dataSource = "default", bool disableCheckTransCtx = false) {
            if (string.IsNullOrEmpty(script)) {
                throw new ArgumentNullException("script");
            }
            if (string.IsNullOrEmpty(dataSource)) {
                throw new ArgumentNullException("dataSource");
            }
            return new SqlServerCommand(dataSource, GetType().Assembly, string.Concat(GetType().Namespace + ".SQLResources.", script), false);
        }

        [OperationContract]
        public int CountDefaultTransitions(WfWorkflowDefinition wfWorkflowDefinition) {
            var cmd = GetSqlServerCommand("CountDefaultTransactions.sql");
            cmd.Parameters.AddWithValue(WfWorkflowDefinition.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId);
            return cmd.ReadScalar<int>();
        }

        [OperationContract]
        public void CreateActivity(WfActivity wfActivity) {
            int id = (int)BrokerManager.GetBroker<WfActivity>().Save(wfActivity);
            wfActivity.WfaId = id;
        }

        [OperationContract]
        public void CreateActivityDefinition(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityDefinition) {
            wfActivityDefinition.WfwdId = (int)wfWorkflowDefinition.WfwdId;
            int id = (int)BrokerManager.GetBroker<WfActivityDefinition>().Save(wfActivityDefinition);
            wfActivityDefinition.WfadId = id;
        }

        [OperationContract]
        public void CreateDecision(WfDecision wfDecision) {
            int id = (int)BrokerManager.GetBroker<WfDecision>().Save(wfDecision);
            wfDecision.Id = id;
        }

        [OperationContract]
        public void DeleteDecision(WfDecision wfDecision)
        {
            BrokerManager.GetBroker<WfDecision>().Delete(wfDecision);
        }

        [OperationContract]
        public void CreateWorkflowDefinition(WfWorkflowDefinition workflowDefinition) {
            int id = (int)BrokerManager.GetBroker<WfWorkflowDefinition>().Save(workflowDefinition);
            workflowDefinition.WfwdId = id;
        }

        [OperationContract]
        public void CreateWorkflowInstance(WfWorkflow workflow) {
            int id = (int)BrokerManager.GetBroker<WfWorkflow>().Save(workflow);
            workflow.WfwId = id;
        }

        [OperationContract]
        public void DeleteActivity(WfActivity wfActivity) {
            BrokerManager.GetBroker<WfWorkflow>().Delete(wfActivity);
        }

        [OperationContract]
        public void DeleteActivityDefinition(WfActivityDefinition wfActivityDefinition) {
            BrokerManager.GetBroker<WfActivityDefinition>().Delete(wfActivityDefinition);
        }

        [OperationContract]
        public WfActivityDefinition FindActivityDefinitionByPosition(WfWorkflowDefinition wfWorkflowDefinition, int position) {
            var cmd = GetSqlServerCommand("FindActivityDefinitionByPosition.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId);
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.LEVEL, position);

            WfActivityDefinition activity = CollectionBuilder<WfActivityDefinition>.ParseCommandForSingleObject(cmd, true);
            return activity;
        }

        [OperationContract]
        public IList<WfActivityDefinition> FindActivityMatchingRules() {
            throw new NotImplementedException();
        }

        [OperationContract]
        public IList<WfDecision> FindAllDecisionByActivity(WfActivity wfActivity) {
            IList<WfDecision> ret;
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfDecision.Cols.WFA_ID, wfActivity.WfaId);
            ret = new List<WfDecision>(BrokerManager.GetBroker<WfDecision>().GetAllByCriteria(filterCriteria));
            return ret;
        }

        [OperationContract]
        public IList<WfActivityDefinition> FindAllDefaultActivityDefinitions(WfWorkflowDefinition wfWorkflowDefinition) {
            var cmd = GetSqlServerCommand("FindAllDefaultActivityDefinitions.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId);
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.NAME, WfCodeTransition.Default.ToString());
            IList<WfActivityDefinition> activities = new List<WfActivityDefinition>(cmd.ReadList<WfActivityDefinition>());
            return activities;
        }

        [OperationContract]
        public WfActivityDefinition FindNextActivity(int wfadId) {
            return FindNextActivity(wfadId, WfCodeTransition.Default.ToString());
        }

        [OperationContract]
        public WfActivityDefinition FindNextActivity(int wfadId, string transitionName) {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfTransitionDefinition.Cols.WFAD_ID_FROM, wfadId);
            filterCriteria.Equals(WfTransitionDefinition.Cols.NAME, transitionName);
            WfTransitionDefinition transition = BrokerManager.GetBroker<WfTransitionDefinition>().GetByCriteria(filterCriteria);

            return BrokerManager.GetBroker<WfActivityDefinition>().Get(transition.WfadIdTo);
        }

        [OperationContract]
        public bool HasNextActivity(WfActivity activity) {
            return HasNextActivity(activity, WfCodeTransition.Default.ToString());
        }

        [OperationContract]
        public bool HasNextActivity(WfActivity activity, string transitionName) {
            var cmd = GetSqlServerCommand("HasNextTransition.sql");
            cmd.Parameters.AddWithValue(WfTransitionDefinition.Cols.WFAD_ID_FROM, activity.WfadId);
            cmd.Parameters.AddWithValue(WfTransitionDefinition.Cols.NAME, transitionName);

            bool hasNext = cmd.ReadScalar<int>() == 1;

            return hasNext;
        }

        [OperationContract]
        public WfActivity ReadActivity(int wfadId) {
            return BrokerManager.GetBroker<WfActivity>().Get(wfadId);
        }

        [OperationContract]
        public WfActivityDefinition ReadActivityDefinition(int wfadId) {
            return BrokerManager.GetBroker<WfActivityDefinition>().Get(wfadId);
        }

        [OperationContract]
        public WfWorkflowDefinition ReadWorkflowDefinition(string definitionName) {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfWorkflowDefinition.Cols.NAME, definitionName);
            return BrokerManager.GetBroker<WfWorkflowDefinition>().GetByCriteria(filterCriteria);
        }

        [OperationContract]
        public WfWorkflowDefinition ReadWorkflowDefinition(int wfwdId) {
            return BrokerManager.GetBroker<WfWorkflowDefinition>().Get(wfwdId);
        }

        [OperationContract]
        public WfWorkflow ReadWorkflowInstanceById(int wfwId) {
            return BrokerManager.GetBroker<WfWorkflow>().Get(wfwId);
        }

        [OperationContract]
        public WfWorkflow ReadWorkflowInstanceByItemId(int wfwdId, int itemId) {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfWorkflow.Cols.ITEM_ID, itemId);
            filterCriteria.Equals(WfWorkflow.Cols.WFWD_ID, wfwdId);
            return BrokerManager.GetBroker<WfWorkflow>().GetByCriteria(filterCriteria);
        }

        [OperationContract]
        public IList<WfDecision> ReadDecisionsByActivityId(int wfaId)
        {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfDecision.Cols.WFA_ID, wfaId);
            return new List<WfDecision> (BrokerManager.GetBroker<WfDecision>().GetAllByCriteria(filterCriteria));
        }


        [OperationContract]
        public void RemoveTransition(WfTransitionDefinition transition) {
            BrokerManager.GetBroker<WfTransitionDefinition>().Delete(transition);
        }

        [OperationContract]
        public void UpdateActivity(WfActivity wfActivity) {
            BrokerManager.GetBroker<WfActivity>().Save(wfActivity);
        }

        [OperationContract]
        public void UpdateActivityDefinition(WfActivityDefinition wfActivityDefinition) {
            BrokerManager.GetBroker<WfActivityDefinition>().Save(wfActivityDefinition);
        }

        [OperationContract]
        public void UpdateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition) {
            BrokerManager.GetBroker<WfWorkflowDefinition>().Save(wfWorkflowDefinition);
        }

        [OperationContract]
        public void UpdateWorkflowInstance(WfWorkflow workflow) {
            BrokerManager.GetBroker<WfWorkflow>().Save(workflow);
        }

        [OperationContract]
        public IList<WfActivity> FindActivitiesByDefinitionId(WfWorkflow wfWorkflow, IList<int> wfadId)
        {
            Debug.Assert(wfWorkflow != null);
            Debug.Assert(wfadId != null);
            //--
            var cmd = GetSqlServerCommand("FindActivitiesByDefinitionId.sql");
            cmd.Parameters.AddInParameter(ACT_DEF_ID, wfadId);
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFW_ID, wfWorkflow.WfwId);
            return new List<WfActivity>(cmd.ReadList<WfActivity>());
        }

        [OperationContract]
        public IList<WfActivity> FindActivitiesByWorkflowId(WfWorkflow wfWorkflow)
        {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfWorkflow.Cols.WFW_ID, wfWorkflow.WfwId);
            return new List<WfActivity>(BrokerManager.GetBroker<WfActivity>().GetAllByCriteria(filterCriteria));
        }

        public IList<WfDecision> FindDecisionsByWorkflowId(WfWorkflow wfWorkflow)
        {
            Debug.Assert(wfWorkflow != null);
            //--
            var cmd = GetSqlServerCommand("FindDecisionsByWorkflowId.sql");
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFW_ID, wfWorkflow.WfwId);
            return new List<WfDecision>(cmd.ReadList<WfDecision>());
        }

        [OperationContract]
        public void UpdateDecision(WfDecision wfDecision)
        {
            BrokerManager.GetBroker<WfDecision>().Save(wfDecision);
        }

        public WfActivity FindActivityByDefinitionWorkflow(WfWorkflow wfWorkflow, WfActivityDefinition wfActivityDefinition)
        {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfActivity.Cols.WFW_ID, wfWorkflow.WfwId.Value);
            filterCriteria.Equals(WfActivity.Cols.WFAD_ID, wfActivityDefinition.WfadId.Value);
            return BrokerManager.GetBroker<WfActivity>().GetByCriteria(filterCriteria);
        }

        public IList<WfWorkflow> FindActiveWorkflows(WfWorkflowDefinition wfWorkflowDefinition)
        {
            Debug.Assert(wfWorkflowDefinition != null);
            //--
            var cmd = GetSqlServerCommand("FindActiveWorkflows.sql");
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId);
            return new List<WfWorkflow>(cmd.ReadList<WfWorkflow>());
        }

        public void UpdateTransition(WfTransitionDefinition transition)
        {
            BrokerManager.GetBroker<WfTransitionDefinition>().Save(transition);
        }

        public WfTransitionDefinition FindTransition(WfTransitionCriteria wfTransitionCriteria)
        {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfTransitionDefinition.Cols.NAME, wfTransitionCriteria.TransitionName);
            if (wfTransitionCriteria.WfadIdFrom != null)
            {
                filterCriteria.Equals(WfTransitionDefinition.Cols.WFAD_ID_FROM, wfTransitionCriteria.WfadIdFrom);
            }

            if (wfTransitionCriteria.WfadIdTo != null)
            {
                filterCriteria.Equals(WfTransitionDefinition.Cols.WFAD_ID_TO, wfTransitionCriteria.WfadIdTo);
            }

            return BrokerManager.GetBroker<WfTransitionDefinition>().FindByCriteria(filterCriteria);
        }

        public void IncrementActivityDefinitionPositionsAfter(int wfwdId, int position)
        {
            var cmd = GetSqlServerCommand("IncrementActivityDefinitionPositionsAfter.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFWD_ID, wfwdId);
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.LEVEL, position);
            cmd.ExecuteNonQuery();
        }

        public void DeleteActivities(int wfadId)
        {
            var cmd = GetSqlServerCommand("DeleteActivitiesByDefinitionIds.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFAD_ID, wfadId);
            cmd.ExecuteNonQuery();
        }

        public void UnsetCurrentActivity(WfActivityDefinition wfActivityDefinition)
        {
            var cmd = GetSqlServerCommand("UnsetCurrentActivity.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFAD_ID, wfActivityDefinition.WfadId);
            cmd.ExecuteNonQuery();
        }
    }
}
