using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;


namespace Kinetix.Rules
{
    class MemoryRuleConstantsStore : IRuleConstantsStorePlugin
    {

        private readonly IDictionary<long, RuleConstants> inMemoryRuleStore = new ConcurrentDictionary<long, RuleConstants>();

        public void AddConstants(long key, RuleConstants ruleConstants)
        {
            Debug.Assert(ruleConstants != null);
            //---
            inMemoryRuleStore[key] = ruleConstants;
        }

        public RuleConstants ReadConstants(long key)
        {
            return inMemoryRuleStore[key];
        }

        public void RemoveConstants(long key)
        {
            inMemoryRuleStore.Remove(key);
        }

        public void UpdateConstants(long key, RuleConstants ruleConstants)
        {
            Debug.Assert(ruleConstants != null);
            //---
            inMemoryRuleStore[key] = ruleConstants;
        }
    }
}
