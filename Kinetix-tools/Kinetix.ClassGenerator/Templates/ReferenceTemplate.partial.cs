using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.ClassGenerator.Model;

namespace Kinetix.ClassGenerator.Templates {

    /// <summary>
    /// Partial du template de génération de références Typescript.
    /// </summary>
    public partial class ReferenceTemplate {

        /// <summary>
        /// Références.
        /// </summary>
        public IEnumerable<ModelClass> References { get; set; }

        /// <summary>UseTypeSafeConstValues
        /// Transforme une liste de constantes en type Typescript.
        /// </summary>
        /// <param name="reference">La liste de constantes.</param>
        /// <returns>Le type de sorte.</returns>
        private string GetConstValues(ModelClass reference) {
            var constValues = string.Join(" | ", reference.ConstValues.Values.Select(value => value.Code));
            if (constValues == string.Empty) {
                return "string";
            } else {
                return constValues;
            }
        }

        /// <summary>
        /// Transforme le type en type Typescript.
        /// </summary>
        /// <param name="property">La propriété dont on cherche le type.</param>
        /// <returns>Le type en sortie.</returns>
        private string ToTSType(ModelProperty property) {
            if (property.Name == "Code") {
                return $"{property.Class.Name}Code";
            } else if (property.Name.EndsWith("Code", StringComparison.Ordinal)) {
                return Utils.ToFirstUpper(property.Name);
            }

            return ToTSType(property.DataDescription?.Domain?.DataType);
        }

        /// <summary>
        /// Transforme le type en type Typescript.
        /// </summary>
        /// <param name="type">Le type d'entrée.</param>
        /// <returns>Le type en sortie.</returns>
        private string ToTSType(string type) {
            switch (type) {
                case "int":
                case "decimal":
                case "short":
                case "System.TimeSpan":
                    return "number";
                case "System.DateTime":
                case "System.Guid":
                case "string":
                    return "string";
                case "bool":
                    return "boolean";
                default:
                    return "any";
            }
        }
    }
}