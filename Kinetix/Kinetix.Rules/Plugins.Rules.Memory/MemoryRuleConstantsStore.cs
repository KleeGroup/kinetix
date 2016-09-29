using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;


namespace Kinetix.Rules
{
    public sealed class MemoryRuleConstantsStore : IRuleConstantsStorePlugin
    {

        private readonly IDictionary<int, RuleConstants> inMemoryRuleStore = new ConcurrentDictionary<int, RuleConstants>();

        public void AddConstants(int key, RuleConstants ruleConstants)
        {
            Debug.Assert(ruleConstants != null);
            //---
            inMemoryRuleStore[key] = ruleConstants;
        }

        public RuleConstants ReadConstants(int key)
        {
            RuleConstants val;
            inMemoryRuleStore.TryGetValue(key, out val);
            return val;
        }

        public void RemoveConstants(int key)
        {
            inMemoryRuleStore.Remove(key);
        }

        public void UpdateConstants(int key, RuleConstants ruleConstants)
        {
            Debug.Assert(ruleConstants != null);
            //---
            inMemoryRuleStore[key] = ruleConstants;
        }
    }
}
