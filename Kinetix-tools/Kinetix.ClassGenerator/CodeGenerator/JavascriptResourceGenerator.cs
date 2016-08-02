using System.IO;
using Kinetix.ClassGenerator.Model;

namespace Kinetix.ClassGenerator.CodeGenerator {

    /// <summary>
    /// Générateur des objets de traduction javascripts.
    /// </summary>
    public class JavascriptResourceGenerator : AbstractJavascriptGenerator {

        /// <summary>
        /// Génère le noeud de la proprité.
        /// </summary>
        /// <param name="writer">Flux de sortie.</param>
        /// <param name="property">Propriété.</param>
        /// <param name="isLast">True s'il s'agit du dernier noeud de la classe.</param>
        protected override void WritePropertyNode(TextWriter writer, ModelProperty property, bool isLast) {
            writer.WriteLine(TAB + TAB + FormatJsPropertyName(property.Name) + ": " + DOUBLE_QUOTE + property.DataDescription.Libelle + DOUBLE_QUOTE + (isLast ? string.Empty : COMA));
        }
    }
}
