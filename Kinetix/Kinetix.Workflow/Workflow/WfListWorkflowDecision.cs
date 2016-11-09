using Kinetix.Workflow.instance;
using System.Collections.Generic;


namespace Kinetix.Workflow
{
    public sealed class WfListWorkflowDecision
    {
        public WfWorkflow WfWorkflow { get; set; }

        public IList<WfWorkflowDecision> WorkflowDecisions { get; set; } = new List<WfWorkflowDecision>();
    }

}
