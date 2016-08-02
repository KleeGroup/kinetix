using System.ComponentModel;
using System.Configuration;

namespace Kinetix.Configuration {
    /// <summary>
    /// Element de configuration lié au monitoring.
    /// </summary>
    public class MonitoringElement : ConfigurationElement {
        private const string PropertyEnabled = "enabled";
        private const string PropertyIsPersistent = "isPersistent";
        private const string PropertyPersistenceInterval = "persistenceInterval";

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        public MonitoringElement() {
        }

        /// <summary>
        /// Obtient ou définit si la collecte des données de monitoring est active.
        /// </summary>
        [ConfigurationProperty(PropertyEnabled, IsRequired = false, DefaultValue = true)]
        [Description("Indique si le monitoring est actif")]
        public bool Enabled {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit si la persitance des données de monitoring est active.
        /// </summary>
        [ConfigurationProperty(PropertyIsPersistent, IsRequired = false, DefaultValue = true)]
        [Description("Indique si les données de monitoring sont persistées")]
        public bool IsPersistent {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit l'intervale de persistance des données de monitoring.
        /// </summary>
        [ConfigurationProperty(PropertyPersistenceInterval, IsRequired = false, DefaultValue = 60)]
        [Description("Intervale de persistance des données de monitoring en seconde")]
        public int PersistenceInterval {
            get;
            set;
        }
    }
}
