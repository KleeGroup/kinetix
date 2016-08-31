using Kinetix.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Kinetix.Workflow.model
{
    /// <summary>
    /// This class defines the Transition Definition.
    /// </summary>
    /// 
    [Table("WF_TRANSITION_DEFINITION")]
    public class WfTransitionDefinition
    {
        [Column("WFTD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public int? Id
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

        [Column("WFAID_FROM")]
        [Domain("DO_X_WORKFLOW_ID")]
        public long WfadIdFrom
        {
            get;
            set;
        }

        [Column("WFAID_TO")]
        [Domain("DO_X_WORKFLOW_ID")]
        public long WfadIdTo
        {
            get;
            set;
        }
    }
}
