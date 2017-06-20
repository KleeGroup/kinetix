using System;
using System.Collections.Generic;
using System.Configuration;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Html;
using Kinetix.Monitoring.Manager;
using Kinetix.Search.Config;
using Nest;

namespace Kinetix.Search.Elastic {

    /// <summary>
    /// Manager pour la gestion d'Elastic Search.
    /// </summary>
    public sealed class ElasticManager : IManager, IManagerDescription {

        /// <summary>
        /// Nom du compteur de requetes SQL.
        /// </summary>
        public const string CounterElasticRequestCount = "ELASTIC_REQUEST_COUNT";

        /// <summary>
        /// Nom du compteur d'erreurs base de données.
        /// </summary>
        public const string CounterElasticErrorCount = "ELASTIC_ERROR_COUNT";

        /// <summary>
        /// Nom du compteur de dead-lock base de données.
        /// </summary>
        public const string CounterElasticDeadLockCount = "ELASTIC_DEADLOCK_COUNT";

        /// <summary>
        /// Nom du compteur de timeout base de données.
        /// </summary>
        public const string CounterElasticTimeoutCount = "ELASTIC_TIMEOUT_COUNT";

        /// <summary>
        /// Nom de la base de données de monitoring.
        /// </summary>
        public const string ElasticHyperCube = "ELASTICDB";

        private static readonly ElasticManager _instance = new ElasticManager();

        private readonly Dictionary<string, SearchSettings> _connectionSettings = new Dictionary<string, SearchSettings>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        private ElasticManager() {
            Analytics.Instance.CreateCounter("Requêtes Elastic", CounterElasticRequestCount, 20, 30, 30);
            Analytics.Instance.CreateCounter("Erreur Elastic", CounterElasticErrorCount, 0, 0, 31);
            Analytics.Instance.CreateCounter("Deadlock Elastic", CounterElasticDeadLockCount, 0, 0, 32);
            Analytics.Instance.CreateCounter("Timeout ELastic", CounterElasticTimeoutCount, 0, 0, 33);
            Analytics.Instance.OpenDataBase(ElasticHyperCube, this);
        }

        /// <summary>
        /// Retourne une instance du manager.
        /// </summary>
        public static ElasticManager Instance {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Nom du manager.
        /// </summary>
        string IManagerDescription.Name {
            get {
                return "Elastic Search";
            }
        }

        /// <summary>
        /// Retourne un objet décrivant le service.
        /// </summary>
        IManagerDescription IManager.Description {
            get {
                return this;
            }
        }

        /// <summary>
        /// Image du manager.
        /// </summary>
        string IManagerDescription.Image {
            get {
                return "DB.png";
            }
        }

        /// <summary>
        /// Image.
        /// </summary>
        byte[] IManagerDescription.ImageData {
            get {
                return null; // TODO
            }
        }

        /// <summary>
        /// Priorité d'affichage du manager.
        /// </summary>
        int IManagerDescription.Priority {
            get {
                return 30;
            }
        }

        /// <summary>
        /// Type mime de l'image.
        /// </summary>
        string IManagerDescription.ImageMimeType {
            get {
                return "image/png";
            }
        }

        /// <summary>
        /// Libération des ressources consommées par le manager lors du undeploy.
        /// Exemples : connexions, thread, flux.
        /// </summary>
        void IManager.Close() {
        }

        /// <summary>
        /// Extension de la méthode toString().
        /// Permet à chaque Manager de présenter son propre état.
        /// </summary>
        /// <param name="writer">Writer HTML.</param>
        void IManagerDescription.ToHtml(System.Web.UI.HtmlTextWriter writer) {
            HtmlPageRenderer.ToHtml(ElasticHyperCube, writer);
        }

        /// <summary>
        /// Enregistre la configuration d'une connexion base de données.
        /// </summary>
        /// <param name="searchSettings">Configuration.</param>
        public void RegisterSearchSettings(SearchSettings searchSettings) {
            if (searchSettings == null) {
                throw new ArgumentNullException("searchSettings");
            }

            _connectionSettings[searchSettings.Name] = searchSettings;
        }

        /// <summary>
        /// Obtient un client ElasticSearch pour une datasource donnée.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        /// <returns>Client Elastic.</returns>
        public ElasticClient ObtainClient(string dataSourceName) {
            var connSettings = LoadSearchSettings(dataSourceName);
            var node = new Uri(connSettings.NodeUri);
            var settings = new ConnectionSettings(node);
            settings.SetDefaultIndex(connSettings.IndexName);
            /* TODO : mettre dans un singleton. */
            return new ElasticClient(settings);
        }

        /// <summary>
        /// Initialise un index avec la configuration Analyser/Tokenizer.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        /// <param name="configurator">Configurateur.</param>
        public void InitIndex(string dataSourceName, IIndexConfigurator configurator) {
            var settings = LoadSearchSettings(dataSourceName);
            var client = ObtainClient(dataSourceName);
            var res = client.CreateIndex(settings.IndexName, configurator.Configure);
            res.CheckStatus("CreateIndex");
        }

        /// <summary>
        /// Supprime un index.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        public void DeleteIndex(string dataSourceName) {
            var settings = LoadSearchSettings(dataSourceName);
            var client = ObtainClient(dataSourceName);
            var res = client.DeleteIndex(settings.IndexName);
            if (res.ConnectionStatus.HttpStatusCode == 404) {
                throw new ElasticException("The " + settings.IndexName + " index to delete doesn't exist.");
            }

            res.CheckStatus("DeleteIndex");
        }

        /// <summary>
        /// Indique si un index existe.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        /// <returns><code>True</code> si l'index existe.</returns>
        public bool ExistIndex(string dataSourceName) {
            var settings = LoadSearchSettings(dataSourceName);
            var client = ObtainClient(dataSourceName);
            var res = client.IndexExists(settings.IndexName);
            res.CheckStatus("IndexExists");
            return res.Exists;
        }

        /// <summary>
        /// Ping un node ES.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        public void PingNode(string dataSourceName) {
            var settings = LoadSearchSettings(dataSourceName);
            var client = ObtainClient(dataSourceName);
            var res = client.Ping();
            res.CheckStatus("Ping");
        }

        /// <summary>
        /// Charge les paramètres de connexion.
        /// </summary>
        /// <param name="dataSourceName">Nom de la DataSource.</param>
        /// <returns>Paramètres de connexion.</returns>
        internal SearchSettings LoadSearchSettings(string dataSourceName) {

            SearchSettings connectionSetting;
            lock (_connectionSettings) {
                if (!_connectionSettings.TryGetValue(dataSourceName, out connectionSetting)) {
                    var configElement = GetSearchConfigElement(dataSourceName);
                    if (configElement == null) {
                        throw new ElasticException("Connection setting not found for '" + dataSourceName + "' !");
                    }

                    connectionSetting = new SearchSettings {
                        Name = configElement.Name,
                        NodeUri = configElement.NodeUri,
                        IndexName = configElement.IndexName
                    };
                    _connectionSettings.Add(dataSourceName, connectionSetting);
                }
            }

            return connectionSetting;
        }

        /// <summary>
        /// Charge l'élément de configuration d'une datasource.
        /// </summary>
        /// <param name="dataSourceName">Nom de la DataSource.</param>
        /// <returns>Élément de configuration.</returns>
        private static SearchDataSourceElement GetSearchConfigElement(string dataSourceName) {
            var section = ConfigurationManager.GetSection("searchConfig");
            if (section == null) {
                return null;
            }

            var dataSources = ((SearchConfigSection)section).DataSources;
            return dataSources[dataSourceName];
        }
    }
}
