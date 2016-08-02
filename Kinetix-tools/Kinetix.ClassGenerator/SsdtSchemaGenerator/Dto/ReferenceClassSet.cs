using System.Collections.Generic;
using Kinetix.ClassGenerator.Model;

namespace Kinetix.ClassGenerator.SsdtSchemaGenerator.Dto {

    /// <summary>
    /// Bean d'entrée du scripter qui pilote l'appel des scripts d'insertions des valeurs de listes de référence.
    /// </summary>
    public class ReferenceClassSet {

        /// <summary>
        /// Liste des classe de référence ordonnée.
        /// </summary>
        public IList<ModelClass> ClassList {
            get;
            set;
        }

        /// <summary>
        /// Nom du script à générer.
        /// </summary>
        public string ScriptName {
            get;
            set;
        }
    }
}
