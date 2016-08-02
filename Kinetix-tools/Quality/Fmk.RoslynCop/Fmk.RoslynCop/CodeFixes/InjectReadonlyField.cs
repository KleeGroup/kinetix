using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Fmk.RoslynCop.CodeFixes {

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InjectReadonlyField))]
    [Shared]
    public class InjectReadonlyField : CodeFixProvider {

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(FRC1116_ReadonlyFieldsShouldBeInjected.DiagnosticId);

        /// <summary>
        /// Permet d'effectuer des corrections de masse via le correcteur en lot par défaut.
        /// </summary>
        /// <returns>Le correcteur de masse.</returns>
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <summary>
        /// Enregistre les corrections de codes.
        /// </summary>
        /// <param name="context">Le contexte.</param>
        /// <returns>Peut être attendu.</returns>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var champ = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Injecter le champ par le constructeur",
                    c => InjecterComposant(context.Document, champ, c),
                    "Injecter le champ par le constructeur"),
                diagnostic);
        }

        /// <summary>
        /// Ajoute une initialisation du champ via un nouveau paramètre du constructeur.
        /// </summary>
        /// <param name="document">Le document.</param>
        /// <param name="champ">Le champ.</param>
        /// <param name="jetonAnnulation">Le jeton d'annulation.</param>
        /// <returns>Le document mis à jour.</returns>
        private async Task<Document> InjecterComposant(Document document, VariableDeclaratorSyntax champ, CancellationToken jetonAnnulation) {
            // On récupère la racine et le modèle sémantique.
            var racine = await document
                .GetSyntaxRootAsync(jetonAnnulation)
                .ConfigureAwait(false);
            var modèleSémantique = await document.GetSemanticModelAsync(jetonAnnulation);

            // On récupère le constructeur de la classe, s'il existe.
            var constructeur = racine.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();

            if (constructeur == null)
                return document;

            // On construit le nom du paramètre en enlevant la première lettre du champ (qui doit être un _).
            var paramètre = SyntaxFactory.Identifier(champ.Identifier.ToString().Substring(1));

            // On construit le texte de documentation en fonction du type de paramètre.
            var type = (champ.Parent as VariableDeclarationSyntax).Type;
            var texte = type is PredefinedTypeSyntax ? "Valeur injectée."
                : type.ToString().StartsWith("IService", System.StringComparison.Ordinal) ? "Service injecté."
                : type.ToString().StartsWith("IDal", System.StringComparison.Ordinal) ? "DAL injectée."
                : "Composant injecté.";

            // On met à jour le constructeur.
            var nouveauConstructeur = constructeur

                // En ajoutant le paramètre.
                .WithParameterList(
                    constructeur.ParameterList.AddParameters(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.List<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(),
                            type,
                            paramètre,
                            null)))

                // En ajoutant la déclaration.
                .WithBody(
                    constructeur.Body.AddStatements(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(champ.Identifier),
                                SyntaxFactory.IdentifierName(paramètre)))))

                // Et la documentation du nouveau paramètre (ouais, tout ça, et encore je crois pas que ça soit bien correct).
                .WithLeadingTrivia(
                    constructeur.GetLeadingTrivia()
                        .Select(i => {
                            var doc = i.GetStructure() as DocumentationCommentTriviaSyntax;

                            if (doc == null)
                                return i;

                            return SyntaxFactory.Trivia(
                                doc.AddContent(
                                    SyntaxFactory.XmlText(
                                        SyntaxFactory.TokenList(
                                            SyntaxFactory.XmlTextLiteral(
                                                SyntaxFactory.TriviaList(
                                                    SyntaxFactory.DocumentationCommentExterior("/// ")),
                                                string.Empty,
                                                string.Empty,
                                                SyntaxFactory.TriviaList()))),
                                    SyntaxFactory.XmlElement(
                                        SyntaxFactory.XmlElementStartTag(
                                            SyntaxFactory.XmlName("param "),
                                            SyntaxFactory.List(
                                                new List<XmlAttributeSyntax>
                                                {
                                                    SyntaxFactory.XmlNameAttribute(
                                                        SyntaxFactory.XmlName("name"),
                                                        SyntaxFactory.Token(SyntaxKind.DoubleQuoteToken),
                                                        SyntaxFactory.IdentifierName(paramètre.ToString()),
                                                        SyntaxFactory.Token(SyntaxKind.DoubleQuoteToken))
                                                })),
                                        SyntaxFactory.List(
                                            new List<XmlNodeSyntax>
                                            {
                                                SyntaxFactory.XmlText(
                                                    SyntaxFactory.TokenList(
                                                        SyntaxFactory.XmlTextLiteral(
                                                            SyntaxFactory.TriviaList(),
                                                            texte,
                                                            string.Empty,
                                                            SyntaxFactory.TriviaList())))
                                            }),
                                        SyntaxFactory.XmlElementEndTag(SyntaxFactory.XmlName("param"))),
                                    SyntaxFactory.XmlText(
                                        SyntaxFactory.TokenList(
                                            SyntaxFactory.XmlTextNewLine(
                                                SyntaxFactory.TriviaList(),
                                                "\r\n",
                                                string.Empty,
                                                SyntaxFactory.TriviaList())))));
                        }));

            // Met à jour la racine, en reformattant le document.
            var nouvelleRacine = Formatter.Format(
                racine.ReplaceNode(constructeur, nouveauConstructeur),
                document.Project.Solution.Workspace);

            return document.WithSyntaxRoot(nouvelleRacine);
        }
    }
}
