using Kinetix.Workflow.instance;
using System.Collections.Generic;

namespace Kinetix.Workflow.Workflow
{
    public class WfMassValidation
    {

        public IList<WfDecisionActivityInsert> ActivitiesDecisionsInsert { get; set; } = new List<WfDecisionActivityInsert>();

        public IList<WfActivityUpdate> ActivitiesUpdate { get; set; } = new List<WfActivityUpdate>();

        public IList<WfDecision> DecisionsInsert { get; set; } = new List<WfDecision>();

        public void AddActivitiesDecisionsInsert(WfActivity wfAct, WfDecision wfDecision)
        {
            ActivitiesDecisionsInsert.Add(new WfDecisionActivityInsert() {Decision = wfDecision, Activity = wfAct });
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
