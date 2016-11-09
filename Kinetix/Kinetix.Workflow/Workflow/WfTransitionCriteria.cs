
namespace Kinetix.Workflow
{
    public class WfTransitionCriteria
    {
        public string TransitionName { get; set; }

        public int? WfadIdFrom { get; set; }

        public int? WfadIdTo { get; set; }
    }
}
