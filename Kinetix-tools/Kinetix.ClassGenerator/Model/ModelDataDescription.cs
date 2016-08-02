namespace Kinetix.ClassGenerator.Model {

    /// <summary>
    /// Cette classe décrit les attributs d'une propriété.
    /// </summary>
    public sealed class ModelDataDescription {
        /// <summary>
        /// Retourne le libellé de la propriété.
        /// </summary>
        public string Libelle {
            get;
            set;
        }

        /// <summary>
        /// Retourne le nom du domaine de la propriété.
        /// </summary>
        public ModelDomain Domain {
            get;
            set;
        }

        /// <summary>
        /// Retourne la référence de la propriété.
        /// </summary>
        public string ReferenceType {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la classe référencée.
        /// </summary>
        public ModelClass ReferenceClass {
            get;
            set;
        }

        /// <summary>
        /// Indique si la propriété est la clé primaire.
        /// </summary>
        public bool IsPrimaryKey {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la clef de ressource.
        /// </summary>
        public string ResourceKey {
            get;
            set;
        }

        /// <summary>
        /// Indique si la propriété est uen clé étrangère.
        /// </summary>
        public bool IsForeignKey {
            get {
                return this.ReferenceClass != null;
            }
        }
    }
}
