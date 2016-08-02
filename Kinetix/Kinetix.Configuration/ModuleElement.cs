using System.ComponentModel;
using System.Configuration;

namespace Kinetix.Configuration {

    /// <summary>
    /// Element de configuration d'un module applicatif.
    /// </summary>
    public class ModuleElement : ConfigurationElement {
        private const string PropertyName = "name";
        private const string PropertyConnectionName = "connectionName";
        private const string PropertyEmail = "email";

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        public ModuleElement() {
        }

        /// <summary>
        /// Obtient ou définit le nom du module.
        /// </summary>
        [ConfigurationProperty(PropertyName, IsRequired = true)]
        [Description("Nom du module applicatif")]
        [StringValidator(MaxLength = 128)]
        public string Name {
            get {
                return (string)this[PropertyName];
            }

            set {
                this[PropertyName] = value;
            }
        }

        /// <summary>
        /// Obtient ou définit la chaine de connexion à la base
        /// de données de configuration.
        /// </summary>
        [ConfigurationProperty(PropertyConnectionName, IsRequired = false)]
        [Description("Nom de la chaine de connexion à la base de données de configuration")]
        public string ConnectionName {
            get {
                return (string)this[PropertyConnectionName];
            }

            set {
                this[PropertyConnectionName] = value;
            }
        }

        /// <summary>
        /// Obtient ou définit l'adresse Email de l'administrateur.
        /// </summary>
        [ConfigurationProperty(PropertyEmail, IsRequired = true)]
        [Description("Adresse Email de l'administrateur")]
        public string Email {
            get {
                return (string)this[PropertyEmail];
            }

            set {
                this[PropertyEmail] = value;
            }
        }
    }
}
