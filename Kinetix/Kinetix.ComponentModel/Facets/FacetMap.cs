using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Kinetix.ComponentModel.Facets {

    /// <summary>
    /// Mapper permettant de déclarer et de calculer les facettes sur une source de données.
    /// </summary>
    /// <typeparam name="TResource">Type d'objet concerné par le facettage.</typeparam>
    public class FacetMap<TResource> : IFacetMap<TResource> {

        private readonly IDictionary<string, Expression> _resourceGenerators = new Dictionary<string, Expression>();
        private readonly IDictionary<string, IFacetFilters<TResource>> _facetFilters = new Dictionary<string, IFacetFilters<TResource>>();

        /// <summary>
        /// Ajout d'une facette à partir d'une expression lambda permettant de sélectionner la facette.
        /// </summary>
        /// <typeparam name="TResult">Type de retour de l'expression.</typeparam>
        /// <param name="facetValue">Sélecteur à partir d'une lambda expression.</param>
        /// <returns>Mapping de la facette.</returns>
        public IFacetMap<TResource> AddFacetOn<TResult>(Expression<Func<TResource, TResult>> facetValue) {
            if (facetValue == null) {
                throw new ArgumentNullException("facetValue");
            }

            AddFacetOn(RetrieveFacetName(facetValue), facetValue);
            return this;
        }

        /// <summary>
        /// Ajoute une facette explicitement nommée.
        /// </summary>
        /// <typeparam name="TResult">Type de retour du sélecteur.</typeparam>
        /// <param name="facetName">Nom de la facette.</param>
        /// <param name="facetValue">Lambda expression permettant de calculer la facette.</param>
        /// <returns>Mapping de la facette.</returns>
        public IFacetMap<TResource> AddFacetOn<TResult>(string facetName, Expression<Func<TResource, TResult>> facetValue) {
            if (string.IsNullOrEmpty(facetName)) {
                throw new ArgumentNullException("facetName");
            }

            if (facetValue == null) {
                throw new ArgumentNullException("facetValue");
            }

            if (_resourceGenerators.ContainsKey(facetName)) {
                throw new NotSupportedException("A facet with the same key exists.");
            }

            _resourceGenerators.Add(facetName, facetValue);
            return this;
        }

        /// <summary>
        /// Ajoute une facette avec filtre explicite dont le nom est tirée du sélecteur de propriété.
        /// </summary>
        /// <typeparam name="TResult">Propriété sélectionnée.</typeparam>
        /// <param name="facetValue">Lambda expression permettant de sélectionner la propriété.</param>
        /// <param name="facets">Filtres appliqués à la facette.</param>
        /// <returns>Mapping de la facette.</returns>
        public IFacetMap<TResource> AddFacetOn<TResult>(Expression<Func<TResource, TResult>> facetValue, IFacetFilters<TResource> facets) {
            if (facetValue == null) {
                throw new ArgumentNullException("facetValue");
            }

            if (facets == null) {
                throw new ArgumentNullException("facets");
            }

            return AddFacetOn(RetrieveFacetName(facetValue), facets);
        }

        /// <summary>
        /// Ajoute une facette explicitement nommée en fournissant des filtres permettant la construction.
        /// </summary>
        /// <param name="facetName">Nom du filtre.</param>
        /// <param name="facets">Filtres appliqués à la facette.</param>
        /// <returns>Le mapper.</returns>
        public IFacetMap<TResource> AddFacetOn(string facetName, IFacetFilters<TResource> facets) {
            if (string.IsNullOrEmpty(facetName)) {
                throw new ArgumentNullException("facetName");
            }

            if (facets == null) {
                throw new ArgumentNullException("facets");
            }

            if (_facetFilters.ContainsKey(facetName)) {
                throw new NotSupportedException("FacetMap already contains a facet with the same name.");
            }

            _facetFilters[facetName] = facets;
            return this;
        }

        /// <summary>
        /// Calcule le facettage à partir de la source de données.
        /// </summary>
        /// <param name="dataSource">Source de données.</param>
        /// <returns>Liste des facettes.</returns>
        public IEnumerable<Facet<TResource>> GenerateFrom(IEnumerable<TResource> dataSource) {
            if (dataSource == null) {
                throw new ArgumentNullException("dataSource");
            }

            var result = from facet in _resourceGenerators
                         select new Facet<TResource> {
                             Name = facet.Key,
                             Headings = (from resource in dataSource
                                         let value = (facet.Value as Expression<Func<TResource, bool>> != null) ? (facet.Value as Expression<Func<TResource, bool>>).Compile().DynamicInvoke(resource) : (facet.Value as Expression<Func<TResource, string>>).Compile().DynamicInvoke(resource)
                                         orderby value
                                         group resource by value into g
                                         select new Heading<TResource> {
                                             Value = g.Key.ToString(),
                                             MatchCount = g.Count(),
                                             Expression = (facet.Value as Expression<Func<TResource, bool>> != null) ? BuildEqFunc(RetrieveFacetName<bool>(facet.Value as Expression<Func<TResource, bool>>), g.Key) : BuildEqFunc(RetrieveFacetName<string>(facet.Value as Expression<Func<TResource, string>>), g.Key)
                                         }).ToList()
                         };
            var addedFilters = from facet in _facetFilters
                               select new Facet<TResource> {
                                   Name = facet.Key,
                                   Headings = (from predicate in facet.Value
                                               from resource in dataSource
                                               let value = (bool)predicate.Value.Compile().DynamicInvoke(resource)
                                               where value == true
                                               group resource by predicate into g
                                               select new Heading<TResource> {
                                                   Value = g.Key.Key,
                                                   MatchCount = g.Count(),
                                                   Expression = g.Key.Value
                                               }).ToList()
                               };
            return result.Union(addedFilters);
        }

        /// <summary>
        /// Retourne le nom de la facette à partir d'une lambda expression permettant de sélectionner la propriété.
        /// </summary>
        /// <typeparam name="TResult">Propriété ciblée.</typeparam>
        /// <param name="facetValue">Lambda sélecteur de propriété.</param>
        /// <returns>Nom de la facette.</returns>
        private static string RetrieveFacetName<TResult>(Expression<Func<TResource, TResult>> facetValue) {
            switch (facetValue.Body.NodeType) {
                case ExpressionType.MemberAccess: {
                        MemberExpression expression = (MemberExpression)facetValue.Body;
                        return expression.Member.Name;
                    }

                case ExpressionType.Call: {
                        MethodCallExpression expression = (MethodCallExpression)facetValue.Body;
                        return expression.Method.Name;
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Create a lambda expression according to the property and value of the generic type.
        /// </summary>
        /// <param name="prop">Property.</param>
        /// <param name="val">Propery Value.</param>
        /// <returns>Expression.</returns>
        private static Expression<Func<TResource, bool>> BuildEqFunc(string prop, object val) {
            var o = Expression.Parameter(typeof(TResource), "t");
            Expression<Func<TResource, bool>> expression = Expression.Lambda<Func<TResource, bool>>(Expression.Equal(Expression.PropertyOrField(o, prop), Expression.Constant(val)), o);
            return expression;
        }
    }
}
