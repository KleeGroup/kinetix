using Kinetix.Rules;
using System;
using System.Collections;
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

        public bool IsRuleValid(IList<RuleDefinition> rules, RuleContext ruleContext)
        {
            foreach (RuleDefinition ruleDefinition in rules)
            {
                IList<RuleConditionDefinition> conditions = _ruleStorePlugin.FindConditionByRuleId((int)ruleDefinition.Id);

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
                        case "IN":
                            string[] expressions = expression.Split(',');
                            if (ruleContext[field] is IList)
                            {
                                IList<string> valueList = (IList<string>) ruleContext[field];
                                result = (expressions.Intersect(valueList).Count() > 0);
                            }
                            else
                            {
                                string valStr = (string)ruleContext[field];
                                result = expressions.Contains<string>(valStr);
                            }
                            break;
                        case "<":
                            double doubleExpressionInf = Double.Parse(expression);
                            double doubleFieldInf = Double.Parse((string)ruleContext[field]);
                            result = doubleExpressionInf < doubleFieldInf;
                            break;
                        case ">":
                            double doubleExpressionSup = Double.Parse(expression);
                            double doubleFieldSup = Double.Parse((string)ruleContext[field]);
                            result = doubleExpressionSup > doubleFieldSup;
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
