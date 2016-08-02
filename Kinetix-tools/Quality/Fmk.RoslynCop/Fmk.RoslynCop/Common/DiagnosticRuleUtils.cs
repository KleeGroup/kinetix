using System.Globalization;
using Microsoft.CodeAnalysis;

namespace Fmk.RoslynCop.Common {
    public static class DiagnosticRuleUtils {

        /// <summary>
        /// Format pour l'URL de l'aide sur le warning (sur .Notes).
        /// </summary>
        private const string HelpLinkUriFormat = @"https://notes.part.klee.lan.net/techno/server/dotnet/fmk/roslyn_cop/{0}";

        public static DiagnosticDescriptor CreateRule(string id, string title, string messageFormat, string category, string description, DiagnosticSeverity defaultSeverity = DiagnosticSeverity.Warning) {
            return new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                defaultSeverity: defaultSeverity,
                isEnabledByDefault: true,
                description: description,
                helpLinkUri: string.Format(CultureInfo.InvariantCulture, HelpLinkUriFormat, id.ToLowerInvariant()));
        }
    }
}
