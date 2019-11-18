using System;
using System.Collections.Generic;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Helper contenant des propriétés static servant pour les différentes autres classes.
    /// </summary>
    public class ReferenceBrokerTestHelper {
        /// <summary>
        /// Code langue actuel. Peut être changé.
        /// </summary>
        public static string LangueCode = "EN";

        /// <summary>
        /// Dictionary: (property code, language) -> value
        /// Représente la table de traduction.
        /// </summary>
        public static IDictionary<Tuple<string, string>, string> Traduction = new Dictionary<Tuple<string, string>, string>();
    }

}
