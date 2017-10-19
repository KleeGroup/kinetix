using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Account;

namespace Kinetix.Rules {
    public sealed class SimpleRuleSelectorPlugin : IRuleSelectorPlugin {
        private readonly IRuleStorePlugin _ruleStorePlugin;
        private readonly IAccountManager _accountManager;

        public SimpleRuleSelectorPlugin(IRuleStorePlugin ruleStorePlugin, IAccountManager accountManager) {
            this._ruleStorePlugin = ruleStorePlugin;
            this._accountManager = accountManager;
        }

        private IList<SelectorDefinition> FindMatchingSelectors(IList<SelectorDefinition> selectors, RuleContext ruleContext) {
            IList<SelectorDefinition> collected = new List<SelectorDefinition>();

            foreach (SelectorDefinition selectorDefinition in selectors) {
                IList<RuleFilterDefinition> filters = _ruleStorePlugin.FindFiltersBySelectorId(selectorDefinition.Id.Value);

                bool selectorMatch = checkFilters(filters, ruleContext);
                if (selectorMatch) {
                    collected.Add(selectorDefinition);
                }
            }

            return collected;
        }


        private IList<SelectorDefinition> FindMatchingSelectors(IList<SelectorDefinition> selectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters, RuleContext ruleContext) {
            IList<SelectorDefinition> collected = new List<SelectorDefinition>();

            foreach (SelectorDefinition selectorDefinition in selectors) {

                List<RuleFilterDefinition> filters;
                dicFilters.TryGetValue(selectorDefinition.Id.Value, out filters);
                if (filters == null) {
                    filters = new List<RuleFilterDefinition>();
                }

                bool selectorMatch = checkFilters(filters, ruleContext);
                if (selectorMatch) {
                    collected.Add(selectorDefinition);
                }
            }

            return collected;
        }

        public IList<AccountUser> SelectAccounts(IList<SelectorDefinition> selectors, RuleContext ruleContext) {
            IList<AccountUser> collected = new List<AccountUser>();
            IList<SelectorDefinition> matchingSelectors = FindMatchingSelectors(selectors, ruleContext);

            IAccountStore accountStore = _accountManager.GetStore();

            foreach (SelectorDefinition selectorDefinition in matchingSelectors) {
                ISet<string> accounts = accountStore.GetAccountIds(selectorDefinition.GroupId);
                foreach (string accountId in accounts) {
                    AccountUser account = accountStore.GetAccount(accountId);
                    collected.Add(account);
                }
            }

            return collected;
        }

        public IList<AccountUser> SelectAccounts(IList<SelectorDefinition> selectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters, RuleContext ruleContext) {
            IList<AccountUser> collected = new List<AccountUser>();
            IList<SelectorDefinition> matchingSelectors = FindMatchingSelectors(selectors, dicFilters, ruleContext);

            IAccountStore accountStore = _accountManager.GetStore();

            foreach (SelectorDefinition selectorDefinition in matchingSelectors) {
                ISet<string> accounts = accountStore.GetAccountIds(selectorDefinition.GroupId);
                foreach (string accountId in accounts) {
                    AccountUser account = accountStore.GetAccount(accountId);
                    collected.Add(account);
                }
            }

            return collected;
        }


        public IList<AccountGroup> SelectGroups(IList<SelectorDefinition> selectors, RuleContext ruleContext) {
            IList<AccountGroup> collected = new List<AccountGroup>();
            IList<SelectorDefinition> matchingSelectors = FindMatchingSelectors(selectors, ruleContext);

            IAccountStore accountStore = _accountManager.GetStore();

            foreach (SelectorDefinition selectorDefinition in matchingSelectors) {
                AccountGroup accountGroup = accountStore.GetGroup(selectorDefinition.GroupId);
                collected.Add(accountGroup);
            }

            return collected;
        }

        public IList<AccountGroup> SelectGroups(IList<SelectorDefinition> selectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters, RuleContext ruleContext) {
            IList<AccountGroup> collected = new List<AccountGroup>();
            IList<SelectorDefinition> matchingSelectors = FindMatchingSelectors(selectors, dicFilters, ruleContext);

            IAccountStore accountStore = _accountManager.GetStore();

            foreach (SelectorDefinition selectorDefinition in matchingSelectors) {
                collected.Add(accountStore.GetGroup(selectorDefinition.GroupId));
            }

            return collected;
        }

        private bool checkFilters(IList<RuleFilterDefinition> filters, RuleContext ruleContext) {
            bool selectorMatch = true;

            foreach (RuleFilterDefinition ruleFilterDefinition in filters) {
                string field = ruleFilterDefinition.Field;
                string operat = ruleFilterDefinition.Operator;
                string expression = ruleFilterDefinition.Expression;

                bool result = false;
                object fieldToTest;
                ruleContext.TryGetValue(field, out fieldToTest);
                if (fieldToTest != null) {
                    switch (operat) {
                        case "=":
                            result = fieldToTest.Equals(expression);
                            break;
                        case "IN":
                            if (fieldToTest is IList) {
                                //Split is O(N), but in 2 pass
                                string[] expressions = expression.Split(',');
                                IList<string> valueList = (IList<string>)fieldToTest;
                                // Intersect is O(N)
                                result = expressions.Intersect(valueList).Any();
                            } else {
                                string valStr = (string)fieldToTest;

                                if (expression.Length == valStr.Length) {
                                    result = expression.Equals(valStr);
                                } else {
                                    if (expression.IndexOf("," + valStr + ",") != -1) {
                                        result = true;
                                    } else {
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
                            decimal doubleFieldInf = (decimal)fieldToTest;
                            result = doubleFieldInf < doubleExpressionInf;
                            break;
                        case ">=":
                            decimal doubleExpressionSupEgal = decimal.Parse(expression, RuleCulture.Culture);
                            decimal doubleFieldSupEgal = (decimal)fieldToTest;
                            result = doubleFieldSupEgal >= doubleExpressionSupEgal;
                            break;
                        case ">":
                            decimal doubleExpressionSup = decimal.Parse(expression, RuleCulture.Culture);
                            decimal doubleFieldSup = (decimal)fieldToTest;
                            result = doubleFieldSup > doubleExpressionSup;
                            break;
                    }

                    if (!result) {
                        selectorMatch = false;
                    }
                } else {
                    selectorMatch = false;
                }
            }

            return selectorMatch;
        }

    }
}
