using System.Diagnostics;

namespace Kinetix.Account.Account
{
    public class AccountUserBuilder
    {
        private string myId;
	    private string myDisplayName;
        private string myEmail;

        /// <summary>
        /// Builder for Account User.
        /// </summary>
        /// <param name="id">Identifiant.</param>
        public AccountUserBuilder(string id)
        {
            Debug.Assert(id != null);
            //---
            this.myId = id;
        }


        /// <summary>
        /// Optionnal displayName.
        /// </summary>
        /// <param name="displayName">the displayName of user.</param>
        /// <returns>The Builder.</returns>
        public AccountUserBuilder WithDisplayName(string displayName)
        {
            myDisplayName = displayName;
            return this;
        }

        /// <summary>
        /// Optionnal displayName.
        /// </summary>
        /// <param name="email">the email of user.</param>
        /// <returns>The Builder.</returns>
        public AccountUserBuilder WithEmail(string email)
        {
            myEmail = email;
            return this;
        }

        /// <summary>
        /// Build a new AccountUser
        /// </summary>
        /// <returns></returns>
        public AccountUser Build()
        {
            return new AccountUser(myId, myDisplayName, myEmail);
        }

    }
}
