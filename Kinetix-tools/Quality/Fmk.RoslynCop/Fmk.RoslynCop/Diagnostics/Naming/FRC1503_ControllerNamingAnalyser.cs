using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie que le nommage des contrôleurs WebAPI.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1503_ControllerNamingAnalyser : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1503";

        private static readonly string Title = "Nommage des contrôleurs Web API";
        private static readonly string MessageFormat = "Le contrôleur {0} n'est pas nommé selon le service injecté {1}.";
        private static readonly string Description = "Nommage des contrôleurs Web API.";
        private const string Category = "Naming";

        private static readonly Regex ServiceContractNamePattern = new Regex(@"^IService(.*)$");

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context) {
            context.RegisterSymbolAction(AnalyseSymbol, SymbolKind.NamedType);
        }

        private void AnalyseSymbol(SymbolAnalysisContext context) {

            /* Vérifie qu'on est dans un contrôleur Web API. */
            var namedTypedSymbol = context.Symbol as INamedTypeSymbol;
            if (namedTypedSymbol == null) {
                return;
            }
            if (!namedTypedSymbol.IsApiController()) {
                return;
            }

            /* Vérifie qu'on a un unique constructeur. */
            var ctrList = namedTypedSymbol.Constructors;
            if (ctrList.Length != 1) {
                return;
            }

            /* Vérifie que le constructeur n'a qu'un seul paramètre. */
            var ctr = ctrList.First();
            var paramList = ctr.Parameters;
            if (paramList.Length != 1) {
                return;
            }

            /* Vérifie que le paramètre est un contrat de service. */
            var namedParamType = paramList.First().Type as INamedTypeSymbol;
            if (namedParamType == null) {
                return;
            }
            if (!namedParamType.IsServiceContract()) {
                return;
            }

            /* Vérification du nommage */
            var contractName = namedParamType.Name;
            if (!ServiceContractNamePattern.IsMatch(contractName)) {
                return;
            }
            var expectedControllerName = ServiceContractNamePattern.Replace(contractName, "$1Controller");
            var actualControllerName = namedTypedSymbol.Name;
            if (actualControllerName == expectedControllerName) {
                return;
            }

            /* Créé le diagnostic. */
            var diagnostic = Diagnostic.Create(Rule, namedTypedSymbol.Locations.First(), actualControllerName, contractName);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
