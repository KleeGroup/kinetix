using Kinetix.ComponentModel;
using Kinetix.Security;
using System.Diagnostics;
using System.Web;
using System.Web.Services;

namespace Kinetix.Account
{
    public sealed class AccountManager : IAccountManager
    {
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
            //No more direct access in session : using KinetixSecurity
            //HttpContext.Current.Session[X_ACCOUNT_ID] = accountId;
        }

        public string GetLoggedAccount()
        {
            string accountId = StandardUser.UserId?.ToString();
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
