using System;
using System.Runtime.Serialization;

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Classe de test.
    /// </summary>
    public class BeanOtherInvalidDomainType {
        /// <summary>
        /// Identifiant.
        /// </summary>
        [DataMember(IsRequired = false)]
        [Domain("LIBELLE_COURT")]
        public RuntimeTypeHandle OtherId {
            get;
            set;
        }
    }
}
