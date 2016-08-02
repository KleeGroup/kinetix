using System.Collections.Immutable;
using System.Linq;
using Fmk.RoslynCop.Common;
using Fmk.RoslynCop.Common.Ordering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Maintainability {

    /// <summary>
    /// La classe d'analyseur pour la règle OrdreConstructeur.
    /// Permet d'enregistrer des diagnostics sur les fichiers sources.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1202_ConstructorShouldBeOrdered : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1202";

        private const string Category = "Maintainability";
        private static readonly string Description = "Les assignations du constructeur ne sont pas dans le bon ordre.";
        private static readonly string MessageFormat = "Les assignations du constructeur ne sont pas dans le bon ordre.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        private static readonly string Title = "Les assignations du constructeur ne sont pas dans le bon ordre.";

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
            if (OrdreAssignationEstFaux(contexte))
                contexte.ReportDiagnostic(Diagnostic.Create(Rule, contexte.Symbol.Locations[0]));
        }

        /// <summary>
        /// Détermine si l'ordre d'assignations des champs est correct (en tête, par ordre alphabétique).
        /// </summary>
        /// <param name="context">Le contexte du symbole.</param>
        /// <returns>Oui ou non.</returns>
        private static bool OrdreAssignationEstFaux(SymbolAnalysisContext context) {

            // On vérifie que la méthode est bien un constructeur.
            if ((context.Symbol as IMethodSymbol)?.MethodKind != MethodKind.Constructor)
                return false;

            // On récupère les informations nécessaires du contexte du symbole.
            var location = context.Symbol.Locations.First();
            var racine = location.SourceTree.GetRoot();
            var modèleSémantique = context.Compilation.GetSemanticModel(location.SourceTree);
            var méthode = racine.FindNode(location.SourceSpan) as ConstructorDeclarationSyntax;

            // On récupère le corps du constructeur.
            var corps = méthode?.ChildNodes().FirstOrDefault(nœud => nœud as BlockSyntax != null) as BlockSyntax;
            if (corps == null)
                return false;

            // On récupère toutes les conditions sur les paramètres.
            var conditions = ConstructorOrdering.TrouveConditionsParametres(corps.Statements, méthode.ParameterList, modèleSémantique);

            // On récupère toutes les assignations de champs par des paramètres.
            var assignations = ConstructorOrdering.TrouverAssignations(corps.Statements, modèleSémantique);

            // On vérifie que toutes les conditions puis toutes les assignations sont au début.
            if (!conditions.Concat(assignations).SequenceEqual(corps.Statements.Take(conditions.Count() + assignations.Count())))
                return true;

            // Et on vérifie l'ordre.
            return !assignations.SequenceEqual(assignations.OrderBy(x => x.ToString()));
        }
    }
}
