namespace Kinetix.Reporting {

    /// <summary>
    /// Définition d'une propriété exportable.
    /// </summary>
    public class ExportPropertyDefinition {

        /// <summary>
        /// Obtient ou définit le nom de la propriété.
        /// </summary>
        /// <remarks>
        /// Supporte un niveau d'imbrication avec notation pointée.
        /// </remarks>
        public string PropertyPath {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le libellé associé à la propriété.
        /// </summary>
        public string PropertyLabel {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la propriété par défaut utilisé si la propriété référence un objet.
        /// </summary>
        public string DefaultPropertyName {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la datasource personnalisable de la propriété, dans le cas d'un type de référence.
        /// </summary>
        public object DataSource {
            get;
            set;
        }

        /// <summary>
        /// Obtient une chaîne de caractères représentant l'objet.
        /// </summary>
        /// <returns>Chaîne de caractères représentant l'objet.</returns>
        public override string ToString() {
            return this.PropertyPath;
        }
    }
}
