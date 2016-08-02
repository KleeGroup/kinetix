using System.Collections.Immutable;
using System.Linq;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Documentation {

    /// <summary>
    /// La classe d'analyseur pour la règle InheritDoc.
    /// Permet d'enregistrer des diagnostics sur les fichiers sources.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1600_InheritdocIsIncorrect : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1600";
        private const string Category = "Documentation";
        private const string MessageFormat = "Le commentaire <inheritdoc /> est manquant ou erroné.";
        private const string Title = "Le commentaire <inheritdoc /> doit être correct.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Title);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <summary>
        /// Méthode d'initialisation de l'analyseur.
        /// </summary>
        /// <param name="context">Le contexte.</param>
        public override void Initialize(AnalysisContext context) {
            context.RegisterSymbolAction(AnalyserMéthode, SymbolKind.Method);
        }

        /// <summary>
        /// Trouve des diagnostics sur une méthode.
        /// </summary>
        /// <param name="contexte">Le contexte.</param>
        private static void AnalyserMéthode(SymbolAnalysisContext contexte) {
            if (InheritDocEstCorrect(contexte) != null) {
                contexte.ReportDiagnostic(Diagnostic.Create(Rule, contexte.Symbol.Locations[0]));
            }
        }

        /// <summary>
        /// Détermine si le inheritDoc du symbole méthode est présent et correct.
        /// </summary>
        /// <param name="context">Le contexte du symbole.</param>
        /// <returns>La ligne inheritDoc correcte dans le cas où l'actuelle est manquante/incorrecte, sinon null.</returns>
        private static string InheritDocEstCorrect(SymbolAnalysisContext context) {

            // On récupère les informations nécessaires du contexte du symbole.
            var location = context.Symbol.Locations.First();
            var modèleSémantique = context.Compilation.GetSemanticModel(location.SourceTree);
            var racine = location.SourceTree.GetRoot();
            var méthode = racine.FindNode(location.SourceSpan) as MethodDeclarationSyntax;

            // On ignore le code généré.
            if ((méthode?.Parent as ClassDeclarationSyntax)?.AttributeLists.Any(node => node.ChildNodes().Any(node2 => node2.ToString().Contains("GeneratedCode"))) ?? true) {
                return null;
            }

            return Inheritdoc.InheritDocEstCorrect(racine, modèleSémantique, méthode);
        }
    }
}
