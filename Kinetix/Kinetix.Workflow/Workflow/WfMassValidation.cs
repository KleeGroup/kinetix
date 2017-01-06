using Kinetix.Workflow.instance;
using System.Collections.Generic;

namespace Kinetix.Workflow.Workflow
{
    public class WfMassValidation
    {

        public IList<WfActivityDecision> ActivitiesDecisionsInsert { get; set; } = new List<WfActivityDecision>();

        public IList<WfActivityUpdate> ActivitiesUpdate { get; set; } = new List<WfActivityUpdate>();

        public IList<WfDecision> DecisionsInsert { get; set; } = new List<WfDecision>();

        public void AddActivityDecisionInsert(WfActivity wfAct, WfDecision wfDecision)
        {
            ActivitiesDecisionsInsert.Add(new WfActivityDecision() { Activity = wfAct, Decision = wfDecision, WfadId = wfAct.WfadId });
        }

        public void AddActivitiesUpdate(WfActivity wfAct)
        {
            ActivitiesUpdate.Add(new WfActivityUpdate() { WfaId = wfAct.WfaId, IsAuto = wfAct.IsAuto, IsValid = wfAct.IsValid});
        }

        public void AddDecisionsInsert(WfDecision wfDecision)
        {
            DecisionsInsert.Add(wfDecision);
        }

    }
}
