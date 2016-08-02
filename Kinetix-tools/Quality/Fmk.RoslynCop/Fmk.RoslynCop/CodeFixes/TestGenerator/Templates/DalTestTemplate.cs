using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fmk.RoslynCop.CodeFixes.TestGenerator.Dto;

namespace Fmk.RoslynCop.CodeFixes.TestGenerator.Templates {

    /// <summary>
    /// Template de test de DAL.
    /// </summary>
    public partial class DalTestTemplate {

        private static readonly IComparer<string> _usingComparer = new UsingComparer();

        /// <summary>
        /// Créé une nouvelle instance de DalTestTemplate.
        /// </summary>
        /// <param name="item">Méthode de DAL.</param>
        public DalTestTemplate(DalMethodItem item) {
            this.Item = item;
        }

        /// <summary>
        /// Méthode de DAL.
        /// </summary>
        public DalMethodItem Item {
            get;
            private set;
        }

        public string Render(DalTestStrategy strategy = DalTestStrategy.Semantic) {

            var sb = new StringBuilder();
            foreach (var usingDirective in GetUsings()) {
                sb.AppendLine($@"using {usingDirective};");
            }

            var methodCall = $@"new {this.Item.DalClassName}().{this.Item.DalMethodName}({this.Item.FlatParams})";
            var methodTest =
                strategy == DalTestStrategy.Semantic ?
                $@"this.CheckDalSyntax(() => {methodCall});" : // Test sémantique : on enveloppe l'appel pour attraper les exceptions liées aux données.
                $@"{methodCall};"; // Test standard

            sb.Append(
            $@"
namespace {this.Item.DalAssemblyName}.Test.{this.Item.DalClassName}Test {{

    [TestClass]
    public class {this.Item.DalMethodName}Test : DalTest {{

        [TestMethod]
        public void Check_{this.Item.DalMethodName}_Ok() {{

            // Act
            {methodTest}
        }}
    }}
}}");
            return sb.ToString();
        }

        /// <summary>
        /// Renvoie les usings triés.
        /// </summary>
        /// <returns>Liste des usings.</returns>
        private ICollection<string> GetUsings() {
            /* Construit la liste de using triés. */
            var usings = new SortedSet<string>(_usingComparer);
            foreach (var usingDirective in this.Item.SpecificUsings) {
                usings.Add(usingDirective);
            }

            foreach (var usingDirective in this.Item.Params.SelectMany(x => x.SpecificUsings)) {
                usings.Add(usingDirective);
            }

            usings.Add(this.Item.DalNamespace);
            usings.Add("Microsoft.VisualStudio.TestTools.UnitTesting");

            return usings;
        }

        /// <summary>
        /// Comparateur de string pour les namespace d'using.
        /// Les namespace System sont prioritaires sur l'ordre alphabétique.
        /// </summary>
        private class UsingComparer : IComparer<string> {

            /// <summary>
            /// Compare x et y.
            /// Renvoie 1 si x gt y.
            /// Renvoie 0 si x eq y.
            /// Renvoie -1 si x lt y.
            /// </summary>
            /// <param name="x">Opérande de gauche.</param>
            /// <param name="y">Opérande de droite.</param>
            /// <returns>Comparaison.</returns>
            public int Compare(string x, string y) {
                var xSystem = x.StartsWith("System", System.StringComparison.Ordinal);
                var ySystem = y.StartsWith("System", System.StringComparison.Ordinal);
                /* Si les deux usings sont System, où les deux usings ne sont pas System, on compare nativement. */
                if (xSystem == ySystem) {
                    return string.Compare(x, y, System.StringComparison.Ordinal);
                }

                /* Sinon si x est System, x est prioritaire. */
                if (xSystem) {
                    return 1;
                }

                /* Sinon si y est System, y est prioritaire. */
                if (ySystem) {
                    return -1;
                }

                return 0;
            }
        }
    }
}