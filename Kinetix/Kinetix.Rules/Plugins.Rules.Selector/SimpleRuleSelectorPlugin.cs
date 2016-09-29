using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinetix.Account;

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
                    }

                    if (!result)
                    {
                        selectorMatch = false;
                    }
                }

                if (selectorMatch)
                {
                    IAccountStore accountStore = _accountManager.GetStore();

                    //FIXME: AccountManager should be a singleton
                    /*ISet<string> accounts = accountStore.GetAccountIds(selectorDefinition.GroupId);

                    foreach (string accountId in accounts)
                    {
                        AccountUser account = accountStore.GetAccount(accountId);
                        collected.Add(account);
                    }*/
                }
            }

            return collected;
        }
    }
}
