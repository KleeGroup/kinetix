using System;
using System.Collections.Generic;
using System.Globalization;
using Kinetix.Caching;
using Kinetix.Security;
using log4net;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Kinetix.ServiceModel.Unity {

    /// <summary>
    /// Intercepteur cachant par utilisateur le retour des services.
    /// </summary>
    public class UserCacheInterceptionBehavior : IInterceptionBehavior {

        /// <summary>
        /// Nom du cache.
        /// </summary>
        private const string CacheName = "UserCache";

        /// <summary>
        /// Retourne <code>True</code> quoiqu'il arrive.
        /// </summary>
        public bool WillExecute
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Retourne l'ensemble des interfaces requises.
        /// </summary>
        /// <returns>Liste vide.</returns>
        public IEnumerable<Type> GetRequiredInterfaces() {
            return new List<Type>();
        }

        /// <summary>
        /// Effectue l'appel de la méthode, avec cache par utilisateur si activé.
        /// </summary>
        /// <param name="input">Méthode cible de l'appel.</param>
        /// <param name="getNext">Delegate.</param>
        /// <returns>Valeur de retour de la cible.</returns>
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            if (getNext == null) {
                throw new ArgumentNullException("getNext");
            }

            IMethodReturn retValue = null;

            /* Gestion du cache par utilisateur. */
            ILog log = LogManager.GetLogger("UserCache");

            /* 1. Calculer la clé du cache */
            string key = CreateCacheKey(input);

            /* 2. Vérification du cache. */
            var cache = CacheManager.Instance.GetCache(CacheName);
            var element = cache.Get(key);
            if (element != null) {
                /* a. Element en cache : on renvoie la valeur cachée. */
                if (log.IsInfoEnabled) {
                    log.InfoFormat("UserCache {0}.{1}", input.MethodBase.DeclaringType.FullName, input.MethodBase.Name);
                }

                retValue = input.CreateMethodReturn(element.Value);
            } else {
                /* b. Cache vide : on appelle la méthode suivante et on cache. */
                retValue = getNext()(input, getNext);
                element = new Element(key, retValue.ReturnValue);
                cache.Put(element);
            }

            return retValue;
        }

        /// <summary>
        /// Créé la clé pour l'appel de méthode.
        /// </summary>
        /// <param name="input">Input d'intercepteur.</param>
        /// <returns>Clé.</returns>
        private static string CreateCacheKey(IMethodInvocation input) {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}#{2}",
                input.MethodBase.DeclaringType.FullName,
                input.MethodBase.Name,
                StandardUser.UserId);
        }
    }
}
