using Kinetix.Workflow.instance;
using System.Collections.Generic;


namespace Kinetix.Workflow
{
    public class WfRecalculationOutput
    {

        public IDictionary<int, WfWorkflowUpdate> WorkflowsUpdateCurrentActivity { get; set; } = new Dictionary<int, WfWorkflowUpdate>();
        public IList<WfActivityUpdate> ActivitiesUpdateIsAuto { get; set; } = new List<WfActivityUpdate>();
        public IList<WfActivity> ActivitiesCreateUpdateCurrentActivity { get; set; } = new List<WfActivity>();
        public IList<WfActivity> ActivitiesCreate { get; set; } = new List<WfActivity>();

        public IList<WfListWorkflowDecision> WfListWorkflowDecision { get; set; } = new List<WfListWorkflowDecision>();

        public void AddWorkflowsUpdateCurrentActivity(WfWorkflow wf) {
            WorkflowsUpdateCurrentActivity[wf.WfwId.Value] = new WfWorkflowUpdate() { WfwId = wf.WfwId, WfaId2 = wf.WfaId2 };
        }

        public void AddActivitiesUpdateIsAuto(WfActivity wfAct)
        {
            ActivitiesUpdateIsAuto.Add(new WfActivityUpdate() { WfaId = wfAct.WfaId, IsAuto = wfAct.IsAuto, IsValid = wfAct.IsValid });
        }

        public void AddActivitiesCreate(WfActivity wfAct)
        {
            ActivitiesCreate.Add(wfAct);
        }

        public void AddActivitiesCreateUpdateCurrentActivity(WfActivity wfAct)
        {
            ActivitiesCreateUpdateCurrentActivity.Add(wfAct);
        }

        public void AddWfListWorkflowDecision(WfListWorkflowDecision wfListWorkflowDecision)
        {
            WfListWorkflowDecision.Add(wfListWorkflowDecision);
        }
    }
}
