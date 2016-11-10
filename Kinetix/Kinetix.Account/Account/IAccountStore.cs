using System.Collections.Generic;

namespace Kinetix.Account
{
    public interface IAccountStore
    {

        /// <summary>
        /// Get the numbers of accounts.
        /// </summary>
        /// <returns>The numbers of accounts.</returns>
        long GetAccountsCount();

        /// <summary>
        /// Get one account.
        /// </summary>
        /// <param name="accountId">the account defined by its Id.</param>
        /// <returns>the account.</returns>
        AccountUser GetAccount(string accountId);

        /// <summary>
        /// Get the groups attached to the account.
        /// </summary>
        /// <param name="accountId">the account defined by its Id.</param>
        /// <returns>the account.</returns>
        ISet<string> GetGroupIds(string accountId);

        /// <summary>
        /// Saves a collection of accounts.
        /// Caution : all the accounts must have an id.
        /// </summary>
        /// <param name="accounts">accounts the list of accounts.</param>
        void SaveAccounts(IList<AccountUser> accounts);

        /// <summary>
        /// Get the numbers of groups.
        /// </summary>
        /// <returns>the number of groups.</returns>
        long GetGroupsCount();

        /// <summary>
        /// Lists all the groups.
        /// </summary>
        /// <returns>all the groups.</returns>
        ICollection<AccountGroup> GetAllGroups();

        /// <summary>
        /// Gets the group defined by an URI.
        /// </summary>
        /// <param name="groupId">the account defined by its Id.</param>
        /// <returns>the group.</returns>
        AccountGroup GetGroup(string groupId);

        /// <summary>
        /// Lists the accounts for a defined group.
        /// </summary>
        /// <param name="groupId">the group URI.</param>
        /// <returns>the list of acccounts.</returns>
        ISet<string> GetAccountIds(string groupId);

        /// <summary>
        /// Gets the photo of an account defined by its Id.
        /// </summary>
        /// <param name="accountId">the account defined by its Id.</param>
        /// <returns>the photo.</returns>
        byte[] GetPhoto(string accountId);

        #region write

        /// <summary>
        /// Reset:
        /// - All the accounts
        /// - All the groups
        /// - All the links accounts-group
        /// - All the Photos
        /// </summary>
        void Reset();

        /// <summary>
        /// Saves a group.
        /// </summary>
        /// <param name="group">the group.</param>
        void SaveGroup(AccountGroup group);

        /// <summary>
        /// Attaches an account to a group.
        /// </summary>
        /// <param name="accountId">the account defined by its Id.</param>
        /// <param name="groupId">the group Id.</param>
        void Attach(string accountId, string groupId);

        /// <summary>
        /// Defines a photo to an account.
        /// </summary>
        /// <param name="accountId">the account defined by its Id.</param>
        /// <param name="photo">the photo.</param>
        void SetPhoto(string accountId, byte[] photo);

        #endregion

    }
}
