using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Fmk.RoslynCop.Common {

    /// <summary>
    /// Méthodes d'extensions pour les code fix.
    /// </summary>
    public static class CodeFixExtensions {

        /// <summary>
        /// Renvoie la méthode avec l'attribut ajouté.
        /// </summary>
        /// <param name="methDecl">Méthode.</param>
        /// <param name="attributeName">Nom simple de l'attribut.</param>
        /// <returns>Nouvelle méthode.</returns>
        public static MethodDeclarationSyntax AddAttribute(this MethodDeclarationSyntax methDecl, string attributeName) {

            var attributeSyntax = CreateAttribute(attributeName);
            var newAttrList = SyntaxFactory.AttributeList(
                SyntaxFactory.SeparatedList(new[] { attributeSyntax
                }));

            /* Récupère le trivia du premier token. */
            var initFirstToken = methDecl.GetFirstToken();
            var initLeadingTrivia = initFirstToken.LeadingTrivia;

            /* Enlève le trivia du premier token. */
            var newMethodSyntax = methDecl.ReplaceToken(
                initFirstToken,
                initFirstToken.WithLeadingTrivia(SyntaxFactory.Whitespace("\n")));

            /* Injecte le trivia sur le nouvel attribut. */
            newAttrList = newAttrList.WithLeadingTrivia(initLeadingTrivia);

            /* Ajoute l'attribut à la méthode. */
            var newAttrLists = newMethodSyntax.AttributeLists.Insert(0, newAttrList);
            newMethodSyntax = newMethodSyntax.WithAttributeLists(newAttrLists);
            return newMethodSyntax;
        }

        /// <summary>
        /// Ajoute un using s'il manque.
        /// </summary>
        /// <param name="rootNode">Node racine.</param>
        /// <param name="nameSpace">Espace de nom à rajouter.</param>
        /// <returns>Node avec le using.</returns>
        public static SyntaxNode AddUsing(this SyntaxNode rootNode, string nameSpace) {
            var unitSyntax = (CompilationUnitSyntax)rootNode;
            if (unitSyntax == null) {
                return rootNode;
            }

            /* Vérifie que le using n'est pas déjà présent. */
            if (unitSyntax.Usings.Any(x => x.Name.ToString() == nameSpace)) {
                return rootNode;
            }

            /* Ajoute le using. */
            var name = SyntaxFactory.ParseName(nameSpace);
            unitSyntax = unitSyntax.AddUsings(SyntaxFactory.UsingDirective(name).NormalizeWhitespace());

            //// TODO : ordre alphabétique avec System en premier.

            return unitSyntax;
        }

        /// <summary>
        /// Renomme un symbole.
        /// </summary>
        /// <param name="context">Contexte de fix.</param>
        /// <param name="symbol">Symbole à renommer.</param>
        /// <param name="newName">Nouveau nom.</param>
        /// <param name="cancellationToken">Token d'annulation.</param>
        /// <returns></returns>
        public static async Task<Solution> RenameSymbol(this CodeFixContext context, ISymbol symbol, string newName, CancellationToken cancellationToken) {
            var solution = context.Document.Project.Solution;
            var options = solution.Workspace.Options;
            return await Renamer.RenameSymbolAsync(solution, symbol, newName, options, cancellationToken);
        }

        /// <summary>
        /// Créé une syntaxe d'attribut.
        /// </summary>
        /// <param name="attributeName">Nom de l'attribut.</param>
        /// <returns>Node attribut.</returns>
        private static AttributeSyntax CreateAttribute(string attributeName) {
            return SyntaxFactory.Attribute(
                                    SyntaxFactory.IdentifierName(
                                        SyntaxFactory.Identifier(attributeName)));
        }
    }
}
