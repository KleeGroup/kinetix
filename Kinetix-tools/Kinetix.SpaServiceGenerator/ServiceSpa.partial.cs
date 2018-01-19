using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.SpaServiceGenerator.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kinetix.SpaServiceGenerator {

    /// <summary>
    /// Template de contrôlleur.
    /// </summary>
    public partial class ServiceSpa {

        /// <summary>
        /// Le chemin vers le répertoire de définitions.
        /// </summary>
        public string DefinitionPath { get; set; }

        /// <summary>
        /// Le nom du projet, namespace global (exemple : "Chaine").
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// La liste des services.
        /// </summary>
        public IEnumerable<ServiceDeclaration> Services { get; set; }

        /// <summary>
        /// Récupère le type Typescript correspondant à un type C#.
        /// </summary>
        /// <param name="type">Le type C#.</param>
        /// <returns>Le type TS.</returns>
        private static string GetTSType(INamedTypeSymbol type) {

            if (type.IsGenericType) {
                if (type.Name == "ICollection" || type.Name == "IEnumerable") {
                    return $"{GetTSType(type.TypeArguments.First() as INamedTypeSymbol)}[]";
                }

                if (type.Name == "Nullable") {
                    return GetTSType(type.TypeArguments.First() as INamedTypeSymbol);
                }

                if (type.Name == "Nullable") {
                    return GetTSType(type.TypeArguments.First() as INamedTypeSymbol);
                }

                if (type.Name == "IDictionary") {
                    return $"{{[key: string]: {GetTSType(type.TypeArguments.Last() as INamedTypeSymbol)}}}";
                }

                if (type.Name == "QueryInput") {
                    return $"QueryInput<{GetTSType(type.TypeArguments.First() as INamedTypeSymbol)}>";
                }

                if (type.Name == "QueryOutput") {
                    return $"QueryOutput<{GetTSType(type.TypeArguments.First() as INamedTypeSymbol)}>";
                }

                return $"{type.Name}<{GetTSType(type.TypeArguments.First() as INamedTypeSymbol)}>";
            }

            if (type.Name == "QueryInput") {
                return "QueryInput";
            }

            if (type.Name == "QueryOutput") {
                return "QueryOutput";
            }

            switch (type.SpecialType) {
                case SpecialType.None:
                    return type.Name;
                case SpecialType.System_Int32:
                case SpecialType.System_Decimal:
                    return "number";
                case SpecialType.System_DateTime:
                case SpecialType.System_String:
                    return "string";
                case SpecialType.System_Boolean:
                    return "boolean";
                case SpecialType.System_Void:
                    return "void";
            }

            return "any";
        }

        /// <summary>
        /// Vérifie que le type est un array.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>Oui / Non.</returns>
        private static bool IsArray(INamedTypeSymbol type) {
            return type.IsGenericType && (type.Name == "ICollection" || type.Name == "IEnumerable");
        }

        /// <summary>
        /// Vérifie que le type est une primitive.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>Oui / Non.</returns>
        private static bool IsPrimitive(INamedTypeSymbol type) {
            return type.SpecialType != SpecialType.None;
        }

        /// <summary>
        /// Récupère la liste d'imports de types pour les services.
        /// </summary>
        /// <returns>La liste d'imports (type, chemin du module, nom du fichier).</returns>
        private IEnumerable<Tuple<string, string, string>> GetImportList() {
            var returnTypes = Services.SelectMany(service => GetTypes(service.ReturnType));
            var parameterTypes = Services.SelectMany(service => service.Parameters.SelectMany(parameter => GetTypes(parameter.Type)));

            var types = returnTypes.Concat(parameterTypes)
                .Where(type => !type.ContainingNamespace.ToString().Contains("Kinetix") && !type.ContainingNamespace.ToString().Contains("System"));

            var referenceTypes = types.Where(type =>
                type.DeclaringSyntaxReferences.Any(s => {
                    var classDecl = s.SyntaxTree
                        .GetRoot()
                        .DescendantNodes()
                        .OfType<ClassDeclarationSyntax>()
                        .First();
                    var hasRefAttribute = classDecl
                        .AttributeLists
                        .FirstOrDefault()
                        ?.Attributes
                        .Any(attr => attr.Name.ToString() == "Reference") ?? false;

                    if (!hasRefAttribute) {
                        return false;
                    } else {
                        return !classDecl
                            .Members
                            .OfType<PropertyDeclarationSyntax>()
                            .Any(p => p.Identifier.ToString() == "Id");
                    }
                }));

            var imports = types.Except(referenceTypes).Select(type => {
                var module = type.ContainingNamespace.ToString()
                    .Replace($"{ProjectName}.", string.Empty)
                    .Replace("DataContract", string.Empty)
                    .Replace("Contract", string.Empty)
                    .Replace(".", "/")
                    .ToDashCase();

                return Tuple.Create(type.Name, $"{DefinitionPath}/{module}", type.Name.ToDashCase());
            }).Distinct().ToList();

            if (returnTypes.Any(type => type?.Name == "QueryOutput")) {
                imports.Add(Tuple.Create("QueryInput, QueryOutput", "focus4", "collections"));
            } else if (parameterTypes.Any(type => type?.Name == "QueryInput")) {
                imports.Add(Tuple.Create("QueryInput", "focus4", "collections"));
            }

            if (referenceTypes.Any()) {
                imports.Add(Tuple.Create(string.Join(", ", referenceTypes.Select(t => t.Name).OrderBy(x => x)), DefinitionPath, "references"));
            }

            return imports.OrderBy(type => type.Item1);
        }

        /// <summary>
        /// Récupère tous les types et sous-types constitutants d'un type donné (génériques).
        /// </summary>
        /// <param name="type">le type d'entrée.</param>
        /// <returns>Les types de sorties.</returns>
        private IEnumerable<INamedTypeSymbol> GetTypes(INamedTypeSymbol type) {
            if (type != null && type.SpecialType == SpecialType.None) {
                yield return type;
                if (type.IsGenericType) {
                    foreach (var typeArg in type.TypeArguments) {
                        if (typeArg is INamedTypeSymbol namedTypeArg) {
                            foreach (var subTypeArg in GetTypes(namedTypeArg)) {
                                yield return subTypeArg;
                            }
                        }
                    }
                }
            }
        }
    }
}
