using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Account
{
    class MemoryAccountStorePlugin : IAccountStorePlugin
    {

        private readonly IDictionary<string, AccountUser> AccountById = new Dictionary<string, AccountUser>();
        private readonly IDictionary<string, AccountGroup> GroupById = new Dictionary<string, AccountGroup>();
        private readonly IDictionary<string, HashSet<string>> GroupByAccountId = new Dictionary<string, HashSet<string>>();
        private readonly IDictionary<string, HashSet<string>> AccountByGroupID = new Dictionary<string, HashSet<string>>();
        private readonly IDictionary<string, byte[]> PhotoByAccountIds = new Dictionary<string, byte[]>();

        public void Attach(string accountId, string groupId)
        {
            HashSet<string> groups = GroupByAccountId[accountId];
            groups.Add(groupId);

            HashSet<string> accounts = AccountByGroupID[groupId];
            accounts.Add(accountId);
        }

        public void Detach(string accountId, string groupId)
        {
            HashSet<string> groups = GroupByAccountId[accountId];
            groups.Remove(groupId);

            HashSet<string> accounts = AccountByGroupID[groupId];
            accounts.Remove(accountId);
        }

        public AccountUser GetAccount(string accountId)
        {
            return AccountById[accountId];
        }

        public long GetAccountsCount()
        {
            return AccountById.Count;
        }

        public ISet<string> GetAccountIds(string groupId)
        {
            return new HashSet<string>(AccountByGroupID[groupId]);
        }

        public ICollection<AccountGroup> GetAllGroups()
        {
            return GroupById.Values;
        }

        public AccountGroup GetGroup(string groupId)
        {
            return GroupById[groupId];
        }

        public long GetGroupsCount()
        {
            return GroupByAccountId.Count;
        }

        public ISet<string> GetGroupIds(string accountId)
        {
            return new HashSet<string>(GroupByAccountId[accountId]);
        }

        public byte[] GetPhoto(string accountId)
        {
            return PhotoByAccountIds[accountId];
        }

        private void SaveAccount(AccountUser Account)
        {
            bool AccountExists = AccountById.ContainsKey(Account.Id);
            AccountById[Account.Id] = Account;
            if (!AccountExists)
            {
                GroupByAccountId[Account.Id] = new HashSet<string>();
            }
        }

        public void SaveAccounts(IList<AccountUser> accounts)
        {
            foreach (AccountUser account in accounts)
            {
                SaveAccount(account);
            }
        }

        public void SaveGroup(AccountGroup group)
        {
            AccountByGroupID[group.Id] = new HashSet<string>();
            GroupById[group.Id] = group;
        }

        public void SetPhoto(string accountId, byte[] photo)
        {
            PhotoByAccountIds[accountId] = photo;
        }
    }
}
