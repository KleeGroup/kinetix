using Kinetix.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow.instance
{

    /// <summary>
    /// This class defines the Workflow Instance.
    /// </summary>
    /// 
    [Table("WF_WORKFLOW")]
    public class WfWorkflow
    {
        [Column("WFW_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public long? WfwId
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

        [Column("ITEM_ID")]
        [Domain("DO_X_WORKFLOW_WEAK_ID")]
        public long? ItemId
        {
            get;
            set;
        }

        [Column("USER")]
        [Domain("DO_X_WORKFLOW_USER")]
        public string User
        {
            get;
            set;
        }

        [Column("USER_LOGIC")]
        [Domain("DO_X_WORKFLOW_FLAG")]
        public bool UserLogic
        {
            get;
            set;
        }

        [Column("WFWD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public long? WfwdId
        {
            get;
            set;
        }


        [Column("WFWD_ID")]
        [Domain("DO_X_WORKFLOW_CODE")]
        public string WfsCode
        {
            get;
            set;
        }

        [Column("WFA_CURRENT")]
        [Domain("DO_X_WORKFLOW_CODE")]
        public long? WfaId2
        {
            get;
            set;
        }

        
    }
}
