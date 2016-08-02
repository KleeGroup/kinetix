namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Interface d'un compteur représentant une grandeur sommable évoluant dans le temps.
    /// </summary>
    public interface ICounter {

        /// <summary>
        /// Retourne le nom de l'événement ayant déclenché la valeur min ou max
        /// (null si non renseigné).
        /// </summary>
        /// <param name="statType">CounterStatType.</param>
        /// <returns>Nom de l'évènement.</returns>
        string GetInfo(CounterStatType statType);

        /// <summary>
        /// Retourne une valeur du compteur.
        /// </summary>
        /// <param name="statType">CounterStatType.</param>
        /// <returns>Valeur.</returns>
        double GetValue(CounterStatType statType);

        /// <summary>
        /// Indique si la statistique gère des infos (min, max).
        /// </summary>
        /// <param name="statType">CounterStatType.</param>
        /// <returns>True si l'info est gérée.</returns>
        bool HasInfo(CounterStatType statType);
    }
}
