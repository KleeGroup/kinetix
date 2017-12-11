﻿using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Contract;
using Kinetix.Search.Elastic.Faceting;
using Kinetix.Search.MetaModel;
using Kinetix.Search.Model;
using log4net;
using Nest;

namespace Kinetix.Search.Elastic {

    /// <summary>
    /// Store ElasticSearch.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public class ElasticStore<TDocument> : ISearchStore<TDocument>
        where TDocument : class {

        /// <summary>
        /// Taille de cluster pour l'insertion en masse.
        /// </summary>
        private const int ClusterSize = 2000;

        private const string MissingGroupPrefix = "_Missing";

        /// <summary>
        /// Nom de l'aggrégation des top hits pour le groupement.
        /// </summary>
        private const string _topHitName = "top";

        private static ILog _log = LogManager.GetLogger("Search");

        /// <summary>
        /// Builder de requête.
        /// </summary>
        private readonly ElasticQueryBuilder _builder = new ElasticQueryBuilder();

        /// <summary>
        /// Usine à mapping ElasticSearch.
        /// </summary>
        private readonly ElasticMappingFactory _factory = new ElasticMappingFactory();

        /// <summary>
        /// Handler des facettes standard.
        /// </summary>
        private readonly IFacetHandler<TDocument> _standardHandler;

        /// <summary>
        /// Handler des facettes portefeuille.
        /// </summary>
        private readonly IFacetHandler<TDocument> _portfolioHandler;

        /// <summary>
        /// Nom de la source de données.
        /// </summary>
        private readonly string _dataSourceName;

        /// <summary>
        /// Définition du document.
        /// </summary>
        private readonly DocumentDefinition _definition;

        /// <summary>
        /// Nom du type du document.
        /// </summary>
        private readonly string _documentTypeName;

        /// <summary>
        /// Nom de l'index.
        /// </summary>
        private readonly string _indexName;

        /// <summary>
        /// Créé une nouvelle instance de ElasticStore.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        public ElasticStore(string dataSourceName) {
            try {
                if (dataSourceName == null) {
                    throw new ArgumentNullException("dataSourceName");
                }

                _definition = DocumentDescriptor.GetDefinition(typeof(TDocument));
                _documentTypeName = _definition.DocumentTypeName;
                _dataSourceName = dataSourceName;
                _indexName = ElasticManager.Instance.LoadSearchSettings(_dataSourceName).IndexName;
                _standardHandler = new StandardFacetHandler<TDocument>(_definition);
                _portfolioHandler = new PortfolioFacetHandler<TDocument>(_definition);
            } catch (Exception e) {
                if (_log.IsErrorEnabled) {
                    _log.Error("Echec d'instanciation du store.", e);
                }

                throw new NotSupportedException("Search Broker<" + typeof(TDocument).FullName + "> " + e.Message, e);
            }
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.CreateDocumentType" />
        public void CreateDocumentType() {

            if (_log.IsInfoEnabled) {
                _log.Info("Create Document type : " + _documentTypeName);
            }

            var client = GetClient();

            var res = client.Map<TDocument>(x => x
                .Type(_documentTypeName)
                .Properties(selector => {
                    foreach (var field in _definition.Fields) {
                        _factory.AddField(selector, field);
                    }
                    return selector;
                }));

            res.CheckStatus("Map");
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.Get" />
        public TDocument Get(string id) {
            var res = this.GetClient().Get(CreateDocumentPath(id));
            res.CheckStatus("Get");
            return res.Source;
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.Put" />
        public void Put(TDocument document) {
            var id = _definition.PrimaryKey.GetValue(document).ToString();

            var res = this.GetClient().Index(FormatSortFields(document), x => x
                .Index(_indexName)
                .Type(_documentTypeName)
                .Id(id));

            res.CheckStatus("Index");
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.PutAll" />
        public void PutAll(IEnumerable<TDocument> documentList) {
            if (documentList == null) {
                throw new ArgumentNullException("documentList");
            }

            if (!documentList.Any()) {
                return;
            }

            /* Découpage en cluster. */
            var total = documentList.Count();
            int left = total % ClusterSize;
            var clusterNb = ((total - left) / ClusterSize) + (left > 0 ? 1 : 0);

            for (int i = 1; i <= clusterNb; i++) {

                /* Extraction du cluster. */
                var cluster = documentList
                    .Skip((i - 1) * ClusterSize)
                    .Take(ClusterSize);

                /* Indexation en masse du cluster. */
                var res = this.GetClient().Bulk(x => {
                    foreach (var document in cluster) {
                        var id = _definition.PrimaryKey.GetValue(document).ToString();
                        x.Index<TDocument>(y => y
                         .Document(FormatSortFields(document))
                         .Index(_indexName)
                         .Type(_documentTypeName)
                         .Id(id));
                    }
                    return x;
                });
                res.CheckStatus("Bulk");
            }
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.Remove" />
        public void Remove(string id) {
            var res = this.GetClient().Delete(CreateDocumentPath(id));

            res.CheckStatus("Delete");
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.Flush" />
        public void Flush() {
            /* SEY : Non testé. */
            var res = this.GetClient().DeleteByQuery<TDocument>(x => x.Index(_indexName).Type(_documentTypeName));

            res.CheckStatus("DeleteAll");
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.AdvancedQuery" />
        public QueryOutput<TDocument> AdvancedQuery(AdvancedQueryInput input) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            var apiInput = input.ApiInput;

            /* Tri */
            var sortDef = GetSortDefinition(input);

            /* Requête de filtrage. */
            var textSubQuery = GetTextSubQuery(input);
            var securitySubQuery = GetSecuritySubQuery(input);
            var facetSubQuery = GetFacetSelectionSubQuery(input);
            var filterSubQuery = GetFilterSubQuery(input);
            var filterQuery = _builder.BuildAndQuery(textSubQuery, securitySubQuery, facetSubQuery, filterSubQuery);
            var hasFilter = !string.IsNullOrEmpty(filterQuery);

            /* Facettage. */
            var facetDefList = GetFacetDefinitionList(input);
            var hasFacet = facetDefList.Any();
            var portfolio = input.Portfolio;

            /* Group */
            var groupFieldName = GetGroupFieldName(input);
            var hasGroup = !string.IsNullOrEmpty(apiInput.Group);

            /* Pagination. */
            var skip = apiInput.Skip ?? 0;
            var size = hasGroup ? 0 : apiInput.Top ?? 1000; // TODO Paramétrable ?

            var res = this.GetClient()
                .Search<TDocument>(s => {
                    s
                        /* Index / type document. */
                        .Index(_indexName)
                        .Type(_documentTypeName)

                        /* Pagination */
                        .From(skip)
                        .Size(size);

                    /* Tri */
                    if (sortDef.HasSort) {
                        s.Sort(x => x
                            .Field(sortDef.FieldName, sortDef.Order));
                    }

                    /* Critère de filtrage. */
                    if (hasFilter) {
                        s.Query(q =>
                                q.QueryString(qs => qs
                                    .Query(filterQuery)));
                    }

                    /* Aggrégations. */
                    if (hasFacet || hasGroup) {
                        s.Aggregations(a => {
                            if (hasFacet) {
                                /* Facettage. */
                                foreach (var facetDef in facetDefList) {
                                    GetHandler(facetDef).DefineAggregation(a, facetDef, portfolio);
                                }
                            }
                            if (hasGroup) {
                                /* Groupement. */
                                a.Terms(groupFieldName, st => st
                                    .Field(groupFieldName)
                                    .Size(input.GroupSize)
                                    .Aggregations(g => g
                                        .TopHits(_topHitName, x => x.Size(input.GroupSize))));
                                /* Groupement pour les valeurs nulles */
                                a.Missing(groupFieldName + MissingGroupPrefix, st => st
                                    .Field(groupFieldName)
                                    .Aggregations(g => g
                                        .TopHits(_topHitName, x => x.Size(input.GroupSize))));
                            }
                            return a;
                        });
                    }

                    return s;
                });

            res.CheckStatus("AdvancedQuery");

            /* Extraction des facettes. */
            var facetListOutput = new List<FacetOutput>();
            if (hasFacet) {
                var aggs = res.Aggs;
                foreach (var facetDef in facetDefList) {
                    facetListOutput.Add(new FacetOutput {
                        Code = facetDef.Code,
                        Label = facetDef.Label,
                        Values = GetHandler(facetDef).ExtractFacetItemList(aggs, facetDef, res.Total)
                    });
                }
            }

            /* Ajout des facettes manquantes */
            if (input.ApiInput.Facets != null) {
                foreach (var facet in input.ApiInput.Facets) {
                    var facetItems = facetListOutput.First(f => f.Code == facet.Key).Values;
                    if (!facetItems.Any(f => f.Code == facet.Value)) {
                        facetItems.Add(new FacetItem {
                            Code = facet.Value,
                            Label = facetDefList.FirstOrDefault(fct => fct.Code == facet.Key)?.ResolveLabel(facet.Value),
                            Count = 0
                        });
                    }
                }
            }

            /* Extraction des résultats. */
            var resultList = new List<TDocument>();
            var groupResultList = new List<GroupResult<TDocument>>();
            if (hasGroup) {
                /* Groupement. */
                var bucket = (BucketAggregate)res.Aggregations[groupFieldName];
                foreach (KeyedBucket<object> group in bucket.Items) {
                    var list = ((TopHitsAggregate)group.Aggregations[_topHitName]).Documents<TDocument>().ToList();
                    groupResultList.Add(new GroupResult<TDocument> {
                        Code = group.Key.ToString(),
                        Label = facetDefList.First(f => f.Code == apiInput.Group).ResolveLabel(group.Key),
                        List = list,
                        TotalCount = (int)group.DocCount
                    });
                }

                /* Groupe pour les valeurs null. */
                var nullBucket = (SingleBucketAggregate)res.Aggregations[groupFieldName + MissingGroupPrefix];
                var nullTopHitAgg = (TopHitsAggregate)nullBucket.Aggregations[_topHitName];
                var nullDocs = nullTopHitAgg.Documents<TDocument>().ToList();
                if (nullDocs.Any()) {
                    groupResultList.Add(new GroupResult<TDocument> {
                        Code = FacetConst.NullValue,
                        Label = input.FacetQueryDefinition.FacetNullValueLabel ?? "focus.search.results.missing",
                        List = nullDocs,
                        TotalCount = (int)nullBucket.DocCount
                    });
                }

                resultList = null;
            } else {
                /* Liste unique. */
                resultList = res.Documents.ToList();
                groupResultList = null;
            }

            /* Construction de la sortie. */
            var output = new QueryOutput<TDocument> {
                List = resultList,
                Facets = facetListOutput,
                Groups = groupResultList,
                Query = apiInput,
                TotalCount = res.Total
            };

            return output;
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.AdvancedCount" />
        public long AdvancedCount(AdvancedQueryInput input) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            /* Requête de filtrage. */
            string filterQuery = GetFilterQuery(input);
            var hasFilter = !string.IsNullOrEmpty(filterQuery);

            var res = this.GetClient()
                .Count<TDocument>(s => {
                    /* Index / type document. */
                    s
                        .Index(_indexName)
                        .Type(_documentTypeName);

                    /* Critère de filtrage. */
                    if (hasFilter) {
                        s.Query(q =>
                                q.QueryString(qs => qs
                                    .Query(filterQuery)));
                    }

                    return s;
                });

            res.CheckStatus("AdvancedCount");

            return res.Count;
        }

        /// <summary>
        /// Créé la requête de filtrage.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Requête de filtrage.</returns>
        private string GetFilterQuery(AdvancedQueryInput input) {
            var textSubQuery = GetTextSubQuery(input);
            var securitySubQuery = GetSecuritySubQuery(input);
            var facetSubQuery = GetFacetSelectionSubQuery(input);
            var filterSubQuery = GetFilterSubQuery(input);
            return _builder.BuildAndQuery(textSubQuery, securitySubQuery, facetSubQuery, filterSubQuery);
        }

        /// <summary>
        /// Crée la sous requête pour les champs de filtre.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Sous-requête.</returns>
        private string GetFilterSubQuery(AdvancedQueryInput input) {
            if (input.FilterList == null || !input.FilterList.Any()) {
                return string.Empty;
            }

            var filterList = new List<string>();
            foreach (KeyValuePair<string, string> entry in input.FilterList) {
                var field = _definition.Fields[entry.Key].FieldName;
                filterList.Add(_builder.BuildFilter(field, entry.Value));
            }

            return _builder.BuildAndQuery(filterList.ToArray());
        }

        /// <summary>
        /// Créé la sous-requête pour le champ textuel.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Sous-requête.</returns>
        private string GetTextSubQuery(AdvancedQueryInput input) {
            var criteria = input.ApiInput.Criteria;
            var value = criteria != null ? criteria.Query : null;

            /* Absence de texte ou joker : sous-requête vide. */
            if (string.IsNullOrEmpty(value) || value == "*") {
                return string.Empty;
            }

            /* Vérifie la présence d'un champ textuel. */
            var fieldDesc = _definition.TextField;
            if (fieldDesc == null) {
                throw new ElasticException("The Document \"" + _definition.DocumentTypeName + "\" needs a Search category field to allow Query.");
            }

            /* Constuit la sous requête. */
            return _builder.BuildFullTextSearch(fieldDesc.FieldName, value);
        }

        /// <summary>
        /// Créé la sous-requête le filtrage de sécurité.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Sous-requête.</returns>
        private string GetSecuritySubQuery(AdvancedQueryInput input) {
            var value = input.Security;

            /* Absence de filtrage de sécurité : sous-requêt vide. */
            if (string.IsNullOrEmpty(value)) {
                return string.Empty;
            }

            /* Vérifie la présence d'un champ de sécurité. */
            var fieldDesc = _definition.SecurityField;
            if (fieldDesc == null) {
                throw new ElasticException("The Document \"" + _definition.DocumentTypeName + "\" needs a Security category field to allow Query with security filtering.");
            }

            /* Constuit la sous requête. */
            return _builder.BuildInclusiveInclude(fieldDesc.FieldName, value);
        }

        /// <summary>
        /// Créé la sous-requête le filtrage par sélection de facette.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Sous-requête.</returns>
        private string GetFacetSelectionSubQuery(AdvancedQueryInput input) {
            var facetList = input.ApiInput.Facets;
            if (facetList == null || !facetList.Any()) {
                return string.Empty;
            }

            var facetSubQueryList =
                facetList.Select(f => {
                    /* Récupère la définition de la facette. */
                    var def = input.FacetQueryDefinition.Facets.Single(x => x.Code == f.Key);
                    /* Créé une sous-requête par facette. */
                    string s = f.Value;
                    return GetHandler(def).CreateFacetSubQuery(s, def, input.Portfolio);
                }).ToArray();

            /* Concatène en "ET" toutes les sous-requêtes. */
            return _builder.BuildAndQuery(facetSubQueryList);
        }

        /// <summary>
        /// Obtient la liste des facettes.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Définitions de facettes.</returns>
        private ICollection<IFacetDefinition> GetFacetDefinitionList(AdvancedQueryInput input) {
            var groupFacetName = input.ApiInput.Group;
            var list = input.FacetQueryDefinition != null ? input.FacetQueryDefinition.Facets : new List<IFacetDefinition>();

            /* Recherche de la facette de groupement. */
            string groupFieldName = null;
            if (!string.IsNullOrEmpty(groupFacetName)) {
                var groupFacetDef = input.FacetQueryDefinition.Facets.SingleOrDefault(x => x.Code == groupFacetName);
                if (groupFacetDef == null) {
                    throw new ElasticException("No facet \"" + groupFacetName + "\" to group on.");
                }

                groupFieldName = groupFacetDef.FieldName;
            }

            foreach (var facetDef in list) {
                /* Vérifie que le champ à facetter existe sur le document. */
                GetHandler(facetDef).CheckFacet(facetDef);
            }

            return list;
        }

        /// <summary>
        /// Obtient la définition du tri.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Définition du tri.</returns>
        private SortDefinition GetSortDefinition(AdvancedQueryInput input) {
            var fieldName = input.ApiInput.SortFieldName;

            /* Cas de l'absence de tri. */
            if (string.IsNullOrEmpty(fieldName)) {
                return new SortDefinition();
            }

            /* Vérifie la présence du champ. */
            if (!_definition.Fields.HasProperty(fieldName)) {
                throw new ElasticException("The Document \"" + _definition.DocumentTypeName + "\" is missing a \"" + fieldName + "\" property to sort on.");
            }

            return new SortDefinition {
                FieldName = _definition.Fields[fieldName].FieldName,
                Order = input.ApiInput.SortDesc ? SortOrder.Descending : SortOrder.Ascending
            };
        }

        /// <summary>
        /// Obtient le nom du champ pour le groupement.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Nom du champ.</returns>
        private string GetGroupFieldName(AdvancedQueryInput input) {
            var groupFacetName = input.ApiInput.Group;

            /* Pas de groupement. */
            if (string.IsNullOrEmpty(groupFacetName)) {
                return null;
            }

            /* Recherche de la facette de groupement. */
            var facetDef = input.FacetQueryDefinition.Facets.SingleOrDefault(x => x.Code == groupFacetName);
            if (facetDef == null) {
                throw new ElasticException("No facet " + groupFacetName + " to group on.");
            }

            var fieldName = facetDef.FieldName;

            /* Vérifie la présence du champ. */
            if (!_definition.Fields.HasProperty(fieldName)) {
                throw new ElasticException("The Document \"" + _definition.DocumentTypeName + "\" is missing a \"" + fieldName + "\" property to group on.");
            }

            return _definition.Fields[fieldName].FieldName;
        }

        /// <summary>
        /// Obtient le client ElastcSearch.
        /// </summary>
        /// <returns>Client Elastic.</returns>
        private ElasticClient GetClient() {
            return ElasticManager.Instance.ObtainClient(_dataSourceName);
        }

        /// <summary>
        /// Créé un DocumentPath.
        /// </summary>
        /// <param name="id">ID du document.</param>
        /// <returns>Le DocumentPath.</returns>
        private DocumentPath<TDocument> CreateDocumentPath(string id) {
            return new DocumentPath<TDocument>(id).Index(_indexName).Type(_documentTypeName);
        }

        /// <summary>
        /// Format les champs de tri du document.
        /// Les champs de tri sont mis manuellement en minuscule avant indexation.
        /// Ceci est nécessaire car en ElasticSearch 5.x, il n'est plus possible de trier sur un champ indexé (à faible coût).
        /// </summary>
        /// <param name="document">Document.</param>
        /// <returns>Document formaté.</returns>
        private TDocument FormatSortFields(TDocument document) {
            foreach (var field in _definition.Fields.Where(x => x.Category == SearchFieldCategory.Sort && x.PropertyType == typeof(string))) {
                var raw = field.GetValue(document);
                if (raw != null) {
                    field.SetValue(document, ((string)raw).ToLowerInvariant());
                }
            }

            return document;
        }

        /// <summary>
        /// Renvoie le handler de facet pour une définition de facet.
        /// </summary>
        /// <param name="def">Définition de facet.</param>
        /// <returns>Handler.</returns>
        private IFacetHandler<TDocument> GetHandler(IFacetDefinition def) {
            if (def.GetType() == typeof(PortfolioFacet)) {
                return _portfolioHandler;
            }

            return _standardHandler;
        }

        /// <summary>
        /// Définition de tri.
        /// </summary>
        private class SortDefinition {

            /// <summary>
            /// Ordre de tri.
            /// </summary>
            public SortOrder Order {
                get;
                set;
            }

            /// <summary>
            /// Champ du tri (camelCase).
            /// </summary>
            public string FieldName {
                get;
                set;
            }

            /// <summary>
            /// Indique si le tri est défini.
            /// </summary>
            public bool HasSort {
                get {
                    return !string.IsNullOrEmpty(this.FieldName);
                }
            }
        }
    }
}
