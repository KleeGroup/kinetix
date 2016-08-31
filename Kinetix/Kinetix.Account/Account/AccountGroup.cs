using Kinetix.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kinetix.Account
{
    public class AccountGroup
    {

        [Column("ID")]
        [Domain("DO_X_ACCOUNT_ID")]
        [Key]
        public string Id {
            get;
        }

        [Domain("DO_X_ACCOUNT_NAME")]
        public string displayName {
            get;
        }

        [Domain("DO_X_ACCOUNT_EMAIL")]
        public string email
        {
            get;
        }

    }
}
