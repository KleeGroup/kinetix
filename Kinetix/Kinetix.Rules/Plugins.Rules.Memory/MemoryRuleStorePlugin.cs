using Kinetix.Rules;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Kinetix.Rules
{
    public sealed class MemoryRuleStorePlugin : IRuleStorePlugin
    {

        private readonly IDictionary<long, RuleDefinition> inMemoryRuleStore = new ConcurrentDictionary<long, RuleDefinition>();
        private int memoryRuleSequenceGenerator = 0;

        private readonly IDictionary<long, RuleConditionDefinition> inMemoryConditionStore = new ConcurrentDictionary<long, RuleConditionDefinition>();
        private int memoryConditionSequenceGenerator = 0;

        private readonly IDictionary<long, SelectorDefinition> inMemorySelectorStore = new ConcurrentDictionary<long, SelectorDefinition>();
        private int memorySelectorSequenceGenerator = 0;

        private readonly IDictionary<long, RuleFilterDefinition> inMemoryFilterStore = new ConcurrentDictionary<long, RuleFilterDefinition>();
        private int memoryFilterSequenceGenerator = 0;

        public void AddCondition(RuleConditionDefinition ruleConditionDefinition)
        {
            Debug.Assert(ruleConditionDefinition != null);
            Debug.Assert(ruleConditionDefinition.Id == null, "A new condition must not have an id");
            //---
            int generatedId = Interlocked.Increment(ref memoryConditionSequenceGenerator);
            ruleConditionDefinition.Id = generatedId;
            inMemoryConditionStore[generatedId] = ruleConditionDefinition;
        }

        public void AddFilter(RuleFilterDefinition ruleFilterDefinition)
        {
            Debug.Assert(ruleFilterDefinition != null);
            Debug.Assert(ruleFilterDefinition.Id == null, "A new filter must not have an id");
            //---
            int generatedId = Interlocked.Increment(ref memoryFilterSequenceGenerator);
            ruleFilterDefinition.Id = generatedId;
            inMemoryFilterStore[generatedId] = ruleFilterDefinition;
        }

        public void AddRule(RuleDefinition ruleDefinition)
        {
            Debug.Assert(ruleDefinition != null);
            Debug.Assert(ruleDefinition.Id == null, "A new rule must not have an id");
            //---
            int generatedId = Interlocked.Increment(ref memoryRuleSequenceGenerator);
            ruleDefinition.Id = generatedId;
            inMemoryRuleStore[generatedId] = ruleDefinition;
        }

        public void AddSelector(SelectorDefinition selectorDefinition)
        {
            Debug.Assert(selectorDefinition != null);
            Debug.Assert(selectorDefinition.Id == null, "A new selector must not have an id");
            //---
            int generatedId = Interlocked.Increment(ref memorySelectorSequenceGenerator);
            selectorDefinition.Id = generatedId;
            inMemorySelectorStore[generatedId] = selectorDefinition;
        }

        public IList<RuleConditionDefinition> FindConditionByRuleId(long ruleId)
        {
            IList<RuleConditionDefinition> ret = (inMemoryConditionStore.Where(r => r.Value.Id.Equals(ruleId)).Select(kp => kp.Value)).ToList();
            return ret;
        }

        public IList<RuleFilterDefinition> FindFiltersBySelectorId(long selectorId)
        {
            IList<RuleFilterDefinition> ret = (inMemoryFilterStore.Where(r => r.Value.Id.Equals(selectorId)).Select(kp => kp.Value)).ToList();
            return ret;
        }

        public IList<RuleDefinition> FindRulesByItemId(long itemId)
        {
            IList<RuleDefinition> ret = (inMemoryRuleStore.Where(r => r.Value.Id.Equals(itemId)).Select(kp => kp.Value)).ToList();
            return ret;
        }

        public IList<SelectorDefinition> FindSelectorsByItemId(long itemId)
        {
            IList<SelectorDefinition> ret = (inMemorySelectorStore.Where(r => r.Value.Id.Equals(itemId)).Select(kp => kp.Value)).ToList();
            return ret;
        }

        public void RemoveCondition(RuleConditionDefinition ruleConditionDefinition)
        {
            Debug.Assert(ruleConditionDefinition != null);
            Debug.Assert(ruleConditionDefinition.Id != null);
            //---
            inMemoryConditionStore.Remove((long)ruleConditionDefinition.Id);
        }

        public void RemoveFilter(RuleFilterDefinition ruleFilterDefinition)
        {
            Debug.Assert(ruleFilterDefinition != null);
            Debug.Assert(ruleFilterDefinition.Id != null);
            //---
            inMemoryFilterStore.Remove((long)ruleFilterDefinition.Id);
        }

        public void RemoveRule(RuleDefinition ruleDefinition)
        {
            Debug.Assert(ruleDefinition != null);
            Debug.Assert(ruleDefinition.Id != null);
            //---
            inMemoryRuleStore.Remove((long)ruleDefinition.Id);
        }

        public void RemoveSelector(SelectorDefinition selectorDefinition)
        {
            Debug.Assert(selectorDefinition != null);
            Debug.Assert(selectorDefinition.Id != null);
            //---
            inMemorySelectorStore.Remove((long)selectorDefinition.Id);
        }

        public void UpdateCondition(RuleConditionDefinition ruleConditionDefinition)
        {
            Debug.Assert(ruleConditionDefinition != null);
            Debug.Assert(ruleConditionDefinition.Id != null);
            //---
            inMemoryConditionStore[(long)ruleConditionDefinition.Id] = ruleConditionDefinition;
        }

        public void UpdateFilter(RuleFilterDefinition ruleFilterDefinition)
        {
            Debug.Assert(ruleFilterDefinition != null);
            Debug.Assert(ruleFilterDefinition.Id != null);
            //---
            inMemoryFilterStore[(long)ruleFilterDefinition.Id] = ruleFilterDefinition;
        }

        public void UpdateRule(RuleDefinition ruleDefinition)
        {
            Debug.Assert(ruleDefinition != null);
            Debug.Assert(ruleDefinition.Id != null);
            //---
            inMemoryRuleStore[(long)ruleDefinition.Id] = ruleDefinition;
        }

        public void UpdateSelector(SelectorDefinition selectorDefinition)
        {
            Debug.Assert(selectorDefinition != null);
            Debug.Assert(selectorDefinition.Id != null);
            //---
            inMemorySelectorStore[(long)selectorDefinition.Id] = selectorDefinition;
        }
    }
}
