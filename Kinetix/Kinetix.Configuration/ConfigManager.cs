using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;

namespace Kinetix.Configuration {

    /// <summary>
    /// Manager de configuration pour gérer la configuration des tests unitaires.
    /// </summary>
    public sealed class ConfigManager {

        /// <summary>
        /// Instance.
        /// </summary>
        private static ConfigManager _instance = null;

        /// <summary>
        /// Dictionnaire de configuration.
        /// </summary>
        private readonly IDictionary<string, string> _configMap;

        /// <summary>
        /// Crée un nouveau configManager.
        /// </summary>
        /// <param name="configMap">Map de configuration.</param>
        private ConfigManager(IDictionary<string, string> configMap) {
            if (configMap == null) {
                throw new ArgumentNullException("configMap");
            }

            _configMap = configMap;
        }

        /// <summary>
        /// Crée un nouveau ConfigManager.
        /// </summary>
        private ConfigManager() {
        }

        /// <summary>
        /// Retourne un singleton.
        /// </summary>
        public static ConfigManager Instance {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Initialise le manager de configuration.
        /// </summary>
        public static void Init() {
            _instance = new ConfigManager();
        }

        /// <summary>
        /// Initialise le manager de configuration.
        /// </summary>
        /// <param name="configMap">Map de configuration.</param>
        public static void Init(IDictionary<string, string> configMap) {
            if (configMap == null) {
                throw new ArgumentNullException("configMap");
            }

            _instance = new ConfigManager(configMap);
        }

        /// <summary>
        /// Retourne la valeur d'une configuration.
        /// </summary>
        /// <param name="key">Clé de la configuration.</param>
        /// <returns>Valeur de la configuration.</returns>
        public string GetConfigValue(string key) {
            if (_configMap != null) {
                if (_configMap.ContainsKey(key)) {
                    return _configMap[key];
                }
            }

            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Retourne la valeur booléenne d'une configuration.
        /// </summary>
        /// <param name="key">Clé de la configuration.</param>
        /// <returns>Valeur de la configuration. Null si la clé est absente.</returns>
        public bool? GetBoolValueSafe(string key) {
            string strValue = ConfigManager.Instance.GetConfigValue(key);
            if (string.IsNullOrEmpty(strValue)) {
                return null;
            }

            return bool.Parse(strValue);
        }

        /// <summary>
        /// Retourne la valeur entière d'une configuration.
        /// </summary>
        /// <param name="key">Clé de la configuration.</param>
        /// <returns>Valeur de la configuration. Null si la clé est absente.</returns>
        public int? GetIntValueSafe(string key) {
            string strValue = ConfigManager.Instance.GetConfigValue(key);
            if (string.IsNullOrEmpty(strValue)) {
                return null;
            }

            return int.Parse(strValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
    }
}
