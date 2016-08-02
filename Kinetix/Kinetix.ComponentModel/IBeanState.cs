using System.Data.Linq;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Interface définissant l'accès à l'état d'un élément d'une collection.
    /// </summary>
    public interface IBeanState {

        /// <summary>
        /// Etat de l'élement.
        /// </summary>
        ChangeAction State {
            get;
            set;
        }
    }
}
