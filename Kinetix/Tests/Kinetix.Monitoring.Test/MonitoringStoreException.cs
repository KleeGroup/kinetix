using System;
using System.Collections.Generic;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Storage;

namespace Kinetix.Monitoring.Test {
    /// <summary>
    /// Store de test du monitoring.
    /// </summary>
    public class MonitoringStoreException : IMonitoringStore {

        /// <summary>
        /// Nom de la dernière base créée.
        /// </summary>
        public string LastDatabaseName {
            get;
            set;
        }

        /// <summary>
        /// Code du dernier compteur créé.
        /// </summary>
        public string LastCounterCode {
            get;
            set;
        }

        /// <summary>
        /// Données compteurs sauvegardées.
        /// </summary>
        public bool HasCounterData {
            get;
            set;
        }

        /// <summary>
        /// Dernière exception notifiée.
        /// </summary>
        public Exception LastException {
            get;
            set;
        }

        /// <summary>
        /// Traite une exception et l'enregistre en base de données.
        /// </summary>
        /// <param name="exception">Exception.</param>
        /// <returns>Numéro d'enregistrement en base de données.</returns>
        int IMonitoringStore.HandleException(Exception exception) {
            if (exception == null) {
                throw new ArgumentNullException("exception");
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sauvegarde les compteurs.
        /// </summary>
        /// <param name="counters">Compteurs.</param>
        void IMonitoringStore.StoreCounters(ICollection<CounterData> counters) {
            if (counters == null) {
                throw new ArgumentNullException("counters");
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Crée une base de données.
        /// </summary>
        /// <param name="databaseDefinition">Définition de la base de données.</param>
        void IMonitoringStore.CreateDatabase(IDatabaseDefinition databaseDefinition) {
            if (databaseDefinition == null) {
                throw new ArgumentNullException("databaseDefinition");
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Crée une instance de CounterDefinition en la conservant en cache.
        /// </summary>
        /// <param name="counterDefinition">Définition du compteur.</param>
        void IMonitoringStore.CreateCounter(ICounterDefinition counterDefinition) {
            if (counterDefinition == null) {
                throw new ArgumentNullException("counterDefinition");
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Libère les ressources.
        /// </summary>
        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }
}
