using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Fmk.MsBuildCop.Core;

namespace Fmk.MsBuildCop.Diagnostics.Bugs {

    /// <summary>
    /// Vérifie que les méthodes de DAL utilisant GetSqlServerCommand ont un test unitaire.
    /// </summary>
    public class FMC1300_MissingDalTestAnalyser : IMsBuildAnalyser {

        public const string DiagnosticId = "FMC1300";
        private static readonly string Category = "Coverage";

        private static readonly Regex DalItemPattern = new Regex(@"DAL\.Implementation\\(Dal.*)\.cs");
        private static readonly Regex GenericTypePattern = new Regex(@"<[^>]*>");
        private static readonly Regex GetSqlServerPattern = new Regex(@"GetBroker<|GetSqlServerCommand\(");
        private static readonly string MessageFormat = "La méthode de DAL {0}.{1} n'a pas de test unitaire.";
        private static readonly Regex MethodDeclarationPattern = new Regex(@"public\s*[^\s]*\s*([^\s]*)\s*\(");

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptor.Create(DiagnosticId, Title, Category, MessageFormat);

        private static readonly string Title = "Présent d'un test unitaire de DAL";

        /// <inheritdoc cref="IMsBuildAnalyser.Analyze" />
        public void Analyze(AnalysisContext context) {
            var project = context.Project;

            /* Sélectionne le projet de test. */
            var testProject = context.GetTestProject();
            var hasTestProject = testProject != null;

            /* Sélectionne les items candidats pour être des fichiers de DAL. */
            var dalFileCandidates = project.Items.Where(x => x.ItemType == BuildAction.Compile);

            /* Parcourt les fichiers candidats. */
            foreach (var dalFileCandidate in dalFileCandidates) {

                /* Vérifie que l'item est un fichier de DAL. */
                var dalClassMatch = DalItemPattern.Match(dalFileCandidate.EvaluatedInclude);
                if (!dalClassMatch.Success) {
                    continue;
                }

                var dalClassName = dalClassMatch.Groups[1].Value;

                /* Vérifie que le fichier existe. */
                var fullPath = Path.Combine(project.DirectoryPath, dalFileCandidate.EvaluatedInclude);
                if (!File.Exists(fullPath)) {
                    continue;
                }

                /* Lit le texte du fichier */
                var lines = File.ReadAllLines(fullPath);
                int lineIdx = 0;
                int lastMethodLineIdx = 0;
                var lastMethodGroup = (Group)null;
                var lastMethodName = (string)null;

                foreach (var line in lines) {
                    ++lineIdx;

                    /* Note le nom de la méthode courante */
                    var methodMatch = MethodDeclarationPattern.Match(line);
                    if (methodMatch.Success) {
                        lastMethodLineIdx = lineIdx;
                        lastMethodGroup = methodMatch.Groups[1];
                        lastMethodName = GenericTypePattern.Replace(lastMethodGroup.Value, string.Empty);
                    }

                    /* Trouve les appels de GetSqlServerCommand */
                    var match = GetSqlServerPattern.IsMatch(line);
                    if (!match) {
                        continue;
                    }

                    /* Vérifie que la méthode possède un test. */
                    var expectedTestItem = $@"{dalClassName}Test\{lastMethodName}Test.cs";
                    var hasTest = hasTestProject && testProject.Items.Any(x => x.EvaluatedInclude == expectedTestItem);
                    if (hasTest) {
                        continue;
                    }

                    /* Créé le diagnostic. */
                    Location loc = new Location {
                        FilePath = dalFileCandidate.EvaluatedInclude,
                        StartLine = lastMethodLineIdx,
                        EndLine = lastMethodLineIdx,
                        StartCharacter = lastMethodGroup.Index + 1,
                        EndCharacter = lastMethodGroup.Index + lastMethodGroup.Length + 1
                    };
                    var diagnostic = Diagnostic.Create(Rule, loc, dalClassName, lastMethodName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
