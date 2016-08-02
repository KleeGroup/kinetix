using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI.WebControls;

namespace Kinetix.ComponentModel.DataAnnotations {

    /// <summary>
    /// Attribut indiquant la taille en pixels pour le rendu d'une propriété.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [DefaultProperty("Width")]
    public sealed class WidthAttribute : Attribute {

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="width">Taille du champ de présentation.</param>
        public WidthAttribute(string width) {
            if (string.IsNullOrEmpty(width)) {
                throw new ArgumentNullException("width");
            }

            this.Width = width;
            Unit.Parse(width, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Obtient ou définit la taille du champ de présentation.
        /// </summary>
        public string Width {
            get;
            private set;
        }
    }
}
