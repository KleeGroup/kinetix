using System.Linq;
using System.Text.RegularExpressions;
using Fmk.MsBuildCop.Core;

namespace Fmk.MsBuildCop.Diagnostics.Bugs {

    /// <summary>
    /// Vérifie qu'un projet d'implémentation n'en référence pas un autre.
    /// </summary>
    public class FMC1100_BusinessImplementationIndependencyAnalyser : IMsBuildAnalyser {

        public const string DiagnosticId = "FMC1100";
        private static readonly string Category = "Design";

        private static readonly Regex ImplementationProjectReferencePattern = new Regex(@"([^\.]*)\.[^\.]*Implementation");
        private static readonly string MessageFormat = "Le projet {0} ne doit pas référencer le projet d'implémentation {1}.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptor.Create(DiagnosticId, Title, Category, MessageFormat);
        private static readonly string Title = "Indépendance des modules métier";

        /// <inheritdoc cref="IMsBuildAnalyser.Analyze" />
        public void Analyze(AnalysisContext context) {

            var currentProjectName = context.Project.GetPropertyValue("AssemblyName");

            /* Liste des référence de projets. */
            var candidates = context.Project.Items.Where(x => x.ItemType == BuildAction.ProjectReference);

            /* Créé les diagnostics. */
            foreach (var projectReference in candidates) {

                var projectName = projectReference.GetMetadataValue("Name");

                /* Vérifie que le projet est un projet d'implémentation.  */
                if (!ImplementationProjectReferencePattern.IsMatch(projectName)) {
                    continue;
                }

                /* Créé un diagnostic. */
                var loc = new Location { FilePath = context.Project.ProjectFileLocation.File, StartLine = 1, StartCharacter = 1, EndCharacter = 1, EndLine = 1 };
                var diagnostic = Diagnostic.Create(Rule, loc, currentProjectName, projectName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
