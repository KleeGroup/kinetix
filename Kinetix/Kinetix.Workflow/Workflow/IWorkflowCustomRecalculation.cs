using Kinetix.Rules;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.Collections.Generic;

namespace Kinetix.Workflow
{
    public interface IWorkflowCustomRecalculation
    {

        /// <summary>
        /// Custom recalculation interface. Use this method to add Business Rules in recalculation.
        /// </summary>
        /// <param name="activityDefinitions"></param>
        /// <param name="ruleConstants"></param>
        /// <param name="wf"></param>
        /// <param name="dicRules"></param>
        /// <param name="dicConditions"></param>
        /// <param name="dicSelectors"></param>
        /// <param name="dicFilters"></param>
        /// <param name="dicActivities"></param>
        /// <param name="dicDecision"></param>
        /// <param name="dicObjects"></param>
        /// <param name="output"></param>
        void CustomRecalculation(IList<WfActivityDefinition> activityDefinitions, RuleConstants ruleConstants, WfWorkflow wf, IDictionary<int, List<RuleDefinition>> dicRules, IDictionary<int, List<RuleConditionDefinition>> dicConditions, IDictionary<int, List<SelectorDefinition>> dicSelectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters, IDictionary<int, List<WfActivity>> dicActivities, IDictionary<int, List<WfDecision>> dicDecision, IDictionary<int, object> dicObjects, WfRecalculationOutput output);


    }
}
