using Kinetix.Account;
using Kinetix.Workflow.instance;
using Kinetix.Workflow.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow.Workflow
{
    public sealed class WfWorkflowDecision
    {

        public WfActivity activity { get; set; }
        public WfActivityDefinition activityDefinition { get; set; }
        public IList<WfDecision> decisions { get; set; }
        public IList<AccountGroup> groups { get; set; }
    }
}
