using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kinetix.ComponentModel;

namespace Kinetix.Rules
{
    /// <summary>
    /// This class defines the Definition of rule condition.
    /// </summary>
    /// 
    [Table("AUDIT_TRACE")]
    public class RuleConditionDefinition
    {
        [Column("ID")]
        [Domain("DO_X_RULES_ID")]
        [Key]
        public long? Id
        {
            get;
            set;
        }

        [Column("FIELD")]
        [Domain("DO_X_RULES_FIELD")]
        public string Field
        {
            get;
        }

        [Column("OPERATOR")]
        [Domain("DO_X_RULES_OPERATOR")]
        public string Operator
        {
            get;
        }

        [Column("EXPRESSION")]
        [Domain("DO_X_RULES_EXPRESSION")]
        public string Expression
        {
            get;
        }

        [Column("RUD_ID")]
        [Domain("DO_X_RULES_RULES_ID")]
        public long? RudId
        {
            get;
            set;
        }

    }
}

