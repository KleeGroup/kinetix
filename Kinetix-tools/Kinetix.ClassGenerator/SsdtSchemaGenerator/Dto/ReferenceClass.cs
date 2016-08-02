using Kinetix.ClassGenerator.Model;
using Kinetix.ServiceModel;

namespace Kinetix.ClassGenerator.SsdtSchemaGenerator.Dto {

    /// <summary>
    /// Table de référence.
    /// Contient une définition de classe et une liste de valeurs.
    /// </summary>
    public class ReferenceClass {

        /// <summary>
        /// Définition de la classe.
        /// </summary>
        public ModelClass Class {
            get;
            set;
        }

        /// <summary>
        /// Liste des valeurs de la table de référence.
        /// </summary>
        public TableInit Values {
            get;
            set;
        }

        /// <summary>
        /// Indique si la liste de référence est statique.
        /// </summary>
        public bool IsStatic {
            get;
            set;
        }
    }
}
