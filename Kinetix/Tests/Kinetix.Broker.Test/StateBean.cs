using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Linq;
using System.Runtime.Serialization;
using Kinetix.ComponentModel;

namespace Kinetix.Broker.Test {
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
