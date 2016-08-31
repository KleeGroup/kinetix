using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using Kinetix.ComponentModel;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Contexte de sécurité pour les tests.
    /// </summary>
    public sealed class TestSecurityContext : IDisposable {

        private readonly IPrincipal _principal;

        /// <summary>
        /// Masquage du constructeur par défaut.
        /// </summary>
        public TestSecurityContext()
            : this(null) {
        }

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="roles">Tableau des roles de l'utilisateur.</param>
        public TestSecurityContext(string[] roles) {
            _principal = Thread.CurrentPrincipal;
            Thread.CurrentPrincipal = new TestPrincipal(_principal.Identity, roles);
        }

        /// <summary>
        /// Restore le contexte initial.
        /// </summary>
        public void Dispose() {
            Thread.CurrentPrincipal = _principal;
        }

        /// <summary>
        /// Principal donnant tous les rôles.
        /// </summary>
        internal class TestPrincipal : IPrincipal {

            private readonly IIdentity _identity;
            private readonly Dictionary<string, string> _roleDictionary;

            /// <summary>
            /// Crée une nouvelle identité.
            /// </summary>
            /// <param name="identity">Identité de l'utilisateur.</param>
            /// <param name="roles">Rôles de l'utilisateur.</param>
            internal TestPrincipal(IIdentity identity, string[] roles) {
                _identity = new TestIdentity();
                if (roles != null) {
                    _roleDictionary = new Dictionary<string, string>();
                    foreach (string role in roles) {
                        _roleDictionary.Add(role, role);
                    }
                }
            }

            /// <summary>
            /// Masquage du constructeur par défaut.
            /// </summary>
            private TestPrincipal() {
            }

            /// <summary>
            /// Retourne l'identité de l'utilisateur.
            /// </summary>
            public IIdentity Identity {
                get {
                    return _identity;
                }
            }

            /// <summary>
            /// Indique que l'utilisateur à tous les droits.
            /// </summary>
            /// <param name="role">Rôle à tester.</param>
            /// <returns>True.</returns>
            public bool IsInRole(string role) {
                if (_roleDictionary == null) {
                    return true;
                } else {
                    return _roleDictionary.ContainsKey(role);
                }
            }
        }

        /// <summary>
        /// Identité dédiée aux tests.
        /// </summary>
        internal class TestIdentity : GenericIdentity {

            /// <summary>
            /// Crée une nouvelle instance.
            /// </summary>
            public TestIdentity() : base("Test") {
                this.AddClaim(new Claim(UserIdClaim.ClaimType, "123456789"));
            }

            /// <summary>
            /// Identifiant de l'utilisateur.
            /// </summary>
            public int Id {
                get {
                    return 0;
                }
            }

            /// <summary>
            /// Nom de l'identité.
            /// </summary>
            public string FullName {
                get {
                    return "Test";
                }
            }

            /// <summary>
            /// Langue de l'utilisateur.
            /// </summary>
            public string LangueCode {
                get {
                    return "FR";
                }
            }

            /// <summary>
            /// Type de l'authentification.
            /// </summary>
            public string AuthenticationType {
                get {
                    return "Forms";
                }
            }

            /// <summary>
            /// Si l'utilisateur est connecté.
            /// </summary>
            public bool IsAuthenticated {
                get {
                    return true;
                }
            }

            /// <summary>
            /// Nom de l'utilisateur.
            /// </summary>
            public string Name {
                get {
                    return "Test";
                }
            }
        }
    }
}
