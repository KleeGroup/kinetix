namespace Fmk.RoslynCop.CodeFixes.TestGenerator {

    /// <summary>
    /// Stratégies de test de DAL.
    /// </summary>
    public enum DalTestStrategy {

        /// <summary>
        /// Test standard : on appelle la méthode.
        /// </summary>
        Standard,

        /// <summary>
        /// Test sémantique : on appelle la méthode et on valide le test même en cas d'exception liées aux données.
        /// </summary>
        Semantic
    }
}
