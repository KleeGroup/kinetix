
using Kinetix.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kinetix.Workflow.model
{
    /// <summary>
    /// This class defines the Workflow Definition.
    /// </summary>
    /// 
    [Table("WF_WORKFLOW_DEFINITION")]
    public class WfWorkflowDefinition
    {
        [Column("WFWD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public long? WfwdId
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

        [Column("CREATION_DATE")]
        [Domain("DO_X_WORKFLOW_DATE")]
        public string CreationDate
        {
            get;
            set;
        }

        [Column("WFADID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public long? WfadId
        {
            get;
            set;
        }

       
    }
}
