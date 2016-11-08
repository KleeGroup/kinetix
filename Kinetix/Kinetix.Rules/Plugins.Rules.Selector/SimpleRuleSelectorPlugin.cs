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

        private IList<SelectorDefinition> FindMatchingSelectors(IList<SelectorDefinition> selectors, RuleContext ruleContext)
        {
            IList<SelectorDefinition> collected = new List<SelectorDefinition>();

            foreach (SelectorDefinition selectorDefinition in selectors)
            {
                IList<RuleFilterDefinition> filters = _ruleStorePlugin.FindFiltersBySelectorId(selectorDefinition.Id.Value);

                bool selectorMatch = checkFilters(filters, ruleContext);
                if (selectorMatch)
                {
                    collected.Add(selectorDefinition);
                }
            }

            return collected;
        }


        private IList<SelectorDefinition> FindMatchingSelectors(IList<SelectorDefinition> selectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters, RuleContext ruleContext)
        {
            IList<SelectorDefinition> collected = new List<SelectorDefinition>();

            foreach (SelectorDefinition selectorDefinition in selectors)
            {
                IList<RuleFilterDefinition> filters = dicFilters[selectorDefinition.Id.Value];

                bool selectorMatch = checkFilters(filters, ruleContext);
                if (selectorMatch)
                {
                    collected.Add(selectorDefinition);
                }
            }

            return collected;
        }

        public IList<AccountUser> SelectAccounts(IList<SelectorDefinition> selectors, RuleContext ruleContext)
        {
            IList<AccountUser> collected = new List<AccountUser>();
            IList<SelectorDefinition> matchingSelectors = FindMatchingSelectors(selectors, ruleContext);

            IAccountStore accountStore = _accountManager.GetStore();

            foreach (SelectorDefinition selectorDefinition in matchingSelectors) { 
                ISet<string> accounts = accountStore.GetAccountIds(selectorDefinition.GroupId);
                foreach (string accountId in accounts)
                {
                    AccountUser account = accountStore.GetAccount(accountId);
                    collected.Add(account);
                }
            }

            return collected;
        }

        public IList<AccountUser> SelectAccounts(IList<SelectorDefinition> selectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters, RuleContext ruleContext)
        {
            IList<AccountUser> collected = new List<AccountUser>();
            IList<SelectorDefinition> matchingSelectors = FindMatchingSelectors(selectors, dicFilters, ruleContext);

            IAccountStore accountStore = _accountManager.GetStore();

            foreach (SelectorDefinition selectorDefinition in matchingSelectors)
            {
                ISet<string> accounts = accountStore.GetAccountIds(selectorDefinition.GroupId);
                foreach (string accountId in accounts)
                {
                    AccountUser account = accountStore.GetAccount(accountId);
                    collected.Add(account);
                }
            }

            return collected;
        }


        public IList<AccountGroup> SelectGroups(IList<SelectorDefinition> selectors, RuleContext ruleContext)
        {
            IList<AccountGroup> collected = new List<AccountGroup>();
            IList<SelectorDefinition> matchingSelectors = FindMatchingSelectors(selectors, ruleContext);

            IAccountStore accountStore = _accountManager.GetStore();

            foreach (SelectorDefinition selectorDefinition in matchingSelectors)
            {
                AccountGroup accountGroup = accountStore.GetGroup(selectorDefinition.GroupId);
                collected.Add(accountGroup);
            }

            return collected;
        }

        private bool checkFilters(IList<RuleFilterDefinition> filters, RuleContext ruleContext)
        {
            bool selectorMatch = true;

            foreach (RuleFilterDefinition ruleFilterDefinition in filters)
            {
                string field = ruleFilterDefinition.Field;
                string operat = ruleFilterDefinition.Operator;
                string expression = ruleFilterDefinition.Expression;

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
                            double doubleExpressionInf = Double.Parse(expression);
                            double doubleFieldInf = Double.Parse((string)fieldToTest);
                            result = doubleFieldInf < doubleExpressionInf;
                            break;
                        case ">":
                            double doubleExpressionSup = Double.Parse(expression);
                            double doubleFieldSup = Double.Parse((string)fieldToTest);
                            result = doubleFieldSup > doubleExpressionSup;
                            break;
                    }

                    if (!result)
                    {
                        selectorMatch = false;
                    }
                }
                else
                {
                    selectorMatch = false;
                }
            }

            return selectorMatch;
        }

    }
}
