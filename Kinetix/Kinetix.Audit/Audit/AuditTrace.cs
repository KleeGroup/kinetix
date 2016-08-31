using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kinetix.ComponentModel;

namespace Kinetix.Audit
{

    /// <summary>
    /// This class defines the Auditing Trace for an Object.
    /// </summary>
    /// 
    [Table("AUDIT_TRACE")]
    public class AuditTrace 
    {
        [Column("ID")]
        [Domain("DO_X_AUDIT_ID")]
        [Key]
        public int? Id{
            get;
            set;
        }

        [Column("CATEGORY")]
        [Domain("DO_X_AUDIT_CATEGORY")]
        public string Category {
            get;
        }

        [Column("USER")]
        [Domain("DO_X_AUDIT_USER")]
        public string User {
            get;
        }

        [Column("BUSINESS_DATE")]
        [Domain("DO_X_AUDIT_DATE")]
        public DateTime? BusinessDate {
            get;
        }

        [Column("EXECUTION_DATE")]
        [Domain("DO_X_AUDIT_DATE")]
        public DateTime? ExecutionDate {
            get;
        }

        [Column("ITEM")]
        [Domain("DO_X_AUDIT_ITEM")]
        public long? Item {
            get;
        }

        [Column("message")]
        [Domain("DO_X_AUDIT_MESSAGE")]
        private string Message {
            get;
        }

        [Column("CONTEXT")]
        [Domain("DO_X_AUDIT_CONTEXT")]
        public string Context {
            get;
        }

    }

}

