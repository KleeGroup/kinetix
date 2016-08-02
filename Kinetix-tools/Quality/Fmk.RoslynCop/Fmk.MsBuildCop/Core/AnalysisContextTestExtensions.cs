using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Evaluation;

namespace Fmk.MsBuildCop.Core {

    /// <summary>
    /// Méthodes d'extensions du contexte d'analyse pour gérer les projets de test.
    /// </summary>
    public static class AnalysisContextTestExtensions {

        private const string TestProjectPattern = @"$1\Tests\$2\$3.Test\$3.Test.csproj";
        private static readonly Regex ImplementationProjectPattern = new Regex(@"(.*)\\Sources\\(.*\.Business)\\.*\.Implementation\\(.*\..*Implementation)\\.*\..*Implementation\.csproj");

        /// <summary>
        /// Obtient le projet MsBuild de test du projet du contexte.
        /// Renvoie <code>null</code> si inexistant.
        /// </summary>
        /// <param name="context">Contexte d'analyse.</param>
        /// <returns>Projet MsBuild de test.</returns>
        public static Project GetTestProject(this AnalysisContext context) {
            var testProjectPath = ImplementationProjectPattern.Replace(
                context.Project.ProjectFileLocation.File,
                TestProjectPattern);

            if (!File.Exists(testProjectPath)) {
                return null;
            }

            return context.LoadProject(testProjectPath);
        }
    }
}
