using System.Collections.Generic;
using Kinetix.Account;

namespace Kinetix.Rules {
    public interface IRuleSelector {

        /// <summary>
        /// Select accounts matching the selector provided from an activity.
        /// </summary>
        /// <param name="selectors">Selectors.</param>
        /// <param name="ruleContext">ruleContext.</param>
        /// <returns>a list of account.</returns>
        IList<AccountUser> SelectAccounts(IList<SelectorDefinition> selectors, RuleContext ruleContext);

        /// <summary>
        /// Select accounts for an activity using selectors and filters provided.
        /// </summary>
        /// <param name="selectors">selectors</param>
        /// <param name="dicFilters">filters linked to the selectors</param>
        /// <param name="ruleContext">ruleContext</param>
        /// <returns></returns>
        IList<AccountUser> SelectAccounts(IList<SelectorDefinition> selectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters, RuleContext ruleContext);

        /// <summary>
        /// Select groups matching the selectors provided from an activity
        /// </summary>
        /// <param name="selectors"></param>
        /// <param name="ruleContext"></param>
        /// <returns>All the groups matching the selectors and rules</returns>
        IList<AccountGroup> SelectGroups(IList<SelectorDefinition> selectors, RuleContext ruleContext);


        /// <summary>
        /// Select groups for an activity using selectors and filters provided
        /// </summary>
        /// <param name="selectors">selectors</param>
        /// <param name="dicFilters">filters linked to the selectors</param>
        /// <param name="ruleContext">ruleContext</param>
        /// <returns>All the groups matching the selectors and rules</returns>
        IList<AccountGroup> SelectGroups(IList<SelectorDefinition> selectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters, RuleContext ruleContext);

    }
}
