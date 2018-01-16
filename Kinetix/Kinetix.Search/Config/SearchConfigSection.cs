using System.Configuration;

namespace Kinetix.Search.Config {

    /// <summary>
    /// Section de configuration du moteur de recherche.
    /// </summary>
    public class SearchConfigSection : ConfigurationSection {

        /// <summary>
        /// Liste des DataSource.
        /// </summary>
        [ConfigurationProperty("dataSources")]
        public SearchDataSourceCollection DataSources {
            get { return (SearchDataSourceCollection)this["dataSources"]; }
            set { this["dataSources"] = value; }
        }
    }
}
