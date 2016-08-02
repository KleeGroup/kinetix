using System.Collections;

namespace Kinetix.ComponentModel.Search {

    /// <summary>
    /// Interface de OrderedDictionary.
    /// </summary>
    public interface IOrderedDictionary {

        /// <summary>
        /// Liste interne.
        /// </summary>
        IList InnerList { get; }
    }
}
