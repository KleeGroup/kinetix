using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Interface contractualisant les collections d'objet.
    /// </summary>
    /// <typeparam name="T">Type contenu dans la collection.</typeparam>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Nommage distinct non applicable ici.")]
    public interface IDTCollection<T> : IList<T>
        where T : class {

        /// <summary>
        /// Retourne la liste des éléments supprimés.
        /// </summary>
        IList<T> RemovedItems {
            get;
        }
    }

    /// <summary>
    /// Interface contractualisant les collections d'objet.
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Nommage distinct non applicable ici.")]
    public interface IDTCollection : IList {

        /// <summary>
        /// Retourne la liste des éléments supprimés.
        /// </summary>
        IList RemovedItems {
            get;
        }
    }
}
