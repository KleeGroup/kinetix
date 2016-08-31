using Kinetix.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kinetix.Rules
{
    /// <summary>
    /// This class defines the Definition of a rule filter.
    /// </summary>
    /// 
    public class RuleFilterDefinition
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

        [Column("SEL_ID")]
        [Domain("DO_X_RULES_ID")]
        public long? SelId
        {
            get;
            set;
        }

    }
}
