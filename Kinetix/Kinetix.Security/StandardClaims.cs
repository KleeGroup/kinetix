using System.Security.Claims;

namespace Kinetix.Security {

    /// <summary>
    /// Nom des claims de sécurité standard.
    /// </summary>
    public class StandardClaims {

        /// <summary>
        /// ID de l'utilisateur.
        /// </summary>
        public const string UserId = "UserId";

        /// <summary>
        /// Nom du claim.
        /// </summary>
        public const string ProfileId = "Profile";

        /// <summary>
        /// User name claim.
        /// </summary>
        public const string UserName = "UserName";

        /// <summary>
        /// User name claim.
        /// </summary>
        public const string Culture = "Culture";

        /// <summary>
        /// Super user flag claim.
        /// </summary>
        public const string IsSuperUser = "IsSuperUser";

        /// <summary>
        /// Authorized flag claim.
        /// </summary>
        public const string IsAuthorized = "IsAuthorized";

        /// <summary>
        /// Authorized flag claim.
        /// </summary>
        public const string Role = ClaimTypes.Role;
    }
}
