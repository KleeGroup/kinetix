using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fmk.MsBuildCop.Core;

namespace Fmk.MsBuildCop.Diagnostics.Bugs {

    /// <summary>
    /// Vérifie que les fichiers du File System sont référencés dans le projet MsBuild.
    /// </summary>
    public class FMC1403_ProjectFilejMissingFileAnalyser : IMsBuildAnalyser {

        public const string DiagnosticId = "FMC1403";
        private static readonly string Category = "Bugs";

        private static readonly string[] EndWithList = {
                "Settings.StyleCop",
                ".tfignore",
                ".csproj",
                ".sqlproj",
                ".csproj.user",
                ".sqlproj.user",
                ".sln",
                ".suo",
                "Thumbs.db",
                ".vspscc",
                ".vssscc",
                ".dbmdl",
                "StyleCop.Cache"
            };

        private static readonly string MessageFormat = "Le fichier {0} n'est pas inclus dans le projet {1}.";
        private static readonly string[] StartWithList = {
                @"bin\",
                @"obj\",
                @"Release\"
            };

        private static readonly string Title = "Fichier dans le File System absent du projet";

        private static DiagnosticDescriptor rule = DiagnosticDescriptor.Create(DiagnosticId, Title, Category, MessageFormat);

        /// <inheritdoc cref="IMsBuildAnalyser.Analyze" />
        public void Analyze(AnalysisContext context) {
            var project = context.Project;

            /* Index les fichiers du projets. */
            var includes = new HashSet<string>();
            foreach (var item in project.Items) {

                /* Vérifie la présence d'un include. */
                var include = item.EvaluatedInclude;
                if (string.IsNullOrEmpty(include)) {
                    continue;
                }

                /* Vérifie que l'item est un fichier du projet */
                if (!BuildAction.IsProjectFile(item.ItemType)) {
                    continue;
                }

                var normalizedInclude = include.ToUpperInvariant();
                includes.Add(normalizedInclude);
            }

            /* Parcourt les fichiers du dossier du projet. */
            var projectDir = Path.GetDirectoryName(project.ProjectFileLocation.File);
            var projectName = Path.GetFileNameWithoutExtension(project.ProjectFileLocation.File);
            var candidates = Directory.GetFiles(projectDir, "*", SearchOption.AllDirectories);
            foreach (string file in candidates) {
                /* Calcule le chemin relatif au projet. */
                var relativePath = file.Substring(projectDir.Length + 1);

                /* Vérifie si le fichier doit être ignoré. */
                if (IsIgnored(relativePath)) {
                    continue;
                }

                /* Vérifie si le fichier est inclu dans le csproj. */
                if (!includes.Contains(relativePath.ToUpper())) {
                    Location loc = new Location {
                        FilePath = project.ProjectFileLocation.File
                    };
                    var diagnostic = Diagnostic.Create(rule, loc, relativePath, projectName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool IsIgnored(string path) {

            if (EndWithList.Any(path.EndsWith)) {
                return true;
            }

            if (StartWithList.Any(path.StartsWith)) {
                return true;
            }

            return false;
        }
    }
}
