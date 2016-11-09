using Kinetix.Workflow.model;
using System;
using System.Diagnostics;

namespace Kinetix.Workflow
{
    public class WfWorkflowDefinitionBuilder
    {

        private string MyName;
        private DateTime MyCreationDate;
        private int? MyWfadId;

        /// <summary>
        /// Builder for Workflow Definition.
        /// </summary>
        /// <param name="name">name.</param>
        public WfWorkflowDefinitionBuilder(string name)
        {
            Debug.Assert(name != null);
            //---
            MyName = name;
            MyCreationDate = DateTime.Now;
        }

        /// <summary>
        /// Add First Activity.
        /// </summary>
        /// <param name="wfadId">the Id of the first activity definition.</param>
        /// <returns>The Builder.</returns>
        public WfWorkflowDefinitionBuilder WithFirstActivityDefinitionId(int wfadId)
        {
            MyWfadId = wfadId;
            return this;
        }

        public WfWorkflowDefinition Build()
        {
            WfWorkflowDefinition wfTransitionDefinition = new WfWorkflowDefinition();
            wfTransitionDefinition.WfadId = MyWfadId;
            wfTransitionDefinition.CreationDate = MyCreationDate;
            wfTransitionDefinition.Name = MyName;
            return wfTransitionDefinition;
        }

    }

}