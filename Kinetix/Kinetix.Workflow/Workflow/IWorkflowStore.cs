using Kinetix.Rules;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System;
using System.Collections.Generic;

namespace Kinetix.Workflow
{

    /// <summary>
    /// This class defines the storage of workflow.
    /// </summary>
    public interface IWorkflowStore
    {
        //Instance

        /// <summary>
        /// Create a new workflow.
        /// </summary>
        /// <param name="workflow">workflow.</param>
        void CreateWorkflowInstance(WfWorkflow workflow);

        /// <summary>
        /// Get a workflow instance.
        /// </summary>
        /// <param name="wfwId">id of the workflow instance.</param>
        /// <returns>the corresponding workflow</returns> 
        WfWorkflow ReadWorkflowInstanceById(int wfwId);

        /// <summary>
        /// Get and lock a workflow instance.
        /// </summary>
        /// <param name="wfwId">id of the workflow instance.</param>
        /// <returns>the corresponding workflow</returns> 
        WfWorkflow ReadWorkflowInstanceForUpdateById(int wfwId);

        /// <summary>
        /// Get and lock all workflows instances for a definition.
        /// </summary>
        /// <param name="wfwId">id of the workflow instance.</param>
        /// <returns>the corresponding workflow</returns> 
        IList<WfWorkflow> ReadWorkflowsInstanceForUpdateById(int wfwdId);

        /// <summary>
        /// Get a workflow instance by an item id.
        /// </summary>
        /// <param name="wfwdId">id of the workflow definition.</param>
        /// <param name="itemId">id of the item id.</param>
        /// <returns>the corresponding workflow</returns>
        WfWorkflow ReadWorkflowInstanceByItemId(int wfwdId, int itemId);

        /// <summary>
        /// Update a workflow instance.
        /// /!\ The id must be set
        /// </summary>
        /// <param name="workflow">the new workflow to update.</param>
        void UpdateWorkflowInstance(WfWorkflow workflow);

        /// <summary>
        /// Fetch an activity by id.
        /// </summary>
        /// <param name="wfadId">wfadId.</param>
        /// <returns>the corresponding activity</returns>
        WfActivity ReadActivity(int wfadId);

        /// <summary>
        /// Get all the decisions associated to an Activity
        /// </summary>
        /// <param name="wfadId"></param>
        /// <returns></returns>
        IList<WfDecision> ReadDecisionsByActivityId(int wfaId);

        /// <summary>
        /// Create a new activity.
        /// </summary>
        /// <param name="wfActivity">wfActivity.</param>
        /// <param name="wfadId">wfadId.</param>
        void CreateActivity(WfActivity wfActivity);


        /// <summary>
        /// Update an existing activity
        /// </summary>
        /// <param name="wfActivity">wfActivity</param>
        void UpdateActivity(WfActivity wfActivity);

        /// <summary>
        /// Delete an activity
        /// </summary>
        /// <param name="wfActivity">wfActivity</param>
        void DeleteActivity(WfActivity wfActivity);

        /// <summary>
        /// Delete all activities for the ActivityDefinition id.
        /// </summary>
        /// <param name="wfadId">ActivityDefinition id</param>
        void DeleteActivities(int wfadId);

        /// <summary>
        /// Delete a workflow instance
        /// </summary>
        /// <param name="wfwId">Workflow id</param>
        void DeleteWorkflow(int wfwId);

        /// <summary>
        /// Decrement position by 1 for all activity definition >= position
        /// </summary>
        /// <param name="wfwdId"></param>
        /// <param name="position"></param>
        void DecrementActivityDefinitionPositionsAfter(int wfwdId, int position);

        /// <summary>
        /// Increment position by 1 for all activity definition >= position
        /// </summary>
        /// <param name="wfwdId"></param>
        /// <param name="position"></param>
        void IncrementActivityDefinitionPositionsAfter(int wfwdId, int position);

        /// <summary>
        /// Shift position number between 2 positions
        /// </summary>
        /// <param name="wfwdId"></param>
        /// <param name="posStart"></param>
        /// <param name="posEnd"></param>
        /// <param name="shift"></param>
        void ShiftActivityDefinitionPositionsBetween(int wfwdId, int posStart, int posEnd, int shift);

        /// <summary>
        /// Create a new decision
        /// </summary>
        /// <param name="wfDecision">wfDecision</param>
        void CreateDecision(WfDecision wfDecision);

        /// <summary>
        /// Update a decision
        /// </summary>
        /// <param name="wfDecision">wfDecision</param>
        void UpdateDecision(WfDecision wfDecision);


        /// <summary>
        /// Delete a Decision
        /// </summary>
        /// <param name="wfDecision">Decision to delete</param>
        void DeleteDecision(WfDecision wfDecision);

        /// <summary>
        /// Find all decision for an activity
        /// </summary>
        /// <param name="wfActivity">wfActivity</param>
        /// <returns>All the decision link to the provided activity</returns>
        IList<WfDecision> FindAllDecisionByActivity(WfActivity wfActivity);

        /// <summary>
        /// Does the provided activity has a next activity using the default transition
        /// </summary>
        /// <param name="activity">activity</param>
        /// <returns>true if the activity has a default transition to another activity. false if the activity is the last activity</returns>
        bool HasNextActivity(WfActivity activity);

        /// <summary>
        /// Does the provided has a next activity using the provided transition name
        /// </summary>
        /// <param name="activity">activity</param>
        /// <param name="transitionName">transitionName</param>
        /// <returns>true if the activity has a transition to another activity. false if the activity is the last activity</returns>
        bool HasNextActivity(WfActivity activity, String transitionName);

        /// <summary>
        /// Find the activities from a list of activity definition ids for a workflow.
        /// </summary>
        /// <param name="wfWorkflow"></param>
        /// <param name="wfadIds"></param>
        /// <returns>All matching activities</returns>
        IList<WfActivity> FindActivitiesByDefinitionId(WfWorkflow wfWorkflow, IList<int> wfadIds);


        /// <summary>
        /// Find All active workflow (Started, or Paused)
        /// </summary>
        /// <param name="wfWorkflowDefinition"></param>
        /// <param name="isForUpdate"></param>
        /// <returns>List of active workflow for the definition</returns>
        IList<WfWorkflow> FindActiveWorkflows(WfWorkflowDefinition wfWorkflowDefinition, bool isForUpdate);

        /// <summary>
        /// Find All active workflow (Started, or Paused)
        /// </summary>
        /// <param name="wfwdId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        IList<WfWorkflow> FindActiveWorkflowInstanceByItemId(int wfwdId, int itemId);

        // Definition

        /// <summary>
        /// Count the number of default transitions for the provided Workflow.
        /// </summary>
        /// <param name="activity">activity</param>
        /// <param name="transitionName">transitionName</param>
        /// <returns>the number of default transition in the workflow.</returns>
        int CountDefaultTransitions(WfWorkflowDefinition wfWorkflowDefinition);

        /// <summary>
        /// Create a new workflow definition.
        /// </summary>
        /// <param name="workflowDefinition">workflowDefinition</param>
        void CreateWorkflowDefinition(WfWorkflowDefinition workflowDefinition);

        /// <summary>
        /// Get an definition of workflow.
        /// </summary>
        /// <param name="wfwdId">the id of the workflow definition</param>
        /// <returns>the corresponding Workflow definition</returns>
        WfWorkflowDefinition ReadWorkflowDefinition(int wfwdId);

        /// <summary>
        /// Get an definition of workflow.
        /// </summary>
        /// <param name="definitionName">the name of the workflow definition</param>
        /// <returns>the corresponding Workflow definition</returns>
        WfWorkflowDefinition ReadWorkflowDefinition(String definitionName);

        /// <summary>
        /// Update the definition of a workflow.
        /// </summary>
        /// <param name="wfWorkflowDefinition">wfWorkflowDefinition</param>
        void UpdateWorkflowDefinition(WfWorkflowDefinition wfWorkflowDefinition);

        /// <summary>
        /// Add an activity to the workflow definition.
        /// </summary>
        /// <param name="wfWorkflowDefinition">the workflow definition.</param>
        /// <param name="wfActivityDefinition">wfActivityDefinition.</param>
        /// <param name="position">position</param>
        void CreateActivityDefinition(WfWorkflowDefinition wfWorkflowDefinition, WfActivityDefinition wfActivityDefinition);

        /// <summary>
        /// Remove an activity to the workflow definition.
        /// </summary>
        /// <param name="wfWorkflowDefinition">the workflow definition.</param>
        /// <param name="wfActivityDefinition">the activity to remove.</param>
        void DeleteActivityDefinition(WfActivityDefinition wfActivityDefinition);

        /// <summary>
        /// Rename an activity definition.
        /// </summary>
        /// <param name="wfActivityDefinition">the activity definition to rename</param>
        void RenameActivityDefinition(WfActivityDefinition wfActivityDefinition);

        /// <summary>
        /// Fetch an activity definition by id.
        /// </summary>
        /// <param name="wfadId">the workflow definition.</param>
        /// <returns>the corresponding activity definition.</returns>
        WfActivityDefinition ReadActivityDefinition(int wfadId);

        /// <summary>
        /// Update an activity definition.
        /// </summary>
        /// <param name="wfWorkflowDefinition">the workflow definition.</param>
        /// <param name="wfWorkflowDefinition">the activity to update.</param>
        void UpdateActivityDefinition(WfActivityDefinition wfActivityDefinition);

        /// <summary>
        /// Find an activity by its positon in the default transition chain.
        /// </summary>
        /// <param name="wfWorkflowDefinition">wfWorkflowDefinition.</param>
        /// <param name="position">position.</param>
        WfActivityDefinition FindActivityDefinitionByPosition(WfWorkflowDefinition wfWorkflowDefinition, int position);

        /// <summary>
        /// Find an activity by its definition for a workflow
        /// </summary>
        /// <param name="wfWorkflow"></param>
        /// <param name="wfActivityDefinition"></param>
        /// <returns></returns>
        WfActivity FindActivityByDefinitionWorkflow(WfWorkflow wfWorkflow, WfActivityDefinition wfActivityDefinition);

        /// <summary>
        /// Find the list of all the definitions following the default transitions.
        /// </summary>
        /// <param name="wfWorkflowDefinition">wfWorkflowDefinition.</param>
        /// <param name="startingPos">startingPos.</param>
        /// <returns>the list of transitions ordered from start to end</returns>
        IList<WfActivityDefinition> FindAllDefaultActivityDefinitions(WfWorkflowDefinition wfWorkflowDefinition, int startingPos = 0);

        /// <summary>
        /// Add a transition.
        /// </summary>
        /// <param name="transition">transition.</param>
        void AddTransition(WfTransitionDefinition transition);

        /// <summary>
        /// Remove a transition.
        /// </summary>
        /// <param name="transition">transition.</param>
        void RemoveTransition(WfTransitionDefinition transition);

        /// <summary>
        /// Update a transition
        /// </summary>
        /// <param name="transition">transition</param>
        void UpdateTransition(WfTransitionDefinition transition);

        /// <summary>
        /// Find a transition by criteria
        /// </summary>
        /// <param name="wfTransitionCriteria">criteria</param>
        /// <returns></returns>
        WfTransitionDefinition FindTransition(WfTransitionCriteria wfTransitionCriteria);

        /// <summary>
        /// Find the next activity using the default transition
        /// </summary>
        /// <param name="wfadId">wfadId</param>
        /// <returns>the next activity definition</returns>
        WfActivityDefinition FindNextActivity(int wfadId);

        /// <summary>
        /// Find the next activity using the provided transition name.
        /// </summary>
        /// <param name="activity">activity</param>
        /// <param name="transitionName">transitionName</param>
        /// <returns>the next activity definition</returns>
        WfActivityDefinition FindNextActivity(int wfadId, String transitionName);

        /// <summary>
        /// Find all activities for a workflow definition.
        /// This method must be only used for the workflow recalculation
        /// </summary>
        /// <param name="wfWorkflowDefinition">wfWorkflowDefinition</param>
        /// <returns></returns>
        IList<WfActivity> FindAllActivitiesByWorkflowDefinitionId(WfWorkflowDefinition wfWorkflowDefinition);

        /// <summary>
        /// Find all decisions for a workflow definition.
        /// This method must be only used for the workflow recalculation
        /// </summary>
        /// <param name="wfWorkflowDefinition">wfWorkflowDefinition</param>
        /// <returns></returns>
        IList<WfDecision> FindAllDecisionsByWorkflowDefinitionId(WfWorkflowDefinition wfWorkflowDefinition);

        /// <summary>
        /// Find all activities for a workflow
        /// </summary>
        /// <param name="wfWorkflow"></param>
        /// <returns></returns>
        IList<WfActivity> FindActivitiesByWorkflowId(WfWorkflow wfWorkflow);

        /// <summary>
        /// Find All Decisions for a workflow
        /// </summary>
        /// <param name="wfWorkflow"></param>
        /// <returns></returns>
        IList<WfDecision> FindDecisionsByWorkflowId(WfWorkflow wfWorkflow);

        /// <summary>
        /// Reset (set to null) the current activity (wfaid2) of all worklow for the activity linked to the provided activityDefinition
        /// </summary>
        /// <param name="wfActivityDefinition"></param>
        void UnsetCurrentActivity(WfActivityDefinition wfActivityDefinition);


        #region Direct Acces To Rules and Selectors
        /// <summary>
        /// Find all the rules for a workflow definition
        /// </summary>
        /// <param name="wfwdId">Workflow Definition Id</param>
        /// <returns>a list of rules linked to the workflow definition</returns>
        IList<RuleDefinition> FindAllRulesByWorkflowDefinitionId(int wfwdId);

        /// <summary>
        /// Find all the conditions for a workflow definition
        /// </summary>
        /// <param name="wfwdId">Workflow Definition Id</param>
        /// <returns>a list of conditions linked to the workflow definition</returns>
        IList<RuleConditionDefinition> FindAllConditionsByWorkflowDefinitionId(int wfwdId);

        /// <summary>
        /// Find all the selectors for a workflow definition
        /// </summary>
        /// <param name="wfwdId">Workflow Definition Id</param>
        /// <returns>a list of selectors linked to the workflow definition</returns>
        IList<SelectorDefinition> FindAllSelectorsByWorkflowDefinitionId(int wfwdId);

        /// <summary>
        /// Find all the filters for a workflow definition
        /// </summary>
        /// <param name="wfwdId">Workflow Definition Id</param>
        /// <returns>a list of selectors linked to the workflow definition</returns>
        IList<RuleFilterDefinition> FindAllFiltersByWorkflowDefinitionId(int wfwdId);
        #endregion


        #region Batch Updates / Insert for recalculation
        
        /// <summary>
        /// Update current activity (wfaId2) for the provided workflows
        /// </summary>
        /// <param name="worfklows">worklow to update</param>
        void UpdateWorkflowCurrentActivities(ICollection<WfWorkflowUpdate> worfklows);

        /// <summary>
        /// Update IsAuto flag for the provided activities
        /// </summary>
        /// <param name="worfklows">Activities to update</param>
        void UpdateActivitiesIsAuto(ICollection<WfActivityUpdate> activities);

        /// <summary>
        /// Create activities and define them as the current activity for the linked workflow instance for the provided activitie
        /// </summary>
        /// <param name="worfklows">Activities to update</param>
        void CreateActiviesAndUpdateWorkflowCurrentActivities(ICollection<WfActivity> activities);

        /// <summary>
        /// Create new activities
        /// </summary>
        /// <param name="activities">Activities to create</param>
        void CreateActivies(ICollection<WfActivity> activities);

        /// <summary>
        /// Create new Activities and Decisions
        /// </summary>
        /// <param name="activities"></param>
        void CreateActivityDecision(ICollection<WfActivityDecision> activities);

        #endregion
    }
}
