using Kinetix.ComponentModel;
using System.CodeDom.Compiler;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Kinetix.Workflow
{

    [Table("WF_WORKFLOW_UPDATE")]
    public class WfWorkflowUpdate
    {

        [Column("WFW_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int? WfwId { get; set; }

        [Column("WFW_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int? WfaId2 { get; set; }

        [Column("WFW_INSERT_KEY")]
        [Domain("DO_X_WORKFLOW_ID")]
        [DataMember]
        [GeneratedCode("InsertKey", "")]
        public int? InsertKey { get; set; }

    }
}