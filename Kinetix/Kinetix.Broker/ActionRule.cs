namespace Kinetix.Broker {

    /// <summary>
    /// Enumération du rendu des actions possibles
    /// sur le champ.
    /// </summary>
    public enum ActionRule {

        /// <summary>
        /// Aucune action sur le champ n'est envisagé.
        /// </summary>
        DoNothing,

        /// <summary>
        /// Mise à jour du champ.
        /// </summary>
        Update,

        /// <summary>
        /// Mise à jour incrémental du champ qui est
        /// de type <code>int</code>.
        /// </summary>
        IncrementalUpdate,

        /// <summary>
        /// Vérifie que le champ est égal à une valeur.
        /// </summary>
        Check
    }
}
