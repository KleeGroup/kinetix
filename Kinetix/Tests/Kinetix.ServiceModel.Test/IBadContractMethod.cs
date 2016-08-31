using System.Collections.Generic;

namespace Kinetix.ServiceModel.Test {
    /// <summary>
    /// Contrat d'exemple éronné pour le test de la factory de proxy.
    /// </summary>
    public interface IBadContractMethod {
        /// <summary>
        /// Retourne une liste d'objets.
        /// </summary>
        /// <param name="arg">Un argument.</param>
        /// <returns>Liste d'objets.</returns>
        [ReferenceAccessor]
        ICollection<object> GetObjectList(int arg);
    }
}
