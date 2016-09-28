using Kinetix.Account;
using Kinetix.Rules;
using System;
using System.Collections.Generic;


namespace Kinetix.Rules
{
    public sealed class RuleManager : IRuleManager
    {
        private readonly IRuleStorePlugin _ruleStorePlugin;
        private readonly IRuleValidatorPlugin _ruleValidatorPlugin;
        private readonly IRuleSelectorPlugin _ruleSelectorPlugin;
        private readonly IRuleConstantsStorePlugin _ruleConstantsStorePlugin;

        public RuleManager(IRuleStorePlugin ruleStorePlugin, IRuleSelectorPlugin ruleSelectorPlugin, IRuleValidatorPlugin ruleValidatorPlugin, IRuleConstantsStorePlugin ruleConstantsStorePlugin)
        {
            _ruleStorePlugin = ruleStorePlugin;
            _ruleSelectorPlugin = ruleSelectorPlugin;
            _ruleValidatorPlugin = ruleValidatorPlugin;
            _ruleConstantsStorePlugin = ruleConstantsStorePlugin;
        }

        public void AddCondition(RuleConditionDefinition ruleConditionDefinition)
        {
            _ruleStorePlugin.AddCondition(ruleConditionDefinition);
        }

        public void AddConstants(int key, RuleConstants ruleConstants)
        {
            _ruleConstantsStorePlugin.AddConstants(key, ruleConstants);
        }

        public void AddFilter(RuleFilterDefinition ruleFilterDefinition)
        {
            _ruleStorePlugin.AddFilter(ruleFilterDefinition);
        }

        public void AddRule(RuleDefinition ruleDefinition)
        {
            _ruleStorePlugin.AddRule(ruleDefinition);
        }

        public void AddSelector(SelectorDefinition selectorDefinition)
        {
            _ruleStorePlugin.AddSelector(selectorDefinition);
        }

        public IList<RuleConditionDefinition> GetConditionsForRuleId(int ruleId)
        {
            return _ruleStorePlugin.FindConditionByRuleId(ruleId);
        }

        public RuleConstants GetConstants(int key)
        {
            return _ruleConstantsStorePlugin.ReadConstants(key);
        }

        public IList<RuleFilterDefinition> GetFiltersForSelectorId(int selectorId)
        {
            return _ruleStorePlugin.FindFiltersBySelectorId(selectorId);
        }

        public IList<RuleDefinition> GetRulesForItemId(int itemId)
        {
            return _ruleStorePlugin.FindRulesByItemId(itemId);
        }

        public IList<SelectorDefinition> GetSelectorsForItemId(int itemId)
        {
            return _ruleStorePlugin.FindSelectorsByItemId(itemId);
        }

        public bool IsRuleValid(int idActivityDefinition, object item, RuleConstants constants)
        {
            IList<RuleDefinition> rules = _ruleStorePlugin.FindRulesByItemId(idActivityDefinition);
            RuleContext context = new RuleContext(item, constants);

            return _ruleValidatorPlugin.IsRuleValid(idActivityDefinition, rules, context);
        }

        public void RemoveCondition(RuleConditionDefinition ruleConditionDefinition)
        {
            _ruleStorePlugin.RemoveCondition(ruleConditionDefinition);
        }

        public void RemoveFilter(RuleFilterDefinition ruleFilterDefinition)
        {
            _ruleStorePlugin.RemoveFilter(ruleFilterDefinition);
        }

        public void RemoveRule(RuleDefinition ruleDefinition)
        {
            _ruleStorePlugin.RemoveRule(ruleDefinition);
        }

        public void RemoveSelector(SelectorDefinition selectorDefinition)
        {
            _ruleStorePlugin.RemoveSelector(selectorDefinition);
        }

        public IList<AccountUser> SelectAccounts(int idActivityDefinition, object item, RuleConstants constants)
        {
            IList<SelectorDefinition> selectors = _ruleStorePlugin.FindSelectorsByItemId(idActivityDefinition);
            RuleContext context = new RuleContext(item, constants);

            return _ruleSelectorPlugin.SelectAccounts(idActivityDefinition, selectors, context);
        }

        public void UpdateCondition(RuleConditionDefinition ruleConditionDefinition)
        {
            _ruleStorePlugin.UpdateCondition(ruleConditionDefinition);
        }

        public void UpdateFilter(RuleFilterDefinition ruleFilterDefinition)
        {
            _ruleStorePlugin.UpdateFilter(ruleFilterDefinition);
        }

        public void UpdateRule(RuleDefinition ruleDefinition)
        {
            _ruleStorePlugin.UpdateRule(ruleDefinition);
        }

        public void UpdateSelector(SelectorDefinition selectorDefinition)
        {
            _ruleStorePlugin.UpdateSelector(selectorDefinition);
        }

        public IList<int> FindItemsByCriteria(RuleCriteria criteria, IList<int> items)
        {
            return _ruleStorePlugin.FindItemsByCriteria(criteria, items);
        }
    }
}
