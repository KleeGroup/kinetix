using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow.Workflow
{
    public class WfTransitionCriteria
    {
        public string TransitionName { get; set; }

        public int? WfadIdFrom { get; set; }

        public int? WfadIdTo { get; set; }
    }
}
