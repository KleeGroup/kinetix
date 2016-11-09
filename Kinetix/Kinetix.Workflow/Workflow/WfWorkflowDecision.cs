using Kinetix.Account;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System.Collections.Generic;

namespace Kinetix.Workflow
{
    public sealed class WfWorkflowDecision
    {

        public WfActivity Activity { get; set; }
        public WfActivityDefinition ActivityDefinition { get; set; }
        public IList<WfDecision> Decisions { get; set; }
        public IList<AccountGroup> Groups { get; set; }
    }
}
