using System;
using System.Collections.Generic;
using Kinetix.Broker;
using Kinetix.Data.SqlClient;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.Diagnostics;
using Kinetix.Rules;

namespace Kinetix.Workflow {
    public class SqlServerWorkflowStorePlugin : IWorkflowStorePlugin {

        private static string ACT_DEF_ID = "ACT_DEF_ID";
        private static string LOCK = "LOCK";
        private static string LEVEL_START = "LEVEL_START";
        private static string LEVEL_END = "LEVEL_END";
        private static string SHIFT = "SHIFT";

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

        public int CountDefaultTransitions(WfWorkflowDefinition wfWorkflowDefinition) {
            var cmd = GetSqlServerCommand("CountDefaultTransactions.sql");
            cmd.Parameters.AddWithValue(WfWorkflowDefinition.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId);
            return cmd.ReadScalar<int>();
        }

        public void CreateActivity(WfActivity wfActivity) {
            int id = (int)BrokerManager.GetBroker<WfActivity>().Save(wfActivity);
            wfActivity.WfaId = id;
        }

        public void CreateActivityDefinition(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityDefinition) {
            wfActivityDefinition.WfwdId = (int)wfWorkflowDefinition.WfwdId;
            int id = (int)BrokerManager.GetBroker<WfActivityDefinition>().Save(wfActivityDefinition);
            wfActivityDefinition.WfadId = id;
        }

        public void CreateDecision(WfDecision wfDecision) {
            int id = (int)BrokerManager.GetBroker<WfDecision>().Save(wfDecision);
            wfDecision.Id = id;
        }

        public void DeleteDecision(WfDecision wfDecision)
        {
            BrokerManager.GetBroker<WfDecision>().Delete(wfDecision.Id.Value);
        }

        public void CreateWorkflowDefinition(WfWorkflowDefinition workflowDefinition) {
            int id = (int)BrokerManager.GetBroker<WfWorkflowDefinition>().Save(workflowDefinition);
            workflowDefinition.WfwdId = id;
        }

        public void CreateWorkflowInstance(WfWorkflow workflow) {
            int id = (int)BrokerManager.GetBroker<WfWorkflow>().Save(workflow);
            workflow.WfwId = id;
        }

        public void DeleteActivity(WfActivity wfActivity) {
            BrokerManager.GetBroker<WfWorkflow>().Delete(wfActivity.WfaId.Value);
        }

        public void DeleteActivityDefinition(WfActivityDefinition wfActivityDefinition) {
            BrokerManager.GetBroker<WfActivityDefinition>().Delete(wfActivityDefinition.WfadId.Value);
        }

        public void RenameActivityDefinition(WfActivityDefinition wfActivityDefinition) {
            var cmd = GetSqlServerCommand("RenameActivityDefinition.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFAD_ID, wfActivityDefinition.WfadId);
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.NAME, wfActivityDefinition.Name);
            cmd.ExecuteNonQuery();
        }

        public WfActivityDefinition FindActivityDefinitionByPosition(WfWorkflowDefinition wfWorkflowDefinition, int position) {
            var cmd = GetSqlServerCommand("FindActivityDefinitionByPosition.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId);
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.LEVEL, position);

            WfActivityDefinition activity = CollectionBuilder<WfActivityDefinition>.ParseCommandForSingleObject(cmd, true);
            return activity;
        }

        public IList<WfDecision> FindAllDecisionByActivity(WfActivity wfActivity) {
            IList<WfDecision> ret;
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfDecision.Cols.WFA_ID, wfActivity.WfaId);
            ret = new List<WfDecision>(BrokerManager.GetBroker<WfDecision>().GetAllByCriteria(filterCriteria));
            return ret;
        }

        public IList<WfActivityDefinition> FindAllDefaultActivityDefinitions(WfWorkflowDefinition wfWorkflowDefinition, int startingPos) {
            var cmd = GetSqlServerCommand("FindAllDefaultActivityDefinitions.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId);
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.NAME, WfCodeTransition.Default.ToString());
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.LEVEL, startingPos);

            IList<WfActivityDefinition> activities = new List<WfActivityDefinition>(cmd.ReadList<WfActivityDefinition>());
            return activities;
        }

        public WfActivityDefinition FindNextActivity(int wfadId) {
            return FindNextActivity(wfadId, WfCodeTransition.Default.ToString());
        }

        public WfActivityDefinition FindNextActivity(int wfadId, string transitionName) {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfTransitionDefinition.Cols.WFAD_ID_FROM, wfadId);
            filterCriteria.Equals(WfTransitionDefinition.Cols.NAME, transitionName);
            WfTransitionDefinition transition = BrokerManager.GetBroker<WfTransitionDefinition>().GetByCriteria(filterCriteria);

            return BrokerManager.GetBroker<WfActivityDefinition>().Get(transition.WfadIdTo);
        }

        public bool HasNextActivity(WfActivity activity) {
            return HasNextActivity(activity, WfCodeTransition.Default.ToString());
        }

        public bool HasNextActivity(WfActivity activity, string transitionName) {
            var cmd = GetSqlServerCommand("HasNextTransition.sql");
            cmd.Parameters.AddWithValue(WfTransitionDefinition.Cols.WFAD_ID_FROM, activity.WfadId);
            cmd.Parameters.AddWithValue(WfTransitionDefinition.Cols.NAME, transitionName);

            bool hasNext = cmd.ReadScalar<int>() == 1;

            return hasNext;
        }

        public WfActivity ReadActivity(int wfaId) {
            return BrokerManager.GetBroker<WfActivity>().Get(wfaId);
        }

        public WfActivityDefinition ReadActivityDefinition(int wfadId) {
            return BrokerManager.GetBroker<WfActivityDefinition>().Get(wfadId);
        }

        public WfWorkflowDefinition ReadWorkflowDefinition(string definitionName) {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfWorkflowDefinition.Cols.NAME, definitionName);
            return BrokerManager.GetBroker<WfWorkflowDefinition>().GetByCriteria(filterCriteria);
        }

        public WfWorkflowDefinition ReadWorkflowDefinition(int wfwdId) {
            return BrokerManager.GetBroker<WfWorkflowDefinition>().Get(wfwdId);
        }

        public WfWorkflow ReadWorkflowInstanceById(int wfwId) {
            return BrokerManager.GetBroker<WfWorkflow>().Get(wfwId);
        }

        public WfWorkflow ReadWorkflowInstanceForUpdateById(int wfwId)
        {
            var cmd = GetSqlServerCommand("ReadWorkflowForUpdate.sql");
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFW_ID, wfwId);
            return cmd.ReadItem<WfWorkflow>();
        }

        public IList<WfWorkflow> ReadWorkflowsInstanceForUpdateById(int wfwdId)
        {
            var cmd = GetSqlServerCommand("ReadWorkflowsForUpdate.sql");
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFWD_ID, wfwdId);
            return new List<WfWorkflow>(cmd.ReadList<WfWorkflow>());
        }

        public WfWorkflow ReadWorkflowInstanceByItemId(int wfwdId, int itemId) {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfWorkflow.Cols.ITEM_ID, itemId);
            filterCriteria.Equals(WfWorkflow.Cols.WFWD_ID, wfwdId);
            return BrokerManager.GetBroker<WfWorkflow>().GetByCriteria(filterCriteria);
        }

        public IList<WfDecision> ReadDecisionsByActivityId(int wfaId)
        {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfDecision.Cols.WFA_ID, wfaId);
            return new List<WfDecision> (BrokerManager.GetBroker<WfDecision>().GetAllByCriteria(filterCriteria));
        }


        public void RemoveTransition(WfTransitionDefinition transition) {
            BrokerManager.GetBroker<WfTransitionDefinition>().Delete(transition.Id);
        }

        public void UpdateActivity(WfActivity wfActivity) {
            BrokerManager.GetBroker<WfActivity>().Save(wfActivity);
        }

        public void UpdateActivityDefinition(WfActivityDefinition wfActivityDefinition) {
            BrokerManager.GetBroker<WfActivityDefinition>().Save(wfActivityDefinition);
        }

        public void UpdateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition) {
            BrokerManager.GetBroker<WfWorkflowDefinition>().Save(wfWorkflowDefinition);
        }

        public void UpdateWorkflowInstance(WfWorkflow workflow) {
            BrokerManager.GetBroker<WfWorkflow>().Save(workflow);
        }

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

        public IList<WfActivity> FindActivitiesByWorkflowId(WfWorkflow wfWorkflow)
        {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfWorkflow.Cols.WFW_ID, wfWorkflow.WfwId);
            return new List<WfActivity>(BrokerManager.GetBroker<WfActivity>().GetAllByCriteria(filterCriteria));
        }

        public IList<WfActivity> FindAllActivitiesByWorkflowDefinitionId(WfWorkflowDefinition wfWorkflowDefinition)
        {
            Debug.Assert(wfWorkflowDefinition != null);
            //--
            var cmd = GetSqlServerCommand("FindAllActivitiesByWorkflowDefinitionId.sql");
            cmd.Parameters.AddWithValue(WfWorkflowDefinition.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId.Value);
            return new List<WfActivity>(cmd.ReadList<WfActivity>());
        }

        public IList<WfDecision> FindAllDecisionsByWorkflowDefinitionId(WfWorkflowDefinition wfWorkflowDefinition)
        {
            Debug.Assert(wfWorkflowDefinition != null);
            //--
            var cmd = GetSqlServerCommand("FindAllDecisionsByWorkflowDefinitionId.sql");
            cmd.Parameters.AddWithValue(WfWorkflowDefinition.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId.Value);
            return new List<WfDecision>(cmd.ReadList<WfDecision>());
        }

        public IList<WfDecision> FindDecisionsByWorkflowId(WfWorkflow wfWorkflow)
        {
            Debug.Assert(wfWorkflow != null);
            //--
            var cmd = GetSqlServerCommand("FindDecisionsByWorkflowId.sql");
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFW_ID, wfWorkflow.WfwId);
            return new List<WfDecision>(cmd.ReadList<WfDecision>());
        }

        public void UpdateDecision(WfDecision wfDecision)
        {
            BrokerManager.GetBroker<WfDecision>().Save(wfDecision);
        }

        public WfActivity FindActivityByDefinitionWorkflow(WfWorkflow wfWorkflow, WfActivityDefinition wfActivityDefinition)
        {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(WfActivity.Cols.WFW_ID, wfWorkflow.WfwId.Value);
            filterCriteria.Equals(WfActivity.Cols.WFAD_ID, wfActivityDefinition.WfadId.Value);
            return BrokerManager.GetBroker<WfActivity>().FindByCriteria(filterCriteria);
        }

        public IList<WfWorkflow> FindActiveWorkflows(WfWorkflowDefinition wfWorkflowDefinition,bool isForUpdate)
        {
            Debug.Assert(wfWorkflowDefinition != null);
            //--
            var cmd = GetSqlServerCommand("FindActiveWorkflowsForUpdate.sql");
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFWD_ID, wfWorkflowDefinition.WfwdId);
            cmd.Parameters.AddWithValue(LOCK, isForUpdate);
            return new List<WfWorkflow>(cmd.ReadList<WfWorkflow>());
        }

        public IList<WfWorkflow> FindActiveWorkflowInstanceByItemId(int wfwdId, int itemId)
        {
            var cmd = GetSqlServerCommand("FindActiveWorkflowInstanceByItemId.sql");
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFWD_ID, wfwdId);
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.ITEM_ID, itemId);
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

        public void DecrementActivityDefinitionPositionsAfter(int wfwdId, int position)
        {
            var cmd = GetSqlServerCommand("DecrementActivityDefinitionPositionsAfter.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFWD_ID, wfwdId);
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.LEVEL, position);
            cmd.ExecuteNonQuery();
        }

        public void ShiftActivityDefinitionPositionsBetween(int wfwdId, int posStart, int posEnd, int shift)
        {
            var cmd = GetSqlServerCommand("ShiftActivityDefinitionPositionsBetween.sql");
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFWD_ID, wfwdId);
            cmd.Parameters.AddWithValue(LEVEL_START, posStart);
            cmd.Parameters.AddWithValue(LEVEL_END, posEnd);
            cmd.Parameters.AddWithValue(SHIFT, shift);
            cmd.ExecuteNonQuery();
        }

        public void DeleteWorkflow(int wfwId)
        {
            var cmd = GetSqlServerCommand("DeleteWorkflowByWorkflowId.sql");
            cmd.Parameters.AddWithValue(WfActivity.Cols.WFW_ID, wfwId);
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
            cmd.Parameters.AddWithValue(WfActivityDefinition.Cols.WFAD_ID, wfActivityDefinition.WfadId.Value);
            cmd.ExecuteNonQuery();
        }


        #region Direct Acces Rules
        public IList<RuleDefinition> FindAllRulesByWorkflowDefinitionId(int wfwdId)
        {
            var cmd = GetSqlServerCommand("FindAllRulesByWorkflowDefinitionId.sql");
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFWD_ID, wfwdId);
            return new List<RuleDefinition>(cmd.ReadList<RuleDefinition>());
        }

        public IList<RuleConditionDefinition> FindAllConditionsByWorkflowDefinitionId(int wfwdId)
        {
            var cmd = GetSqlServerCommand("FindAllConditionsByWorkflowDefinitionId.sql");
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFWD_ID, wfwdId);
            return new List<RuleConditionDefinition>(cmd.ReadList<RuleConditionDefinition>());
        }

        public IList<SelectorDefinition> FindAllSelectorsByWorkflowDefinitionId(int wfwdId)
        {
            var cmd = GetSqlServerCommand("FindAllSelectorsByWorkflowDefinitionId.sql");
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFWD_ID, wfwdId);
            return new List<SelectorDefinition>(cmd.ReadList<SelectorDefinition>());
        }

        public IList<RuleFilterDefinition> FindAllFiltersByWorkflowDefinitionId(int wfwdId)
        {
            var cmd = GetSqlServerCommand("FindAllFiltersByWorkflowDefinitionId.sql");
            cmd.Parameters.AddWithValue(WfWorkflow.Cols.WFWD_ID, wfwdId);
            return new List<RuleFilterDefinition>(cmd.ReadList<RuleFilterDefinition>());
        }

        public void UpdateWorkflowCurrentActivities(ICollection<WfWorkflowUpdate> worfklows)
        {
            var cmd = GetSqlServerCommand("UpdateWorkflowCurrentActivities.sql");
            cmd.Parameters.AddBeanCollectionProperties(worfklows);
            cmd.ExecuteNonQuery();
        }

        public void UpdateActivitiesIsAuto(ICollection<WfActivityUpdate> activities)
        {
            var cmd = GetSqlServerCommand("UpdateActivitiesIsAuto.sql");
            cmd.Parameters.AddBeanCollectionProperties(activities);
            cmd.ExecuteNonQuery();
        }

        public void CreateActiviesAndUpdateWorkflowCurrentActivities(ICollection<WfActivity> activities)
        {
            var cmd = GetSqlServerCommand("InsertActivityUpdateWorkflow.sql");
            cmd.Parameters.AddBeanCollectionProperties(activities);
            cmd.ExecuteNonQuery();
        }

        public void CreateActivies(ICollection<WfActivity> activities)
        {
            BrokerManager.GetBroker<WfActivity>().InsertAll(activities);
        }

        public void CreateActivityDecision(ICollection<WfActivityDecision> activities)
        {
            throw new NotImplementedException();
        }




        #endregion
    }
}
