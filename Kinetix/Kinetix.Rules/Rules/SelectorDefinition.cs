using Kinetix.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Kinetix.Rules
{
    public class SelectorDefinition
    {

        [Column("ID")]
        [Domain("DO_X_RULES_ID")]
        [Key]
        public long? Id
        {
            get;
            set;
        }

        [Column("CREATION_DATE")]
        [Domain("DO_X_RULES_DATE")]
        public string CreationDate
        {
            get;
        }

        [Column("ITEM_ID")]
        [Domain("DO_X_RULES_WEAK_ID")]
        public long? ItemId
        {
            get;
            set;
        }

        [Column("GROUP_ID")]
        [Domain("DO_X_RULES_WEAK_ID")]
        public string GroupId
        {
            get;
        }

    }
}
