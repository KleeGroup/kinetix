using System.Collections.Generic;
using Kinetix.ComponentModel.Search;
using Kinetix.Search.Model;

namespace Kinetix.Search.Contract {

    /// <summary>
    /// Contrat des stores de recherche.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public interface ISearchStore<TDocument> {

        /// <summary>
        /// Créé l'index.
        /// </summary>
        void CreateDocumentType();

        /// <summary>
        /// Obtient un document à partir de son ID.
        /// </summary>
        /// <param name="id">ID du document.</param>
        /// <returns>Document.</returns>
        TDocument Get(string id);

        /// <summary>
        /// Pose un document dans l'index.
        /// </summary>
        /// <param name="document">Document à poser.</param>
        void Put(TDocument document);

        /// <summary>
        /// Pose un ensemble de documents dans l'index.
        /// </summary>
        /// <param name="documentList">Liste de documents.</param>
        void PutAll(IEnumerable<TDocument> documentList);

        /// <summary>
        /// Supprime un document dans l'index.
        /// </summary>
        /// <param name="id">ID du document.</param>
        void Remove(string id);

        /// <summary>
        /// Supprime tous les documents.
        /// </summary>
        void Flush();

        /// <summary>
        /// Effectue une recherche avancé.
        /// </summary>
        /// <param name="input">Entrée de la recherche.</param>
        /// <returns>Sortie de la recherche.</returns>
        QueryOutput<TDocument> AdvancedQuery(AdvancedQueryInput input);
    }
}
