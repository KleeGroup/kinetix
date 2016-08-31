using Kinetix.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Account
{
    [Table("ACCOUNT")]
    public class AccountUser
    {

        [Column("ID")]
        [Domain("DO_X_ACCOUNT_ID")]
        [Key]
        public string Id
        {
            get;
            set;
        }

        [Column("NAME")]
        [Domain("DO_X_ACCOUNT_NAME")]
        public string Name
        {
            get;
        }

        [Column("EMAIL")]
        [Domain("DO_X_ACCOUNT_EMAIL")]
        public string Email
        {
            get;
        }

    }
}
