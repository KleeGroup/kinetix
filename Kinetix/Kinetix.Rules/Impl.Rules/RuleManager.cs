using System.Collections.Generic;
using System.Linq;
using Kinetix.Account;

namespace Kinetix.Rules {
    public sealed class RuleManager : IRuleManager {
        private readonly IRuleStorePlugin _ruleStorePlugin;
        private readonly IRuleValidatorPlugin _ruleValidatorPlugin;
        private readonly IRuleSelectorPlugin _ruleSelectorPlugin;
        private readonly IRuleConstantsStorePlugin _ruleConstantsStorePlugin;

        public RuleManager(IRuleStorePlugin ruleStorePlugin, IRuleSelectorPlugin ruleSelectorPlugin, IRuleValidatorPlugin ruleValidatorPlugin, IRuleConstantsStorePlugin ruleConstantsStorePlugin) {
            _ruleStorePlugin = ruleStorePlugin;
            _ruleSelectorPlugin = ruleSelectorPlugin;
            _ruleValidatorPlugin = ruleValidatorPlugin;
            _ruleConstantsStorePlugin = ruleConstantsStorePlugin;
        }

        public void AddCondition(RuleConditionDefinition ruleConditionDefinition) {
            _ruleStorePlugin.AddCondition(ruleConditionDefinition);
        }

        public void AddConstants(int key, RuleConstants ruleConstants) {
            _ruleConstantsStorePlugin.AddConstants(key, ruleConstants);
        }

        public void AddFilter(RuleFilterDefinition ruleFilterDefinition) {
            _ruleStorePlugin.AddFilter(ruleFilterDefinition);
        }

        public void AddRule(RuleDefinition ruleDefinition) {
            _ruleStorePlugin.AddRule(ruleDefinition);
        }

        public void AddSelector(SelectorDefinition selectorDefinition) {
            _ruleStorePlugin.AddSelector(selectorDefinition);
        }

        public IList<RuleConditionDefinition> GetConditionsForRuleId(int ruleId) {
            return _ruleStorePlugin.FindConditionByRuleId(ruleId);
        }

        public RuleConstants GetConstants(int key) {
            return _ruleConstantsStorePlugin.ReadConstants(key);
        }

        public IList<RuleFilterDefinition> GetFiltersForSelectorId(int selectorId) {
            return _ruleStorePlugin.FindFiltersBySelectorId(selectorId);
        }

        public IList<RuleDefinition> GetRulesForItemId(int itemId) {
            return _ruleStorePlugin.FindRulesByItemId(itemId);
        }

        public IList<SelectorDefinition> GetSelectorsForItemId(int itemId) {
            return _ruleStorePlugin.FindSelectorsByItemId(itemId);
        }

        public bool IsRuleValid(int idActivityDefinition, RuleContext ruleContext) {
            IList<RuleDefinition> rules = _ruleStorePlugin.FindRulesByItemId(idActivityDefinition);

            return _ruleValidatorPlugin.IsRuleValid(rules, ruleContext);
        }

        public bool IsRuleValid(int idActivityDefinition, RuleContext ruleContext, IDictionary<int, List<RuleDefinition>> dicRules, IDictionary<int, List<RuleConditionDefinition>> dicConditions) {
            List<RuleDefinition> rules;
            dicRules.TryGetValue(idActivityDefinition, out rules);

            if (rules == null) {
                rules = new List<RuleDefinition>();
            }

            return _ruleValidatorPlugin.IsRuleValid(rules, dicConditions, ruleContext);
        }

        public void RemoveCondition(RuleConditionDefinition ruleConditionDefinition) {
            _ruleStorePlugin.RemoveCondition(ruleConditionDefinition);
        }

        public void RemoveFilter(RuleFilterDefinition ruleFilterDefinition) {
            _ruleStorePlugin.RemoveFilter(ruleFilterDefinition);
        }

        public void RemoveRule(RuleDefinition ruleDefinition) {
            RemoveRules(new List<RuleDefinition>() { ruleDefinition });
        }

        public void RemoveSelector(SelectorDefinition selectorDefinition) {
            RemoveSelectors(new List<SelectorDefinition>() { selectorDefinition });
        }

        public IList<AccountUser> SelectAccounts(int idActivityDefinition, RuleContext context) {
            IList<SelectorDefinition> selectors = _ruleStorePlugin.FindSelectorsByItemId(idActivityDefinition);

            return _ruleSelectorPlugin.SelectAccounts(selectors, context);
        }

        public IList<AccountUser> SelectAccounts(int idActivityDefinition, RuleContext context, IDictionary<int, List<SelectorDefinition>> dicSelectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters) {
            List<SelectorDefinition> selectors;
            dicSelectors.TryGetValue(idActivityDefinition, out selectors);

            if (selectors == null) {
                selectors = new List<SelectorDefinition>();
            }

            return _ruleSelectorPlugin.SelectAccounts(selectors, dicFilters, context);
        }

        public IList<AccountGroup> SelectGroups(int idActivityDefinition, RuleContext context) {
            IList<SelectorDefinition> selectors = _ruleStorePlugin.FindSelectorsByItemId(idActivityDefinition);

            return _ruleSelectorPlugin.SelectGroups(selectors, context);
        }

        public IList<AccountGroup> SelectGroups(int idActivityDefinition, RuleContext context, IDictionary<int, List<SelectorDefinition>> dicSelectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters) {
            List<SelectorDefinition> selectors;
            dicSelectors.TryGetValue(idActivityDefinition, out selectors);

            if (selectors == null) {
                selectors = new List<SelectorDefinition>();
            }

            return _ruleSelectorPlugin.SelectGroups(selectors, dicFilters, context);
        }

        public void UpdateCondition(RuleConditionDefinition ruleConditionDefinition) {
            _ruleStorePlugin.UpdateCondition(ruleConditionDefinition);
        }

        public void UpdateFilter(RuleFilterDefinition ruleFilterDefinition) {
            _ruleStorePlugin.UpdateFilter(ruleFilterDefinition);
        }

        public void UpdateRule(RuleDefinition ruleDefinition) {
            _ruleStorePlugin.UpdateRule(ruleDefinition);
        }

        public void UpdateSelector(SelectorDefinition selectorDefinition) {
            _ruleStorePlugin.UpdateSelector(selectorDefinition);
        }

        public IList<int> FindItemsByCriteria(RuleCriteria criteria, IList<int> items) {
            IList<RuleDefinition> rules = _ruleStorePlugin.FindRulesByCriteria(criteria, items);

            return rules.Select(r => r.ItemId).Cast<int>().Distinct().ToList();
        }

        public void RemoveRules(IList<RuleDefinition> ruleDefinitions) {
            IList<int> ids = ruleDefinitions.Where(r => r.Id != null).Select(r => r.Id).Cast<int>().ToList();
            _ruleStorePlugin.RemoveRules(ids);
        }

        public void RemoveSelectors(IList<SelectorDefinition> selectorDefinitions) {
            IList<int> ids = selectorDefinitions.Where(s => s.Id != null).Select(s => s.Id).Cast<int>().ToList();
            _ruleStorePlugin.RemoveSelectors(ids);
        }


        public void RemoveSelectorsFiltersByGroupId(string groupId) {
            _ruleStorePlugin.RemoveSelectorsFiltersByGroupId(groupId);
        }


    }
}
