using Kinetix.Rules;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.Collections.Generic;

namespace Kinetix.Workflow
{
    public interface IWorkflowManager
    {

        // Instances:
        /// <summary>
        /// Instantiate a new workflow instance 
        /// </summary>
        /// <param name="definitionName">definitionName</param>
        /// <param name="user">user</param>
        /// <param name="userLogic">userLogic</param>
        /// <param name="item">item</param>
        /// <returns>a new workflow instance</returns>
        WfWorkflow CreateWorkflowInstance(string definitionName, string user, bool userLogic, int item);

        /// <summary>
        /// Get a workflow instance by its Id. 
        /// </summary>
        /// <param name="wfwId">wfwId</param>
        /// <return>the workflow instance</return>
        WfWorkflow GetWorkflowInstance(int wfwId);

        /// <summary>
        /// Get a workflow instance by the item Id. 
        /// </summary>
        /// <param name="wfwdId">wfwdId of the item</param>
        /// <param name="itemId">id of the item</param>
        /// <return>the workflow instance</return>
        WfWorkflow GetWorkflowInstanceByItemId(int wfwdId, int itemId);


        /// <summary>
        /// Start a workflow instance. 
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        void StartInstance(WfWorkflow wfWorkflow);

        /// <summary>
        /// Stop a workflow instance. 
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        void EndInstance(WfWorkflow wfWorkflow);

        /// <summary>
        /// Pause a workflow instance. 
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        void PauseInstance(WfWorkflow wfWorkflow);

        /// <summary>
        /// Resume a paused workflow instance. 
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        void ResumeInstance(WfWorkflow wfWorkflow);

        /// <summary>
        /// Save the decision for the current activity without moving to the next.
        /// Use this method when the decision has to be saved before pausing the or ending the worklfow.
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <param name="wfDecision">wfDecision</param>
        void SaveDecision(WfWorkflow wfWorkflow, WfDecision wfDecision);

        /// <summary>
        /// Save the decision for the current activity and go to the next activity using the default transition
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <param name="wfDecision">wfDecision</param>
        void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, WfDecision wfDecision);

        /// <summary>
        /// Go to the next activity using the provided transition name
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <param name="transitionName">transitionName</param>
        /// <param name="wfDecision">wfDecision</param>
        void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, string transitionName, WfDecision wfDecision);

        /// <summary>
        /// Autovalidate all the next activities using the default transition the the provided activity.
        /// This autovalidation can validate 0, 1 or N activities.
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <param name="wfActivityDefinitionId">wfActivityDefinitionId</param>
        void AutoValidateNextActivities(WfWorkflow wfWorkflow, int wfActivityDefinitionId);

        /// <summary>
        /// Does the provided activity can be autovalidated.
        /// </summary>
        /// <param name="activityDefinition">wfWorkflow</param>
        /// <returns>true if the provided activty can be auto validated, false otherwise</returns>
        bool CanAutoValidateActivity(WfActivityDefinition activityDefinition, object wfWorkflow);

        /// <summary>
        /// Get the list of activities following the default transition from the start until the end.
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <returns>the list of activities following the default path from the start until the end</returns>
        IList<WfActivityDefinition> GetActivities(WfWorkflow wfWorkflow);

        // Definitions:
        /// <summary>
        /// Create a new Workflow Definition.
        /// </summary>
        /// <param name="wfWorkflowDefinition">wfWorkflowDefinition</param>
        void CreateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition);

        /// <summary>
        /// Add an activity to the workflow definition.
        /// </summary>
        /// <param name="wfWorkflowDefinition">wfWorkflowDefinition</param>
        /// <param name="wfActivityDefinition">wfActivityDefinition</param>
        /// <param name="position">position</param>
        void AddActivity(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityDefinition, int position);

        /// <summary>
        /// Remove an activity to the workflow definition.
        /// </summary>
        /// <param name="wfActivityDefinition">the activity to remove</param>
        void RemoveActivity(WfActivityDefinition wfActivityDefinition);

        /// <summary>
        /// Move an activity from a position to another position.
        /// </summary>
        /// <param name="wfWorkflowDefinition">wfWorkflowDefinition</param>
        /// <param name="src">activity original position</param>
        /// <param name="dst">activity destination position</param>
        /// <param name="after">true to move the activity after the referential activity. false before</param>
        void MoveActivity(WfWorkflowDefinition wfWorkflowDefinition, int src, int dst, bool after);

        /// <summary>
        /// Move an activity from a position to another position.
        /// </summary>
        /// <param name="wfWorkflowDefinition">wfWorkflowDefinition</param>
        /// <param name="wfActivity">activity to move</param>
        /// <param name="wfActivityReferential">the referential activity where the activity should move (before or after)</param>
        /// <param name="after">true to move the activity after the referential activity. false before</param>
        void MoveActivity(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivity, WfActivityDefinition wfActivityReferential, bool after);

        //Rules/selectors
        /// <summary>
        /// Add and attach the provided rule to the designed activity.
        /// </summary>
        /// <param name="wfActivity">wfActivity</param>
        /// <param name="ruleDefinition">ruleDefinition</param>
        /// <param name="conditions">conditions</param>
        void AddRule(WfActivityDefinition wfActivity, RuleDefinition ruleDefinition, List<RuleConditionDefinition> conditions);

        /// <summary>
        /// Remove and dettach the provided rules from the activity.
        /// </summary>
        /// <param name="rule">rule</param>
        void RemoveRule(RuleDefinition rule);

        /// <summary>
        /// Add and attach the provided selectors to the designed activity.
        /// </summary>
        /// <param name="wfActivity">wfActivity</param>
        /// <param name="selector">selector</param>
        /// <param name="filters">filters</param>
        void AddSelector(WfActivityDefinition wfActivity, SelectorDefinition selector, List<RuleFilterDefinition> filters);

        /// <summary>
        /// Remove and dettach the provided selector from the activity.
        /// </summary>
        /// <param name="selector">selector</param>
        void RemoveSelector(SelectorDefinition selector);

        /// <summary>
        /// Find activities matching the criteria in parameters
        /// </summary>
        /// <param name="criteria"></param>
        IList<WfActivityDefinition> FindActivitiesByCriteria(RuleCriteria criteria);
    }
}
