using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Kinetix.Security {

    /// <summary>
    /// Méthodes d'extensions de IIdentity.
    /// </summary>
    public static class IdentityExtensions {

        /// <summary>
        /// Indique si l'identité est autorisé à avoir accès à l'application.
        /// </summary>
        /// <param name="identity">Identité.</param>
        /// <returns><code>True</code> si l'accès est autorisé.</returns>
        public static bool IsAuthorized(this IIdentity identity) {
            if (identity == null) {
                throw new ArgumentNullException("identity");
            }

            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            if (claimsIdentity == null) {
                return false;
            }

            return claimsIdentity
                    .FindAll(StandardClaims.IsAuthorized)
                    .Where(c => c.Issuer == ClaimsIdentity.DefaultIssuer)
                    .Any(c => c.Value == "true");
        }
    }
}
