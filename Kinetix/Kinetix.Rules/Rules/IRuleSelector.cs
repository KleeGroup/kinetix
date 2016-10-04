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
        /// Select accounts matching the selector provided from an activity.
        /// </summary>
        /// <param name="selectors">Selectors.</param>
        /// <param name="ruleContext">ruleContext.</param>
        /// <returns>a list of account.</returns>
        IList<AccountUser> SelectAccounts(IList<SelectorDefinition> selectors, RuleContext ruleContext);


        /// <summary>
        /// Select groups mathing the selectors provided from an activity
        /// </summary>
        /// <param name="selectors"></param>
        /// <param name="ruleContext"></param>
        /// <returns></returns>
        IList<AccountGroup> SelectGroups(IList<SelectorDefinition> selectors, RuleContext ruleContext);

    }
}
