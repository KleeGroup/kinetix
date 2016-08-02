using System.Linq;
using Fmk.MsBuildCop.Core;

namespace Fmk.MsBuildCop.Diagnostics.Bugs {

    /// <summary>
    /// Vérifie la build action des fichiers SQL d'un projet C#.
    /// </summary>
    public class FMC1401_DalSqlFileBuildActionAnalyser : IMsBuildAnalyser {

        public const string DiagnosticId = "FMC1401";
        private static readonly string Category = "Bugs";
        private static readonly string MessageFormat = "Le fichier SQL {0} doit avoir une build action à Embedded Resource.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptor.Create(DiagnosticId, Title, Category, MessageFormat);
        private static readonly string Title = "Validité du fichier SQL d'une SqlServerCommand";

        /// <inheritdoc cref="IMsBuildAnalyser.Analyze" />
        public void Analyze(AnalysisContext context) {

            /* Liste des fichiers SQL qui ne sont pas en EmbeddedRessource. */
            var issues = context.Project.Items.Where(x =>
                x.EvaluatedInclude.EndsWith(".sql", System.StringComparison.Ordinal) &&
                x.ItemType != BuildAction.EmbeddedResource);

            /* Créé les diagnostics. */
            foreach (var issue in issues) {
                var loc = new Location { FilePath = issue.EvaluatedInclude };
                var simpleName = issue.EvaluatedInclude.Split('\\').Last();
                var diagnostic = Diagnostic.Create(Rule, loc, simpleName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
