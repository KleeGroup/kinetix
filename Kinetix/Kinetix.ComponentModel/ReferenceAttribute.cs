using System;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Attribut marquant les classes de référence.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ReferenceAttribute : Attribute {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public ReferenceAttribute() {
            this.IsStatic = false;
        }

        /// <summary>
        /// Constructeur paramétré.
        /// </summary>
        /// <param name="isStatic"><code>True</code> si la liste est statique, <code>False</code> sinon.</param>
        public ReferenceAttribute(bool isStatic) {
            this.IsStatic = isStatic;
        }

        /// <summary>
        /// Obtient ou définit si la liste de référence est statique.
        /// </summary>
        public bool IsStatic {
            get;
            private set;
        }
    }
}
