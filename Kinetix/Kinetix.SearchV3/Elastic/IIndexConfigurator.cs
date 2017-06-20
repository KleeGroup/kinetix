using Nest;

namespace Kinetix.Search.Elastic {

    /// <summary>
    /// Contrat des configurateurs d'index.
    /// </summary>
    public interface IIndexConfigurator {

        /// <summary>
        /// Configure une création d'index.
        /// </summary>
        /// <param name="descriptor">Descripteur.</param>
        /// <returns>Descripteur Fluent.</returns>
        CreateIndexDescriptor Configure(CreateIndexDescriptor descriptor);
    }
}
