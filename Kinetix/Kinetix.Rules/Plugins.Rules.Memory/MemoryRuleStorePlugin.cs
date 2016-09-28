using Kinetix.Rules;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System;

namespace Kinetix.Rules
{
    public sealed class MemoryRuleStorePlugin : IRuleStorePlugin
    {

        private readonly IDictionary<int, RuleDefinition> inMemoryRuleStore = new ConcurrentDictionary<int, RuleDefinition>();
        private int memoryRuleSequenceGenerator = 0;

        private readonly IDictionary<int, RuleConditionDefinition> inMemoryConditionStore = new ConcurrentDictionary<int, RuleConditionDefinition>();
        private int memoryConditionSequenceGenerator = 0;

        private readonly IDictionary<int, SelectorDefinition> inMemorySelectorStore = new ConcurrentDictionary<int, SelectorDefinition>();
        private int memorySelectorSequenceGenerator = 0;

        private readonly IDictionary<int, RuleFilterDefinition> inMemoryFilterStore = new ConcurrentDictionary<int, RuleFilterDefinition>();
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

        public IList<RuleConditionDefinition> FindConditionByRuleId(int ruleId)
        {
            IList<RuleConditionDefinition> ret = (inMemoryConditionStore.Where(r => r.Value.Id.Equals(ruleId)).Select(kp => kp.Value)).ToList();
            return ret;
        }

        public IList<RuleFilterDefinition> FindFiltersBySelectorId(int selectorId)
        {
            IList<RuleFilterDefinition> ret = (inMemoryFilterStore.Where(r => r.Value.Id.Equals(selectorId)).Select(kp => kp.Value)).ToList();
            return ret;
        }

        public IList<RuleDefinition> FindRulesByItemId(int itemId)
        {
            IList<RuleDefinition> ret = (inMemoryRuleStore.Where(r => r.Value.Id.Equals(itemId)).Select(kp => kp.Value)).ToList();
            return ret;
        }

        public IList<SelectorDefinition> FindSelectorsByItemId(int itemId)
        {
            IList<SelectorDefinition> ret = (inMemorySelectorStore.Where(r => r.Value.Id.Equals(itemId)).Select(kp => kp.Value)).ToList();
            return ret;
        }

        public void RemoveCondition(RuleConditionDefinition ruleConditionDefinition)
        {
            Debug.Assert(ruleConditionDefinition != null);
            Debug.Assert(ruleConditionDefinition.Id != null);
            //---
            inMemoryConditionStore.Remove((int)ruleConditionDefinition.Id);
        }

        public void RemoveFilter(RuleFilterDefinition ruleFilterDefinition)
        {
            Debug.Assert(ruleFilterDefinition != null);
            Debug.Assert(ruleFilterDefinition.Id != null);
            //---
            inMemoryFilterStore.Remove((int)ruleFilterDefinition.Id);
        }

        public void RemoveRule(RuleDefinition ruleDefinition)
        {
            Debug.Assert(ruleDefinition != null);
            Debug.Assert(ruleDefinition.Id != null);
            //---
            inMemoryRuleStore.Remove((int)ruleDefinition.Id);
        }

        public void RemoveSelector(SelectorDefinition selectorDefinition)
        {
            Debug.Assert(selectorDefinition != null);
            Debug.Assert(selectorDefinition.Id != null);
            //---
            inMemorySelectorStore.Remove((int)selectorDefinition.Id);
        }

        public void UpdateCondition(RuleConditionDefinition ruleConditionDefinition)
        {
            Debug.Assert(ruleConditionDefinition != null);
            Debug.Assert(ruleConditionDefinition.Id != null);
            //---
            inMemoryConditionStore[(int)ruleConditionDefinition.Id] = ruleConditionDefinition;
        }

        public void UpdateFilter(RuleFilterDefinition ruleFilterDefinition)
        {
            Debug.Assert(ruleFilterDefinition != null);
            Debug.Assert(ruleFilterDefinition.Id != null);
            //---
            inMemoryFilterStore[(int)ruleFilterDefinition.Id] = ruleFilterDefinition;
        }

        public void UpdateRule(RuleDefinition ruleDefinition)
        {
            Debug.Assert(ruleDefinition != null);
            Debug.Assert(ruleDefinition.Id != null);
            //---
            inMemoryRuleStore[(int)ruleDefinition.Id] = ruleDefinition;
        }

        public void UpdateSelector(SelectorDefinition selectorDefinition)
        {
            Debug.Assert(selectorDefinition != null);
            Debug.Assert(selectorDefinition.Id != null);
            //---
            inMemorySelectorStore[(int)selectorDefinition.Id] = selectorDefinition;
        }

        public IList<int> FindItemsByCriteria(RuleCriteria criteria, IList<int> items)
        {
            Debug.Assert(criteria != null);
            //---
            IList<int> ret = new List<int>();
            foreach (int itemId in items)
            {
                IList<RuleDefinition> rules = (inMemoryRuleStore.Where(r => r.Value.Id.Equals(itemId)).Select(kp => kp.Value)).ToList();

                foreach(RuleDefinition rule in rules)
                {
                    Dictionary<string, RuleConditionDefinition> conditions = (inMemoryConditionStore.Where(r => r.Value.RudId.Equals(rule.Id)).Select(kp => kp.Value)).ToDictionary(r => r.Field);

                    int match = 0;
                    foreach(RuleConditionCriteria RuleConditionCriteria in criteria.ConditionCriteria)
                    {
                        RuleConditionDefinition currentRule = conditions[RuleConditionCriteria.Field];

                        if (currentRule != null && currentRule.Expression.Equals(RuleConditionCriteria.Value))
                        {
                            match++;
                        }
                    }

                    if (match == criteria.ConditionCriteria.Count)
                    {
                        ret.Add(itemId);
                        break;
                    }
                }
            }

            return ret;            
        }
    }
}
