using Kinetix.Account;
using Kinetix.Rules;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Workflow
{
    public class ValidateExistingDecisionsRecalculationPlugin : IWorkflowRecalculationPlugin
    {
        private readonly IRuleManager _ruleManager;
        private readonly IWorkflowStorePlugin _workflowStorePlugin;

        public ValidateExistingDecisionsRecalculationPlugin(IRuleManager ruleManager, IWorkflowStorePlugin workflowStorePlugin)
        {
            _ruleManager = ruleManager;
            _workflowStorePlugin = workflowStorePlugin;
        }

        private WfActivity GetNewActivity(WfActivityDefinition activityDefinition, WfWorkflow wfWorkflow, bool isAuto, bool isValid)
        {
            WfActivity wfActivity = new WfActivity();
            wfActivity.CreationDate = DateTime.Now;
            wfActivity.WfadId = activityDefinition.WfadId.Value;
            wfActivity.WfwId = wfWorkflow.WfwId.Value;
            wfActivity.IsAuto = isAuto;
            wfActivity.IsValid = isValid;
            return wfActivity;
        }

        public void CustomRecalculation(IList<WfActivityDefinition> activityDefinitions, RuleConstants ruleConstants, WfWorkflow wf, IDictionary<int, List<RuleDefinition>> dicRules, IDictionary<int, List<RuleConditionDefinition>> dicConditions, IDictionary<int, List<SelectorDefinition>> dicSelectors, IDictionary<int, List<RuleFilterDefinition>> dicFilters, IDictionary<int, List<WfActivity>> dicActivities, IDictionary<int, List<WfDecision>> dicDecision, IDictionary<int, object> dicObjects, WfRecalculationOutput output)
        {
            object obj;
            dicObjects.TryGetValue(wf.ItemId.Value, out obj);

            if (obj == null)
            {
                // No item associated to this workflow.
                return;
            }

            List<WfActivity> allActivities;
            dicActivities.TryGetValue(wf.WfwId.Value, out allActivities);

            if (allActivities == null)
            {
                // No activity for this workflow.
                allActivities = new List<WfActivity>();
            }

            IDictionary<int, WfActivity> activities = allActivities.ToDictionary(a => a.WfadId);

            WfActivity currentActivity = allActivities.Where(a => a.WfaId.Equals(wf.WfaId2.Value)).FirstOrDefault();
            IList<WfActivityDefinition> nextActivityDefinitions;
            if (currentActivity != null)
            {
                nextActivityDefinitions = activityDefinitions.SkipWhile(ad => ad.WfadId.Equals(currentActivity.WfadId) == false).ToList();
            }
            else
            {
                nextActivityDefinitions = activityDefinitions;
            }

            RuleContext ruleContext = new RuleContext(obj, ruleConstants);

            foreach (WfActivityDefinition ad in nextActivityDefinitions)
            {
                WfActivity activity;
                activities.TryGetValue(ad.WfadId.Value, out activity);
                int actDefId = ad.WfadId.Value;

                bool isManual = _ruleManager.IsRuleValid(actDefId, ruleContext, dicRules, dicConditions);

                if (isManual)
                {
                    IList<AccountUser> accounts = _ruleManager.SelectAccounts(actDefId, ruleContext, dicSelectors, dicFilters);
                    isManual = accounts.Count > 0;
                }

                if (isManual)
                {
                    if (activity == null)
                    {
                        WfActivity wfActivity = GetNewActivity(ad, wf, false, false);
                        output.AddActivitiesCreateUpdateCurrentActivity(wfActivity);
                        break;
                    }

                    if (activity.IsValid == false)
                    {
                        wf.WfaId2 = activity.WfaId;
                        output.AddWorkflowsUpdateCurrentActivity(wf);
                        break;
                    }
                }
                else
                {
                    if (activity == null)
                    {
                        WfActivity wfActivity = GetNewActivity(ad, wf, true, false);
                        output.AddActivitiesCreate(wfActivity);
                    }
                    else
                    {
                        activity.IsAuto = true;
                        output.AddActivitiesUpdateIsAuto(activity);
                    }
                }
            }
        }
    }
}
