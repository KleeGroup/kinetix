using System.Threading;
using Kinetix.Search.Broker;
using Kinetix.Search.Config;
using Kinetix.Search.Elastic;
using Kinetix.Search.Test.Dum;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinetix.Search.Test {

    [TestClass]
    public class AssemblySetup {

        /* Config */
        private const string DataSourceName = "default";
        private const string NodeUri = "http://docker-vertigo.part.klee.lan.net:9200/";
        private const string IndexName = "kinetix_search_test";

        [AssemblyInitialize]
        public static void Init(TestContext context) {

            /* Initialise le store Elastic. */
            SearchBrokerManager.RegisterDefaultDataSource(DataSourceName);
            SearchBrokerManager.Instance.RegisterStore(DataSourceName, typeof(ElasticStore<>));
            ElasticManager.Instance.RegisterSearchSettings(new SearchSettings {
                Name = DataSourceName,
                NodeUri = NodeUri,
                IndexName = IndexName
            });

            /* Créé l'index. */
            if (ElasticManager.Instance.ExistIndex(DataSourceName)) {
                ElasticManager.Instance.DeleteIndex(DataSourceName);
            }

            ElasticManager.Instance.InitIndex(DataSourceName, new IndexConfigurator());

            /* Créé le type de document. */
            var broker = SearchBrokerManager.GetBroker<PersonneDocument>();
            broker.CreateDocumentType();

            /* Ajoute des documents. */
            var doc1 = new PersonneDocument {
                Id = 7,
                Nom = "TOUTLEMONDE",
                NomSort = "TOUTLEMONDE",
                Prenom = "Robert",
                Text = "Robert TOUTLEMONDE",
                TextSearch = "Robert TOUTLEMONDE",
                DepartementList = "92 75",
                Genre = "M"
            };

            var doc2 = new PersonneDocument {
                Id = 8,
                Nom = "MARCHAND",
                NomSort = "MARCHAND",
                Prenom = "Camille",
                Text = "Camille MARCHAND",
                TextSearch = "Camille MARCHAND",
                DepartementList = "01 02",
                Genre = "F"
            };

            var doc3 = new PersonneDocument {
                Id = 9,
                Nom = "RODRIGEZ",
                NomSort = "RODRIGEZ",
                Prenom = "Clémentine",
                Text = "Clémentine RODRIGEZ",
                TextSearch = "Clémentine RODRIGEZ",
                DepartementList = "03 04",
                Genre = "F"
            };

            var doc4 = new PersonneDocument {
                Id = 10,
                Nom = "BUCHE",
                NomSort = "BUCHE",
                Prenom = "Géraldine",
                Text = "Géraldine BUCHE",
                TextSearch = "Géraldine BUCHE",
                DepartementList = "99 98",
                Genre = null
            };

            var doc5 = new PersonneDocument {
                Id = 11,
                Nom = "RAY",
                NomSort = "RAY",
                Prenom = "Jean-Baptiste",
                Text = "Jean-Baptiste RAY",
                TextSearch = "Jean-Baptiste RAY",
                DepartementList = "92 75",
                Genre = "M"
            };

            var doc6 = new PersonneDocument {
                Id = 12,
                Nom = "D'ALEMBERT",
                NomSort = "D'ALEMBERT",
                Prenom = "Roger",
                Text = "Roger D'ALEMBERT",
                TextSearch = "Roger D'ALEMBERT",
                DepartementList = "92 75",
                Genre = "M"
            };

            var doc7 = new PersonneDocument
            {
                Id = 13,
                Nom = "ROBERT",
                NomSort = "ROBERT",
                Prenom = "Martin",
                Text = "Martin ROBERT",
                TextSearch = "Martin ROBERT",
                DepartementList = "92 75",
                Genre = "M"
            };

            var doc8 = new PersonneDocument
            {
                Id = 14,
                Nom = "SANCHEZ",
                NomSort = "SANCHEZ",
                Prenom = "Roberto",
                Text = "Roberto SANCHEZ",
                TextSearch = "Roberto SANCHEZ",
                DepartementList = "92 75",
                Genre = "M"
            };

            var doc9 = new PersonneDocument
            {
                Id = 15,
                Nom = "ROBERTO",
                NomSort = "ROBERTO",
                Prenom = "Jean",
                Text = "Jean ROBERTO",
                TextSearch = "Jean ROBERTO",
                DepartementList = "92 75",
                Genre = "M"
            };

            broker.Put(doc1);
            broker.Put(doc2);
            broker.Put(doc3);
            broker.Put(doc4);
            broker.Put(doc5);
            broker.Put(doc6);
            broker.Put(doc7);
            broker.Put(doc8);
            broker.Put(doc9);


            /* Attends que les documents soit disponibles. */
            Thread.Sleep(1000);
        }

        [AssemblyCleanup]
        public static void Clean() {
            try {
                /* Supprime l'index. */
                if (ElasticManager.Instance.ExistIndex(DataSourceName)) {
                    ElasticManager.Instance.DeleteIndex(DataSourceName);
                }
            } catch {
                // RAS.
            }
        }
    }
}
