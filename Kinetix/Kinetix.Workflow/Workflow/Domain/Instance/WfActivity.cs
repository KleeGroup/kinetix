using Kinetix.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Kinetix.Workflow
{
    /// <summary>
    /// This class defines the Activity.
    /// </summary>
    /// 
    [Table("WF_ACTIVITY")]
    public class WfActivity
    {
        [Column("WFA_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public long? WfaId
        {
            get;
            set;
        }

        [Column("CREATION_DATE")]
        [Domain("DO_X_WORKFLOW_DATE")]
        public DateTime? CreationDate
        {
            get;
            set;
        }

        [Column("WFW_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public long WfwId
        {
            get;
            set;
        }

        [Column("WFAD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public long WfadId
        {
            get;
            set;
        }

    }

}
