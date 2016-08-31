using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Rules
{
    public interface IRuleConstantsStore
    {
        /// <summary>
        /// Add constants.
        /// </summary>
        /// <param name="key">key.</param>
        /// <param name="ruleConstants">ruleConstants.</param>
        void AddConstants(long key, RuleConstants ruleConstants);

        /// <summary>
        /// Remove constants.
        /// </summary>
        /// <param name="key">key.</param>
        void RemoveConstants(long key);

        /// <summary>
        /// Update constants.
        /// </summary>
        /// <param name="key">key.</param>
        /// <param name="ruleConstants">ruleConstants.</param>
        void UpdateConstants(long key, RuleConstants ruleConstants);

        /// <summary>
        /// Get constants.
        /// </summary>
        /// <param name="key">key.</param>
        /// <returns>the rule constants matching the key</returns>
        RuleConstants ReadConstants(long key);
    }
}
