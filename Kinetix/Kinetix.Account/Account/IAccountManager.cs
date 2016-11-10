using Kinetix.ComponentModel;

namespace Kinetix.Account
{
    public interface IAccountManager
    {

        /// <summary>
        /// Log the account.
        /// </summary>
        /// <param name="accountId">account to be logged.</param>
        void Login(string accountId);

        /// <summary>
        /// Get the logged account.
        /// </summary>
        /// <returns>logged account.</returns>
        string GetLoggedAccount();

        /// <summary>
        /// Gets the default photo of an account.
        /// </summary>
        /// <returns>the photo as a file.</returns>
        DownloadedFile GetDefaultPhoto();

        /// <summary>
        /// Get the store of account.
        /// </summary>
        /// <returns>store of account.</returns>
        IAccountStore GetStore();
    }
}
