using Kinetix.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow.model
{

    /// <summary>
    /// This class defines the Activity Definition.
    /// </summary>
    /// 
    [Table("WF_ACTIVITY_DEFINITION")]
    public class WfActivityDefinition
    {
        [Column("WFAD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public long? WfadId
        {
            get;
            set;
        }

        [Column("NAME")]
        [Domain("DO_X_WORKFLOW_LABEL")]
        public string Name
        {
            get;
            set;
        }

        [Column("LEVEL")]
        [Domain("DO_X_WORKFLOW_LEVEL")]
        public int Level
        {
            get;
            set;
        }

        [Column("WFMD_CODE")]
        [Domain("DO_X_WORKFLOW_CODE")]
        public string WfmdCode
        {
            get;
            set;
        }

        [Column("WFWD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public long WfwdId
        {
            get;
            set;
        }

    }
}
