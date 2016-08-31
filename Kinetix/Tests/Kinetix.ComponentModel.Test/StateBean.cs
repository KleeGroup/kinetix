using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Kinetix.ComponentModel.Test.Contract;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Linq;

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Bean sans clef primaire.
    /// </summary>
    [DataContract]
    [Table("BEAN")]
    [DefaultProperty("Name")]
    public class StateBean : Bean, IBeanState {
        /// <summary>
        /// Représente l'état du bean.
        /// </summary>
        public ChangeAction State {
            get;
            set;
        }
    }
}
