using Kinetix.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Rules
{
    public interface IRuleSelector
    {

        /// <summary>
        /// Select accounts matching the selector for an activity.
        /// </summary>
        /// <param name="idActivityDefinition">Criteria.</param>
        /// <param name="selectors">Criteria.</param>
        /// <param name="ruleContext">Criteria.</param>
        /// <returns>a list of account.</returns>
        IList<AccountUser> SelectAccounts(long idActivityDefinition, IList<SelectorDefinition> selectors, RuleContext ruleContext);


    }
}
