using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinetix.Account;
using System.Collections;

namespace Kinetix.Rules
{
    public sealed class SimpleRuleSelectorPlugin : IRuleSelectorPlugin
    {
        private readonly IRuleStorePlugin _ruleStorePlugin;
        private readonly IAccountManager _accountManager;

        public SimpleRuleSelectorPlugin(IRuleStorePlugin ruleStorePlugin, IAccountManager accountManager)
        {
            this._ruleStorePlugin = ruleStorePlugin;
            this._accountManager = accountManager;
        }

        public IList<AccountUser> SelectAccounts(long idActivityDefinition, IList<SelectorDefinition> selectors, RuleContext ruleContext)
        {

            IList<AccountUser> collected = new List<AccountUser>();

            foreach (SelectorDefinition selectorDefinition in selectors)
            {
                IList<RuleFilterDefinition> filters = _ruleStorePlugin.FindFiltersBySelectorId((int)selectorDefinition.Id);

                bool selectorMatch = true;
                foreach (RuleFilterDefinition ruleFilterDefinition in filters)
                {
                    string field = ruleFilterDefinition.Field;
                    string operat = ruleFilterDefinition.Operator;
                    string expression = ruleFilterDefinition.Expression;

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
                                IList<string> valueList = (IList<string>)ruleContext[field];
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
                        selectorMatch = false;
                    }
                }

                if (selectorMatch)
                {
                    IAccountStore accountStore = _accountManager.GetStore();
                    ISet<string> accounts = accountStore.GetAccountIds(selectorDefinition.GroupId);

                    foreach (string accountId in accounts)
                    {
                        AccountUser account = accountStore.GetAccount(accountId);
                        collected.Add(account);
                    }
                }
            }

            return collected;
        }
    }
}
