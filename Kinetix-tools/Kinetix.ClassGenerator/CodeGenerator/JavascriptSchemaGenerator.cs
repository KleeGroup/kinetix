using System.IO;
using Kinetix.ClassGenerator.Model;

namespace Kinetix.ClassGenerator.CodeGenerator {

    /// <summary>
    /// Générateur des objets javascripts.
    /// </summary>
    public class JavascriptSchemaGenerator : AbstractJavascriptGenerator {

        /// <summary>
        /// Génère le noeud de la proprité.
        /// </summary>
        /// <param name="writer">Flux de sortie.</param>
        /// <param name="property">Propriété.</param>
        /// <param name="isLast">True s'il s'agit du dernier noeud de la classe.</param>
        protected override void WritePropertyNode(TextWriter writer, ModelProperty property, bool isLast) {
            writer.WriteLine(TAB + TAB + FormatJsPropertyName(property.Name) + OPEN_BRACKET);
            if (property.DataDescription.Domain != null) {
                writer.WriteLine(TAB + TAB + TAB + "domain: '" + property.DataDescription.Domain.Code + "'" + COMA);
            } else if (property.IsFromComposition) {
                writer.WriteLine(TAB + TAB + TAB + "domain: 'DO_" + property.Name.ToUpper() + "'" + COMA);
            }

            writer.WriteLine(TAB + TAB + TAB + "required: " + (property.DataMember.IsRequired ? "true" : "false"));
            WriteCloseBracket(writer, 2, isLast);
        }
    }
}
