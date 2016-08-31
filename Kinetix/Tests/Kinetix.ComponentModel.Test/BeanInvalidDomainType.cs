using System.Runtime.Serialization;

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Classe de test.
    /// </summary>
    public class BeanInvalidDomainType {
        /// <summary>
        /// Identifiant.
        /// </summary>
        [DataMember(IsRequired = false)]
        [Domain("LIBELLE_COURT")]
        public int? OtherId {
            get;
            set;
        }
    }
}
