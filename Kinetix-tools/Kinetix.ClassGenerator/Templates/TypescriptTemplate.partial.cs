using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kinetix.ClassGenerator.Model;

namespace Kinetix.ClassGenerator.Templates {

    /// <summary>
    /// Partial du template de génération de code Typescript.
    /// </summary>
    public partial class TypescriptTemplate {

        /// <summary>
        /// Objet de modèle.
        /// </summary>
        public ModelClass Model { get; set; }

        /// <summary>
        /// Namespace de base de l'application.
        /// </summary>
        public string RootNamespace { get; set; }


        private string FocusImport {
            get {
                var list = new List<string>();
                if (!Model.PropertyList.All(p => IsArray(p) || p.IsFromComposition)) {
                    list.Add("EntityField");
                }

                if (Model.PropertyList.Any(p => IsArray(p))) {
                    list.Add("StoreListNode");
                }

                if (Model.ParentClass == null) {
                    list.Add("StoreNode");
                }

                return string.Join(", ", list);
            }
        }

        private string GetDomain(ModelProperty property) {
            return property?.DataDescription?.Domain?.Code;
        }

        private IEnumerable<string> GetDomainList() {
            return Model.PropertyList
                .Select(property => property?.DataDescription?.Domain?.Code)
                .Where(domain => domain != null)
                .Distinct();
        }

        /// <summary>
        /// Récupère la liste d'imports de types pour les services.
        /// </summary>
        /// <returns>La liste d'imports (type, chemin du module, nom du fichier).</returns>
        private IEnumerable<Tuple<string, string, string>> GetImportList() {
            var types = Model.PropertyList
                .Where(property =>
                    (property.DataDescription?.ReferenceClass?.FullyQualifiedName.StartsWith(RootNamespace, StringComparison.Ordinal) ?? false)
                 && property.DataType != "string" && property.DataType != "int")
                .Select(property => property.DataDescription?.ReferenceClass?.FullyQualifiedName);

            string parentClassName = null;
            if (Model.ParentClass != null) {
                parentClassName = Model.ParentClass.FullyQualifiedName;
                types = types.Concat(new[] { parentClassName });
            }

            var currentModule = GetModuleName(Model.FullyQualifiedName);

            var imports = types.Select(type => {
                var module = GetModuleName(type);
                var name = type.Split('.').Last();

                if (module == currentModule) {
                    module = $".";
                } else {
                    module = $"../{module}";
                }

                return Tuple.Create($"{name}, {name}Node" + (type == parentClassName ? $", {name}Entity" : string.Empty), module, name.ToDashCase());
            }).Distinct().OrderBy(type => type.Item1).ToList();

            var references = Model.PropertyList
                .Where(property => property.DataDescription?.ReferenceClass != null && property.DataType == "string")
                .Select(property => $"{property.DataDescription.ReferenceClass.Name}Code")
                .Distinct();

            if (references.Any()) {
                imports.Add(Tuple.Create(string.Join(", ", references), "..", "references"));
            }

            return imports;
        }

        /// <summary>
        /// Récupère le nom du module à partir du nom complet.
        /// </summary>
        /// <param name="fullyQualifiedName">Nom complet.</param>
        /// <returns>Le nom du module.</returns>
        private string GetModuleName(string fullyQualifiedName) =>
            fullyQualifiedName.Split('.')[1]
                .Replace("DataContract", string.Empty)
                .Replace("Contract", string.Empty)
                .ToLower();

        private string GetReferencedType(ModelProperty property) {
            if (GetDomain(property) != null) {
                return null;
            }

            return property?.DataDescription?.ReferenceClass?.Name;
        }

        private bool IsArray(ModelProperty property) {
            return property.IsCollection && property.DataDescription?.Domain?.DataType == null;
        }

        /// <summary>
        /// Transforme une liste de constantes en type Typescript.
        /// </summary>
        /// <param name="constValues">La liste de constantes.</param>
        /// <returns>Le type de sorte.</returns>
        private string ToTSType(IEnumerable<string> constValues) {
            return string.Join(" | ", constValues);
        }

        /// <summary>
        /// Transforme le type en type Typescript.
        /// </summary>
        /// <param name="property">La propriété dont on cherche le type.</param>
        /// <param name="removeBrackets">Retire les brackets sur les types de liste.</param>
        /// <returns>Le type en sortie.</returns>
        private string ToTSType(ModelProperty property, bool removeBrackets = false) {
            var type = property.DataDescription?.Domain?.DataType;
            switch (type) {
                case null:
                    type = property.DataDescription?.ReferenceClass?.FullyQualifiedName;
                    if (property.IsCollection) {
                        type = $"System.Collections.Generic.ICollection<{type}>";
                    }

                    break;
                case "System.Collections.Generic.ICollection<string>":
                    return "string[]";
                case "System.Collections.Generic.ICollection<int>":
                    return "number[]";
            }

            if (type == "string" && property.DataDescription.ReferenceClass != null) {
                return $"{property.DataDescription.ReferenceClass.Name}Code";
            }

            return ToTSType(type, removeBrackets);
        }

        /// <summary>
        /// Transforme le type en type Typescript.
        /// </summary>
        /// <param name="type">Le type d'entrée.</param>
        /// <param name="removeBrackets">Retire les brackets sur les types de liste.</param>
        /// <returns>Le type en sortie.</returns>
        private string ToTSType(string type, bool removeBrackets = false) {
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
                    if (type?.StartsWith("System.Collections.Generic.ICollection") ?? false) {
                        var typeName = $"{ToTSType(Regex.Replace(type, ".+<(.+)>", "$1"))}";
                        if (!removeBrackets) {
                            typeName += "[]";
                        }

                        return typeName;
                    }

                    if (type?.StartsWith(RootNamespace) ?? false) {
                        return type.Split('.').Last();
                    }

                    return "any";
            }
        }
    }
}