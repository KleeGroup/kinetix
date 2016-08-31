using System.Collections.Generic;

namespace Kinetix.ServiceModel.Test {
    /// <summary>
    /// Implémentation de IBadContractMethod pour les tests.
    /// </summary>
    public class BadServiceMethod : IBadContractMethod {
        /// <summary>
        /// Retourne une liste d'objets.
        /// </summary>
        /// <param name="arg">Un argurment.</param>
        /// <returns>Liste d'objets.</returns>
        public ICollection<object> GetObjectList(int arg) {
            return null;
        }
    }
}
