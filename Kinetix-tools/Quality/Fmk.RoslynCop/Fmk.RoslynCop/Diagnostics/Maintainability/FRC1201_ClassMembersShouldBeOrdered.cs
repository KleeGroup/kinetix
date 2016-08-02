using System.Collections.Immutable;
using System.Linq;
using Fmk.RoslynCop.Common;
using Fmk.RoslynCop.Common.Ordering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Maintainability {

    /// <summary>
    /// La classe d'analyseur pour la règle OrdreClasse.
    /// Permet d'enregistrer des diagnostics sur les fichiers sources.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1201_ClassMembersShouldBeOrdered : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1201";

        private const string Category = "Maintainability";
        private static readonly string Description = "Les éléments de la classe ne sont pas dans le bon ordre.";
        private static readonly string MessageFormat = "Les éléments de la classe ne sont pas dans le bon ordre.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        private static readonly string Title = "Les éléments de la classe ne sont pas dans le bon ordre.";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <summary>
        /// Méthode d'initialisation de l'analyseur.
        /// </summary>
        /// <param name="context">Le contexte.</param>
        public override void Initialize(AnalysisContext context) {
            context.RegisterSymbolAction(AnalyserTypeNommé, SymbolKind.NamedType);
        }

        /// <summary>
        /// Trouve des diagnostics sur un type nommé.
        /// </summary>
        /// <param name="contexte">Le contexte.</param>
        private static void AnalyserTypeNommé(SymbolAnalysisContext contexte) {
            if (OrdreÉlémentsEstFaux(contexte))
                contexte.ReportDiagnostic(Diagnostic.Create(Rule, contexte.Symbol.Locations[0]));
        }

        /// <summary>
        /// Détermine si l'ordre d'assignations des champs est correct (en tête, par ordre alphabétique).
        /// </summary>
        /// <param name="context">Le contexte du symbole.</param>
        /// <returns>Oui ou non.</returns>
        private static bool OrdreÉlémentsEstFaux(SymbolAnalysisContext context) {

            // On récupère les informations nécessaires du contexte du symbole.
            var location = context.Symbol.Locations.First();
            var racine = location.SourceTree.GetRoot();
            var type = racine.FindNode(location.SourceSpan) as TypeDeclarationSyntax;
            var modèleSémantique = context.Compilation.GetSemanticModel(location.SourceTree);

            // On ignore la vérification sur les classes partielles.
            if (type == null)
                return false;

            // On ignore le code généré.
            if (type.AttributeLists.Any(node => node.ChildNodes().Any(node2 => node2.ToString().Contains("GeneratedCode")))) {
                return false;
            }

            // On récupère les éléments et on les ordonnes.
            var membresOrdonnés = ClassOrdering.OrdonnerMembres(type.Members, modèleSémantique);

            // Et on vérifie que l'ordre est le même.
            return !type.Members.SequenceEqual(membresOrdonnés);
        }
    }
}
