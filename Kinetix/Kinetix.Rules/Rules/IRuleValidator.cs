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
        /// <param name="idActivityDefinition">idActivityDefinition.</param>
        /// <param name="rules">rules</param>
        /// <param name="ruleContext">ruleContext</param>
        /// <param name="idActivityDefinition">idActivityDefinition.</param>
        /// <returns>true is the rule is valid, false otherwise.</returns>
        bool IsRuleValid(long idActivityDefinition, IList<RuleDefinition> rules, RuleContext ruleContext);

    }
}
