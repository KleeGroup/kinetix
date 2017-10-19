using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading;

namespace Fmk.ComponentModel {
    /// <summary>
    /// Représente le Claim UserId.
    /// </summary>
    public static class UserIdClaim {

        /// <summary>
        /// Nom du claim.
        /// </summary>
        public const string ClaimType = "UserId";

        /// <summary>
        /// Default public user id.
        /// </summary>
        public const int PublicUserId = -1;

        /// <summary>
        /// Retourne l'identité de l'utilisateur.
        /// </summary>
        public static int? UserId {
            get {
                ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                if (identity != null) {
                    Claim userIdClaim = identity.FindFirst(UserIdClaim.ClaimType);
                    if (userIdClaim != null) {
                        return Int32.Parse(userIdClaim.Value, CultureInfo.InvariantCulture);
                    }
                }
                return null;
            }
        }
    }
}
