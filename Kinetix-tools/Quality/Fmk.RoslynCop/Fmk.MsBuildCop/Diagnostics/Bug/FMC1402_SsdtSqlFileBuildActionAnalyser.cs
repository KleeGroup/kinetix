using System.Collections.Generic;
using System.Linq;
using Fmk.MsBuildCop.Core;

namespace Fmk.MsBuildCop.Diagnostics.Bugs {

    /// <summary>
    /// Vérifie la build action des fichiers SQL d'un projet SSDT.
    /// </summary>
    public class FMC1402_SsdtSqlFileBuildActionAnalyser : IMsBuildAnalyser {

        public const string DiagnosticId = "FMC1402";
        private static readonly string Category = "Bugs";

        /// <summary>
        /// Map un dossier à la BuildAction attendue.
        /// </summary>
        private static readonly Dictionary<string, string> FolderBuildActionMap = new Dictionary<string, string> {
            ["dbo"] = BuildAction.Build,
            ["Init"] = BuildAction.None,
            ["PreUpdate"] = BuildAction.None,
            ["Update"] = BuildAction.None
        };

        private static readonly string MessageFormat = "Le fichier SQL {0} doit avoir une build action à {1}.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptor.Create(DiagnosticId, Title, Category, MessageFormat);
        private static readonly string Title = "Validité de la Build Action d'un fichier SQL SSDT";

        /// <inheritdoc cref="IMsBuildAnalyser.Analyze" />
        public void Analyze(AnalysisContext context) {

            /* Parcourt la map des dossier à vérifier. */
            foreach (var tuple in FolderBuildActionMap) {
                var folder = tuple.Key;
                var expectedBuildAction = tuple.Value;

                /* Sélectionne les items du dossier qui ne sont pas des dossiers et qui n'ont pas la bonne BuildAction.*/
                var items = context.Project.Items
                    .Where(x => x.EvaluatedInclude.StartsWith(folder + "\\", System.StringComparison.Ordinal) &&
                                x.ItemType != expectedBuildAction && x.ItemType != BuildAction.Folder);

                /* Parcourt les items. */
                foreach (var item in items) {

                    /* Créé un diagnostic. */
                    var loc = new Location { FilePath = item.EvaluatedInclude };
                    var simpleName = item.EvaluatedInclude.Split('\\').Last();
                    var diagnostic = Diagnostic.Create(Rule, loc, simpleName, expectedBuildAction);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
