using System.Collections.Generic;
using Kinetix.Search.Model;

namespace Kinetix.Search.Contract {

    /// <summary>
    /// Interface des chargeurs pour l'indexation.
    /// L'indexation en masse se fait par groupe (chunk) de documents.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public interface ISearchLoader<TDocument> {

        /// <summary>
        /// Enumérable des groupes de documents à indexer.
        /// </summary>
        /// <returns>Enumérable.</returns>
        IEnumerable<SearchChunk> LoadSearchChunkList();

        /// <summary>
        /// Charge les documents pour un groupe donné.
        /// </summary>
        /// <param name="chunk">Groupe de documents à charger.</param>
        /// <returns>Documents.</returns>
        ICollection<TDocument> LoadDocumentListBySearchChunk(SearchChunk chunk);
    }
}
