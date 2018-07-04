namespace Kinetix.Search.ComponentModel
{

    /// <summary>
    /// Critère de boost.
    /// </summary>
    public class Boost
    {
        /// <summary>
        /// Champ sur lequel porte le boost.
        /// </summary>
        public string Field {
            get;
            set;
        }

        /// <summary>
        /// Valeur du boost.
        /// </summary>
        public decimal BoostValue {
            get;
            set;
        }

    }
}