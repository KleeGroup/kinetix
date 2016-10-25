using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Rules
{
    public interface IRuleValidator
    {
        /// <summary>
        /// Validate a rule for an activity.
        /// </summary>
        /// <param name="rules">rules</param>
        /// <param name="ruleContext">ruleContext</param>
        /// <returns>true is the rule is valid, false otherwise.</returns>
        bool IsRuleValid(IList<RuleDefinition> rules, RuleContext ruleContext);


        /// <summary>
        /// Validate a rule for an activity using rules and conditions provided.
        /// </summary>
        /// <param name="rules">rules</param>
        /// <param name="dicConditions">conditions linked to the rules</param>
        /// <param name="ruleContext">ruleContext</param>
        /// <returns></returns>
        bool IsRuleValid(IList<RuleDefinition> rules, IDictionary<int, List<RuleConditionDefinition>> dicConditions, RuleContext ruleContext);
    }
}
