using System;

namespace Kinetix.ComponentModel.DataAnnotations {

    /// <summary>
    /// Type énuméré précisant le padding pour l'export des fichiers plats.
    /// </summary>
    public enum PaddingPosition {

        /// <summary>
        ///  Padding à droite.
        /// </summary>
        Right = 0,

        /// <summary>
        /// Padding à gauche.
        /// </summary>
        Left = 1,
    }

    /// <summary>
    /// Attribut destiné aux exports de fichiers plats.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class FlatFileField : Attribute {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="position">Position du champ pour l'export.</param>
        /// <param name="length">Taille du champ pour l'export.</param>
        /// <param name="paddingDirection">Padding à droite ou à gauche pour le champ.</param>
        public FlatFileField(int position, int length, PaddingPosition paddingDirection = PaddingPosition.Right) {
            if (length <= 0) {
                throw new NotSupportedException("Length has to be a positive integer.");
            }

            if (position < 0) {
                throw new NotSupportedException("position has to be a positive integer.");
            }

            this.Length = length;
            this.PaddingDirection = paddingDirection;
            this.Position = position;
        }

        /// <summary>
        /// Obtient la position du champs à l'export.
        /// </summary>
        public int Position {
            get;
            private set;
        }

        /// <summary>
        /// Obtient la taille du champ pour l'export.
        /// </summary>
        public int Length {
            get;
            private set;
        }

        /// <summary>
        /// Sens du padding (droite au gauche).
        /// </summary>
        public PaddingPosition PaddingDirection {
            get;
            private set;
        }
    }
}
