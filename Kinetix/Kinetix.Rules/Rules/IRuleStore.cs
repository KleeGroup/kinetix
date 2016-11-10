using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Rules
{

    /// <summary>
    /// This interface defines the storage of workflow.
    /// </summary>
    /// 
    public interface IRuleStore
    {

        /// <summary>
        /// Add a rule.
        /// </summary>
        /// <param name="idActivityDefinition">idActivityDefinition.</param>
        /// <param name="ruleDefinition">ruleDefinition.</param>
        void AddRule(RuleDefinition ruleDefinition);

        /// <summary>
        /// Find rules.
        /// </summary>
        /// <param name="itemId">itemId.</param>
        /// <returns>a list of all the rules defined for the itemId</returns>
        IList<RuleDefinition> FindRulesByItemId(int itemId);

        /// <summary>
        /// Update a rule.
        /// </summary>
        /// <param name="ruleDefinition">ruleDefinition.</param>
        void UpdateRule(RuleDefinition ruleDefinition);

        /// <summary>
        /// Add a condition.
        /// </summary>
        /// <param name="ruleConditionDefinition">ruleConditionDefinition.</param>
        void AddCondition(RuleConditionDefinition ruleConditionDefinition);

        /// <summary>
        /// Remove a condition.
        /// </summary>
        /// <param name="ruleConditionDefinition">ruleConditionDefinition.</param>
        void RemoveCondition(RuleConditionDefinition ruleConditionDefinition);

        /// <summary>
        /// Update a condition.
        /// </summary>
        /// <param name="ruleConditionDefinition">ruleConditionDefinition.</param>
        void UpdateCondition(RuleConditionDefinition ruleConditionDefinition);

        /// <summary>
        /// Find all conditions for a specified rule Id.
        /// </summary>
        /// <param name="ruleId">ruleId.</param>
        /// <returns>a list of all the rules defined for the itemId</returns>
        IList<RuleConditionDefinition> FindConditionByRuleId(int ruleId);

        /// <summary>
        /// Add a selector.
        /// </summary>
        /// <param name="selectorDefinition">selectorDefinition.</param>
        void AddSelector(SelectorDefinition selectorDefinition);

        /// <summary>
        /// Find all selectors for an item Id.
        /// </summary>
        /// <param name="itemId">itemId.</param>
        /// <returns>a list of all the selectors defined for the itemId</returns>
        IList<SelectorDefinition> FindSelectorsByItemId(int itemId);

        /// <summary>
        /// Update a Selector.
        /// </summary>
        /// <param name="selectorDefinition">selectorDefinition.</param>
        void UpdateSelector(SelectorDefinition selectorDefinition);

        /// <summary>
        /// Add a filter.
        /// </summary>
        /// <param name="ruleFilterDefinition">ruleFilterDefinition.</param>
        void AddFilter(RuleFilterDefinition ruleFilterDefinition);

        /// <summary>
        /// Remove a filter.
        /// </summary>
        /// <param name="ruleFilterDefinition">ruleFilterDefinition.</param>
        void RemoveFilter(RuleFilterDefinition ruleFilterDefinition);

        /// <summary>
        /// Find the filters associated to a selector id.
        /// </summary>
        /// <param name="selectorId">selectorId.</param>
        /// <returns>a list of all the filters defined for the selectorId</returns>
        IList<RuleFilterDefinition> FindFiltersBySelectorId(int selectorId);

        /// <summary>
        /// Update a filter.
        /// </summary>
        /// <param name="ruleFilterDefinition">ruleFilterDefinition.</param>
        void UpdateFilter(RuleFilterDefinition ruleFilterDefinition);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        IList<RuleDefinition> FindRulesByCriteria(RuleCriteria criteria, IList<int> items);

        /// <summary>
        /// Removes all the specified rules
        /// </summary>
        /// <param name="list">list of ids</param>
        void RemoveRules(IList<int> list);

        /// <summary>
        /// Removes all the specified Selectors
        /// </summary>
        /// <param name="list">list of ids</param>
        void RemoveSelectors(IList<int> list);


        /// <summary>
        /// Removes all Selectors and linked filters associated to the provided groupId
        /// </summary>
        /// <param name="groupId">groupId</param>
        void RemoveSelectorsFiltersByGroupId(string groupId);
    }
}
