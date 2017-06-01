using System;
using System.Collections.Generic;
using Kinetix.ComponentModel.Search;
using Kinetix.Monitoring.Counter;
using Kinetix.Search.Contract;
using Kinetix.Search.Model;

namespace Kinetix.Search.Broker {

    /// <summary>
    /// Décorateur de monitoring des brokers de recherche.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public class MonitoredBroker<TDocument> : ISearchBroker<TDocument> {

        private readonly ISearchBroker<TDocument> _broker;

        /// <summary>
        /// Créé une nouvelle instance de MonitoredBroker.
        /// </summary>
        /// <param name="broker">Broker à décorer.</param>
        public MonitoredBroker(ISearchBroker<TDocument> broker) {
            if (broker == null) {
                throw new ArgumentNullException(nameof(broker));
            }

            _broker = broker;
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.CreateDocumentType" />
        public void CreateDocumentType() {
            StartProcess(nameof(CreateDocumentType));
            try {
                _broker.CreateDocumentType();
            } finally {
                StopProcess();
            }
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Get" />
        public TDocument Get(string id) {
            StartProcess(nameof(Get));
            try {
                return _broker.Get(id);
            } finally {
                StopProcess();
            }
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Put" />
        public void Put(TDocument document) {
            StartProcess(nameof(Put));
            try {
                _broker.Put(document);
            } finally {
                StopProcess();
            }
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.PutAll" />
        public void PutAll(IEnumerable<TDocument> documentList) {
            StartProcess(nameof(PutAll));
            try {
                _broker.PutAll(documentList);
            } finally {
                StopProcess();
            }
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Remove" />
        public void Remove(string id) {
            StartProcess(nameof(Remove));
            try {
                _broker.Remove(id);
            } finally {
                StopProcess();
            }
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Flush" />
        public void Flush() {
            StartProcess(nameof(Flush));
            try {
                _broker.Flush();
            } finally {
                StopProcess();
            }
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.Query" />
        public IEnumerable<TDocument> Query(string text, string security = null) {
            StartProcess(nameof(Query));
            try {
                return _broker.Query(text, security);
            } finally {
                StopProcess();
            }
        }

        /// <inheritdoc cref="ISearchBroker{TDocument}.AdvancedQuery" />
        public QueryOutput<TDocument> AdvancedQuery(AdvancedQueryInput input) {
            StartProcess(nameof(AdvancedQuery));
            try {
                return _broker.AdvancedQuery(input);
            } finally {
                StopProcess();
            }
        }

        /// <summary>
        /// Démarre un processus monitoré.
        /// </summary>
        /// <param name="command">Nom de la commande.</param>
        private static void StartProcess(string command) {
            Analytics.Instance.StartProcess($"{typeof(TDocument).Name}.{command}");
        }

        /// <summary>
        /// Arrête un processus monitoré.
        /// </summary>
        private static void StopProcess() {
            Analytics.Instance.StopProcess(SearchBrokerManager.SearchCube);
        }
    }
}
