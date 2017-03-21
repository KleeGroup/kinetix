using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
                IList<RuleConditionDefinition> conditions = _ruleStorePlugin.FindConditionByRuleId(ruleDefinition.Id.Value);
                bool ruleValid = checkRules(conditions, ruleContext);

                if (ruleValid)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsRuleValid(IList<RuleDefinition> rules, IDictionary<int, List<RuleConditionDefinition>> dicConditions, RuleContext ruleContext)
        {
            foreach (RuleDefinition ruleDefinition in rules)
            {
                List<RuleConditionDefinition> conditions;
                dicConditions.TryGetValue(ruleDefinition.Id.Value, out conditions);
                if (conditions == null)
                {
                    conditions = new List<RuleConditionDefinition>();
                }

                bool ruleValid = checkRules(conditions, ruleContext);

                if (ruleValid)
                {
                    return true;
                }
            }

            return false;
        }

        private bool checkRules(IList<RuleConditionDefinition> conditions, RuleContext ruleContext)
        {
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
                            if (fieldToTest is IList)
                            {
                                //Split is O(N), but in 2 pass
                                string[] expressions = expression.Split(',');
                                IList<string> valueList = (IList<string>)fieldToTest;
                                // Intersect is O(N)
                                result = expressions.Intersect(valueList).Any();
                            }
                            else
                            {
                                string valStr = (string)fieldToTest;

                                if (expression.Length == valStr.Length)
                                {
                                    result = expression.Equals(valStr);
                                }
                                else
                                {
                                    if (expression.IndexOf("," + valStr + ",") != -1)
                                    {
                                        result = true;
                                    }
                                    else
                                    {
                                        result = expression.StartsWith(valStr + ",") || expression.EndsWith("," + valStr);
                                    }
                                }
                            }
                            break;
                        case "<=":
                            decimal doubleExpressionInfEgal = decimal.Parse(expression, RuleCulture.Culture);
                            decimal doubleFieldInfEgal = (decimal)fieldToTest;
                            result = doubleFieldInfEgal <= doubleExpressionInfEgal;
                            break;
                        case "<":
                            decimal doubleExpressionInf = decimal.Parse(expression, RuleCulture.Culture);
                            decimal doubleFieldInf = (decimal) fieldToTest;
                            result = doubleFieldInf < doubleExpressionInf;
                            break;
                        case ">=":
                            decimal doubleExpressionSupEgal = decimal.Parse(expression, RuleCulture.Culture);
                            decimal doubleFieldSupEgal = (decimal)fieldToTest;
                            result = doubleFieldSupEgal >= doubleExpressionSupEgal;
                            break;
                        case ">":
                            decimal doubleExpressionSup = decimal.Parse(expression, RuleCulture.Culture);
                            decimal doubleFieldSup = (decimal) fieldToTest;
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

            return ruleValid;
        }
    }
}
