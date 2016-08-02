using System.Collections.Generic;

namespace Kinetix.Search.Model {

    /// <summary>
    /// Chunk de documents.
    /// </summary>
    public class SearchChunk {

        /// <summary>
        /// Ids des documents.
        /// </summary>
        public ICollection<string> Ids {
            get;
            set;
        }
    }
}
