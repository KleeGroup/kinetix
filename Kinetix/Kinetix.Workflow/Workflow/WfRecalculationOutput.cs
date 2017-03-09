using Kinetix.Workflow.instance;
using System.Collections.Generic;


namespace Kinetix.Workflow
{
    public class WfRecalculationOutput
    {

        public IDictionary<int, WfWorkflowUpdate> WorkflowsUpdateCurrentActivity { get; set; } = new Dictionary<int, WfWorkflowUpdate>();
        public IDictionary<int, WfActivityUpdate> ActivitiesUpdateIsAuto { get; set; } = new Dictionary<int, WfActivityUpdate>();
        public IDictionary<string, WfActivity> ActivitiesCreateUpdateCurrentActivity { get; set; } = new Dictionary<string, WfActivity>();
        public IDictionary<string, WfActivity> ActivitiesCreate { get; set; } = new Dictionary<string, WfActivity>();

        public IList<WfListWorkflowDecision> WfListWorkflowDecision { get; set; } = new List<WfListWorkflowDecision>();

        public void AddWorkflowsUpdateCurrentActivity(WfWorkflow wf) {
            WorkflowsUpdateCurrentActivity[wf.WfwId.Value] = new WfWorkflowUpdate() { WfwId = wf.WfwId, WfaId2 = wf.WfaId2 };
        }

        public void AddActivitiesUpdateIsAuto(WfActivity wfAct)
        {
            ActivitiesUpdateIsAuto[wfAct.WfaId.Value] = new WfActivityUpdate() { WfaId = wfAct.WfaId, IsAuto = wfAct.IsAuto, IsValid = wfAct.IsValid };
        }

        public void AddActivitiesCreate(WfActivity wfAct)
        {
            ActivitiesCreate[wfAct.WfadId+"|"+wfAct.WfwId] = wfAct;
        }

        public void AddActivitiesCreateUpdateCurrentActivity(WfActivity wfAct)
        {
            ActivitiesCreateUpdateCurrentActivity[wfAct.WfadId+"|"+wfAct.WfwId] = wfAct;
        }

        public void AddWfListWorkflowDecision(WfListWorkflowDecision wfListWorkflowDecision)
        {
            WfListWorkflowDecision.Add(wfListWorkflowDecision);
        }
    }
}
