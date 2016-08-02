using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Kinetix.ComponentModel.Facets {

    /// <summary>
    /// Interface contractualisant le conteneur des filtres applicables à une facette.
    /// </summary>
    /// <typeparam name="TResource">Type cible de la facette.</typeparam>
    public interface IFacetFilters<TResource> : IDictionary<string, Expression<Func<TResource, bool>>> {
    }
}
