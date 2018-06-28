using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Model;

namespace Kinetix.Search.Elastic.Faceting {

    /// <summary>
    /// Helper pour le facettage.
    /// </summary>
    public static class FacetingUtil {

        /// <summary>
        /// Crée le filtre pour une facette sur les autres facettes multi-sélectionnables.
        /// </summary>
        /// <param name="builder">Query builder.</param>
        /// <param name="facet">Facette.</param>
        /// <param name="facetList">Liste des facettes.</param>
        /// <param name="selectedFacets">Facettes sélectionnées.</param>
        /// <param name="facetQueryBuilder">Builder de requête pour une facette.</param>
        /// <returns>La filtre.</returns>
        public static string BuildMultiSelectableFacetFilter(ElasticQueryBuilder builder, IFacetDefinition facet, ICollection<IFacetDefinition> facetList, FacetListInput selectedFacets, Func<string, IFacetDefinition, string, string> facetQueryBuilder) {
            return builder.BuildAndQuery(selectedFacets?
                .Select(sf => {
                    /* On ne filtre pas sur la facette en cours. */
                    if (sf.Key == facet.Code) {
                        return null;
                    }

                    var targetFacet = facetList.Single(f => f.Code == sf.Key);

                    /* On n'ajoute que les facettes multi-sélectionnables */
                    if (targetFacet.IsMultiSelectable == false) {
                        return null;
                    }

                    return builder.BuildOrQuery(sf.Value
                        .Select(v => facetQueryBuilder(v, targetFacet, null))
                        .ToArray());
                })
                .Where(sf => sf != null)
                .ToArray());
        }
    }
}
