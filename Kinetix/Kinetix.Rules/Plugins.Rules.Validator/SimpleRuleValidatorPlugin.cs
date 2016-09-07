using Kinetix.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Rules
{
    public sealed class SimpleRuleValidatorPlugin : IRuleValidatorPlugin
    {
        private readonly IRuleStorePlugin _ruleStorePlugin;

        public SimpleRuleValidatorPlugin(IRuleStorePlugin ruleStorePlugin)
        {
            this._ruleStorePlugin = ruleStorePlugin;
        }

        public bool IsRuleValid(long idActivityDefinition, IList<RuleDefinition> rules, RuleContext ruleContext)
        {
            foreach (RuleDefinition ruleDefinition in rules)
            {
                IList<RuleConditionDefinition> conditions = _ruleStorePlugin.FindConditionByRuleId((long)ruleDefinition.Id);

                bool ruleValid = true;
                foreach (RuleConditionDefinition ruleConditionDefinition in conditions)
                {
                    string field = ruleConditionDefinition.Field;
                    string operat = ruleConditionDefinition.Operator;
                    string expression = ruleConditionDefinition.Expression;

                    bool result = false;
                    switch (operat)
                    {
                        case "=":
                            result = ruleContext[field].Equals(expression);
                            break;
                    }

                    if (!result)
                    {
                        ruleValid = false;
                    }
                }

                if (ruleValid)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
