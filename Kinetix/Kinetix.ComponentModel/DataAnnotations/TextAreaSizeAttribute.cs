using System;

namespace Kinetix.ComponentModel.DataAnnotations {

    /// <summary>
    /// Attribut définissant la taille de TextArea rendu pour un domaine.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TextAreaSizeAttribute : Attribute {

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        public TextAreaSizeAttribute() {
            this.Rows = 4;
            this.Columns = 50;
        }

        /// <summary>
        /// Obtient ou definit le nombre de colonnes du champ.
        /// </summary>
        public int Rows {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou definit le nombre de colonnes du champ.
        /// </summary>
        public int Columns {
            get;
            set;
        }
    }
}
