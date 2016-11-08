using Kinetix.Workflow.instance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow.Workflow
{
    public sealed class WfListWorkflowDecision
    {
        public WfWorkflow wfWorkflow { get; set; }

        public IList<WfWorkflowDecision> workflowDecisions { get; set; } = new List<WfWorkflowDecision>();
    }

}
