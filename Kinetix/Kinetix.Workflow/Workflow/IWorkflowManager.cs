using Kinetix.Rules;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.Collections.Generic;

namespace Kinetix.Workflow
{
    public interface IWorkflowManager
    {

        #region instances
        /// <summary>
        /// Instantiate a new workflow instance 
        /// </summary>
        /// <param name="wfwdId">wfwdId</param>
        /// <param name="username">username</param>
        /// <param name="userLogic">userLogic</param>
        /// <param name="item">item</param>
        /// <returns>a new workflow instance</returns>
        WfWorkflow CreateWorkflowInstance(int wfwdId, string username, bool userLogic, int item);

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
        /// Remove a workflow instance
        /// </summary>
        /// <param name="wfwId">Workflow Id</param>
        void RemoveWorkflow(int wfwId);

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
        /// Get The User Id used for autovalidating activities.
        /// </summary>
        /// <returns>User Id</returns>
        string GetUserAuto();

        /// <summary>
        /// Get an activity from Id
        /// </summary>
        /// <param name="wfaId">Activity Id</param>
        /// <returns></returns>
        WfActivity GetActivity(int wfaId);

        /// <summary>
        /// Get an activity from a definition and a workflow instance
        /// </summary>
        /// <param name="wfWorkflow">workflow instance</param>
        /// <param name="wfActivityDefinition">Activity Definition</param>
        /// <returns></returns>
        WfActivity GetActivity(WfWorkflow wfWorkflow, WfActivityDefinition wfActivityDefinition);

        /// <summary>
        /// Save the decision for the current activity without moving to the next.
        /// Use this method when the decision has to be saved before pausing the or ending the worklfow.
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <param name="wfDecision">wfDecision</param>
        void SaveDecision(WfWorkflow wfWorkflow, WfDecision wfDecision);

        /// <summary>
        /// Save the decision for the current activity without moving to the next.
        /// Use this method when the decision has to be saved before pausing the or ending the worklfow.
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <param name="wfDecision">wfDecision</param>
        /// <param name="forceValid">Force the current activity to be valid (Mul only)</param>
        void SaveDecision(WfWorkflow wfWorkflow, WfDecision wfDecision, bool forceValid);

        /// <summary>
        /// Get the decision for a single activity.
        /// </summary>
        /// <param name="wfActivity">Activity</param>
        /// <returns>The decision for this activity</returns>
        WfDecision GetDecision(WfActivity wfActivity);

        /// <summary>
        /// Get the decisions for an multiple activity
        /// </summary>
        /// <param name="wfActivity">Activity</param>
        /// <returns>All the decisions for this activity</returns>
        IList<WfDecision> GetDecisions(WfActivity wfActivity);

        /// <summary>
        /// Delete one decision
        /// </summary>
        /// <param name="wfDecision">The decision to remove</param>
        void DeleteDecision(WfDecision wfDecision);

        /// <summary>
        /// Save the decision for the current activity and go to the next activity using the default transition
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <param name="wfDecision">wfDecision</param>
        void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, WfDecision wfDecision);

        /// <summary>
        /// Save the decision for the current activity and go to the next activity using the default transition
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <param name="wfDecision">wfDecision</param>
        /// <param name="forceValid">Force the current activity to be valid (Mul only)</param>
        void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, WfDecision wfDecision, bool forceValid);

        /// <summary>
        /// Go to the next activity using the provided transition name
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <param name="transitionName">transitionName</param>
        /// <param name="wfDecision">wfDecision</param>
        void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, string transitionName, WfDecision wfDecision);

        /// <summary>
        /// Go to the next activity using the provided transition name
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <param name="transitionName">transitionName</param>
        /// <param name="wfDecision">wfDecision</param>
        /// <param name="forceValid">Force the current activity to be valid (Mul only)</param>
        void SaveDecisionAndGoToNextActivity(WfWorkflow wfWorkflow, string transitionName, WfDecision wfDecision, bool forceValid);

        /// <summary>
        /// Go To the next activity
        /// </summary>
        /// <param name="wfWorkflow">workflow</param>
        void GoToNextActivity(WfWorkflow wfWorkflow);

        /// <summary>
        /// Predicate method to test if we can go to the next activity.
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <returns>
        /// For a single activity:
        ///     True when a decision exist for the current activity, False otherwise
        /// For a multiple activity:
        ///     True when all the decisions exist for the accounts linked to this activity, False otherwise
        /// </returns>
        bool CanGoToNextActivity(WfWorkflow wfWorkflow);

       
        /// <summary>
        /// Autovalidate all the next activities using the default transition the the provided activity.
        /// This autovalidation can validate 0, 1 or N activities.
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <param name="wfActivity">wfActivity</param>
        /// <param name="wfActivityDefinitionId">wfActivityDefinitionId</param>
        /// <returns>true if all the default activities has been reached, false otherwise</returns>
        bool AutoValidateNextActivities(WfWorkflow wfWorkflow, WfActivity wfActivity, int wfActivityDefinitionId);

        /// <summary>
        /// Does the provided activity can be autovalidated.
        /// </summary>
        /// <param name="activityDefinition">wfWorkflow</param>
        /// <returns>true if the provided activty can be auto validated, false otherwise</returns>
        bool CanAutoValidateActivity(WfActivityDefinition activityDefinition, object wfWorkflow);

        /// <summary>
        /// Get the list of activities matching the rules following the default transition from the start until the end.
        /// </summary>
        /// <param name="wfWorkflow">wfWorkflow</param>
        /// <returns>the list of activities matching the rules following the default path from the start until the end</returns>
        IList<WfActivityDefinition> GetActivityDefinitions(WfWorkflow wfWorkflow);


        /// <summary>
        /// Get all default activity definitions
        /// </summary>
        /// <param name="wfWorkflowDefinition"></param>
        /// <returns>The list of all the definitions matching or not the rules</returns>
        IList<WfActivityDefinition> GetAllDefaultActivities(WfWorkflowDefinition wfWorkflowDefinition);

        /// <summary>
        /// Get activities instance from definition for a workflow
        /// </summary>
        /// <param name="wfWorkflow"></param>
        /// <param name="wfadId">List of activity definitinon ids </param>
        /// <returns></returns>
        IList<WfActivity> GetActivities(WfWorkflow wfWorkflow, IList<int> wfadIds);

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
        /// Rename an activity definition.
        /// </summary>
        /// <param name="wfActivityDefinition">the activity definition to rename</param>
        void RenameActivity(WfActivityDefinition wfActivityDefinition);

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
        void MoveActivity(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityToMove, WfActivityDefinition wfActivityReferential, bool after);

        #endregion

        #region Rules/selectors
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

        /// <summary>
        /// Get the conditions associated to a rule
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns>a list of all the conditions for the rule</returns>
        IList<RuleConditionDefinition> GetConditionsForRuleId(int ruleId);

        /// <summary>
        /// Get the conditions associated to a rule
        /// </summary>
        /// <param name="selectorId"></param>
        /// <returns>a list of all the filters for the selector</returns>
        IList<RuleFilterDefinition> GetFiltersForSelectorId(int selectorId);

        /// <summary>
        /// Get the rules associated to an activity definition
        /// </summary>
        /// <param name="wfadId"></param>
        /// <returns>a list of all the rules for the activity definition</returns>
        IList<RuleDefinition> GetRulesForActivityDefinition(int wfadId);

        /// <summary>
        /// Get the selectors associated to an activity definition
        /// </summary>
        /// <param name="wfadId"></param>
        /// <returns>a list of all the selectors for the activity definition</returns>
        IList<SelectorDefinition> GetSelectorsForActivityDefinition(int wfadId);

        /// <summary>
        /// Get the workflow definition from its id
        /// </summary>
        /// <param name="wfwdId"></param>
        /// <returns>the workflow definition</returns>
        WfWorkflowDefinition GetWorkflowDefinition(int wfwdId);

        /// <summary>
        /// Get the workflow definition from its name
        /// </summary>
        /// <param name="wfdName"></param>
        /// <returns>the workflow definition</returns>
        WfWorkflowDefinition GetWorkflowDefinition(string wfdName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rules"></param>
        void RemoveRules(IList<RuleDefinition> rules);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectors"></param>
        void RemoveSelectors(IList<SelectorDefinition> selectors);
        #endregion

        #region recalculation

        /// <summary>
        /// Recalculate a workflow instance.
        /// This methode should be called when the item linked to the workflow has changed.
        /// </summary>
        /// <param name="wfworkflow">the workflow to recalculate</param>
        WfRecalculationOutput RecalculateWorkflow(WfWorkflow wfworkflow, bool fetchWorkflowDecisions = false);

        /// <summary>
        /// Recalculate a workflow definition. All the started instances linked to the workflow will be recalculated.
        /// </summary>
        /// <param name="wfWorkflowDefinition"></param>
        WfRecalculationOutput RecalculateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition, bool fetchWorkflowDecisions = false);

        #endregion
        

        #region Custom Methods

        /// <summary>
        /// Get a workflow with all the associated elements
        /// </summary>
        /// <param name="wfwId">Workflow Id</param>
        /// <returns>a List of WfWorkflowDecision</returns>
        IList<WfWorkflowDecision> GetWorkflowDecisions(int wfwId);


        /// <summary>
        /// Get a workflow with all the associated elements
        /// </summary>
        /// <param name="wfwId">Workflow Definition Id</param>
        /// <returns>a List of WfWorkflowDecision</returns>
        IList<WfListWorkflowDecision> GetAllWorkflowDecisions(int wfwdId);


        /// <summary>
        /// Remove all selectors and filters for a specified groupId
        /// </summary>
        /// <param name="groupId">groupId</param>
        void RemoveSelectorsFiltersByGroupId(string groupId);

        #endregion

    }
}
