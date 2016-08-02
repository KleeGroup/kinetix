using System.Collections.Generic;

namespace Kinetix.ComponentModel.Facets {

    /// <summary>
    /// Interface contractualisant une facette.
    /// </summary>
    /// <typeparam name="TResource">Type sur laquelle porte la facette.</typeparam>
    public interface IFacet<TResource> {

        /// <summary>
        /// Nom de la facette.
        /// </summary>
        string Name {
            get;
            set;
        }

        /// <summary>
        /// Lignes de facettage.
        /// </summary>
        ICollection<Heading<TResource>> Headings {
            get;
            set;
        }
    }
}
