using System.Collections.Generic;
using System.Diagnostics;
using Kinetix.Broker;
using System;
using Kinetix.Data.SqlClient;

namespace Kinetix.Rules {
    public class SqlServerRuleStorePlugin : IRuleStorePlugin {


        private static string ITEMS_ID = "ITEMS_ID";

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

        public void AddCondition(RuleConditionDefinition ruleConditionDefinition) {
            Debug.Assert(ruleConditionDefinition.Id == null);
            ruleConditionDefinition.Id = (int) BrokerManager.GetBroker<RuleConditionDefinition>().Save(ruleConditionDefinition);
        }

        public void AddFilter(RuleFilterDefinition ruleFilterDefinition) {
            Debug.Assert(ruleFilterDefinition.Id == null);
            ruleFilterDefinition.Id = (int) BrokerManager.GetBroker<RuleFilterDefinition>().Save(ruleFilterDefinition);
        }

        public void AddRule(RuleDefinition ruleDefinition) {
            Debug.Assert(ruleDefinition.Id == null);
            ruleDefinition.Id = (int) BrokerManager.GetBroker<RuleDefinition>().Save(ruleDefinition);
        }

        public void AddSelector(SelectorDefinition selectorDefinition) {
            Debug.Assert(selectorDefinition.Id == null);
            selectorDefinition.Id = (int) BrokerManager.GetBroker<SelectorDefinition>().Save(selectorDefinition);
        }

        public IList<RuleConditionDefinition> FindConditionByRuleId(int ruleId) {
            IList<RuleConditionDefinition> ret;
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(RuleConditionDefinition.Cols.RUD_ID, ruleId);
            ret = new List<RuleConditionDefinition>(BrokerManager.GetBroker<RuleConditionDefinition>().GetAllByCriteria(filterCriteria));
            return ret;
        }

        public IList<RuleFilterDefinition> FindFiltersBySelectorId(int selectorId) {
            IList<RuleFilterDefinition> ret;
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(RuleFilterDefinition.Cols.SEL_ID, selectorId);
            ret = new List<RuleFilterDefinition>(BrokerManager.GetBroker<RuleFilterDefinition>().GetAllByCriteria(filterCriteria));
            return ret;
        }

        public IList<RuleDefinition> FindRulesByItemId(int itemId) {
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(RuleDefinition.Cols.ITEM_ID, itemId);
            IList<RuleDefinition> ret = new List<RuleDefinition>(BrokerManager.GetBroker<RuleDefinition>().GetAllByCriteria(filterCriteria));
            return ret;
        }

        public IList<SelectorDefinition> FindSelectorsByItemId(int itemId) {
            IList<SelectorDefinition> ret;
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(SelectorDefinition.Cols.ITEM_ID, itemId);
            ret = new List<SelectorDefinition>(BrokerManager.GetBroker<SelectorDefinition>().GetAllByCriteria(filterCriteria));
            return ret;
        }

        public void RemoveCondition(RuleConditionDefinition ruleConditionDefinition) {
            Debug.Assert(ruleConditionDefinition.Id != null);
            BrokerManager.GetBroker<RuleConditionDefinition>().Delete(ruleConditionDefinition.Id.Value);
        }

        public void RemoveFilter(RuleFilterDefinition ruleFilterDefinition) {
            Debug.Assert(ruleFilterDefinition.Id != null);
            BrokerManager.GetBroker<RuleFilterDefinition>().Delete(ruleFilterDefinition.Id.Value);
        }


        public void UpdateCondition(RuleConditionDefinition ruleConditionDefinition) {
            Debug.Assert(ruleConditionDefinition.Id != null);
            BrokerManager.GetBroker<RuleConditionDefinition>().Save(ruleConditionDefinition);
        }

        public void UpdateFilter(RuleFilterDefinition ruleFilterDefinition) {
            Debug.Assert(ruleFilterDefinition.Id != null);
            BrokerManager.GetBroker<RuleFilterDefinition>().Save(ruleFilterDefinition);
        }

        public void UpdateRule(RuleDefinition ruleDefinition) {
            Debug.Assert(ruleDefinition.Id != null);
            BrokerManager.GetBroker<RuleDefinition>().Save(ruleDefinition);
        }

        public void UpdateSelector(SelectorDefinition selectorDefinition) {
            Debug.Assert(selectorDefinition.Id != null);
            BrokerManager.GetBroker<SelectorDefinition>().Save(selectorDefinition);
        }


        public IList<RuleDefinition> FindRulesByCriteria(RuleCriteria criteria, IList<int> items)
        {
            Debug.Assert(criteria != null);
            Debug.Assert(criteria.ConditionCriteria1 != null);
            //--
            
            var cmd = GetSqlServerCommand("FindItemsByCriteria.sql");
            cmd.Parameters.AddWithValue(RuleCriteria.Cols.FIELD_1, criteria.ConditionCriteria1.Field);
            cmd.Parameters.AddWithValue(RuleCriteria.Cols.VALUE_1, criteria.ConditionCriteria1.Value);
            cmd.Parameters.AddInParameter(ITEMS_ID, items);

            if (criteria.ConditionCriteria2 != null)
            {
                cmd.Parameters.AddWithValue(RuleCriteria.Cols.FIELD_2, criteria.ConditionCriteria2.Field);
                cmd.Parameters.AddWithValue(RuleCriteria.Cols.VALUE_2, criteria.ConditionCriteria2.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue(RuleCriteria.Cols.FIELD_2, null);
                cmd.Parameters.AddWithValue(RuleCriteria.Cols.VALUE_2, null);
            }

            return new List<RuleDefinition>(cmd.ReadList<RuleDefinition>());
        }

        public void RemoveRules(IList<int> list)
        {
            Debug.Assert(list != null);
            //--
            var cmd = GetSqlServerCommand("DeleteRulesByIds.sql");
            cmd.Parameters.AddInParameter(ITEMS_ID, list);

            cmd.ExecuteNonQuery();
        }

        public void RemoveSelectors(IList<int> list)
        {
            Debug.Assert(list != null);
            //--
            var cmd = GetSqlServerCommand("DeleteSelectorsByIds.sql");
            cmd.Parameters.AddInParameter(ITEMS_ID, list);

            cmd.ExecuteNonQuery();
        }

        public void RemoveSelectorsFiltersByGroupId(string groupId)
        {
            var cmd = GetSqlServerCommand("DeleteSelectorsFiltersByGroupIds.sql");
            cmd.Parameters.AddWithValue(SelectorDefinition.Cols.GROUP_ID, groupId);

            cmd.ExecuteNonQuery();
        }
    }
}
