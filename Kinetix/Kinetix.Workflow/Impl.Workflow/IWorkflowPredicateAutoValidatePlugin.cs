
using Kinetix.Rules;
using Kinetix.Workflow.model;
using System.Collections.Generic;

namespace Kinetix.Workflow
{
    public interface IWorkflowPredicateAutoValidatePlugin
    {

        /// <summary>
        /// Predicate to determine if the current activityDefinition can be autovalidated for the provided object.
        /// Version typically used for massive recalculation
        /// </summary>
        /// <param name="activityDefinition"></param>
        /// <param name="obj"></param>
        /// <param name="ruleConstants"></param>
        /// <param name="dicRules"></param>
        /// <param name="dicConditions"></param>
        /// <param name="dicSelectors"></param>
        /// <param name="dicFilters"></param>
        /// <returns></returns>
        bool CanAutoValidateActivity(WfActivityDefinition activityDefinition, RuleContext ruleContext,
            IDictionary<int, List<RuleDefinition>> dicRules, IDictionary<int, List<RuleConditionDefinition>> dicConditions,
            IDictionary<int, List<SelectorDefinition>> dicSelectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters);

        /// <summary>
        /// Predicate to determine if the current activityDefinition can be autovalidated for the provided object
        /// </summary>
        /// <param name="activityDefinition"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool CanAutoValidateActivity(WfActivityDefinition activityDefinition, object obj);

    }
}
