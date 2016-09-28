using Kinetix.Account;
using System.Collections.Generic;

namespace Kinetix.Rules
{
    public interface IRuleManager
    {

        /// <summary>
        /// Select accounts matching the selector for an activity.
        /// </summary>
        /// <param name="idActivityDefinition">Activity definition id.</param>
        /// <param name="item">Business object.</param>
        /// <param name="constants">constants</param>
        /// <returns>a list of account.</returns>
        IList<AccountUser> SelectAccounts(int idActivityDefinition, object item, RuleConstants constants);
        
        /// <summary>
        /// Validate a rule for an activity.
        /// </summary>
        /// <param name="idActivityDefinition">Activity definition id.</param>
        /// <param name="item">Business object.</param>
        /// <param name="constants">constants</param>
        /// <returns>a list of account.</returns>
        bool IsRuleValid(int idActivityDefinition, object item, RuleConstants constants);


        /// <summary>
        /// Add a new rule.
        /// </summary>
        /// <param name="ruleDefinition">the rule to add.</param>
        void AddRule(RuleDefinition ruleDefinition);


        /// <summary>
        /// Get the rules for an itemId.
        /// </summary>
        /// <param name="itemId">itemId.</param>
        /// <returns>all the rules defined for the provided itemId.</returns>
        IList<RuleDefinition> GetRulesForItemId(int itemId);

        /// <summary>
        /// Remove a rule.
        /// </summary>
        /// <param name="ruleDefinition">rule to remove.</param>
        void RemoveRule(RuleDefinition ruleDefinition);

        /// <summary>
        /// Update a Rule.
        /// </summary>
        /// <param name="ruleDefinition">rule to remove.</param>
        void UpdateRule(RuleDefinition ruleDefinition);

        /// <summary>
        /// Add a condition.
        /// </summary>
        /// <param name="ruleConditionDefinition">condition to add.</param>
        void AddCondition(RuleConditionDefinition ruleConditionDefinition);

        /// <summary>
        /// Add a condition.
        /// </summary>
        /// <param name="ruleConditionDefinition">condition to remove.</param>
        void RemoveCondition(RuleConditionDefinition ruleConditionDefinition);


        /// <summary>
        /// Get all the conditions for a specified rule.
        /// </summary>
        /// <param name="ruleId">the rule Id.</param>
        /// <returns>all the conditions associated to the provided rule</returns>
        IList<RuleConditionDefinition> GetConditionsForRuleId(int ruleId);

        /// <summary>
        /// Update a rule.
        /// </summary>
        /// <param name="ruleConditionDefinition">the rule to update.</param>
        void UpdateCondition(RuleConditionDefinition ruleConditionDefinition);

        /// <summary>
        /// Add a new selector.
        /// </summary>
        /// <param name="selectorDefinition">The selector to add.</param>
        void AddSelector(SelectorDefinition selectorDefinition);

        /// <summary>
        /// Get the selectors for the specified item Id.
        /// </summary>
        /// <param name="itemId">itemId.</param>
        /// <returns>all the conditions associated to the provided rule</returns>
        IList<SelectorDefinition> GetSelectorsForItemId(int itemId);

        /// <summary>
        /// Remove a selector the selector to remove.
        /// </summary>
        /// <param name="itemId">itemId.</param>
        /// <returns>all the conditions associated to the provided rule</returns>
        void RemoveSelector(SelectorDefinition selectorDefinition);

        /// <summary>
        /// Update a selector .
        /// </summary>
        /// <param name="selectorDefinition">the selector to update.</param>
        /// <returns>all the conditions associated to the provided rule</returns>
        void UpdateSelector(SelectorDefinition selectorDefinition);

        /// <summary>
        /// Add a new filter.
        /// </summary>
        /// <param name="ruleFilterDefinition">the filter to add.</param>
        void AddFilter(RuleFilterDefinition ruleFilterDefinition);

        /// <summary>
        /// Remove a filter.
        /// </summary>
        /// <param name="ruleFilterDefinition">the filter to remove.</param>
        void RemoveFilter(RuleFilterDefinition ruleFilterDefinition);

        /// <summary>
        /// Get filters for the selectors id.
        /// </summary>
        /// <param name="selectorId">the selector id.</param>
        /// <returns>the all the filters associated to the provided selector</returns>
        IList<RuleFilterDefinition> GetFiltersForSelectorId(int selectorId);

        /// <summary>
        /// Update the provided Filter.
        /// </summary>
        /// <param name="ruleFilterDefinition">the filter to update.</param>
        void UpdateFilter(RuleFilterDefinition ruleFilterDefinition);

        /// <summary>
        /// Define the constants for this key.
        /// </summary>
        /// <param name="key">the key.</param>
        /// <param name="ruleConstants">constants to associate.</param>
        void AddConstants(int key, RuleConstants ruleConstants);

        /// <summary>
        /// Get the constants associated to a key.
        /// </summary>
        /// <param name="key">the key.</param>
        /// <returns>the constants defined for this key</returns>
        RuleConstants GetConstants(int key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        IList<int> FindItemsByCriteria(RuleCriteria criteria, IList<int> items);
    }
}
