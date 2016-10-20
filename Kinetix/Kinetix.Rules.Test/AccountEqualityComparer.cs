using Kinetix.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Rules.Test
{
    class AccountEqualityComparer : IEqualityComparer<AccountUser>
    {
        public bool Equals(AccountUser x, AccountUser y)
        {
            if (object.ReferenceEquals(x, y)) return true;

            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null)) return false;

            return x.Id == y.Id && x.Name == y.Name && x.Email == y.Email;
        }

        public int GetHashCode(AccountUser obj)
        {
            if (object.ReferenceEquals(obj, null)) return 0;

            int hashId = obj.Id.GetHashCode();
            int hashName = obj.Name.GetHashCode();
            int hashEmail = obj.Email.GetHashCode();

            return hashId ^ hashName ^ hashEmail;
        }
    }
}
