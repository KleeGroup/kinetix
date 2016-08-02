using System.Collections.Immutable;
using System.Linq;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie qu'une implémentation de service n'est pas injectée directement dans un constructeur.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1100_DoNotDependOnServiceImplementationAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1100";
        private const string Category = "Design";
        private static readonly string Description = "Les services doivent être injectés via leur contrat.";
        private static readonly string MessageFormat = "La classe {0} ne doit pas dépendre de l'implémentation de service {1}.";

        private static readonly string Title = "Ne pas injecter une implémentation de service";

        private static DiagnosticDescriptor rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        public override void Initialize(AnalysisContext context) {
            /* Analyse pour les symboles de méthodes (pour analyser les constructeurs). */
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context) {

            /* 1. Vérifier qu'on est dans un constructeur d'une classe. */
            var method = context.Symbol as IMethodSymbol;
            if (method == null || method.MethodKind != MethodKind.Constructor) {
                return;
            }

            var clazz = method.ContainingType;
            if (clazz == null || !(clazz.TypeKind == TypeKind.Class)) {
                return;
            }

            /* 2. Parcourir les paramètres du constructeur */
            var className = clazz.Name;
            var root = method.Locations.First().SourceTree.GetRoot(context.CancellationToken);
            foreach (var parameter in method.Parameters) {
                var paramType = parameter.Type;
                /* 2.a. Vérifier si le paramètre est typé par une implémentation de services. */
                var paramClass = paramType as INamedTypeSymbol;
                if (paramClass == null || !paramClass.IsServiceImplementation()) {
                    continue;
                }

                /* 2.b. Créér le diagnostic. */
                var paramTypeLocation = GetTypeLocation(root, parameter);
                var dependencyName = paramClass.Name;
                var diagnostic = Diagnostic.Create(rule, paramTypeLocation, className, dependencyName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static Location GetTypeLocation(SyntaxNode root, IParameterSymbol parameter) {
            var location = parameter.Locations.FirstOrDefault();
            var paramNode = root.FindNode(location.SourceSpan) as ParameterSyntax;
            return paramNode.Type.GetLocation();
        }
    }
}
