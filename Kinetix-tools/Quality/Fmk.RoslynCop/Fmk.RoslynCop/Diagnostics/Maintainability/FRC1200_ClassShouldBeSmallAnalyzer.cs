using System.Collections.Immutable;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Maintainability {

    /// <summary>
    /// Vérifie qu'une classe ne dépasse pas un nombre maximum de lignes.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1200_ClassShouldBeSmallAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1200";
        private const string Category = "Design";
        private static readonly string Description = "La classe a une taille trop importante, il faut la refactoriser pour réduire sa taille.";
        private static readonly string MessageFormat = "La classe {0} fait {1} lignes, ce qui dépasse la limite de {2}.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        private static readonly string Title = "Une classe doit être de taille limitée";

        // TODO rendre configurable
        private readonly int _maxRows = 1000;

        public FRC1200_ClassShouldBeSmallAnalyzer() {
        }

        public FRC1200_ClassShouldBeSmallAnalyzer(int maxRows) {
            _maxRows = maxRows;
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            /* Analyse pour les déclaration de classes. */
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, ImmutableArray.Create(SyntaxKind.ClassDeclaration));
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {

            var clazz = context.Node as ClassDeclarationSyntax;
            if (clazz == null) {
                return;
            }

            /* Vérifie le nombre de ligne. */
            int nbLignes = clazz.GetLocation().GetLineCount();
            if (nbLignes <= _maxRows) {
                return;
            }

            /* Créé un diagnostic. */
            var className = clazz.GetClassName();
            var location = clazz.GetNameDeclarationLocation();
            var diagnostic = Diagnostic.Create(Rule, location, className, nbLignes, _maxRows);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
