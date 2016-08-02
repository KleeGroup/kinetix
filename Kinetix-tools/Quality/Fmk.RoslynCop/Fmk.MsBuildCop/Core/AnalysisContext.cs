using Fmk.MsBuildCop.Tasks;
using Microsoft.Build.Evaluation;

namespace Fmk.MsBuildCop.Core {

    /// <summary>
    /// Contexte d'analyse.
    /// </summary>
    public class AnalysisContext {

        private readonly Project _project;
        private readonly AnalysisTask _task;

        /// <summary>
        /// Créé une nouvelle instance de AnalysisContext.
        /// </summary>
        /// <param name="task">Task courante.</param>
        /// <param name="project">Projet courant.</param>
        public AnalysisContext(AnalysisTask task, Project project) {
            _project = project;
            _task = task;
        }

        /// <summary>
        /// Projet MsBuild.
        /// </summary>
        public Project Project {
            get {
                return _project;
            }
        }

        /// <summary>
        /// Charge un projet MsBuild à partir du chemin du fichier.
        /// </summary>
        /// <param name="filePath">Chemin du fichier MsBuild.</param>
        /// <returns>Projet MsBuild.</returns>
        public Project LoadProject(string filePath) {
            return _task.ProjectCollection.LoadProject(filePath);
        }

        /// <summary>
        /// Rapporte un diagnostic.
        /// </summary>
        /// <param name="diagnostic">Diagnostic.</param>
        public void ReportDiagnostic(Diagnostic diagnostic) {
            var location = diagnostic.Location;
            var descr = diagnostic.Descriptor;
            _task.Log.LogWarning(
                subcategory: descr.Category,
                warningCode: descr.Id,
                helpKeyword: "HELP",
                file: location.FilePath,
                lineNumber: location.StartLine,
                columnNumber: location.StartCharacter,
                endLineNumber: location.EndLine,
                endColumnNumber: location.EndCharacter,
                message: diagnostic.GetMessage());
        }
    }
}
