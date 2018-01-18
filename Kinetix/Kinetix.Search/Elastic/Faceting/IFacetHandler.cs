using System.Collections.Generic;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Model;
using Nest;

namespace Kinetix.Search.Elastic.Faceting {

    /// <summary>
    /// Contrat des handlers de facette.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public interface IFacetHandler<TDocument>
        where TDocument : class {

        /// <summary>
        /// Vérifie qu'une définition de facette est valide.
        /// </summary>
        /// <param name="facetDef">Définition de facette.</param>
        void CheckFacet(IFacetDefinition facetDef);

        /// <summary>
        /// Créé la sous-requête QSL de filtrage pour la facette sélectionnée.
        /// </summary>
        /// <param name="facet">Sélection de facette.</param>
        /// <param name="facetDef">Définition de la facette.</param>
        /// <param name="portfolio">Portefeuille de l'utilisateur.</param>
        /// <returns>Sous-requête.</returns>
        string CreateFacetSubQuery(string facet, IFacetDefinition facetDef, string portfolio);

        /// <summary>
        /// Définit l'agrégation correspondant à la facette lors de la recherche à facettes.
        /// </summary>
        /// <param name="agg">Descripteur d'agrégation.</param>
        /// <param name="facet">Définition de la facet.</param>
        /// <param name="facetList">Définitions de toutes les facettes.</param>
        /// <param name="selectedFacets">Facettes sélectionnées, pour filtrer.</param>
        /// <param name="portfolio">Portefeuille de l'utilisateur.</param>
        void DefineAggregation(AggregationContainerDescriptor<TDocument> agg, IFacetDefinition facet, ICollection<IFacetDefinition> facetList, FacetListInput selectedFacets, string portfolio);

        /// <summary>
        /// Extrait les facets du résultat d'une requête.
        /// </summary>
        /// <param name="aggs">Aggrégations Elastic.</param>
        /// <param name="facetDef">Définition de la facette.</param>
        /// <param name="total">Nombre total de résultats.</param>
        /// <returns>Sortie des facettes.</returns>
        ICollection<FacetItem> ExtractFacetItemList(AggregationsHelper aggs, IFacetDefinition facetDef, long total);
    }
}