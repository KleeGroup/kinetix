using System.Collections.Generic;
using Kinetix.Rules;
using Kinetix.Workflow.model;
using Kinetix.Account;

namespace Kinetix.Workflow
{
    public class SelectorRuleWorkflowPredicateAutoValidatePlugin : IWorkflowPredicateAutoValidatePlugin
    {
        private readonly IRuleManager _ruleManager;

        public SelectorRuleWorkflowPredicateAutoValidatePlugin(IRuleManager ruleManager)
        {
            _ruleManager = ruleManager;
        }

        public bool CanAutoValidateActivity(WfActivityDefinition activityDefinition, object obj)
        {
            RuleConstants ruleConstants = _ruleManager.GetConstants(activityDefinition.WfwdId);
            RuleContext ruleContext = new RuleContext(obj, ruleConstants);

            bool ruleValid = _ruleManager.IsRuleValid(activityDefinition.WfadId.Value, ruleContext);

            if (ruleValid == false)
            {
                return true;
            }

            IList<AccountUser> accounts = _ruleManager.SelectAccounts(activityDefinition.WfadId.Value, ruleContext);

            bool atLeastOnePerson = accounts.Count > 0;

            // If no rule is defined for validation or no one can validate this activity, we can autovalidate it.
            return atLeastOnePerson == false;
        }

        public bool CanAutoValidateActivity(WfActivityDefinition activityDefinition, RuleContext ruleContext, IDictionary<int, List<RuleDefinition>> dicRules, IDictionary<int, List<RuleConditionDefinition>> dicConditions, IDictionary<int, List<SelectorDefinition>> dicSelectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters)
        {
            bool ruleValid = _ruleManager.IsRuleValid(activityDefinition.WfadId.Value, ruleContext, dicRules, dicConditions);

            if (ruleValid == false)
            {
                return true;
            }

            IList<AccountUser> accounts = _ruleManager.SelectAccounts(activityDefinition.WfadId.Value, ruleContext, dicSelectors, dicFilters);

            bool atLeastOnePerson = accounts.Count > 0;

            // If no rule is defined for validation or no one can validate this activity, we can autovalidate it.
            return atLeastOnePerson == false;
        }

    }
}
