using Fmk.MsBuildCop.Core;
using Fmk.MsBuildCop.Diagnostics.Bugs;

namespace Fmk.MsBuildCop.Tasks {

    /// <summary>
    /// Tâche d'analyse d'un projet C# métier.
    /// </summary>
    public class BusinessImplementationProjectTask : AnalysisTask {

        /// <summary>
        /// Renvoie la liste des analyseurs.
        /// </summary>
        protected override IMsBuildAnalyser[] Analysers {
            get {
                return new IMsBuildAnalyser[] {
                    new FMC1100_BusinessImplementationIndependencyAnalyser(),
                    new FMC1300_MissingDalTestAnalyser(),
                    new FMC1400_DalSqlFileExistsAnalyser(),
                    new FMC1401_DalSqlFileBuildActionAnalyser(),
                    new FMC1403_ProjectFilejMissingFileAnalyser()
                };
            }
        }
    }
}
