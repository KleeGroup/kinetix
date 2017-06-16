using System;
using System.Collections.Generic;
using Kinetix.ComponentModel.Search;
using Kinetix.Search.Contract;
using Kinetix.Search.Model;

namespace Kinetix.Search.Broker {

    /// <summary>
    /// Implémentation standard des brokers de recherche.
    /// TODO : rajouter monitoring.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public class StandardSearchBroker<TDocument> : ISearchBroker<TDocument> {

        /// <summary>
        /// Nombre de résultat par défaut pour une query.
        /// </summary>
        private const int QueryDefaultSize = 10;

        private readonly ISearchStore<TDocument> _store;

        /// <summary>
        /// Créé une nouvelle instance de StandardSearchBroker.
        /// </summary>
        /// <param name="datasourceName">Nom de la datasource.</param>
        public StandardSearchBroker(string datasourceName) {
            _store = CreateStore(datasourceName);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.CreateDocumentType" />
        public void CreateDocumentType() {
            _store.CreateDocumentType();
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Get" />
        public TDocument Get(string id) {
            return _store.Get(id);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Put" />
        public void Put(TDocument document) {
            _store.Put(document);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.PutAll" />
        public void PutAll(IEnumerable<TDocument> documentList) {
            _store.PutAll(documentList);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Remove" />
        public void Remove(string id) {
            _store.Remove(id);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Flush" />
        public void Flush() {
            _store.Flush();
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Query" />
        public IEnumerable<TDocument> Query(string text, string security = null) {
            if (string.IsNullOrEmpty(text)) {
                return new List<TDocument>();
            }

            var input = new AdvancedQueryInput {
                ApiInput = new QueryInput {
                    Criteria = new Criteria {
                        Query = text
                    },
                    Skip = 0,
                    Top = QueryDefaultSize
                },
                Security = security
            };
            var output = _store.AdvancedQuery(input);
            return output.List;
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.AdvancedQuery" />
        public QueryOutput<TDocument> AdvancedQuery(AdvancedQueryInput input) {
            return _store.AdvancedQuery(input);
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.AdvancedCount" />
        public long AdvancedCount(AdvancedQueryInput input) {
            return _store.AdvancedCount(input);
        }

        /// <summary>
        /// Retourne le store à utiliser.
        /// </summary>
        /// <param name="dataSourceName">Source de données.</param>
        /// <returns>Store.</returns>
        private static ISearchStore<TDocument> CreateStore(string dataSourceName) {
            Type storeType = SearchBrokerManager.Instance.GetStoreType(dataSourceName);
            Type realStoreType = storeType.MakeGenericType(typeof(TDocument));
            return (ISearchStore<TDocument>)Activator.CreateInstance(realStoreType, dataSourceName);
        }
    }
}
