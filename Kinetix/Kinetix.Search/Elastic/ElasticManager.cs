using System;
using System.Collections.Generic;
using System.Configuration;
using Kinetix.Search.Config;
using Nest;

namespace Kinetix.Search.Elastic {

    /// <summary>
    /// Manager pour la gestion d'Elastic Search.
    /// </summary>
    public sealed class ElasticManager {

        private static readonly ElasticManager _instance = new ElasticManager();

        private readonly Dictionary<string, SearchSettings> _connectionSettings = new Dictionary<string, SearchSettings>();

        /// <summary>
        /// Retourne une instance du manager.
        /// </summary>
        public static ElasticManager Instance {
            get {
                return _instance;
            }
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
            var settings = new ConnectionSettings(node)
                .DefaultIndex(connSettings.IndexName)
                .DisableDirectStreaming();
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
            if (res.ApiCall.HttpStatusCode == 404) {
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
