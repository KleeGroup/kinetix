using System.Diagnostics;


namespace Kinetix.Account.Account
{
    public class AccountGroupBuilder
    {
        private string myId;
        private string myDisplayName;

        /// <summary>
        /// Builder for Account Group.
        /// </summary>
        /// <param name="id">Identifiant.</param>
        public AccountGroupBuilder(string id)
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
        public AccountGroupBuilder WithDisplayName(string displayName)
        {
            myDisplayName = displayName;
            return this;
        }

   
        /// <summary>
        /// Build a new AccountUser
        /// </summary>
        /// <returns></returns>
        public AccountGroup Build()
        {
            return new AccountGroup(myId, myDisplayName);
        }
    }
}
