using Kinetix.ComponentModel.Search;
using Kinetix.Search.Broker;
using Kinetix.Search.Model;
using Kinetix.Search.Test.Dum;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinetix.Search.Test.SearchBrokerTest {

    /// <summary>
    /// Teste la méthode AdvancedCount.
    /// </summary>
    [TestClass]
    public class AdvancedCountTest {

        private const string GenreFacet = "GenreFacet";
        private const string GenreField = "Genre";

        [TestMethod]
        public void Check_InitialQueryFacet() {
            var facetsInput = new FacetListInput();

            CheckFacets(6, facetsInput);
        }

        [TestMethod]
        public void Check_FacetSelection() {

            var facetsInput = new FacetListInput();
            facetsInput[GenreFacet] = "F";

            CheckFacets(2, facetsInput);
        }

        [TestMethod]
        public void Check_FacetNullSelection() {

            var facetsInput = new FacetListInput();
            facetsInput[GenreFacet] = FacetConst.NullValue;

            CheckFacets(1, facetsInput);
        }

        [TestMethod]
        public void Check_FacetNoEmptyValueFacet() {
            var facetsInput = new FacetListInput();

            CheckFacets(1, facetsInput, "MARCHAND");
        }

        private static void CheckFacets(long expected, FacetListInput facetsInput, string query = null) {

            var facetQueryDefinition = new FacetQueryDefinition(new BooleanFacet {
                Code = GenreFacet,
                FieldName = GenreField
            });
            var input = new AdvancedQueryInput {
                ApiInput = new QueryInput {
                    Criteria = new Criteria { Query = query },
                    Skip = 0,
                    Top = 10,
                    Facets = facetsInput,
                },
                FacetQueryDefinition = facetQueryDefinition
            };
            var broker = SearchBrokerManager.GetBroker<PersonneDocument>();
            var actual = broker.AdvancedCount(input);
            Assert.AreEqual(expected, actual, "Le compte n'est pas bon.");
        }
    }
}
