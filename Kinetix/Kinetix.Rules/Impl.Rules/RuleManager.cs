using Kinetix.Account;
using System;
using System.Collections.Generic;


namespace Kinetix.Rules
{
    public sealed class RuleManager : IRuleManager
    {
        private readonly IRuleStore _ruleStorePlugin;
        private readonly IRuleValidator _ruleValidatorPlugin;
        private readonly IRuleSelector _ruleSelectorPlugin;
        private readonly IRuleConstantsStore _ruleConstantsStorePlugin;

        public RuleManager(IRuleStore ruleStorePlugin, IRuleSelector ruleSelectorPlugin, IRuleValidator ruleValidatorPlugin, IRuleConstantsStore ruleConstantsStorePlugin)
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

        public void AddConstants(long key, RuleConstants ruleConstants)
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

        public IList<RuleConditionDefinition> GetConditionsForRuleId(long ruleId)
        {
            return _ruleStorePlugin.FindConditionByRuleId(ruleId);
        }

        public RuleConstants GetConstants(long key)
        {
            return _ruleConstantsStorePlugin.ReadConstants(key);
        }

        public IList<RuleFilterDefinition> GetFiltersForSelectorId(long selectorId)
        {
            return _ruleStorePlugin.FindFiltersBySelectorId(selectorId);
        }

        public IList<RuleDefinition> GetRulesForItemId(long itemId)
        {
            return _ruleStorePlugin.FindRulesByItemId(itemId);
        }

        public IList<SelectorDefinition> GetSelectorsForItemId(long itemId)
        {
            return _ruleStorePlugin.FindSelectorsByItemId(itemId);
        }

        public bool IsRuleValid(long idActivityDefinition, object item, RuleConstants constants)
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

        public IList<AccountUser> SelectAccounts(long idActivityDefinition, object item, RuleConstants constants)
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
    }
}
