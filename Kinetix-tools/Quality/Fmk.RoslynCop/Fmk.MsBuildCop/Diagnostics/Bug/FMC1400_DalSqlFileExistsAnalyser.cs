using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Fmk.MsBuildCop.Core;

namespace Fmk.MsBuildCop.Diagnostics.Bugs {

    /// <summary>
    /// Vérifie que les fichiers référencés dans les appels de SqlServerCommand existent.
    /// </summary>
    public class FMC1400_DalSqlFileExistsAnalyser : IMsBuildAnalyser {

        public const string DiagnosticId = "FMC1400";
        private static readonly string Category = "Bugs";

        private static readonly Regex DalItemPattern = new Regex(@"DAL\.Implementation\\Dal.*\.cs");
        private static readonly Regex GetSqlServerPattern = new Regex(@"GetSqlServerCommand\(""(.*\.sql)""");
        private static readonly string MessageFormat = "Le fichier SQL {0} référencé dans la méthode de DAL {1} n'existe pas.";
        private static readonly Regex MethodDeclarationPattern = new Regex(@"public\s*[^\s]*\s*([^\s]*)\s*\(");

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptor.Create(DiagnosticId, Title, Category, MessageFormat);
        private static readonly string Title = "Validité du fichier SQL d'une SqlServerCommand";

        /// <inheritdoc cref="IMsBuildAnalyser.Analyze" />
        public void Analyze(AnalysisContext context) {
            var project = context.Project;

            /* Sélectionne les items de DAL.Implementation */
            var dalFileCandidates = project.Items.Where(
                x => x.ItemType == BuildAction.Compile &&
                    DalItemPattern.IsMatch(x.EvaluatedInclude));

            /* Parcourt les fichiers de DAL. */
            foreach (var dalFileCandidate in dalFileCandidates) {

                /* Vérifie que le fichier existe. */
                var fullPath = Path.Combine(project.DirectoryPath, dalFileCandidate.EvaluatedInclude);
                if (!File.Exists(fullPath)) {
                    continue;
                }

                /* Lit le texte du fichier */
                var lines = File.ReadAllLines(fullPath);
                int lineIdx = 0;
                var lastMethodName = (string)null;

                foreach (var line in lines) {
                    ++lineIdx;

                    /* Note les noms des méthodes pour la diagnostic */
                    var methodMath = MethodDeclarationPattern.Match(line);
                    if (methodMath.Success) {
                        lastMethodName = methodMath.Groups[1].Value;
                    }

                    /* Trouve les appels de GetSqlServerCommand */
                    var match = GetSqlServerPattern.Match(line);
                    if (!match.Success) {
                        continue;
                    }

                    /* Vérifie que le premier paramètre est le nom d'un fichier SQL. */
                    var group = match.Groups[1];
                    var sqlFileName = group.Value;
                    if (!sqlFileName.EndsWith(".sql", System.StringComparison.Ordinal)) {
                        continue;
                    }

                    /* Vérifier que le fichier SQL existe dans le projet. */
                    var expectedInclude = $@"DAL.Implementation\SQLResources\{sqlFileName}";
                    var sqlFileCandidates = project.Items.Where(x => x.EvaluatedInclude == expectedInclude);
                    if (sqlFileCandidates.Any()) {
                        continue;
                    }

                    /* Créé le diagnostic. */
                    Location loc = new Location {
                        FilePath = dalFileCandidate.EvaluatedInclude,
                        StartLine = lineIdx,
                        EndLine = lineIdx,
                        StartCharacter = group.Index,
                        EndCharacter = group.Index + group.Length + 2
                    };
                    var diagnostic = Diagnostic.Create(Rule, loc, sqlFileName, lastMethodName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
