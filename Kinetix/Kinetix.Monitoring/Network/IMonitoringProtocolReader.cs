using System.Collections.Generic;
using System.IO;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Storage;

namespace Kinetix.Monitoring.Network {
    /// <summary>
    /// Interface pour la lecture des données de monitoring.
    /// </summary>
    public interface IMonitoringProtocolReader {

        /// <summary>
        /// Nom d'hote.
        /// </summary>
        string HostName {
            get;
        }

        /// <summary>
        /// Point de terminaison.
        /// </summary>
        string EndPoint {
            get;
        }

        /// <summary>
        /// Nom du module.
        /// </summary>
        string ModuleName {
            get;
        }

        /// <summary>
        /// Lit la définition d'une base de données.
        /// </summary>
        /// <param name="reader">Lecteur binaire.</param>
        /// <returns>La définition de la base de données.</returns>
        IDatabaseDefinition ReadDatabaseDefinition(BinaryReader reader);

        /// <summary>
        /// Lit la définition d'un compteur.
        /// </summary>
        /// <param name="reader">Lecteur binaire.</param>
        /// <returns>La définition du compteur.</returns>
        ICounterDefinition ReadCounterDefinition(BinaryReader reader);

        /// <summary>
        /// Lit les données sur les compteurs.
        /// </summary>
        /// <param name="reader">Lecteur binaire.</param>
        /// <returns>Liste des données de compteur.</returns>
        ICollection<CounterData> ReadCounterData(BinaryReader reader);
    }
}
