﻿using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.Broker;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Model;
using Kinetix.Search.Test.Dum;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinetix.Search.Test.SearchBrokerTest {

    /// <summary>
    /// Teste la méthode AdvancedQuery.
    /// </summary>
    [TestClass]
    public class AdvancedQueryTest {

        private const string GenreFacet = "GenreFacet";
        private const string GenreField = "Genre";

        [TestMethod]
        public void Check_InitialQueryFacet() {
            var facetsInput = new FacetListInput();

            var output = CheckFacets(facetsInput);

            Assert.IsNotNull(output);

            /* Total. */
            Assert.AreEqual(9, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(9, output.TotalCount, "Nombre de résultats attendu incorrect.");

            /* Facettes */
            var facetsOutput = output.Facets;
            Assert.IsNotNull(facetsOutput, "Facettes non null attendues.");
            Assert.AreEqual(1, facetsOutput.Count, "Nombre de facettes attendu incorrect.");

            var facet = facetsOutput.FirstOrDefault(d => d.Code == GenreFacet);
            var facetOutput = facet.Values;

            Assert.IsNotNull(facet.Code, "Facette genre manquante.");

            Assert.IsNotNull(facetOutput, "Valeur de facette attendue.");
            Assert.AreEqual(3, facetOutput.Count, "Nombre de genre.");
            foreach (var value in facetOutput) {
                var s = value.Code;
                var v = value.Count;
                switch (s) {
                    case "M":
                        Assert.AreEqual(6, v);
                        break;
                    case "F":
                        Assert.AreEqual(2, v);
                        break;
                    case FacetConst.NullValue:
                        Assert.AreEqual(1, v);
                        break;
                    default:
                        Assert.Fail("Clée inattendue : " + v);
                        break;
                }
            }
        }

        [TestMethod]
        public void Check_FacetSelection() {

            var facetsInput = new FacetListInput();
            facetsInput[GenreFacet] = new[] { "F" };

            var output = CheckFacets(facetsInput);

            /* Total. */
            Assert.AreEqual(2, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(2, output.TotalCount, "Nombre total de résultats attendu incorrect.");
        }

        [TestMethod]
        public void Check_FacetNullSelection() {

            var facetsInput = new FacetListInput();
            facetsInput[GenreFacet] = new[] { FacetConst.NullValue };

            var output = CheckFacets(facetsInput);

            /* Total. */
            Assert.AreEqual(1, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(1, output.TotalCount, "Nombre total de résultats attendu incorrect.");
        }

        [TestMethod]
        public void Check_FacetNoEmptyValueFacet() {
            var facetsInput = new FacetListInput();

            var output = CheckFacets(facetsInput, "MARCHAND");

            Assert.IsNotNull(output);

            /* Total. */
            Assert.AreEqual(1, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(1, output.TotalCount, "Nombre de résultats attendu incorrect.");

            /* Facettes */
            var facetsOutput = output.Facets;
            Assert.IsNotNull(facetsOutput, "Facettes non null attendues.");
            Assert.AreEqual(1, facetsOutput.Count, "Nombre de facettes attendu incorrect.");

            var facet = facetsOutput.FirstOrDefault(d => d.Code == GenreFacet);
            var facetOutput = facet.Values;

            Assert.IsNotNull(facet.Code, "Facette genre manquante.");

            Assert.IsNotNull(facetOutput, "Valeur de facette attendue.");
            Assert.AreEqual(1, facetOutput.Count, "Nombre de genre.");
            foreach (var value in facetOutput) {
                var s = value.Code;
                var v = value.Count;
                switch (s) {
                    case "F":
                        Assert.AreEqual(1, v);
                        break;
                    default:
                        Assert.Fail("Clée inattendue : " + v);
                        break;
                }
            }
        }

        [TestMethod]
        public void Check_SortAsc() {
            Check_Sort(false, new List<string> { "BUCHE", "CARLOS", "D'ALEMBERT", "MARCHAND", "RAY", "ROBERT", "ROBERTO", "RODRIGEZ", "TOUTLEMONDE" });
        }

        [TestMethod]
        public void Check_SortDesc() {
            Check_Sort(true, new List<string> { "TOUTLEMONDE", "RODRIGEZ", "ROBERTO", "ROBERT", "RAY", "MARCHAND", "D'ALEMBERT", "CARLOS", "BUCHE" });
        }

        [TestMethod]
        public void Check_Group() {

            var facetQueryDefinition = new FacetQueryDefinition();
            facetQueryDefinition.Facets.Add(new BooleanFacet { Code = GenreFacet, FieldName = GenreField });

            var input = new AdvancedQueryInput {
                ApiInput = new QueryInput {
                    Criteria = new Criteria(),
                    Skip = 0,
                    Top = 10,
                    Group = GenreFacet
                },
                FacetQueryDefinition = facetQueryDefinition
            };
            var broker = SearchBrokerManager.GetBroker<PersonneDocument>();
            var output = broker.AdvancedQuery(input);

            /* Total. */
            Assert.AreEqual(9, output.TotalCount, "Nombre total de résultats attendu incorrect.");

            /* Groupes */
            var groups = output.Groups;
            Assert.IsNotNull(groups);
            Assert.AreEqual(3, groups.Count, "Nombre de groupes attendu incorrect.");

            foreach (var group in groups) {
                var bucket = group.List;
                switch (group.Code) {
                    case "M":
                        Assert.AreEqual(6, bucket.Count);
                        break;
                    case "F":
                        Assert.AreEqual(2, bucket.Count);
                        break;
                    case FacetConst.NullValue:
                        Assert.AreEqual(1, bucket.Count);
                        break;
                    default:
                        Assert.Fail("Clée inattendue : " + group.Code);
                        break;
                }
            }
        }

        [TestMethod]
        public void Check_FullTextSearch()
        {

            var facetsInput = new FacetListInput();
            facetsInput[GenreFacet] = new[] { "F" };

            var filterList = new Dictionary<string, string>() { { "TextSearch", "Clémentine" } };

            var output = CheckFacets(facetsInput, null, filterList);

            /* Total. */
            Assert.AreEqual(1, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(1, output.TotalCount, "Nombre total de résultats attendu incorrect.");
        }


        [TestMethod]
        public void Check_TextSearchWithTermAndTextSearchFilter()
        {

            var facetsInput = new FacetListInput();

            var filterList = new Dictionary<string, string>() {
                { "Genre", "F" }, // Term search
                { "TextSearch", "RODRIGEZ" }, // Text Search
            };

            var output = CheckFacets(facetsInput, "Clémentine", filterList);

            /* Total. */
            Assert.AreEqual(1, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(1, output.TotalCount, "Nombre total de résultats attendu incorrect.");
        }

        [TestMethod]
        public void Check_TextSearchWithBoosts()
        {
            var facetsInput = new FacetListInput();

            
            IList<Boost> boosts = new List<Boost>() {
                new Boost() { Field = "Nom", BoostValue = 10.0M },
            };

            // Les personnes dont le nom match avec "Robert" doivent arriver avant les personnes ayant le prénom "Robert"
            var output = CheckFacets(facetsInput, "Robert", null, boosts);

            /* Total. */
            Assert.AreEqual(4, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(4, output.TotalCount, "Nombre total de résultats attendu incorrect.");

            Assert.AreEqual("ROBERTO", output.List.ElementAt(0).Nom, "Nom de famille");
            Assert.AreEqual("ROBERT", output.List.ElementAt(1).Nom, "Nom de famille");
            Assert.AreEqual("Roberto", output.List.ElementAt(2).Prenom, "Nom de famille");
            Assert.AreEqual("Robert", output.List.ElementAt(3).Prenom, "Nom de famille");
        }

        private static void Check_Sort(bool sortDescending, IEnumerable<string> expectedNomList) {

            var input = new AdvancedQueryInput {
                ApiInput = new QueryInput {
                    Criteria = new Criteria(),
                    Skip = 0,
                    Top = 10,
                    SortFieldName = "NomSort",
                    SortDesc = sortDescending
                }
            };
            var broker = SearchBrokerManager.GetBroker<PersonneDocument>();
            var output = broker.AdvancedQuery(input);

            /* Total. */
            Assert.AreEqual(9, output.List.Count, "Nombre de résultats attendu incorrect.");
            Assert.AreEqual(9, output.TotalCount, "Nombre total de résultats attendu incorrect.");

            var nomList = output.List.Select(x => x.Nom);
            Assert.IsTrue(
                Enumerable.SequenceEqual(expectedNomList, nomList),
                "Tri attendu : " + string.Join(",", expectedNomList) + Environment.NewLine + " | Tri constaté : " + string.Join(",", nomList));
        }

        private static QueryOutput<PersonneDocument> CheckFacets(FacetListInput facetsInput, string query = null, IDictionary<string, string> filterList = null, IList<Boost> boosts = null) {

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
                    Boosts = boosts
                },
                FacetQueryDefinition = facetQueryDefinition,
                FilterList = filterList,
            };
            var broker = SearchBrokerManager.GetBroker<PersonneDocument>();
            return broker.AdvancedQuery(input);
        }

    }
}
