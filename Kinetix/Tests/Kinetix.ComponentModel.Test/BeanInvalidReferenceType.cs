using System.Runtime.Serialization;

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Classe de test.
    /// </summary>
    public class BeanInvalidReferenceType {
        /// <summary>
        /// Identifiant.
        /// </summary>
        [DataMember(IsRequired = false)]
        [Domain("LIBELLE_COURT")]
        [ReferencedType(typeof(Kinetix.ComponentModel.Test.Contract.Bean))]
        public double OtherId {
            get;
            set;
        }
    }
}
