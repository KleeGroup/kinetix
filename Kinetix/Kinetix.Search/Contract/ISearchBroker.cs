using System.Collections.Generic;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Model;

namespace Kinetix.Search.Contract {

    /// <summary>
    /// Contrat des brokers de recherche.
    /// </summary>
    public interface ISearchBroker {
    }

    /// <summary>
    /// Contrat des brokers de recherche.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public interface ISearchBroker<TDocument> : ISearchBroker {

        /// <summary>
        /// Créé le type de document.
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
        /// <param name="id">L'id du document.</param>
        void Remove(string id);

        /// <summary>
        /// Supprime tous les documents.
        /// </summary>
        void Flush();

        /// <summary>
        /// Effectue une requête sur le champ text.
        /// </summary>
        /// <param name="text">Texte à chercher.</param>
        /// <param name="filterList">Filtrage sur des critères métiers</param>
        /// <param name="security">Filtrage de périmètre de sécurité.</param>
        /// <returns>Documents trouvés.</returns>
        IEnumerable<TDocument> Query(string text, string security = null, IDictionary<string, string> filterList = null);

        /// <summary>
        /// Effectue une recherche avancé.
        /// TODO : update doc
        /// </summary>
        /// <param name="input">Paramètre de recherche (facettes...)</param>
        /// <returns>Documents trouvés.</returns>
        QueryOutput<TDocument> AdvancedQuery(AdvancedQueryInput input);

        /// <summary>
        /// Effectue un count avancé.
        /// </summary>
        /// <param name="input">Entrée de la recherche.</param>
        /// <returns>Nombre de documents.</returns>
        long AdvancedCount(AdvancedQueryInput input);
    }
}
