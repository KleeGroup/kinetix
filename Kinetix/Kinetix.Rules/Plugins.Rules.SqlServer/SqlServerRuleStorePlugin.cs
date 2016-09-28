using System.Collections.Generic;
using System.Diagnostics;
using Kinetix.Broker;
using System;

namespace Kinetix.Rules {
    public class SqlServerRuleStorePlugin : IRuleStorePlugin {
        public void AddCondition(RuleConditionDefinition ruleConditionDefinition) {
            Debug.Assert(ruleConditionDefinition.Id == null);
            BrokerManager.GetBroker<RuleConditionDefinition>().Save(ruleConditionDefinition);
        }

        public void AddFilter(RuleFilterDefinition ruleFilterDefinition) {
            Debug.Assert(ruleFilterDefinition.Id == null);
            BrokerManager.GetBroker<RuleFilterDefinition>().Save(ruleFilterDefinition);
        }

        public void AddRule(RuleDefinition ruleDefinition) {
            Debug.Assert(ruleDefinition.Id == null);
            ruleDefinition.Id = (int)BrokerManager.GetBroker<RuleDefinition>().Save(ruleDefinition);
        }

        public void AddSelector(SelectorDefinition selectorDefinition) {
            Debug.Assert(selectorDefinition.Id == null);
            BrokerManager.GetBroker<SelectorDefinition>().Save(selectorDefinition);
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
            IList<RuleDefinition> ret;
            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Equals(RuleDefinition.Cols.ITEM_ID, itemId);
            ret = new List<RuleDefinition>(BrokerManager.GetBroker<RuleDefinition>().GetAllByCriteria(filterCriteria));
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
            BrokerManager.GetBroker<RuleConditionDefinition>().Delete(ruleConditionDefinition);
        }

        public void RemoveFilter(RuleFilterDefinition ruleFilterDefinition) {
            Debug.Assert(ruleFilterDefinition.Id != null);
            BrokerManager.GetBroker<RuleFilterDefinition>().Delete(ruleFilterDefinition);
        }

        public void RemoveRule(RuleDefinition ruleDefinition) {
            Debug.Assert(ruleDefinition.Id != null);
            BrokerManager.GetBroker<RuleDefinition>().Delete(ruleDefinition);
        }

        public void RemoveSelector(SelectorDefinition selectorDefinition) {
            Debug.Assert(selectorDefinition.Id != null);
            BrokerManager.GetBroker<SelectorDefinition>().Delete(selectorDefinition);
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


        public IList<int> FindItemsByCriteria(RuleCriteria criteria, IList<int> items)
        {
            throw new NotImplementedException();
        }
    }
}
