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
            IList<RuleConditionDefinition> ret = (inMemoryConditionStore.Where(r => r.Value.RudId.Equals(ruleId)).Select(kp => kp.Value)).ToList();
            return ret;
        }

        public IList<RuleFilterDefinition> FindFiltersBySelectorId(int selectorId)
        {
            IList<RuleFilterDefinition> ret = (inMemoryFilterStore.Where(r => r.Value.SelId.Equals(selectorId)).Select(kp => kp.Value)).ToList();
            return ret;
        }

        public IList<RuleDefinition> FindRulesByItemId(int itemId)
        {
            IList<RuleDefinition> ret = (inMemoryRuleStore.Where(r => r.Value.ItemId.Equals(itemId)).Select(kp => kp.Value)).ToList();
            return ret;
        }

        public IList<SelectorDefinition> FindSelectorsByItemId(int itemId)
        {
            IList<SelectorDefinition> ret = (inMemorySelectorStore.Where(r => r.Value.ItemId.Equals(itemId)).Select(kp => kp.Value)).ToList();
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

        public IList<RuleDefinition> FindRulesByCriteria(RuleCriteria criteria, IList<int> items)
        {
            Debug.Assert(criteria != null);
            //---
            IList<RuleDefinition> ret = new List<RuleDefinition>();
            foreach (int itemId in items)
            {
                IList<RuleDefinition> rules = (inMemoryRuleStore.Where(r => r.Value.Id.Equals(itemId)).Select(kp => kp.Value)).ToList();

                foreach(RuleDefinition rule in rules)
                {
                    Dictionary<string, RuleConditionDefinition> conditions = (inMemoryConditionStore.Where(r => r.Value.RudId.Equals(rule.Id)).Select(kp => kp.Value)).ToDictionary(r => r.Field);

                    int match = 0;
                    RuleConditionDefinition currentRule1 = conditions[criteria.ConditionCriteria1.Field];

                    if (currentRule1 != null && currentRule1.Expression.Equals(criteria.ConditionCriteria1.Value))
                    {
                        match++;
                    }

                    if (criteria.ConditionCriteria2 != null)
                    {
                        RuleConditionDefinition currentRule2 = conditions[criteria.ConditionCriteria2.Field];
                        if (currentRule2 != null && currentRule2.Expression.Equals(criteria.ConditionCriteria2.Value))
                        {
                            match++;
                        }
                    }

                    int expectedMatch = criteria.ConditionCriteria2 != null ? 2 : 1;

                    if (match == expectedMatch)
                    {
                        ret.Add(rule);
                        break;
                    }
                }
            }

            return ret;            
        }

        public void RemoveRules(IList<int> list)
        {
            foreach (int id in list)
            {
                inMemoryRuleStore.Remove(id);
            }
        }

        public void RemoveSelectors(IList<int> list)
        {
            foreach (int id in list)
            {
                inMemorySelectorStore.Remove(id);
            }
        }

        public void RemoveSelectorsFiltersByGroupId(string groupId)
        {
            foreach (SelectorDefinition sel in inMemorySelectorStore.Values)
            {
                if (sel.GroupId.Equals(groupId))
                {
                    foreach(RuleFilterDefinition filter in inMemoryFilterStore.Values)
                    {
                        if (filter.SelId.Equals(sel.Id))
                        {
                            inMemoryFilterStore.Remove(filter.Id.Value);
                        }
                    }
                }

            }

        }
    }
}
