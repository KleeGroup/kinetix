using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kinetix.ComponentModel;
using Kinetix.ServiceModel;
using log4net;

namespace Kinetix.Broker {
    /// <summary>
    /// Manager pour les brokers.
    /// </summary>
    public sealed class BrokerManager {

        /// <summary>
        /// Paramètre permettant de ne pas définir de limite
        /// aux nombres de résultats ramenées par une requête
        /// du Broker.
        /// </summary>
        public const int NoLimit = 0;

        private static readonly BrokerManager _instance = new BrokerManager();
        private static string _defaultDataSourceName;
        private static IResourceServiceFactory _resourceServiceFactory;
        private readonly Dictionary<string, IBroker> _brokerMap = new Dictionary<string, IBroker>();
        private readonly Dictionary<string, Type> _storeMap = new Dictionary<string, Type>();
        private readonly List<IStoreRule> _storeRules = new List<IStoreRule>();
        private readonly IDictionary<Func<object, bool>, ISet<IStoreRule>> _typedRules = new Dictionary<Func<object, bool>, ISet<IStoreRule>>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        private BrokerManager() {
        }

        /// <summary>
        /// Retourne une instance du manager.
        /// </summary>
        public static BrokerManager Instance {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Enregistre la source de données.
        /// </summary>
        /// <param name="dataSource">Source de données.</param>
        public static void RegisterDefaultDataSource(string dataSource) {
            if (string.IsNullOrEmpty(dataSource)) {
                throw new ArgumentNullException("dataSource");
            }

            _defaultDataSourceName = dataSource;
        }

        /// <summary>
        /// Enregistre la source de données.
        /// </summary>
        /// <param name="dataSource">Source de données.</param>
        [Obsolete("Use RegisterDefaultDataSource.")]
        public static void RegisterDataSource(string dataSource) {
            RegisterDefaultDataSource(dataSource);
        }

        /// <summary>
        /// Enregistre la factory qui fabrique les services d'accès aux resources.
        /// </summary>
        /// <param name="factory">Factory.</param>
        public static void RegisterResourceServiceFactory(IResourceServiceFactory factory) {
            if (factory == null) {
                throw new ArgumentNullException("factory");
            }

            _resourceServiceFactory = factory;
        }

        /// <summary>
        /// Retourne l'instance du broker associé au type.
        /// </summary>
        /// <typeparam name="T">Type du broker.</typeparam>
        /// <returns>Le broker.</returns>
        public static IBroker<T> GetBroker<T>()
            where T : class, new() {
            return Instance.ReturnBroker<T>(null);
        }

        /// <summary>
        /// Retourne l'instance du broker associé au type.
        /// </summary>
        /// <typeparam name="T">Type du broker.</typeparam>
        /// <param name="dataSourceName">Nom de la source de données.</param>
        /// <returns>Le broker.</returns>
        public static IBroker<T> GetBroker<T>(string dataSourceName)
            where T : class, new() {
            if (string.IsNullOrEmpty(dataSourceName)) {
                throw new ArgumentNullException("dataSourceName");
            }

            return Instance.ReturnBroker<T>(dataSourceName);
        }

        /// <summary>
        /// Retourne l'instance du standard broker associé au type.
        /// </summary>
        /// <typeparam name="T">Type du broker.</typeparam>
        /// <returns>Le broker.</returns>
        public static IBroker<T> GetStandardBroker<T>()
            where T : class, new() {
            return Instance.ReturnBroker<T>(null, true);
        }

        /// <summary>
        /// Ajoute une règle appliquée par les brokers.
        /// </summary>
        /// <param name="rule">IStoreRule.</param>
        public void AddRule(IStoreRule rule) {
            if (rule == null) {
                throw new ArgumentNullException("rule");
            }

            if (!_storeRules.Contains(rule)) {
                _storeRules.Add(rule);
            }
        }

        /// <summary>
        /// Ajoute une règle appliquée conditionnellement par les brokers.
        /// </summary>
        /// <param name="predicate">Prédicat d'application à l'objet géré par le broker.</param>
        /// <param name="rule">Règle à appliquer si le prédicat est vrai.</param>
        public void AddRule(Func<object, bool> predicate, IStoreRule rule) {
            if (!_typedRules.ContainsKey(predicate)) {
                _typedRules[predicate] = new HashSet<IStoreRule>();
            }

            _typedRules[predicate].Add(rule);
        }

        /// <summary>
        /// Enregistre un nouveau store.
        /// </summary>
        /// <param name="dataSourceName">Nom de la source de données.</param>
        /// <param name="storeType">Type du store à enregistrer.</param>
        public void RegisterStore(string dataSourceName, Type storeType) {
            if (dataSourceName == null) {
                throw new ArgumentNullException("dataSourceName");
            }

            if (storeType == null) {
                throw new ArgumentNullException("storeType");
            }

            ILog log = LogManager.GetLogger("Application");
            if (log.IsDebugEnabled) {
                log.Debug("Enregistrement du store " + dataSourceName + " du type " + storeType.FullName);
            }

            _storeMap[dataSourceName] = storeType;
        }

        /// <summary>
        /// Retourne le type de store à utiliser pour une source de données.
        /// </summary>
        /// <param name="dataSourceName">Nom de la source de données.</param>
        /// <returns>Type de store à utiliser.</returns>
        internal Type GetStoreType(string dataSourceName) {
            return _storeMap[dataSourceName];
        }

        /// <summary>
        /// Retourne si le type supporte la suppresison logique.
        /// </summary>
        /// <param name="t">Type cible.</param>
        /// <returns>True si suppression logique, False sinon.</returns>
        private static bool IsLogicalDelete(Type t) {
            LogicalDeleteAttribute[] attrs = (LogicalDeleteAttribute[])t.GetCustomAttributes(typeof(LogicalDeleteAttribute), false);
            if (attrs.Length == 0) {
                return false;
            }

            return t.GetProperties().Count(p => p.Name == "IsActif") == 1;
        }

        /// <summary>
        /// Retourne l'instance du broker associé au type.
        /// </summary>
        /// <typeparam name="T">Type du broker.</typeparam>
        /// <param name="dataSourceName">Source de données : source par défaut si nulle.</param>
        /// <param name="forceStandardBroker">Force le type de broker à StandardBroker.</param>
        /// <returns>Le broker.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "La variable basicBroker n'est au final castée qu'une fois.")]
        private IBroker<T> ReturnBroker<T>(string dataSourceName, bool forceStandardBroker = false)
            where T : class, new() {
            string dsName = dataSourceName;
            if (string.IsNullOrEmpty(dsName)) {
                if (string.IsNullOrEmpty(_defaultDataSourceName)) {
                    throw new NotSupportedException("dataSource not registered, call first method RegisterDataSource");
                }

                dsName = _defaultDataSourceName;
            }

            string key = typeof(T).AssemblyQualifiedName + "/" + dsName;
            IBroker basicBroker;
            if (_brokerMap.TryGetValue(key, out basicBroker)) {
                return (IBroker<T>)basicBroker;
            }

            lock (_brokerMap) {
                if (_brokerMap.TryGetValue(key, out basicBroker)) {
                    return (IBroker<T>)basicBroker;
                }

                IBroker<T> broker;
                if (IsLogicalDelete(typeof(T))) {
                    broker = new LogicalDeleteBroker<T>(dsName);
                } else {
                    object[] attrs = typeof(T).GetCustomAttributes(typeof(ReferenceAttribute), false);
                    if (attrs.Length == 0 || forceStandardBroker) {
                        return new StandardBroker<T>(dsName);
                    } else {
                        /* SEY : Pas d'internationalisation à gérer sur Chaine. */
                        ////return new ReferenceBroker<T>(dsName, _resourceServiceFactory.GetLoaderService(), _resourceServiceFactory.GetWriterService());
                        return new StandardBroker<T>(dsName);
                    }
                }

                foreach (IStoreRule storeRule in _instance._storeRules) {
                    broker.AddRule(storeRule);
                }

                foreach (KeyValuePair<Func<object, bool>, ISet<IStoreRule>> typedRules in _typedRules) {
                    if (!typedRules.Key.Invoke(new T())) {
                        continue;
                    }

                    foreach (IStoreRule rule in typedRules.Value) {
                        broker.AddRule(rule);
                    }
                }

                _brokerMap[key] = broker;
                return broker;
            }
        }
    }
}
