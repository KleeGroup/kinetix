using System.Collections.Generic;
using Kinetix.Monitoring.Storage;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Interface d'un cube de données.
    /// Ce cube est placé sur deux axes :
    /// - un axe fonctionnel,
    /// - un axe temporel.
    /// Ces axes sont représentés par une clé.
    ///
    /// Si les données sont étendues alors les temps min, max sont aussi stockées.
    /// </summary>
    public interface ICube {
        /// <summary>
        /// Heure en millisecondes du premier évènement ajouté sur ce cube.
        /// </summary>
        long FirstHitMsec {
            get;
        }

        /// <summary>
        /// Heure en millisecondes du dernier évènement ajouté sur ce cube.
        /// </summary>
        long LastHitMsec {
            get;
        }

        /// <summary>
        /// Retourne le compteur d'une valeur.
        /// Valable uniquement si mode étendu.
        /// </summary>
        /// <param name="counterDefinitionCode">Code de la valeur.</param>
        /// <returns>Compteur.</returns>
        ICounter GetCounter(string counterDefinitionCode);

        /// <summary>
        /// Ajoute tous les compteurs à la collection.
        /// </summary>
        /// <param name="moduleKey">Clef de module.</param>
        /// <param name="counterDataList">Listes des compteurs.</param>
        void ExportCounters(object moduleKey, ICollection<CounterData> counterDataList);
    }
}
