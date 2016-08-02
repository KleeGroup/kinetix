using System.Collections.Generic;

namespace Kinetix.ComponentModel.Facets {

    /// <summary>
    /// Facette sur une ressource de l'objet (ie. généralement une propriété).
    /// </summary>
    /// <typeparam name="TResource">Type de la ressource.</typeparam>
    public class Facet<TResource> : IFacet<TResource> {

        /// <summary>
        /// Nom affichable de la facette.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Liste des différentes valeurs associées à cette facette.
        /// </summary>
        public ICollection<Heading<TResource>> Headings {
            get;
            set;
        }
    }
}
