namespace Kinetix.Broker {

    /// <summary>
    /// Type d'expression géré.
    /// </summary>
    public enum Expression {

        /// <summary>
        /// L'expression est une date dans l'intervalle.
        /// </summary>
        Between,

        /// <summary>
        /// L'expression est contenue dans la chaine de caractères.
        /// </summary>
        Contains,

        /// <summary>
        /// La valeur est équivalente.
        /// </summary>
        Equals,

        /// <summary>
        /// La valeur est supérieur ou égale.
        /// </summary>
        GreaterOrEquals,

        /// <summary>
        /// La valeur est inférieure ou égale.
        /// </summary>
        LowerOrEquals,

        /// <summary>
        /// L'expression termine par la chaine de caractères.
        /// </summary>
        EndsWith,

        /// <summary>
        /// L'expression est supérieure.
        /// </summary>
        Greater,

        /// <summary>
        /// La valeur est nulle.
        /// </summary>
        IsNull,

        /// <summary>
        /// La valeur n'est pas nulle.
        /// </summary>
        IsNotNull,

        /// <summary>
        /// La valeur est inférieure.
        /// </summary>
        Lower,

        /// <summary>
        /// La valeur commence par la chaine.
        /// </summary>
        StartsWith,

        /// <summary>
        /// La valeur ne commence pas par la chaine.
        /// </summary>
        NotStartsWith,

        /// <summary>
        /// La valeur est différente.
        /// </summary>
        NotEquals,
    }
}
