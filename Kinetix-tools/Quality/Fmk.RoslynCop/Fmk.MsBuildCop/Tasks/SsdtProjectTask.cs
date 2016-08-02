using Fmk.MsBuildCop.Core;
using Fmk.MsBuildCop.Diagnostics.Bugs;

namespace Fmk.MsBuildCop.Tasks {

    /// <summary>
    /// Tâche d'analyse d'un projet SSDT.
    /// </summary>
    public class SsdtProjectTask : AnalysisTask {

        /// <summary>
        /// Renvoie la liste des analyseurs.
        /// </summary>
        protected override IMsBuildAnalyser[] Analysers {
            get {
                return new IMsBuildAnalyser[] {
                    new FMC1402_SsdtSqlFileBuildActionAnalyser(),
                    new FMC1403_ProjectFilejMissingFileAnalyser()
                };
            }
        }
    }
}
