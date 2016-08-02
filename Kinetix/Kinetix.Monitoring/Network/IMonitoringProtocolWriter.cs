using System.Collections.Generic;
using System.IO;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Storage;

namespace Kinetix.Monitoring.Network {
    /// <summary>
    /// Interface pour l'écriture des données de monitoring.
    /// </summary>
    public interface IMonitoringProtocolWriter {

        /// <summary>
        /// Ecrit la définition d'une base de données.
        /// </summary>
        /// <param name="writer">Writer binaire.</param>
        /// <param name="databaseDefinition">Définition de la base de données.</param>
        void WriteDatabaseDefinition(BinaryWriter writer, IDatabaseDefinition databaseDefinition);

        /// <summary>
        /// Lit la définition d'un compteur.
        /// </summary>
        /// <param name="writer">Writer binaire.</param>
        /// <param name="counterDefinition">Définition du compteur.</param>
        void WriteCounterDefinition(BinaryWriter writer, ICounterDefinition counterDefinition);

        /// <summary>
        /// Lit les données sur les compteurs.
        /// </summary>
        /// <param name="writer">Writer binaire.</param>
        /// <param name="counters">Compteurs.</param>
        void WriteCounterData(BinaryWriter writer, ICollection<CounterData> counters);
    }
}
