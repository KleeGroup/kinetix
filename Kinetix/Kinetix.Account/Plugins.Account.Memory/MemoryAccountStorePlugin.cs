using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Kinetix.Account
{
    public class MemoryAccountStorePlugin : IAccountStorePlugin
    {

        private readonly IDictionary<string, AccountUser> AccountById = new Dictionary<string, AccountUser>();
        private readonly IDictionary<string, AccountGroup> GroupById = new Dictionary<string, AccountGroup>();
        private readonly IDictionary<string, HashSet<string>> GroupByAccountId = new Dictionary<string, HashSet<string>>();
        private readonly IDictionary<string, HashSet<string>> AccountByGroupID = new Dictionary<string, HashSet<string>>();
        private readonly IDictionary<string, byte[]> PhotoByAccountIds = new Dictionary<string, byte[]>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Attach(string accountId, string groupId)
        {
            HashSet<string> groups = GroupByAccountId[accountId];
            groups.Add(groupId);

            HashSet<string> accounts = AccountByGroupID[groupId];
            accounts.Add(accountId);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public AccountUser GetAccount(string accountId)
        {
            return AccountById[accountId];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public long GetAccountsCount()
        {
            return AccountById.Count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ISet<string> GetAccountIds(string groupId)
        {
            return new HashSet<string>(AccountByGroupID[groupId]);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ICollection<AccountGroup> GetAllGroups()
        {
            return GroupById.Values;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public AccountGroup GetGroup(string groupId)
        {
            return GroupById[groupId];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public long GetGroupsCount()
        {
            return GroupByAccountId.Count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ISet<string> GetGroupIds(string accountId)
        {
            return new HashSet<string>(GroupByAccountId[accountId]);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] GetPhoto(string accountId)
        {
            return PhotoByAccountIds[accountId];
        }

        #region Write

        private void SaveAccount(AccountUser Account)
        {
            bool AccountExists = AccountById.ContainsKey(Account.Id);
            AccountById[Account.Id] = Account;
            if (!AccountExists)
            {
                GroupByAccountId[Account.Id] = new HashSet<string>();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SaveAccounts(IList<AccountUser> accounts)
        {
            foreach (AccountUser account in accounts)
            {
                SaveAccount(account);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SaveGroup(AccountGroup group)
        {
            AccountByGroupID[group.Id] = new HashSet<string>();
            GroupById[group.Id] = group;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetPhoto(string accountId, byte[] photo)
        {
            PhotoByAccountIds[accountId] = photo;
        }

        public void Reset()
        {
            PhotoByAccountIds.Clear();
            AccountByGroupID.Clear();
            AccountById.Clear();
            GroupByAccountId.Clear();
            GroupById.Clear();
        }

        #endregion
    }
}
