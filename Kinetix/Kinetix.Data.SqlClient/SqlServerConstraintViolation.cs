namespace Kinetix.Data.SqlClient {

    /// <summary>
    /// Violation de contraintes.
    /// </summary>
    public enum SqlServerConstraintViolation {

        /// <summary>
        /// Contrainte CHECK.
        /// </summary>
        Check,

        /// <summary>
        /// Contrainte unique.
        /// </summary>
        Unique,

        /// <summary>
        /// Clef étrangère (cas où une valeur insérée ou mise à jour ne correspond pas à une clé primaire dans la table étrangère).
        /// </summary>
        ForeignKey,

        /// <summary>
        /// Clef étrangère (cas où l'objet est pointé par une clé étrangère et ne peut donc pas être supprimé).
        /// </summary>
        ReferenceKey,
    }
}
