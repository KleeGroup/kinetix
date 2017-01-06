using Kinetix.Workflow.instance;

namespace Kinetix.Workflow.Workflow
{
    public class WfActivityUpdateDecision
    {

        public int WfadId { get; set; }

        public WfDecision Decision { get; set; }

        public WfActivityUpdate ActivityUpdate { get; set; }

    }
}
