namespace Kinetix.ServiceModel {

    /// <summary>
    /// Ressource pour une liste de référence.
    /// </summary>
    public sealed class ReferenceResource {

        /// <summary>
        /// Crée un nouvelle ressource.
        /// </summary>
        /// <param name="id">Identifiant.</param>
        /// <param name="propertyName">Propriété.</param>
        /// <param name="locale">Locale.</param>
        /// <param name="label">Libellé de la ressource.</param>
        public ReferenceResource(object id, string propertyName, string locale, string label) {
            this.Id = id;
            this.PropertyName = propertyName;
            this.Locale = locale;
            this.Label = label;
        }

        /// <summary>
        /// Retourne l'identifiant de l'objet.
        /// </summary>
        public object Id {
            get;
            private set;
        }

        /// <summary>
        /// Retourne le nom de la propriété.
        /// </summary>
        public string PropertyName {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la locale.
        /// </summary>
        public string Locale {
            get;
            private set;
        }

        /// <summary>
        /// Retourne le libellé de la ressource.
        /// </summary>
        public string Label {
            get;
            private set;
        }
    }
}
