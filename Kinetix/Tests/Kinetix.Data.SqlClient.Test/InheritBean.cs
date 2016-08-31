using System.Runtime.Serialization;

namespace Kinetix.Data.SqlClient.Test {

    /// <summary>
    /// Classe héritant de Bean, contenant une propriété non primitive.
    /// </summary>
    public class InheritBean : PkBean {

        /// <summary>
        /// Propriété comportant le bean hérité.
        /// </summary>
        [DataMember]
        public Bean Bean {
            get;
            set;
        }
    }
}
