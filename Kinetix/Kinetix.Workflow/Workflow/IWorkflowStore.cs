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
        /// Find the next activity using the default transition
        /// </summary>
        /// <param name="activity">activity</param>
        /// <returns>the next activity definition</returns>
        WfActivityDefinition FindNextActivity(WfActivity activity);

        /// <summary>
        /// Find the next activity using the provided transition name.
        /// </summary>
        /// <param name="activity">activity</param>
        /// <param name="transitionName">transitionName</param>
        /// <returns>the next activity definition</returns>
        WfActivityDefinition FindNextActivity(WfActivity activity, String transitionName);

        /// <summary>
        /// Find the next activity using the provided transition name.
        /// </summary>
        /// <returns>the list of transitions ordered from start to end.</returns>
        IList<WfActivityDefinition> FindActivityMatchingRules();

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
        /// <returns>the list of transitions ordered from start to end</returns>
        IList<WfActivityDefinition> FindAllDefaultActivityDefinitions(WfWorkflowDefinition wfWorkflowDefinition);

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

    }
}
