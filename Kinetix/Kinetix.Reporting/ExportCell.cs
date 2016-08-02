using System;
using System.Drawing;

namespace Kinetix.Reporting {

    /// <summary>
    /// Classe modélisant l'export d'une cellule.
    /// </summary>
    public sealed class ExportCell {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="colspan">Colspan de la cellule.</param>
        public ExportCell(int colspan) {
            if (colspan < 1) {
                throw new NotSupportedException("Colspan must be at greater than zero.");
            }

            this.Colspan = colspan;
        }

        /// <summary>
        /// Colspan de la cellule.
        /// </summary>
        public int Colspan {
            get;
            set;
        }

        /// <summary>
        /// Couleur de la cellule.
        /// </summary>
        public Color BackgroundColor {
            get;
            set;
        }
    }
}
