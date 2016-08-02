using Kinetix.Monitoring.Manager;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Interface d'accès centralisé à toutes les fonctions Analytiques.
    /// </summary>
    public interface IAnalytics {

        /// <summary>
        /// Crée une instance de CounterDefinition en la conservant en cache.
        /// </summary>
        /// <param name="label">Libellé à afficher.</param>
        /// <param name="code">Clé de l'instance dans le cache.</param>
        /// <param name="warningThreshold">Seuil d'alerte premier niveau (-1 si non défini).</param>
        /// <param name="criticalThreshold">Seuil d'alerte seconde niveau (-1 si non défini).</param>
        /// <param name="priority">Priorité d'affichage du compteur (minimum en premier).</param>
        void CreateCounter(string label, string code, long warningThreshold, long criticalThreshold, int priority);

        /// <summary>
        /// Ouverture d'une nouvelle instance de base de données.
        /// </summary>
        /// <param name="dataBaseName">Nom de la base de données.</param>
        /// <param name="description">Description du manager associé.</param>
        void OpenDataBase(string dataBaseName, IManagerDescription description);

        /// <summary>
        /// Démarre un processus.
        /// </summary>
        /// <param name="processName">Nom du processus.</param>
        void StartProcess(string processName);

        /// <summary>
        /// On termine l'enristrement du process et on l'ajoute à la base de données.
        /// </summary>
        /// <param name="dataBaseName">Nom de la base de données.</param>
        /// <returns>Durée du processus.</returns>
        long StopProcess(string dataBaseName);

        /// <summary>
        /// Incrémente le compteur.
        /// </summary>
        /// <param name="counterDefinitionCode">Compteur.</param>
        /// <param name="value">Increment du compteur.</param>
        void IncValue(string counterDefinitionCode, long value);

        /// <summary>
        /// Réinitialise un base de données.
        /// </summary>
        /// <param name="dataBaseName">Nom de la base de données.</param>
        void Reset(string dataBaseName);
    }
}
