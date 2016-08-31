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
    /// This class defines the Decision.
    /// </summary>
    /// 
    [Table("WF_DECISION")]
    public class WfDecision
    {

        [Column("WFE_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public int? Id
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

        [Column("CHOICE")]
        [Domain("DO_X_WORKFLOW_CHOICE")]
        public int? Choice
        {
            get;
            set;
        }

        [Column("DECISION_DATE")]
        [Domain("DO_X_WORKFLOW_DATE")]
        public DateTime? DecisionDate
        {
            get;
            set;
        }

        [Column("COMMENTS")]
        [Domain("DO_X_WORKFLOW_COMMENTS")]
        public string Comments
        {
            get;
            set;
        }


        [Column("WFA_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public long WfaId
        {
            get;
            set;
        }

        [Column("WFE_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public long? WfeId
        {
            get;
            set;
        }



    }
}
