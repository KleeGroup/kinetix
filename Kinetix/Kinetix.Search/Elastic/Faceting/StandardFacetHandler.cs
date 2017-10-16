using System.Collections.Generic;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.MetaModel;
using Kinetix.Search.Model;

namespace Kinetix.Search.Elastic.Faceting {

    /// <summary>
    /// Handler de facette standard.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public class StandardFacetHandler<TDocument> : IFacetHandler<TDocument>
        where TDocument : class {

        private const string MissingFacetPrefix = "_Missing";
        private readonly DocumentDefinition _document;
        private readonly ElasticQueryBuilder _builder = new ElasticQueryBuilder();

        /// <summary>
        /// Créé une nouvelle instance de StandardFacetHandler.
        /// </summary>
        /// <param name="document">Définition du document.</param>
        public StandardFacetHandler(DocumentDefinition document) {
            _document = document;
        }

        /// <inheritdoc/>
        public void DefineAggregation(Nest.AggregationContainerDescriptor<TDocument> agg, IFacetDefinition facet, string portfolio) {
            /* Récupère le nom du champ. */
            string fieldName = _document.Fields[facet.FieldName].FieldName;
            /* Créé une agrégation sur les valeurs discrètes du champ. */
            agg.Terms(facet.Code, st => st.Field(fieldName).Size(50));
            /* Créé une agrégation pour les valeurs non renseignées du champ. */
            agg.Missing(facet.Code + MissingFacetPrefix, ad => ad.Field(fieldName));
        }

        /// <inheritdoc />
        public ICollection<FacetItem> ExtractFacetItemList(Nest.AggregationsHelper aggs, IFacetDefinition facetDef, long total) {
            var facetOutput = new List<FacetItem>();
            /* Valeurs renseignées. */
            var bucket = aggs.Terms(facetDef.Code);
            foreach (var b in bucket.Buckets) {
                facetOutput.Add(new FacetItem { Code = b.Key, Label = facetDef.ResolveLabel(b.Key), Count = b.DocCount ?? 0 });
            }

            /* Valeurs non renseignées. */
            var missingBucket = aggs.Missing(facetDef.Code + MissingFacetPrefix);
            var missingCount = missingBucket.DocCount;
            if (missingCount > 0) {
                facetOutput.Add(new FacetItem { Code = FacetConst.NullValue, Label = "Non renseigné", Count = missingCount });
            }

            return facetOutput;
        }

        /// <inheritdoc/>
        public void CheckFacet(IFacetDefinition facetDef) {
            if (!_document.Fields.HasProperty(facetDef.FieldName)) {
                throw new ElasticException("The Document \"" + _document.DocumentTypeName + "\" is missing a \"" + facetDef.FieldName + "\" property to facet on.");
            }
        }

        /// <inheritdoc/>
        public string CreateFacetSubQuery(string facet, IFacetDefinition facetDef, string portfolio) {
            var fieldDesc = _document.Fields[facetDef.FieldName];
            var fieldName = fieldDesc.FieldName;

            /* Traite la valeur de sélection NULL */
            if (facet == FacetConst.NullValue) {
                return _builder.BuildMissingField(fieldName);
            }

            return _builder.BuildFilter(fieldName, facet);
        }
    }
}
