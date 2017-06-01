using System.Collections.Generic;

namespace Kinetix.TestUtils.Helpers {

    /// <summary>
    /// Provides some helper function to work with Collections.
    /// </summary>
    public static class CollectionUtils {

        /// <summary>
        /// Compares two collections. Returns true if the collections are equal, not considering the order.
        /// (every element in one collection is also in the other collection).
        /// </summary>
        /// <typeparam name="T">Type of the collections.</typeparam>
        /// <param name="collection1">First collection to compare.</param>
        /// <param name="collection2">Second collection to compare.</param>
        /// <returns>True if the collections are equal, not considering the order.</returns>
        public static bool UnsortedEquals<T>(ICollection<T> collection1, ICollection<T> collection2) {
            foreach (var i in collection1) {
                bool isMatched = false;
                foreach (var j in collection2) {
                    if (i.Equals(j)) {
                        isMatched = true;
                        break;
                    }
                }

                if (!isMatched) {
                    return false;
                }
            }

            return true;
        }
    }
}
