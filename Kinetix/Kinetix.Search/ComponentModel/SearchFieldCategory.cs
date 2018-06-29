namespace Kinetix.Search.ComponentModel {

    /// <summary>
    /// Catégorie de champ pour le moteur de recherche.
    /// </summary>
    public enum SearchFieldCategory {

        /// <summary>
        /// Champ ID : indexé et stocké tel quel.
        /// </summary>
        Id,

        /// <summary>
        /// Champ de résultat, destiné à l'affichage : non indexé, stocké.
        /// </summary>
        Result,

        /// <summary>
        /// Champ de recherche : indexé tokenisé en minuscule, non stocké.
        /// </summary>
        Search,

        /// <summary>
        /// Champ de recherche : indexé tokenisé en minuscule, non stocké.
        /// </summary>
        TextSearch,

        /// <summary>
        /// Champ de filtrage de sécurité : indexé tokenisé en minuscule, non stocké.
        /// </summary>
        Security,

        /// <summary>
        /// Champ de tri : indexé en minuscule, non stocké.
        /// </summary>
        Sort,

        /// <summary>
        /// Champ de facette : indexé tel quel, non stocké.
        /// </summary>
        Term,

        /// <summary>
        /// Champ de facette contenant une liste de valeurs : indexé tel quel, non stocké.
        /// </summary>
        ListTerm,

        /// <summary>
        /// Champ utilisé pour filtrer les résultats de recherche.
        /// </summary>
        Filter
    }
}
