using Kinetix.Workflow.instance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow
{
    public class WfMassValidation
    {

        public IList<WfActivityDecision> ActivitiesDecisions { get; set; } = new List<WfActivityDecision>();


        public void AddActivityDecision(WfActivity wfAct, WfDecision wfDecision)
        {
            ActivitiesDecisions.Add(new WfActivityDecision() { Activity = wfAct, Decision = wfDecision });
        }
    }
}
