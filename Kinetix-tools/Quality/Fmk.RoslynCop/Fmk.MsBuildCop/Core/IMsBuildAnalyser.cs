namespace Fmk.MsBuildCop.Core {

    /// <summary>
    /// Contrat des analyseurs de projet MsBuild.
    /// </summary>
    public interface IMsBuildAnalyser {

        /// <summary>
        /// Analyse un projet.
        /// </summary>
        /// <param name="context">Contexte d'analyse.</param>
        void Analyze(AnalysisContext context);
    }
}