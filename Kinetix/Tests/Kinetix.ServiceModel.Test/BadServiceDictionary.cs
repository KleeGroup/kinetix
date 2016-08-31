using System.Collections.Generic;

namespace Kinetix.ServiceModel.Test {
    /// <summary>
    /// Implémentation de IBadContractDictionary pour les tests.
    /// </summary>
    public class BadServiceDictionary : IBadContractDictionary {
        /// <summary>
        /// Retourne une liste d'objets.
        /// </summary>
        /// <returns>Liste d'objets.</returns>
        public IDictionary<object, object> GetObjectList() {
            return null;
        }
    }
}
