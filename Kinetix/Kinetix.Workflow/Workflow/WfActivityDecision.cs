﻿using Kinetix.Workflow.instance;

namespace Kinetix.Workflow.Workflow
{
    public class WfActivityDecision
    {

        public int WfadId { get; set; }

        public WfDecision Decision { get; set; }

        public WfActivity Activity { get; set; }

    }
}
