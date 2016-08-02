using System;
using System.Data.Entity;
using System.Globalization;
using System.Threading;
using System.Web;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Cette classe propose plusieurs méthodes statiques permettant de charger des DbContext.
    /// Permet de choisir un chargement :
    ///     - A chaque appel
    ///     - En fonction d'un scope par défaut
    ///         - WebRequest, si contexte HTTP
    ///         - Thread, si pas de contexte HTTP de disponible.
    /// </summary>
    public static class DbContextFactory {

        /// <summary>
        /// Crée un nouveau DbContext pour le type spécifié.
        /// Identifique à un appel constructeur.
        /// </summary>
        /// <typeparam name="TDbContext">Type de contexte souhaité.</typeparam>
        /// <returns>Le DbContext associé.</returns>
        public static TDbContext GetDbContext<TDbContext>()
                where TDbContext : DbContext, new() {
            return new TDbContext();
        }

        /// <summary>
        /// Crée un nouveau DbContext avec la chaine de connection spécifiée.
        /// Indentique à un appel au constructeur avec la chaine de connection en paramètre.
        /// </summary>
        /// <typeparam name="TDbContext">Type de DbContext.</typeparam>
        /// <param name="connectionString">Chaine de connexion associée.</param>
        /// <returns>Le DbContext associé.</returns>
        public static TDbContext GetDbContext<TDbContext>(string connectionString)
                where TDbContext : DbContext, new() {
            if (string.IsNullOrEmpty(connectionString)) {
                throw new ArgumentNullException("connectionString");
            }

            Type t = typeof(TDbContext);
            return (TDbContext)Activator.CreateInstance(t, connectionString);
        }

        /// <summary>
        /// Crée un DbContext lié à la requête HTTP courante. Retourne le contexte déclaré dans le context HTTP existant.
        /// Fonctionne à partir d'une clef calculée sur la requête.
        /// </summary>
        /// <typeparam name="TDbContext">Type de DbContext.</typeparam>
        /// <returns>Le DbContext.</returns>
        public static TDbContext GetWebRequestScopedDbContext<TDbContext>()
                where TDbContext : DbContext, new() {
            return (TDbContext)GetWebRequestScopedDbContextInternal(typeof(TDbContext), null, null);
        }

        /// <summary>
        /// Crée un DbContext lié à la requête HTTP courante. Retourne le contexte déclaré dans le context HTTP existant.
        ///
        /// Cette version permet l'écriture d'une clef spécifique permettant de partager explicitement des DbContexts.
        /// </summary>
        /// <typeparam name="TDbContext">Type de DbContext.</typeparam>
        /// <param name="key">Clef de DbContext.</param>
        /// <returns>Le DbContext.</returns>
        public static TDbContext GetWebRequestScopedDbContext<TDbContext>(string key)
                                   where TDbContext : DbContext, new() {
            if (string.IsNullOrEmpty(key)) {
                throw new ArgumentNullException("key");
            }

            return (TDbContext)GetWebRequestScopedDbContextInternal(typeof(TDbContext), key, null);
        }

        /// <summary>
        /// Crée un DbContext lié à la requête HTTP courante. Retourne le contexte déclaré dans le context HTTP existant.
        ///
        /// Cette version permet l'écriture d'une clef spécifique permettant de partager explicitement des DbContexts.
        /// </summary>
        /// <typeparam name="TDbContext">Type de DbContext.</typeparam>
        /// <param name="key">Chaine de connexion.</param>
        /// <param name="connectionString">Chaine de connexion.</param>
        /// <returns>DbContext.</returns>
        public static TDbContext GetWebRequestScopedDbContext<TDbContext>(string key, string connectionString)
                                   where TDbContext : DbContext, new() {
            if (string.IsNullOrEmpty(key)) {
                throw new ArgumentNullException("key");
            }

            if (string.IsNullOrEmpty(connectionString)) {
                throw new ArgumentNullException("connectionString");
            }

            return (TDbContext)GetWebRequestScopedDbContextInternal(typeof(TDbContext), key, connectionString);
        }

        /// <summary>
        /// Crée un DbContext lié au thread d'exécution pouvant être réutilisé.
        /// Le DbContext est stocké dans la TLS.
        /// </summary>
        /// <typeparam name="TDbContext">Type de DbContext.</typeparam>
        /// <returns>Le DbContext.</returns>
        public static TDbContext GetThreadScopedDbContext<TDbContext>()
                                   where TDbContext : DbContext, new() {
            return (TDbContext)GetThreadScopedDbContextInternal(typeof(TDbContext), null, null);
        }

        /// <summary>
        /// Crée un DbContext lié au thread d'exécution pouvant être réutilisé.
        /// Le DbContext est stocké dans la TLS.
        /// </summary>
        /// <typeparam name="TDbContext">Type de DbContext.</typeparam>
        /// <param name="key">Clef identifiant le DbContext dans la TLS.</param>
        /// <returns>Le DbContext.</returns>
        public static TDbContext GetThreadScopedDbContext<TDbContext>(string key)
                                   where TDbContext : DbContext, new() {
            if (string.IsNullOrEmpty(key)) {
                throw new ArgumentNullException("key");
            }

            return (TDbContext)GetThreadScopedDbContextInternal(typeof(TDbContext), key, null);
        }

        /// <summary>
        /// Retourne le DbContext associé soit au thread soit au contexte HTTP (si HttpContext.Current != null).
        /// </summary>
        /// <typeparam name="TDbContext">Type de DbContext.</typeparam>
        /// <param name="key">Clef d'identification.</param>
        /// <param name="connectionString">Chaine de connexion.</param>
        /// <returns>Le DbContext.</returns>
        public static TDbContext GetScopedDbContext<TDbContext>(string key, string connectionString) {
            if (string.IsNullOrEmpty(connectionString)) {
                throw new ArgumentNullException("connectionString");
            }

            if (HttpContext.Current != null) {
                return (TDbContext)GetWebRequestScopedDbContextInternal(typeof(TDbContext), key, connectionString);
            }

            return (TDbContext)GetThreadScopedDbContextInternal(typeof(TDbContext), key, connectionString);
        }

        /// <summary>
        /// Retourne le DbContext associé soit au thread soit au contexte HTTP (si HttpContext.Current != null).
        /// </summary>
        /// <typeparam name="TDbContext">Type de DbContext.</typeparam>
        /// <param name="key">Clef d'identification.</param>
        /// <returns>Le DbContext.</returns>
        public static TDbContext GetScopedDbContext<TDbContext>(string key) {
            if (string.IsNullOrEmpty(key)) {
                throw new ArgumentNullException("key");
            }

            if (HttpContext.Current != null) {
                return (TDbContext)GetWebRequestScopedDbContextInternal(typeof(TDbContext), key, null);
            }

            return (TDbContext)GetThreadScopedDbContextInternal(typeof(TDbContext), key, null);
        }

        /// <summary>
        /// Retourne le DbContext associé soit au thread soit au contexte HTTP (si HttpContext.Current != null).
        /// </summary>
        /// <typeparam name="TDbContext">Type de DbContext.</typeparam>
        /// <returns>Le DbContext.</returns>
        public static TDbContext GetScopedDbContext<TDbContext>() {
            if (HttpContext.Current != null) {
                return (TDbContext)GetWebRequestScopedDbContextInternal(typeof(TDbContext), null, null);
            }

            return (TDbContext)GetThreadScopedDbContextInternal(typeof(TDbContext), null, null);
        }

        /// <summary>
        /// Crée le DbContext et le stoque dans HttpContext.Current.Items.
        /// </summary>
        /// <param name="type">Type de DbContext.</param>
        /// <param name="key">Clef de stockage.</param>
        /// <param name="connectionString">Chaine de connection.</param>
        /// <returns>Le DbContexte.</returns>
        private static object GetWebRequestScopedDbContextInternal(Type type, string key, string connectionString) {
            object context;
            if (HttpContext.Current == null) {
                context = connectionString == null ? Activator.CreateInstance(type) : Activator.CreateInstance(type, connectionString);
                return context;
            }

            if (key == null) {
                key = "__WRSCDC_" + HttpContext.Current.GetHashCode().ToString("x", CultureInfo.InvariantCulture) + Thread.CurrentContext.ContextID.ToString(CultureInfo.InvariantCulture);
            }

            context = HttpContext.Current.Items[key];
            if (context == null) {
                context = connectionString == null ? Activator.CreateInstance(type) : Activator.CreateInstance(type, connectionString);
                if (context != null) {
                    HttpContext.Current.Items[key] = context;
                }
            }

            return context;
        }

        /// <summary>
        /// Crée le DbContext et le stoque dans la TLS du Thread d'exécution.
        /// </summary>
        /// <param name="type">Type de DbContext.</param>
        /// <param name="key">Clef de stockage.</param>
        /// <param name="connectionString">Chaine de connexion.</param>
        /// <returns>Le DbContext.</returns>
        private static object GetThreadScopedDbContextInternal(Type type, string key, string connectionString) {
            if (key == null) {
                key = "__THSCDC_" + Thread.CurrentContext.ContextID.ToString(CultureInfo.InvariantCulture);
            }

            LocalDataStoreSlot threadData = Thread.GetNamedDataSlot(key);
            object context = null;
            if (threadData != null) {
                context = Thread.GetData(threadData);
            }

            if (context == null) {
                context = connectionString == null ? Activator.CreateInstance(type) : Activator.CreateInstance(type, connectionString);
                if (context != null) {
                    if (threadData == null) {
                        threadData = Thread.AllocateNamedDataSlot(key);
                    }

                    Thread.SetData(threadData, context);
                }
            }

            return context;
        }
    }
}