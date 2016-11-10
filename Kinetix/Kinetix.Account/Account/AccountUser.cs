using Kinetix.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Kinetix.Account
{
    [Table("ACCOUNT")]
    public class AccountUser
    {

        /// <summary>
        /// AccountUser construcutor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        public AccountUser(string id, string name, string email)
        {
            this.Id = id;
            this.Name = name;
            this.Email = email;
        }

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
