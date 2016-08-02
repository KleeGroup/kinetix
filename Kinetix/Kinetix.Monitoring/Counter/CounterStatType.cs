namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Type de statistiques gérées.
    /// </summary>
    public enum CounterStatType {

        /// <summary>
        /// Valeur totale.
        /// </summary>
        Total,

        /// <summary>
        /// Nombre de valeurs insérées.
        /// </summary>
        Hits,

        /// <summary>
        /// Dernière valeur (-1 si non renseigné).
        /// </summary>
        Last,

        /// <summary>
        /// Valeur min (-1 si non renseigné).
        /// </summary>
        Min,

        /// <summary>
        /// Valeur max (-1 si non renseigné).
        /// </summary>
        Max,

        /// <summary>
        /// Valeur moyenne (calculée).
        /// </summary>
        Avg,

        /// <summary>
        /// Somme des carrés.
        /// </summary>
        TotalOfSquares,

        /// <summary>
        /// Moyenne des sous process.
        /// </summary>
        SubAvg,

        /// <summary>
        /// Nombre de hits en dessous de 50 ms.
        /// </summary>
        Hits50,

        /// <summary>
        /// Nombre de hits entre 50 ms et 100ms.
        /// </summary>
        Hits100,

        /// <summary>
        /// Nombre de hits entre 100 ms et 200ms.
        /// </summary>
        Hits200,

        /// <summary>
        /// Nombre de hits entre 200 ms et 500ms.
        /// </summary>
        Hits500,

        /// <summary>
        /// Nombre de hits entre 500 ms et 1000ms.
        /// </summary>
        Hits1000,

        /// <summary>
        /// Nombre de hits entre 1000 ms et 2000ms.
        /// </summary>
        Hits2000,

        /// <summary>
        /// Nombre de hits entre 2000 ms et 5000ms.
        /// </summary>
        Hits5000,

        /// <summary>
        /// Nombre de hits entre 5000 ms et 10000ms.
        /// </summary>
        Hits10000
    }
}
