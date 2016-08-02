using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Kinetix.ComponentModel.Facets {

    /// <summary>
    /// Interface contractualisant les mappings de facettage.
    /// </summary>
    /// <typeparam name="TResource">Type soumis au facettage.</typeparam>
    public interface IFacetMap<TResource> {

        /// <summary>
        /// Ajoute une facette non explicitement nommée, le nom est déduit du sélecteur.
        /// </summary>
        /// <typeparam name="TResult">Element soumis au facettage.</typeparam>
        /// <param name="facetValue">Valeur de la facette.</param>
        /// <returns>Le mapper.</returns>
        IFacetMap<TResource> AddFacetOn<TResult>(Expression<Func<TResource, TResult>> facetValue);

        /// <summary>
        /// Ajoute une facette nommé sur l'objet.
        /// </summary>
        /// <typeparam name="TResult">Type soumis au facettage.</typeparam>
        /// <param name="facetName">Nom de la facette.</param>
        /// <param name="facetValue">Delegate permettant de calculer si l'élément est compris dans la facette.</param>
        /// <returns>Le mapper.</returns>
        IFacetMap<TResource> AddFacetOn<TResult>(string facetName, Expression<Func<TResource, TResult>> facetValue);

        /// <summary>
        /// Ajoute une facette en spécifiant des filtres permettant de définir le facettage.
        /// </summary>
        /// <typeparam name="TResult">Propriété soumise au facettage.</typeparam>
        /// <param name="facetValue">Lambda expression permettant de récupérer le nom de la facette.</param>
        /// <param name="facets">Filtres permettant de définir le facettage.</param>
        /// <returns>Le mapper.</returns>
        IFacetMap<TResource> AddFacetOn<TResult>(Expression<Func<TResource, TResult>> facetValue, IFacetFilters<TResource> facets);

        /// <summary>
        /// Ajoute une facette explicitement nommée en fournissant des filtres permettant la construction.
        /// </summary>
        /// <param name="facetName">Nom du filtre.</param>
        /// <param name="facets">Filtres permettant de définir le facettage.</param>
        /// <returns>Le mapper.</returns>
        IFacetMap<TResource> AddFacetOn(string facetName, IFacetFilters<TResource> facets);

        /// <summary>
        /// Génère les facettes à partir de la source de données.
        /// </summary>
        /// <param name="dataSource">Source de données.</param>
        /// <returns>Liste des facettes.</returns>
        IEnumerable<Facet<TResource>> GenerateFrom(IEnumerable<TResource> dataSource);
    }
}
