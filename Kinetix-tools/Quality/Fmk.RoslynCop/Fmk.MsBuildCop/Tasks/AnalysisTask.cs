using Fmk.MsBuildCop.Core;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities;

namespace Fmk.MsBuildCop.Tasks {

    /// <summary>
    /// Classe de base pour l'analyse de fichier MsBuild.
    /// </summary>
    public abstract class AnalysisTask : Task {

        /// <summary>
        /// Collection de projet MsBuild.
        /// </summary>
        public ProjectCollection ProjectCollection {
            get;
            private set;
        }

        /// <summary>
        /// Liste des analyseurs.
        /// </summary>
        protected abstract IMsBuildAnalyser[] Analysers {
            get;
        }

        /// <summary>
        /// Exécute la tâche.
        /// </summary>
        /// <returns><code>True</code> si aucune erreur.</returns>
        public override bool Execute() {
            using (this.ProjectCollection = new ProjectCollection()) {

                /* Charge le projet MsBuild courant. */
                var projectPath = this.BuildEngine.ProjectFileOfTaskNode;
                var project = this.ProjectCollection.LoadProject(projectPath);

                /* Créé un contexte d'analyse. */
                var context = new AnalysisContext(this, project);

                /* Exécute les analyseurs. */
                foreach (var analyser in this.Analysers) {
                    analyser.Analyze(context);
                }
            }

            return true;
        }
    }
}
