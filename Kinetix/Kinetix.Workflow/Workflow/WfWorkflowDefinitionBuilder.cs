using Kinetix.Workflow.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow.Workflow
{
    public class WfWorkflowDefinitionBuilder
    {


        private string myName;
        private DateTime myCreationDate;
        private int? myWfadId;

        /// <summary>
        /// Builder for Workflow Definition.
        /// </summary>
        /// <param name="name">name.</param>
        public WfWorkflowDefinitionBuilder(string name)
        {
            Debug.Assert(name != null);
            //---
            myName = name;
            myCreationDate = DateTime.Now;
        }

        /// <summary>
        /// Add First Activity.
        /// </summary>
        /// <param name="wfadId">the Id of the first activity definition.</param>
        /// <returns>The Builder.</returns>
        public WfWorkflowDefinitionBuilder WithFirstActivityDefinitionId(int wfadId)
        {
            myWfadId = wfadId;
            return this;
        }

        public WfWorkflowDefinition Build()
        {
            WfWorkflowDefinition wfTransitionDefinition = new WfWorkflowDefinition();
            wfTransitionDefinition.WfadId = myWfadId;
            wfTransitionDefinition.CreationDate = myCreationDate;
            wfTransitionDefinition.Name = myName;
            return wfTransitionDefinition;
        }

    }

}