using Kinetix.Rules.Impl.Rules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
                            string[] expressions = expression.Split(',');
                            if (fieldToTest is IList)
                            {
                                IList<string> valueList = (IList<string>)fieldToTest;
                                result = (expressions.Intersect(valueList).Count() > 0);
                            }
                            else
                            {
                                string valStr = (string)fieldToTest;
                                result = expressions.Contains<string>(valStr);
                            }
                            break;
                        case "<":
                            decimal doubleExpressionInf = decimal.Parse(expression, RuleCulture.Culture);
                            decimal doubleFieldInf = (decimal) fieldToTest;
                            result = doubleFieldInf < doubleExpressionInf;
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
