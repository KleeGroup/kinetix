using System.Linq;
using Kinetix.Search.Broker;
using Kinetix.Search.Test.Dum;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinetix.Search.Test.SearchBrokerTest {

    /// <summary>
    /// Teste la méthode Query.
    /// </summary>
    [TestClass]
    public class QueryTest {

        [TestMethod]
        public void Check_PrenomNom_Ok() {
            CheckQuery("Robert TOUTLEMONDE", 1);
        }

        [TestMethod]
        public void Check_Prenom_Ok() {
            CheckQuery("Camille", 1);
        }

        [TestMethod]
        public void Check_Nom_Ok() {
            CheckQuery("TOUTLEMONDE", 1);
        }

        [TestMethod]
        public void Check_NomPrenom_Ok() {
            CheckQuery("TOUTLEMONDE Robert", 1);
        }

        [TestMethod]
        public void Check_NomMinuscule_Ok() {
            CheckQuery("toutlemonde", 1);
        }

        [TestMethod]
        public void Check_Autre_Ko() {
            CheckQuery("Alice", 0);
        }

        [TestMethod]
        public void Check_NomDeb_Ok() {
            CheckQuery("TOUTLE", 1);
        }

        [TestMethod]
        public void Check_PrenomDeb_Ok() {
            CheckQuery("Cam", 1);
        }

        [TestMethod]
        public void Check_NomDebPrenomDeb_Ok() {
            CheckQuery("TOUTLE Rob", 1);
        }

        [TestMethod]
        public void Check_PrenomDebNomDeb_Ok() {
            CheckQuery("Rob TOUTLE", 1);
        }

        [TestMethod]
        public void Check_NomFin_Ko() {
            CheckQuery("LEMONDE", 0);
        }

        [TestMethod]
        public void Check_NomMilieu_Ko() {
            CheckQuery("LEMON", 0);
        }

        [TestMethod]
        public void Check_SpecialChar_Ko() {
            CheckQuery("]*[", 0);
        }

        [TestMethod]
        public void Check_With_Hyphen_Ok() {
            CheckQuery("Jean-Baptiste", 1);
        }

        [TestMethod]
        public void Check_Without_Hyphen_Ok() {
            CheckQuery("Jean Baptiste", 1);
        }

        [TestMethod]
        public void Check_Without_Hyphen_And_One_Word_Ok() {
            CheckQuery("Jean", 1);
        }

        [TestMethod]
        public void Check_Without_Quote_Ok() {
            CheckQuery("D ALEMBERT", 1);
        }

        [TestMethod]
        public void Check_With_Quote_Ok() {
            CheckQuery("D'ALEMBERT", 1);
        }

        [TestMethod]
        public void Check_Without_Quote_And_One_Word_Ok() {
            CheckQuery("ALEMBERT", 1);
        }

        [TestMethod]
        public void Check_Empty_Ko() {
            CheckQuery(string.Empty, 0);
        }

        [TestMethod]
        public void Check_PrenomAccentTextAccent_Ok() {
            CheckQuery("Clémentine", 1);
        }

        [TestMethod]
        public void Check_PrenomAccentTextSansAccent_Ok() {
            CheckQuery("Clementine", 1);
        }

        [TestMethod]
        public void Check_PrenomSansAccentTextAccent_Ok() {
            CheckQuery("Càmille", 1);
        }

        [TestMethod]
        public void Check_PrenomSansAccentTextSansAccent_Ok() {
            CheckQuery("Camille", 1);
        }

        [TestMethod]
        public void Check_SecurityVraiPositifUnSeul_Ok() {
            CheckQuery("TOUTLEMONDE", 1, "92");
        }

        [TestMethod]
        public void Check_SecurityVraiPositifPlusieurs_Ok() {
            CheckQuery("TOUTLEMONDE", 1, "92 29");
        }

        [TestMethod]
        public void Check_SecurityVraiNegatifUnSeul_Ko() {
            CheckQuery("TOUTLEMONDE", 0, "29");
        }

        [TestMethod]
        public void Check_SecurityVraiNegatifPlusieurs_Ko() {
            CheckQuery("TOUTLEMONDE", 0, "29 26");
        }

        private static void CheckQuery(string text, int count, string security = null) {
            var broker = SearchBrokerManager.GetBroker<PersonneDocument>();

            var result = broker.Query(text, security);
            Assert.IsNotNull(result);
            Assert.AreEqual(count, result.Count());
        }
    }
}
