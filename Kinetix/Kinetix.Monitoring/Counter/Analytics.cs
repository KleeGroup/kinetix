using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Web;
using Kinetix.Configuration;
using Kinetix.Monitoring.Manager;
using Kinetix.Monitoring.Storage;
using log4net;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Accès centralisé à toutes les fonctions Analytiques.
    /// </summary>
    public sealed class Analytics : IAnalytics, IDisposable {

        /// <summary>
        /// Définition du compteur générique des temps de réponse.
        /// </summary>
        internal const string ElapsedTime = "ELAPSED_TIME";

        /// <summary>
        /// Nom du ProcessCounter dans le context Http.
        /// </summary>
        private const string ItemProcessCounter = "ItemProcessCounter";

        /// <summary>
        /// CounterProcess du thread si aucun contexte HTTP n'est disponible.
        /// </summary>
        [ThreadStatic]
        private static CounterProcess _counterProcess;

        /// <summary>
        /// Singleton.
        /// </summary>
        private static Analytics _instance = new Analytics();

        /// <summary>
        /// Stockage des Bases par leur nom.
        /// </summary>
        private readonly Dictionary<string, CounterDataBase> _map = new Dictionary<string, CounterDataBase>();

        /// <summary>
        /// Conserve les définitions de compteurs.
        /// </summary>
        private readonly CounterDefinitionRepository _counterDefinitionRepository;
        private readonly Dictionary<string, IManagerDescription> _mapDescription = new Dictionary<string, IManagerDescription>();
        private readonly List<IMonitoringStore> _monitoringStore = new List<IMonitoringStore>();
        private readonly Thread _storageThread;
        private readonly ManualResetEvent _resetEvent;
        private readonly MonitoringElement _monitoringElement;
        private readonly SortedDictionary<string, Tuple<int, long>> _summaryMap = new SortedDictionary<string, Tuple<int, long>>();

        private bool _threadStarted;

        /// <summary>
        /// Constructeur.
        /// </summary>
        private Analytics() {
            _counterDefinitionRepository = new CounterDefinitionRepository();
            this.CreateCounter(SR.CounterTime, ElapsedTime, 1000, 10 * 1000, 5);

            ModuleSection section = (ModuleSection)ConfigurationManager.GetSection(ModuleSection.ModuleSectionName);
            if (section == null) {
                _monitoringElement = new MonitoringElement();
            } else {
                _monitoringElement = section.Monitoring;
            }

            _resetEvent = new ManualResetEvent(false);
            _storageThread = new Thread(new ThreadStart(StorageThreadMain));
            _storageThread.Name = "MonitoringThread";
        }

        /// <summary>
        /// Retourne l'instance singleton.
        /// </summary>
        public static Analytics Instance {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Indique si le monitoring est actif.
        /// </summary>
        public bool IsEnabled {
            get;
            set;
        }

        /// <summary>
        /// Indique si le monitoring est actif en mode résumé.
        /// </summary>
        public bool IsSummaryEnabled {
            get;
            set;
        }

        /// <summary>
        /// Retourne le repository des définitions.
        /// </summary>
        internal CounterDefinitionRepository DefinitionRepository {
            get {
                return _counterDefinitionRepository;
            }
        }

        /// <summary>
        /// Traite une exception et l'enregistre en base de données.
        /// </summary>
        /// <param name="exception">Exception.</param>
        /// <returns>Numéro d'enregistrement en base de données.</returns>
        public int HandleException(Exception exception) {
            if (!IsEnabled) {
                return -1;
            }

            int id = -1;
            foreach (IMonitoringStore store in _monitoringStore) {
                id = ExceptionSafeHandleException(store, exception);
            }

            return id;
        }

        /// <summary>
        /// Crée une instance de CounterDefinition en la conservant en cache.
        /// </summary>
        /// <param name="label">Libellé à afficher.</param>
        /// <param name="code">Clé de l'instance dans le cache.</param>
        /// <param name="warningThreshold">Seuil d'alerte premier niveau (-1 si non défini).</param>
        /// <param name="criticalThreshold">Seuil d'alerte seconde niveau (-1 si non défini).</param>
        /// <param name="priority">Priorité d'affichage du compteur (minimum en premier).</param>
        public void CreateCounter(string label, string code, long warningThreshold, long criticalThreshold, int priority) {
            if (!IsEnabled) {
                return;
            }

            if (string.IsNullOrEmpty(label)) {
                throw new ArgumentNullException("label");
            }

            if (string.IsNullOrEmpty(code)) {
                throw new ArgumentNullException("code");
            }

            CounterDefinition counter;
            lock (this) {
                if (_counterDefinitionRepository.CreateDefinition(label, code, warningThreshold, criticalThreshold, priority, out counter)) {
                    foreach (IMonitoringStore store in _monitoringStore) {
                        ExceptionSafeCreateCounter(store, counter);
                    }
                }
            }
        }

        /// <summary>
        /// Ajoute un nouveau store.
        /// </summary>
        /// <param name="store">Store.</param>
        public void AddMonitoringStore(IMonitoringStore store) {
            if (!IsEnabled) {
                return;
            }

            if (store == null) {
                throw new ArgumentNullException("store");
            }

            _monitoringStore.Add(store);

            // Enregistrement en base de données des bases déja enregistrés.
            foreach (string dataBaseName in _mapDescription.Keys) {
                IManagerDescription description = _mapDescription[dataBaseName];
                ExceptionSafeCreateDatabase(store, dataBaseName, description);
            }

            // Enregistrement en base de données des compteurs déja enregistrés.
            foreach (ICounterDefinition counter in _counterDefinitionRepository.Values) {
                ExceptionSafeCreateCounter(store, counter);
            }

            lock (_resetEvent) {
                if (!_threadStarted) {
                    _threadStarted = true;
                    _storageThread.Start();
                }
            }
        }

        /// <summary>
        /// Ouverture d'une nouvelle instance de base de données.
        /// </summary>
        /// <param name="dataBaseName">Nom de la base de données.</param>
        /// <param name="description">Description du manager associé.</param>
        public void OpenDataBase(string dataBaseName, IManagerDescription description) {
            if (!IsEnabled) {
                return;
            }

            if (dataBaseName == null) {
                throw new ArgumentNullException("dataBaseName");
            }

            CounterDataBase db = new CounterDataBase(dataBaseName);
            lock (this) {
                if (_map.ContainsKey(dataBaseName)) {
                    return;
                }

                _map.Add(dataBaseName, db);

                if (description != null) {
                    _mapDescription.Add(dataBaseName, description);

                    foreach (IMonitoringStore store in _monitoringStore) {
                        ExceptionSafeCreateDatabase(store, dataBaseName, description);
                    }
                }
            }
        }

        /// <summary>
        /// Démarre un processus.
        /// </summary>
        /// <param name="processName">Nom du processus.</param>
        public void StartProcess(string processName) {
            if (!IsEnabled && !IsSummaryEnabled) {
                return;
            }

            if (processName == null) {
                throw new ArgumentNullException("processName");
            }

            HttpContext context = HttpContext.Current;
            if (context == null) {
                if (_counterProcess == null) {
                    _counterProcess = new CounterProcess(processName);
                } else {
                    _counterProcess = _counterProcess.CreateSubProcess(processName);
                }
            } else {
                CounterProcess counterProcess = context.Items[ItemProcessCounter] as CounterProcess;
                if (counterProcess == null) {
                    context.Items[ItemProcessCounter] = new CounterProcess(processName);
                } else {
                    context.Items[ItemProcessCounter] = counterProcess.CreateSubProcess(processName);
                }
            }
        }

        /// <summary>
        /// On termine l'enristrement du process et on l'ajoute à la base de données.
        /// </summary>
        /// <param name="dataBaseName">Nom de la base de données.</param>
        /// <returns>Retourne la durée du processus.</returns>
        public long StopProcess(string dataBaseName) {
            if (string.IsNullOrEmpty(dataBaseName)) {
                throw new ArgumentNullException("dataBaseName");
            }

            if (!IsEnabled && !IsSummaryEnabled) {
                return 0;
            }

            CounterProcess counterProcess;
            HttpContext context = HttpContext.Current;
            if (context == null) {
                counterProcess = _counterProcess;
            } else {
                counterProcess = context.Items[ItemProcessCounter] as CounterProcess;
            }

            // On agrège tous les compteurs
            if (counterProcess == null) {
                return 0;
            }

            long duration;
            if (IsSummaryEnabled) {
                counterProcess.Close();
                duration = counterProcess.Duration;

                lock (_summaryMap) {
                    Tuple<int, long> data;
                    if (_summaryMap.TryGetValue(counterProcess.Name, out data)) {
                        _summaryMap[counterProcess.Name] = new Tuple<int, long>(data.Item1 + 1, data.Item2 + duration);
                    } else {
                        _summaryMap[counterProcess.Name] = new Tuple<int, long>(1, duration);
                    }
                }
            } else {
                CounterDataBase counterDataBase = GetDataBase(dataBaseName);
                duration = counterDataBase.AddProcess(counterProcess);
            }

            // On ferme les compteurs
            if (context == null) {
                _counterProcess = counterProcess.Parent;
            } else {
                context.Items[ItemProcessCounter] = counterProcess.Parent;
            }

            return duration;
        }

        /// <summary>
        /// Affiche le résumé.
        /// </summary>
        public void PrintSummary() {
            lock (_summaryMap) {
                foreach (string s in _summaryMap.Keys) {
                    Tuple<int, long> data = _summaryMap[s];
                    Console.WriteLine("{0};{1};{2}", s, data.Item1, data.Item2);
                }
            }
        }

        /// <summary>
        /// Incrémente le compteur.
        /// </summary>
        /// <param name="counterDefinitionCode">Compteur.</param>
        /// <param name="value">Increment du compteur.</param>
        public void IncValue(string counterDefinitionCode, long value) {
            if (!IsEnabled) {
                return;
            }

            if (counterDefinitionCode == null) {
                throw new ArgumentNullException("counterDefinitionCode");
            }

            if (_counterDefinitionRepository.ValueOf(counterDefinitionCode) == null) {
                throw new NotSupportedException(string.Format(
                    CultureInfo.CurrentCulture,
                    SR.ExceptionUnknowCounter,
                    counterDefinitionCode));
            }

            CounterProcess counterProcess;
            HttpContext context = HttpContext.Current;
            if (context == null) {
                counterProcess = _counterProcess;
            } else {
                counterProcess = context.Items[ItemProcessCounter] as CounterProcess;
            }

            if (counterProcess != null) {
                counterProcess.IncValue(counterDefinitionCode, value);
            }
        }

        /// <summary>
        /// Réinitialise un base de données.
        /// </summary>
        /// <param name="dataBaseName">Nom de la base de données.</param>
        public void Reset(string dataBaseName) {
            if (!IsEnabled) {
                return;
            }

            if ("ALL".Equals(dataBaseName)) {
                foreach (CounterDataBase dataBase in _map.Values) {
                    dataBase.Reset();
                }
            } else {
                GetDataBase(dataBaseName).Reset();
            }
        }

        /// <summary>
        /// Libère les ressources de la classe.
        /// </summary>
        public void Dispose() {
            if (_threadStarted) {
                _resetEvent.Set();
                _storageThread.Join();
            }

            _resetEvent.Close();
            _instance = null;
            _counterProcess = null;
        }

        /// <summary>
        /// Récupération d'une instance de base de données déjà créée.
        /// </summary>
        /// <param name="dataBaseName">Nom de la base de données.</param>
        /// <returns>Instance de la base de données.</returns>
        internal CounterDataBase GetDataBase(string dataBaseName) {
            CounterDataBase database;
            if (!_map.TryGetValue(dataBaseName, out database)) {
                throw new NotSupportedException("Aucune base trouvee pour " + dataBaseName);
            }

            return database;
        }

        /// <summary>
        /// Crée une base de données dans un store et intercepte toutes les exceptions.
        /// </summary>
        /// <param name="store">Store.</param>
        /// <param name="dataBaseName">Nom de la base de données.</param>
        /// <param name="description">Description de la base.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "La gestion des erreurs est de la responsabilité du store.")]
        private static void ExceptionSafeCreateDatabase(IMonitoringStore store, string dataBaseName, IManagerDescription description) {
            try {
                store.CreateDatabase(new DatabaseDefinition(dataBaseName, description));
            } catch (Exception e) {
                ILog log = LogManager.GetLogger("Monitoring");
                if (log.IsErrorEnabled) {
                    log.Error("Erreur de persistance du monitoring.", e);
                }
            }
        }

        /// <summary>
        /// Crée un compteur dans un store et intercepte toutes les exceptions.
        /// </summary>
        /// <param name="store">Store.</param>
        /// <param name="counter">Compteur.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "La gestion des erreurs est de la responsabilité du store.")]
        private static void ExceptionSafeCreateCounter(IMonitoringStore store, ICounterDefinition counter) {
            try {
                store.CreateCounter(counter);
            } catch (Exception e) {
                ILog log = LogManager.GetLogger("Kinetix.Monitoring");
                if (log.IsErrorEnabled) {
                    log.Error("Erreur de persistance du monitoring.", e);
                }
            }
        }

        /// <summary>
        /// Enregistre une exception dans un store.
        /// </summary>
        /// <param name="store">Store.</param>
        /// <param name="exception">Exception.</param>
        /// <returns>Numéro d'enregistrement.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "La gestion des erreurs est de la responsabilité du store.")]
        private static int ExceptionSafeHandleException(IMonitoringStore store, Exception exception) {
            try {
                return store.HandleException(exception);
            } catch (Exception e) {
                ILog log = LogManager.GetLogger("Kinetix.Monitoring");
                if (log.IsErrorEnabled) {
                    log.Error("Erreur de persistance du monitoring.", e);
                }

                return -1;
            }
        }

        /// <summary>
        /// Sauvegarde les compteurs dans un store.
        /// </summary>
        /// <param name="store">Store.</param>
        /// <param name="counterDataList">Autres compteurs.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "La gestion des erreurs est de la responsabilité du store.")]
        private static void ExceptionSafeStoreCounters(IMonitoringStore store, ICollection<CounterData> counterDataList) {
            try {
                store.StoreCounters(counterDataList);
            } catch (Exception e) {
                ILog log = LogManager.GetLogger("Kinetix.Monitoring");
                if (log.IsErrorEnabled) {
                    log.Error("Erreur de persistance du monitoring.", e);
                }
            }
        }

        /// <summary>
        /// Gére la persistence des données.
        /// </summary>
        private void StorageThreadMain() {
            int interval = _monitoringElement.PersistenceInterval;
            if (interval == 0) {
                interval = 60;
            }

            while (!_resetEvent.WaitOne(interval * 1000, false)) {
                this.RunStorage();
            }

            this.RunStorage();
        }

        /// <summary>
        /// Exécute une passe de persistence des données.
        /// </summary>
        private void RunStorage() {
            if (_monitoringStore.Count > 0) { // && _monitoringElement.IsPersistent) {
                ICollection<Cube> collection = new List<Cube>();
                foreach (CounterDataBase database in _map.Values) {
                    database.RunStorage(collection);
                }

                ICollection<CounterData> counterDataList = new List<CounterData>();
                foreach (ICube cube in collection) {
                    cube.ExportCounters(null, counterDataList);
                }

                foreach (IMonitoringStore store in _monitoringStore) {
                    ExceptionSafeStoreCounters(store, counterDataList);
                }
            }
        }
    }
}
