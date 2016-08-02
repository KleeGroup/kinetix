using System;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Attribut indiquant la source de données d'un service de persistance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataAccessServiceAttribute : Attribute {

        /// <summary>
        /// Crée un nouvel attribut.
        /// </summary>
        /// <param name="dataSourceName">Nom de la source de données.</param>
        public DataAccessServiceAttribute(string dataSourceName) {
            if (string.IsNullOrEmpty(dataSourceName)) {
                throw new ArgumentNullException("dataSourceName");
            }

            this.DataSourceName = dataSourceName;
        }

        /// <summary>
        /// Obtient ou définit le nom de la source de données.
        /// </summary>
        public string DataSourceName {
            get;
            private set;
        }
    }
}
