using Kinetix.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kinetix.Account
{
    [Table("ACCOUNT_GROUP")]
    public class AccountGroup
    {

        /// <summary>
        /// AccountUser constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public AccountGroup(string id, string displayName)
        {
            this.Id = id;
            this.DisplayName = displayName;
        }

        [Column("ID")]
        [Domain("DO_X_ACCOUNT_ID")]
        [Key]
        public string Id {
            get;
            set;
        }

        [Domain("DO_X_ACCOUNT_NAME")]
        public string DisplayName {
            get;
        }


    }
}
