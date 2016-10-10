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
                    object fieldToTest;
                    ruleContext.TryGetValue(field, out fieldToTest);
                    if (fieldToTest != null)
                    {
                        switch (operat)
                        {
                            case "=":
                                result = fieldToTest.Equals(expression);
                                break;
                            case "IN":
                                string[] expressions = expression.Split(',');
                                if (fieldToTest is IList)
                                {
                                    IList<string> valueList = (IList<string>) fieldToTest;
                                    result = (expressions.Intersect(valueList).Count() > 0);
                                }
                                else
                                {
                                    string valStr = (string) fieldToTest;
                                    result = expressions.Contains<string>(valStr);
                                }
                                break;
                            case "<":
                                double doubleExpressionInf = Double.Parse(expression);
                                double doubleFieldInf = Double.Parse((string) fieldToTest);
                                result = doubleFieldInf < doubleExpressionInf;
                                break;
                            case ">":
                                double doubleExpressionSup = Double.Parse(expression);
                                double doubleFieldSup = Double.Parse((string) fieldToTest);
                                result = doubleFieldSup > doubleExpressionSup;
                                break;
                        }

                        if (!result)
                        {
                            ruleValid = false;
                        }
                    }
                    else
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
