using System;
using System.Windows;

namespace Kinetix.ComponentModel.DataAnnotations {

    /// <summary>
    /// Attribut présentant l'alignement du domaine.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class AlignmentAttribute : Attribute {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="alignment">Alignement horizontale.</param>
        public AlignmentAttribute(HorizontalAlignment alignment) {
            this.Alignment = alignment;
        }

        /// <summary>
        /// Obtient l'alignement horizontal.
        /// </summary>
        public HorizontalAlignment Alignment {
            get;
            private set;
        }
    }
}
