

using Kinetix.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace Kinetix.Account
{
    public sealed class AccountManager : IAccountManager
    {
        private readonly string X_ACCOUNT_ID = "X_ACCOUNT_ID";
        private readonly IAccountStorePlugin _accountStorePlugin;
        private readonly DownloadedFile _defaultPhoto = new DownloadedFile();

        public AccountManager(IAccountStorePlugin accountStore)
        {
            _accountStorePlugin = accountStore;
            _defaultPhoto.ContentType = "image/png";
            //_defaultPhoto.Fichier = File.ReadAllBytes("defaultPhoto.png");
            _defaultPhoto.FileName = "defaultPhoto.png";
        }

        public void Login(string accountId)
        {
            HttpContext.Current.Session[X_ACCOUNT_ID] = accountId;
        }

        public string GetLoggedAccount()
        {
            string accountId = (string) HttpContext.Current.Session[X_ACCOUNT_ID];
            Debug.Assert(accountId != null, "Account was not logged");
            return accountId;
        }

        public DownloadedFile GetDefaultPhoto()
        {
            return _defaultPhoto;
        }

        public IAccountStore GetStore()
        {
            return _accountStorePlugin;
        }

    }
}
