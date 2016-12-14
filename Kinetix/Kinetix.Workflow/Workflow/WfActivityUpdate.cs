using Kinetix.ComponentModel;
using System.CodeDom.Compiler;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Kinetix.Workflow
{

    [Table("WF_ACTIVITY_UPDATE")]
    public class WfActivityUpdate
    {

        [Column("WFA_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int? WfaId { get; set; }

        [Column("IS_AUTO")]
        [Domain("DO_X_WORKFLOW_FLAG")]
        public bool IsAuto { get; set; }

        [Column("IS_VALID")]
        [Domain("DO_X_WORKFLOW_FLAG")]
        public bool IsValid { get; set; }

        [Column("WFW_INSERT_KEY")]
        [Domain("DO_X_WORKFLOW_ID")]
        [DataMember]
        [GeneratedCode("InsertKey", "")]
        public int? InsertKey { get; set; }

    }
}