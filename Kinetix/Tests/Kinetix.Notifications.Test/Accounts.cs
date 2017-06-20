using System;
using System.Collections.Generic;
using Kinetix.Account;
using Kinetix.Account.Account;

namespace Kinetix.Notifications {
    public class Accounts {

        public static void InitData(IAccountManager accountManager) {
            AccountUser testAccount0 = new AccountUserBuilder("0").WithDisplayName("John doe").WithEmail("john.doe@yopmail.com").Build();
            AccountUser testAccount1 = new AccountUserBuilder("1").WithDisplayName("Palmer Luckey").WithEmail("palmer.luckey@yopmail.com").Build();
            AccountUser testAccount2 = new AccountUserBuilder("2").WithDisplayName("Bill Clinton").WithEmail("bill.clinton@yopmail.com").Build();
            accountManager.GetStore().SaveAccounts(new List<AccountUser>() { testAccount0, testAccount1, testAccount2 });


            //---create 5 000 noisy data
            List<AccountUser> accounts = CreateAccounts();
            foreach (AccountUser account in accounts) {
                accountManager.GetStore().SaveAccounts(accounts);
            }
        }

        private static int SEQ_ID = 10;

        private static List<AccountUser> CreateAccounts() {
            return new List<AccountUser>() {
                    CreateAccount("Jean Meunier", "jmeunier@yopmail.com"),
                    CreateAccount("Emeline Granger", "egranger@yopmail.com"),
                    CreateAccount("Silvia Robert", "sylv.robert@yopmail.com"),
                    CreateAccount("Manuel Long", "manu@yopmail.com"),
                    CreateAccount("David Martin", "david.martin@yopmail.com"),
                    CreateAccount("Véronique LeBourgeois", "vero89@yopmail.com"),
                    CreateAccount("Bernard Dufour", "bdufour@yopmail.com"),
                    CreateAccount("Nicolas Legendre", "nicolas.legendre@yopmail.com"),
                    CreateAccount("Marie Garnier", "marie.garnier@yopmail.com"),
                    CreateAccount("Hugo Bertrand", "hb@yopmail.com")
                };
        }

        private static AccountUser CreateAccount(string displayName, string email) {
            return new AccountUserBuilder(Convert.ToString(SEQ_ID++))
                    .WithDisplayName(displayName)
                    .WithEmail(email)
                    .Build();
        }
    }
}

