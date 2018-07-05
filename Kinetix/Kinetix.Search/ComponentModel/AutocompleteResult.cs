using System.Collections.Generic;

namespace Kinetix.Search.ComponentModel
{
    /// <summary>
    /// Résultat d'une requête d'autocomplete.
    /// </summary>
    public class AutocompleteResult
    {
        /// <summary>
        /// Les données.
        /// </summary>
        public ICollection<AutocompleteItem> Data { get; set; }

        /// <summary>
        /// Le nombre total de résultats.
        /// </summary>
        public int TotalCount { get; set; }
    }

}

