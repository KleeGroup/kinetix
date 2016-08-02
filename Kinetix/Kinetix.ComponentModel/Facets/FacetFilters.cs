using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Kinetix.ComponentModel.Facets {

    /// <summary>
    /// Conteneurs permettant de déclarer l'ensemble des filtres appliquables à la facette.
    /// </summary>
    /// <typeparam name="TResource">Type cible de la facette.</typeparam>
    [Serializable]
    public class FacetFilters<TResource> : Dictionary<string, Expression<Func<TResource, bool>>>, IFacetFilters<TResource> {
    }
}
