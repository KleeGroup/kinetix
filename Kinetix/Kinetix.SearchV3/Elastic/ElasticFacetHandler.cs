using Kinetix.ComponentModel.SearchV3;
using Kinetix.Search.MetaModel;
using Kinetix.Search.Model;

namespace Kinetix.Search.Elastic
{

    /// <summary>
    /// Handler de facette.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public class ElasticFacetHandler<TDocument>
        where TDocument : class
    {

        private const string MissingFacetPrefix = "_Missing";
        private readonly DocumentDefinition _document;
        private readonly ElasticQueryBuilder _builder = new ElasticQueryBuilder();

        /// <summary>
        /// Créé une nouvelle instance de ElasticFacetHandler.
        /// </summary>
        /// <param name="document">Définition du document.</param>
        public ElasticFacetHandler(DocumentDefinition document)
        {
            _document = document;
        }

        /// <summary>
        /// Définit l'agrégation correspondant à la facette lors de la recherche à facettes.
        /// </summary>
        /// <param name="agg">Descripteur d'agrégation.</param>
        /// <param name="facet">Définition de la facet.</param>
        public void DefineAggregation(Nest.AggregationDescriptor<TDocument> agg, IFacetDefinition facet)
        {
            /* Récupère le nom du champ. */
            string fieldName = _document.Fields[facet.FieldName].FieldName;
            /* Créé une agrégation sur les valeurs discrètes du champ. */
            agg.Terms(facet.Name, st => st.Field(fieldName));
            /* Créé une agrégation pour les valeurs non renseignées du champ. */
            agg.Missing(facet.Name + MissingFacetPrefix, ad => ad.Field(fieldName));
        }

        /// <summary>
        /// Extrait les facets du résultat d'une requête.
        /// </summary>
        /// <param name="aggs">Aggrégations Elastic.</param>
        /// <param name="facetDef">Définition de la facette.</param>
        /// <returns>Sortie des facettes.</returns>
        public FacetOutput ExtractFacetOutput(Nest.AggregationsHelper aggs, IFacetDefinition facetDef)
        {
            var facetOutput = new FacetOutput();
            /* Valeurs renseignées. */
            var bucket = aggs.Terms(facetDef.Name);
            foreach (var b in bucket.Items)
            {
                facetOutput.Values.Add(new FacetItem { Code = b.Key, Label = facetDef.ResolveLabel(b.Key), Count = b.DocCount });
            }

            /* Valeurs non renseignées. */
            var missingBucket = aggs.Missing(facetDef.Name + MissingFacetPrefix);
            var missingCount = missingBucket.DocCount;
            if (missingCount > 0)
            {
                facetOutput.Values.Add(new FacetItem { Code = FacetConst.NullValue, Label = "Non renseigné", Count = missingCount });
            }

            return facetOutput;
        }

        /// <summary>
        /// Vérifie qu'une définition de facette est valide.
        /// </summary>
        /// <param name="facetDef">Définition de facette.</param>
        public void CheckFacet(IFacetDefinition facetDef)
        {
            if (!_document.Fields.HasProperty(facetDef.FieldName))
            {
                throw new ElasticException("The Document \"" + _document.DocumentTypeName + "\" is missing a \"" + facetDef.FieldName + "\" property to facet on.");
            }
        }

        /// <summary>
        /// Créé la sous-requête QSL de filtrage pour la facette sélectionnée.
        /// </summary>
        /// <param name="facet">Sélection de facette.</param>
        /// <param name="facetDef">Définition de la facette.</param>
        /// <returns>Sous-requête.</returns>
        public string CreateFacetSubQuery(string facet, IFacetDefinition facetDef)
        {
            var fieldDesc = _document.Fields[facetDef.FieldName];
            var fieldName = fieldDesc.FieldName;

            /* Traite la valeur de sélection NULL */
            if (facet == FacetConst.NullValue)
            {
                return _builder.BuildMissingField(fieldName);
            }

            return _builder.BuildFilter(fieldName, facet);
        }
    }
}
