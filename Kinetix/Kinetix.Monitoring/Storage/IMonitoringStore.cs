using System;
using System.Collections.Generic;
using Kinetix.Monitoring.Counter;

namespace Kinetix.Monitoring.Storage {
    /// <summary>
    /// Interface d'enregistrement des compteurs du monitoring.
    /// </summary>
    public interface IMonitoringStore : IDisposable {

        /// <summary>
        /// Crée une base de données.
        /// </summary>
        /// <param name="databaseDefinition">Définition de la base de données.</param>
        void CreateDatabase(IDatabaseDefinition databaseDefinition);

        /// <summary>
        /// Crée une instance de CounterDefinition en la conservant en cache.
        /// </summary>
        /// <param name="counterDefinition">Définition du compteur.</param>
        void CreateCounter(ICounterDefinition counterDefinition);

        /// <summary>
        /// Traite une exception et l'enregistre en base de données.
        /// </summary>
        /// <param name="exception">Exception.</param>
        /// <returns>Numéro d'enregistrement en base de données.</returns>
        int HandleException(Exception exception);

        /// <summary>
        /// Sauvegarde les compteurs.
        /// </summary>
        /// <param name="counters">Compteurs.</param>
        void StoreCounters(ICollection<CounterData> counters);
    }
}
