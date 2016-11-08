using Kinetix.Workflow.instance;
using Kinetix.Workflow.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow.Impl.Workflow
{
    public class RecalculationOutput
    {

        public IList<WfWorkflowUpdate> WorkflowsUpdateCurrentActivity { get; set; } = new List<WfWorkflowUpdate>();
        public IList<WfActivityUpdate> ActivitiesUpdateIsAuto { get; set; } = new List<WfActivityUpdate>();
        public IList<WfActivity> ActivitiesCreateUpdateCurrentActivity { get; set; } = new List<WfActivity>();
        
        //public IList<WfActivityInsert> ActivitiesCreate { get; set; } = new List<WfActivityInsert>();
        public IList<WfActivity> ActivitiesCreate { get; set; } = new List<WfActivity>();


        public void AddWorkflowsUpdateCurrentActivity(WfWorkflow wf) {
            WorkflowsUpdateCurrentActivity.Add(new WfWorkflowUpdate() { WfwId = wf.WfwId, WfaId2 = wf.WfaId2 });
        }

        public void AddActivitiesUpdateIsAuto(WfActivity wfAct)
        {
            
            ActivitiesUpdateIsAuto.Add(new WfActivityUpdate() { WfaId = wfAct.WfaId, IsAuto = wfAct.IsAuto });
        }

        public void AddActivitiesCreate(WfActivity wfAct)
        {
            //ActivitiesCreate.Add(new WfActivityInsert() { IsAuto = wfAct.IsAuto, CreationDate = wfAct.CreationDate, WfwId = wfAct.WfwId, WfadId = wfAct.WfadId });
            ActivitiesCreate.Add(wfAct);
        }

        public void AddActivitiesCreateUpdateCurrentActivity(WfActivity wfAct)
        {
            //ActivitiesCreateUpdateCurrentActivity.Add(new WfActivityInsert() { IsAuto = wfAct.IsAuto, CreationDate = wfAct.CreationDate, WfwId = wfAct.WfwId, WfadId = wfAct.WfadId });
            ActivitiesCreateUpdateCurrentActivity.Add(wfAct);
        }

    }
}
