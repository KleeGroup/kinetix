using System.Configuration;

namespace Kinetix.Configuration {
    /// <summary>
    /// Section de configuration de base pour un module d'application.
    /// Cette classe doit être surchargée au niveau de l'application pour
    /// référencer l'ensemble des éléments de configuration utilisés.
    /// </summary>
    public class ModuleSection : ConfigurationSection {
        /// <summary>
        /// Nom de la section dans le fichier de configuration.
        /// </summary>
        public const string ModuleSectionName = "pic";

        /// <summary>
        /// Nom de la propriété module dans le fichier de configuration.
        /// </summary>
        internal const string PropertyModule = "module";

        /// <summary>
        /// Nom de la propriété monitoring dans le fichier de configuration.
        /// </summary>
        internal const string PropertyMonitoring = "monitoring";

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        public ModuleSection() {
        }

        /// <summary>
        /// Bloc de configuration obligatoire pour le module.
        /// </summary>
        [ConfigurationProperty(PropertyModule, IsRequired = true)]
        public ModuleElement Module {
            get {
                return (ModuleElement)this[PropertyModule];
            }
        }

        /// <summary>
        /// Bloc de configuration pour le paramétrage des logs.
        /// </summary>
        [ConfigurationProperty(PropertyMonitoring, IsRequired = false)]
        public MonitoringElement Monitoring {
            get {
                return (MonitoringElement)this[PropertyMonitoring];
            }
        }
    }
}
