using System.Linq;
using Kinetix.Search.Broker;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Model;
using Kinetix.Search.Test.Dum;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinetix.Search.Test.SearchBrokerTest {

    /// <summary>
    /// Teste la méthode AdvancedQuery avec une facet Portefeuille.
    /// </summary>
    [TestClass]
    public class PortfolioTest {

        private const string DepartementFacet = "DepartementFacet";
        private const string DepartementField = "DepartementList";

        [TestMethod]
        public void Check_PortfolioInitialQueryFacet() {
            var facetsInput = new FacetListInput();

            var output = CheckFacets(facetsInput, portfolio: "02 03");

            Assert.IsNotNull(output);

            /* Total. */
            Assert.AreEqual(6, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(6, output.TotalCount, "Nombre de résultats attendu incorrect.");

            /* Facettes */
            var facetsOutput = output.Facets;
            Assert.IsNotNull(facetsOutput, "Facettes non null attendues.");
            Assert.AreEqual(1, facetsOutput.Count, "Nombre de facettes attendu incorrect.");

            var facet = facetsOutput.FirstOrDefault(d => d.Code == DepartementFacet);
            var facetOutput = facet.Values;

            Assert.IsNotNull(facet.Code, "Facette portfolio manquante.");

            Assert.IsNotNull(facetOutput, "Valeur de facette attendue.");
            Assert.AreEqual(2, facetOutput.Count, "Nombre de valeurs de facettes.");
            foreach (var value in facetOutput) {
                var s = value.Code;
                var v = value.Count;
                switch (s) {
                    case "0":
                        Assert.AreEqual(4, v);
                        break;
                    case "1":
                        Assert.AreEqual(2, v);
                        break;
                    default:
                        Assert.Fail("Clée inattendue : " + v);
                        break;
                }
            }
        }

        [TestMethod]
        public void Check_PortfolioFacetSelectionOui() {

            var facetsInput = new FacetListInput();
            facetsInput[DepartementFacet] = "1";

            var output = CheckFacets(facetsInput, portfolio: "02 03");

            /* Total. */
            Assert.AreEqual(2, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(2, output.TotalCount, "Nombre total de résultats attendu incorrect.");
        }

        [TestMethod]
        public void Check_PortfolioFacetPortfolioNull() {
            var facetsInput = new FacetListInput();
            CheckWithNoPortfolio(facetsInput, null);
        }

        [TestMethod]
        public void Check_PortfolioFacetPortfolioEmpty() {
            var facetsInput = new FacetListInput();
            CheckWithNoPortfolio(facetsInput, string.Empty);
        }

        [TestMethod]
        public void Check_PortfolioFacetSelectionNon() {

            var facetsInput = new FacetListInput();
            facetsInput[DepartementFacet] = "0";

            var output = CheckFacets(facetsInput, portfolio: "02 03");

            /* Total. */
            Assert.AreEqual(4, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(4, output.TotalCount, "Nombre total de résultats attendu incorrect.");
        }

        [TestMethod]
        public void Check_PortfolioFacetSelectionNonPortfolioNull() {

            var facetsInput = new FacetListInput();
            facetsInput[DepartementFacet] = "0";

            CheckWithNoPortfolio(facetsInput, string.Empty);
        }

        [TestMethod]
        public void Check_PortfolioFacetNoEmptyValueFacet() {
            var facetsInput = new FacetListInput();

            var output = CheckFacets(facetsInput, "MARCHAND", portfolio: "02 03");

            Assert.IsNotNull(output);

            /* Total. */
            Assert.AreEqual(1, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(1, output.TotalCount, "Nombre de résultats attendu incorrect.");

            /* Facettes */
            var facetsOutput = output.Facets;
            Assert.IsNotNull(facetsOutput, "Facettes non null attendues.");
            Assert.AreEqual(1, facetsOutput.Count, "Nombre de facettes attendu incorrect.");

            var facet = facetsOutput.FirstOrDefault(d => d.Code == DepartementFacet);
            var facetOutput = facet.Values;

            Assert.IsNotNull(facet.Code, "Facette portfolio manquante.");

            Assert.IsNotNull(facetOutput, "Valeur de facette attendue.");
            Assert.AreEqual(1, facetOutput.Count, "Nombre de valeurs de facettes.");
            foreach (var value in facetOutput) {
                var s = value.Code;
                var v = value.Count;
                switch (s) {
                    case "1":
                        Assert.AreEqual(1, v);
                        break;
                    default:
                        Assert.Fail("Clée inattendue : " + v);
                        break;
                }
            }
        }

        private static void CheckWithNoPortfolio(FacetListInput facetsInput, string portfolio) {

            var output = CheckFacets(facetsInput, portfolio: portfolio);

            /* Total. */
            Assert.AreEqual(6, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(6, output.TotalCount, "Nombre total de résultats attendu incorrect.");

            /* Facettes */
            var facetsOutput = output.Facets;
            Assert.IsNotNull(facetsOutput, "Facettes non null attendues.");
            Assert.AreEqual(1, facetsOutput.Count, "Nombre de facettes attendu incorrect.");

            var facet = facetsOutput.FirstOrDefault(d => d.Code == DepartementFacet);
            var facetOutput = facet.Values;

            Assert.IsNotNull(facet.Code, "Facette portfolio manquante.");

            Assert.IsNotNull(facetOutput, "Valeur de facette attendue.");
            Assert.AreEqual(1, facetOutput.Count, "Nombre de valeurs de facettes.");
            foreach (var value in facetOutput) {
                var s = value.Code;
                var v = value.Count;
                switch (s) {
                    case "0":
                        Assert.AreEqual(6, v);
                        break;
                    default:
                        Assert.Fail("Clée inattendue : " + v);
                        break;
                }
            }
        }

        private static QueryOutput<PersonneDocument> CheckFacets(FacetListInput facetsInput, string query = null, string security = null, string portfolio = null) {

            var facetQueryDefinition = new FacetQueryDefinition(new PortfolioFacet {
                Code = DepartementFacet,
                FieldName = DepartementField
            });
            var input = new AdvancedQueryInput {
                ApiInput = new QueryInput {
                    Criteria = new Criteria { Query = query },
                    Skip = 0,
                    Top = 10,
                    Facets = facetsInput,
                },
                FacetQueryDefinition = facetQueryDefinition,
                Security = security,
                Portfolio = portfolio
            };
            var broker = SearchBrokerManager.GetBroker<PersonneDocument>();
            return broker.AdvancedQuery(input);
        }
    }
}
