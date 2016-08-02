using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.Common {

    /// <summary>
    /// Extensions pour les objets du modèle syntaxique.
    /// </summary>
    internal static class SyntaxExtensions {

        private static readonly Regex _dalContractFileRegex = new Regex(@"\\DAL\.Interface\\IDal[^\\]*.cs");
        private static readonly Regex _dalImplementationFileRegex = new Regex(@"\\DAL\.Implementation\\Dal[^\\]*.cs");
        private static readonly Regex _serviceContractFileRegex = new Regex(@"\\[^\\]*\.Contract\\[^\\]*Contract\\IService[^\\]*\.cs");
        private static readonly Regex _serviceImplementationFileRegex = new Regex(@"\\Service\.Implementation\\Service[^\\]*.cs");

        /// <summary>
        /// Obtient le nom d'une classe.
        /// </summary>
        /// <param name="classNode">Node de la classe.</param>
        /// <returns>Nom non qualifié de la classe.</returns>
        public static string GetClassName(this ClassDeclarationSyntax classNode) {
            return classNode.Identifier.ValueText;
        }

        /// <summary>
        /// Obtient le nom d'une interface.
        /// </summary>
        /// <param name="classNode">Node de l'interface.</param>
        /// <returns>Nom non qualifié de l'interface.</returns>
        public static string GetInterfaceName(this InterfaceDeclarationSyntax classNode) {
            return classNode.Identifier.ValueText;
        }

        /// <summary>
        /// Obtient le nombre de lignes d'une localisation.
        /// </summary>
        /// <param name="location">Localisation.</param>
        /// <returns>Nombre de lignes.</returns>
        public static int GetLineCount(this Location location) {
            if (location == Location.None) {
                return 0;
            }

            var lineSpan = location.GetLineSpan();
            return lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line + 1;
        }

        /// <summary>
        /// Obtient la localisation du nom de la méthode.
        /// </summary>
        /// <param name="methNode">Node de la méthode.</param>
        /// <returns>Localisation.</returns>
        public static Location GetMethodLocation(this MethodDeclarationSyntax methNode) {
            return methNode.ChildTokens()
                            .First(x => x.Kind() == SyntaxKind.IdentifierToken)
                            .GetLocation();
        }

        /// <summary>
        /// Obtient le nom d'une méhode.
        /// </summary>
        /// <param name="methNode">Node de la méhode.</param>
        /// <returns>Nom de la méthode.</returns>
        public static string GetMethodName(this MethodDeclarationSyntax methNode) {
            return methNode.Identifier.ValueText;
        }

        /// <summary>
        /// Obtient la localisation du nom du node de déclaration.
        /// </summary>
        /// <param name="node">Node de la classe.</param>
        /// <returns>Localisation.</returns>
        public static Location GetNameDeclarationLocation(this SyntaxNode node) {
            return node.ChildTokens()
                            .First(x => x.Kind() == SyntaxKind.IdentifierToken)
                            .GetLocation();
        }

        /// <summary>
        /// Obtient le nom de l'espace de nom d'une classe.
        /// </summary>
        /// <param name="classNode">Node de la classe.</param>
        /// <returns>Nom complet de l'espace de nom.</returns>
        public static string GetNameSpaceFullName(this ClassDeclarationSyntax classNode) {
            var nsNode = classNode.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
            if (nsNode == null) {
                return null;
            }

            return nsNode.Name.ToString();
        }

        /// <summary>
        /// Indique si un document est un fichier de contrat de DAL.
        /// </summary>
        /// <param name="tree">Arbre du document.</param>
        /// <returns><code>True</code> si fichier d'implémentation de DAL.</returns>
        public static bool IsDalContractFile(this SyntaxTree tree) {
            var filePath = tree.FilePath;
            if (string.IsNullOrEmpty(filePath)) {
                return false;
            }

            return _dalContractFileRegex.IsMatch(filePath);
        }

        /// <summary>
        /// Indique si un document est un fichier d'implémentation de DAL.
        /// </summary>
        /// <param name="tree">Arbre du document.</param>
        /// <returns><code>True</code> si fichier d'implémentation de DAL.</returns>
        public static bool IsDalImplementationFile(this SyntaxTree tree) {
            var filePath = tree.FilePath;
            if (string.IsNullOrEmpty(filePath)) {
                return false;
            }

            return _dalImplementationFileRegex.IsMatch(filePath);
        }

        /// <summary>
        /// Indique si une méthode est publique.
        /// </summary>
        /// <param name="methNode">Node de la méthode.</param>
        /// <returns><code>True</code> si la méthode est publique.</returns>
        public static bool IsPublic(this MethodDeclarationSyntax methNode) {
            return methNode.Modifiers.Any(x => x.Kind() == SyntaxKind.PublicKeyword);
        }

        /// <summary>
        /// Indique si une classe est publique.
        /// </summary>
        /// <param name="classNode">Node de la classe.</param>
        /// <returns><code>True</code> si la classe est publique.</returns>
        public static bool IsPublic(this ClassDeclarationSyntax classNode) {
            return classNode.Modifiers.Any(x => x.Kind() == SyntaxKind.PublicKeyword);
        }

        /// <summary>
        /// Indique si un document est un fichier de contrat de service.
        /// </summary>
        /// <param name="tree">Arbre du document.</param>
        /// <returns><code>True</code> si fichier de contrat de service.</returns>
        public static bool IsServiceContractFile(this SyntaxTree tree) {
            var filePath = tree.FilePath;
            if (string.IsNullOrEmpty(filePath)) {
                return false;
            }

            return _serviceContractFileRegex.IsMatch(filePath);
        }

        /// <summary>
        /// Indique si un document est un fichier d'implémentation de service.
        /// </summary>
        /// <param name="tree">Arbre du document.</param>
        /// <returns><code>True</code> si fichier d'implémentation de service.</returns>
        public static bool IsServiceImplementationFile(this SyntaxTree tree) {
            var filePath = tree.FilePath;
            if (string.IsNullOrEmpty(filePath)) {
                return false;
            }

            return _serviceImplementationFileRegex.IsMatch(filePath);
        }

        /// <summary>
        /// Obtient le nom d'un node de field.
        /// </summary>
        /// <param name="node">Node de field.</param>
        /// <returns>Nom du field.</returns>
        public static string GetFieldName(this FieldDeclarationSyntax node) {
            /* Suppose qu'il n'y a qu'une seule variable. */
            return node.Declaration.Variables.First().Identifier.ValueText;
        }

        /// <summary>
        /// Obtient la location d'un nom de field.
        /// </summary>
        /// <param name="node">Node de field.</param>
        /// <returns>Location du field.</returns>
        public static Location GetFieldNameLocation(this FieldDeclarationSyntax node) {
            /* Suppose qu'il n'y a qu'une seule variable. */
            return node.Declaration.Variables.First().Identifier.GetLocation();
        }

        /// <summary>
        /// Obtient le nom d'un node de paramètre.
        /// </summary>
        /// <param name="node">Node de paramètre.</param>
        /// <returns>Nom du paramètre.</returns>
        public static string GetParameterName(this ParameterSyntax node) {
            return node.Identifier.ValueText;
        }

        /// <summary>
        /// Obtient la location d'un nom de paramètre.
        /// </summary>
        /// <param name="node">Node de paramètre.</param>
        /// <returns>Location du paramètre.</returns>
        public static Location GetParameterNameLocation(this ParameterSyntax node) {
            return node.Identifier.GetLocation();
        }
    }
}
