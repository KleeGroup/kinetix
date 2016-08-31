using System.Collections.Generic;

namespace Kinetix.ServiceModel.Test {
    /// <summary>
    /// Contrat d'exemple éronné pour le test de la factory de proxy.
    /// </summary>
    public interface IBadContractDictionary {
        /// <summary>
        /// Retourne une liste d'objets.
        /// </summary>
        /// <returns>Liste d'objets.</returns>
        [ReferenceAccessor]
        IDictionary<object, object> GetObjectList();
    }
}
