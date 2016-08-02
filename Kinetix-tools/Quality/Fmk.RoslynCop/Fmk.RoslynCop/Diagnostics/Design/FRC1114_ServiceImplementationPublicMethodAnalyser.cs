using System.Collections.Immutable;
using System.Linq;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie que les méthodes publiques des classes d'implémentations de service sont publiées dans un contrat de service.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1114_ServiceImplementationPublicMethodAnalyser : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1114";
        private const string Category = "Design";
        private static readonly string Description = "Les méthodes publiques d'implémentation de services doivent être publiés dans un contrat de service.";
        private static readonly string MessageFormat = "La méthode publique {0} du service {1} doit être publiée dans un contrat de service, ou rendue privée.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        private static readonly string Title = "Les méthodes publiques d'implémentation de services doivent être publiés dans un contrat de service.";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            /* Analyse pour les déclaration de classes. */
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {

            /* Vérifie que la classe est une implémentation de service. */
            var classNode = context.Node as ClassDeclarationSyntax;
            if (classNode == null) {
                return;
            }

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classNode, context.CancellationToken);
            if (classSymbol == null) {
                return;
            }

            if (!classSymbol.IsServiceImplementation()) {
                return;
            }

            /* Recherche les interfaces de services. */
            var interfaceCandidates = classSymbol.Interfaces.Where(x => x.IsServiceContract());
            if (!interfaceCandidates.Any()) {
                return;
            }

            /* Index les méthodes d'interfaces candidates */
            var interfaceMethCandidates = interfaceCandidates
                .SelectMany(x => x
                    .GetMembers().OfType<IMethodSymbol>());

            /* Parcourt les méthodes */
            var publicMethods = classNode.Members.OfType<MethodDeclarationSyntax>().Where(x => x.IsPublic());
            foreach (var methDecl in publicMethods) {

                /* Vérifie qu'il existe une méthode d'interface correspondant */
                var methSymbol = context.SemanticModel.GetDeclaredSymbol(methDecl, context.CancellationToken);
                var hasMatchingInterfaceMeth = interfaceMethCandidates.Any(x => x.SignatureEquals(methSymbol));
                if (!hasMatchingInterfaceMeth) {

                    /* Créé le diagnostic. */
                    var location = methDecl.GetNameDeclarationLocation();
                    var diagnostic = Diagnostic.Create(Rule, location, methSymbol.Name, classSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
