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
        public List<AccountUser> SelectAccounts(long idActivityDefinition, IList<SelectorDefinition> selectors, RuleContext ruleContext)
        {
            //TODO
            throw new NotImplementedException();
        }
    }
}
