using System.Configuration;

namespace Kinetix.Search.Config {

    /// <summary>
    /// DataSource de moteur de recherche.
    /// </summary>
    public class SearchDataSourceElement : ConfigurationElement {

        /// <summary>
        /// Nom de la DataSource.
        /// </summary>
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// URI du noeud.
        /// </summary>
        [ConfigurationProperty("nodeUri", IsRequired = true)]
        public string NodeUri {
            get { return (string)this["nodeUri"]; }
            set { this["nodeUri"] = value; }
        }

        /// <summary>
        /// Nom de l'index.
        /// </summary>
        [ConfigurationProperty("indexName", IsRequired = true)]
        public string IndexName {
            get { return (string)this["indexName"]; }
            set { this["indexName"] = value; }
        }
    }
}
