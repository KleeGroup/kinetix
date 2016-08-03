using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Kinetix.ClassGenerator.Checker;
using Kinetix.ClassGenerator.CodeGenerator;
using Kinetix.ClassGenerator.Configuration;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.NVortex;
using Kinetix.ClassGenerator.SchemaGenerator;
using Kinetix.ClassGenerator.SsdtSchemaGenerator;
using Kinetix.ClassGenerator.SsdtSchemaGenerator.Contract;
using Kinetix.ClassGenerator.Tfs;
using Kinetix.ClassGenerator.XmlParser;
using Kinetix.ClassGenerator.XmlParser.EapReader;
using Kinetix.ClassGenerator.XmlParser.OomReader;
using Kinetix.ComponentModel;
using Kinetix.ServiceModel;
using Kinetix.Tfs.Tools.Client;

namespace Kinetix.ClassGenerator.Main {

    /// <summary>
    /// Générateur principal fédérant les autres générateurs.
    /// </summary>
    public class MainGenerator {

        /// <summary>
        /// Composant de chargement de la configuration.
        /// </summary>
        private readonly ConfigurationLoader _configurationLoader = new ConfigurationLoader();

        /// <summary>
        /// Composant de génération des scripts de schéma pour SSDT.
        /// </summary>
        private readonly ISqlServerSsdtSchemaGenerator _ssdtSchemaGenerator = new SqlServerSsdtSchemaGenerator();

        /// <summary>
        /// Composant de génération des scripts d'insertion pour SSDT.
        /// </summary>
        private readonly ISqlServerSsdtInsertGenerator _ssdtInsertGenerator = new SqlServerSsdtInsertGenerator();

        /// <summary>
        /// Composant de génération du schéma en Javascript.
        /// </summary>
        private readonly JavascriptSchemaGenerator _javascriptSchemaGenerator = new JavascriptSchemaGenerator();

        /// <summary>
        /// Composant de génération des ressources en javascript.
        /// </summary>
        private readonly JavascriptResourceGenerator _javascriptResourceGenerator = new JavascriptResourceGenerator();

        /// <summary>
        /// Liste des domaines.
        /// </summary>
        private ICollection<IDomain> _domainList;

        /// <summary>
        /// Liste des modèles objet en mémoire.
        /// </summary>
        private ICollection<ModelRoot> _modelList;

        /// <summary>
        /// Générateur de schéma SQL.
        /// </summary>
        private AbstractSchemaGenerator _schemaGenerator;

        /// <summary>
        /// Exécute la génération.
        /// </summary>
        public void Generate() {

            // Vérification.
            CheckModelFiles();
            CheckOutputDirectory();

            // Chargements des domaines.
            _domainList = LoadDomain();

            // On initialise dans un singleton le client TFS.
            string workspaceDir = Path.GetFullPath(".");
            using (TfsManager.Client = TfsClient.Connect(GeneratorParameters.TfsCollectionUrl, workspaceDir)) {

                // Chargement des modèles objet en mémoire.
                LoadObjectModel();

                // Génération.
                GenerateSqlSchema();
                GenerateJavascript();
            }

            // Pause.
            if (GeneratorParameters.Pause) {
                Console.Out.WriteLine();
                Console.Out.Write("Traitement terminé, veuillez appuyer sur une touche pour fermer cette fenêtre...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Vérifie la capacité de traduire les fichiers modeles.
        /// </summary>
        private static void CheckModelFiles() {
            foreach (string file in GeneratorParameters.ModelFiles) {
                if (!File.Exists(file)) {
                    Console.Error.WriteLine("Le fichier " + file + " n'existe pas dans le dossier courant " + System.IO.Directory.GetCurrentDirectory() + ".");
                    Environment.Exit(-1);
                }
            }
        }

        /// <summary>
        /// Charge le générateur de schéma SQL.
        /// </summary>
        /// <returns>Générateur de schéma SQL.</returns>
        private static AbstractSchemaGenerator LoadSchemaGenerator() {
            AbstractSchemaGenerator schemaGenerator;
            if (GeneratorParameters.IsOracle) {
                schemaGenerator = new OracleSchemaGenerator();
            } else {
                schemaGenerator = new SqlServerSchemaGenerator();
            }

            return schemaGenerator;
        }

        /// <summary>
        /// Vérifie la validité du répertoire de génération.
        /// </summary>
        private static void CheckOutputDirectory() {
            if (!Directory.Exists(GeneratorParameters.OutputDirectory)) {
                Console.Error.WriteLine("Le répertoire de génération " + GeneratorParameters.OutputDirectory + " n'existe pas.");
            }

            DirectoryInfo dirInfo = new DirectoryInfo(GeneratorParameters.OutputDirectory);
            if ((dirInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                Console.Error.WriteLine("Le répertoire de génération " + GeneratorParameters.OutputDirectory + " est en lecture seule.");
            }
        }

        /// <summary>
        /// Charge les domaines utilisés par l'application.
        /// </summary>
        /// <returns>Liste des domaines de l'application.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "Chargement dynamique des domaines")]
        private static ICollection<IDomain> LoadDomain() {
            List<IDomain> domainList = new List<IDomain>();
            Assembly assembly = Assembly.LoadFile(GeneratorParameters.DomainFactoryAssembly);
            foreach (Module module in assembly.GetModules()) {
                foreach (Type type in module.GetTypes()) {
                    if (type.GetCustomAttributes(typeof(DomainMetadataTypeAttribute), false).Length > 0) {
                        domainList.AddRange(DomainManager.Instance.RegisterDomainMetadataType(type));
                    }
                }
            }

            return domainList;
        }

        /// <summary>
        /// Returns collection of TableInit.
        /// </summary>
        /// <param name="isStatic">Indique si la table est statique.</param>
        /// <returns>Collection of TableInit.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "Chargement dynamique des domaines")]
        private static ICollection<TableInit> LoadTableInitList(bool isStatic) {
            ICollection<TableInit> list = null;
            Assembly assembly = Assembly.LoadFile(GeneratorParameters.DomainFactoryAssembly);
            foreach (Module module in assembly.GetModules()) {
                foreach (Type type in module.GetTypes()) {
                    if (typeof(AbstractListFactory).IsAssignableFrom(type)) {
                        AbstractListFactory factory = (AbstractListFactory)Activator.CreateInstance(type);
                        factory.Init();
                        if ((isStatic && factory.IsStatic) || (!isStatic && !factory.IsStatic)) {
                            list = factory.Items;
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Ecrit les erreurs sur la sortie standard
        /// et retourne <code>True</code> si aucune erreur bloquante n'a été trouvée, <code>False</code> sinon.
        /// </summary>
        /// <param name="msgList">Liste des messages.</param>
        /// <returns><code>True</code> si les classes sont générables, <code>False</code> sinon.</returns>
        private static bool CanGenerate(List<NVortexMessage> msgList) {
            if (msgList == null) {
                throw new ArgumentNullException("msgList");
            }

            bool hasError = false;
            foreach (NVortexMessage msg in msgList) {
                if (msg.IsError) {
                    hasError = true;
                }

                Console.Out.WriteLine(string.Format(CultureInfo.InvariantCulture, "[{0}] - {1} : {2}", msg.Category, msg.Code, msg.Description));
            }

            return !hasError;
        }

        /// <summary>
        /// Retourne le nombre de messages de type erreur de la pile de message en paramètres.
        /// </summary>
        /// <param name="msgList">La liste des messages.</param>
        /// <returns>Le nombre de message de type erreur.</returns>
        private static int NbErrorMessage(List<NVortexMessage> msgList) {
            int i = 0;
            foreach (NVortexMessage msg in msgList) {
                if (msg.IsError) {
                    ++i;
                }
            }

            return i;
        }

        /// <summary>
        /// Charge le parser de modèle objet.
        /// </summary>
        /// <returns>Parser de modèle objet.</returns>
        private IModelParser LoadModelParser() {
            IModelParser parser;
            switch (GeneratorParameters.ModelType) {
                case "oom":
                    parser = new OomParser(GeneratorParameters.ModelFiles, _domainList);
                    break;
                case "eap":
                    parser = new EapParser(GeneratorParameters.ModelFiles, _domainList);
                    break;
                default:
                    throw new NotSupportedException("Le modèle doit être de type oom ou eap.");
            }

            return parser;
        }

        /// <summary>
        /// Charge en mémoire les modèles objet et génères les warnings.
        /// </summary>
        private void LoadObjectModel() {

            // Charge le parser.
            IModelParser modelParser = LoadModelParser();

            // Parse le modèle.
            _modelList = modelParser.Parse();

            // Charge les listes de références.
            ICollection<TableInit> staticTableInitList = LoadTableInitList(true);
            ICollection<TableInit> referenceTableInitList = LoadTableInitList(false);

            // Génère les warnings pour le modèle.
            List<NVortexMessage> messageList = new List<NVortexMessage>(modelParser.ErrorList);
            messageList.AddRange(CodeChecker.Check(_modelList, _domainList));
            messageList.AddRange(StaticListChecker.Instance.Check(_modelList, staticTableInitList));
            messageList.AddRange(ReferenceListChecker.Instance.Check(_modelList, referenceTableInitList));
            if (GeneratorParameters.GenerateSql) {
                _schemaGenerator = LoadSchemaGenerator();
                messageList.AddRange(_schemaGenerator.CheckAllIdentifiersNames(_modelList));
            }

            NVortexGenerator.Generate(messageList, GeneratorParameters.VortexFile, GeneratorParameters.SourceRepository, "ClassGenerator");
            if (GeneratorParameters.Generate) {
                if (CanGenerate(messageList)) {
                    Console.Out.WriteLine("***** Génération du modèle *****");
                    AbstractCodeGenerator generator = new CSharpCodeGenerator(GeneratorParameters.OutputDirectory);
                    generator.Generate(_modelList);
                }
            }

            if (NbErrorMessage(messageList) != 0) {
                Environment.Exit(-NbErrorMessage(messageList));
            }
        }

        /// <summary>
        /// Génère le schéma SQL.
        /// </summary>
        private void GenerateSqlSchema() {
            if (GeneratorParameters.GenerateSql) {
                bool generateSsdt = true;
                bool generateProcedural = false;

                if (generateSsdt) {

                    // Charge la configuration de génération (default values, no table, historique de l'ordre de création des colonnes).
                    _configurationLoader.LoadConfigurationFiles(_modelList);

                    // Génération pour déploiement SSDT.
                    _ssdtSchemaGenerator.GenerateSchemaScript(
                        _modelList,
                        GeneratorParameters.SsdtTableScriptFolder,
                        GeneratorParameters.SsdtTableTypeScriptFolder);


                    _ssdtInsertGenerator.GenerateListInitScript(
                        StaticListChecker.Instance.DictionaryItemInit,
                        GeneratorParameters.SsdtInitStaticListScriptFolder,
                        GeneratorParameters.SsdtInitStaticListMainScriptName,
                        "delta_static_lists.sql",
                        true);
                    _ssdtInsertGenerator.GenerateListInitScript(
                        ReferenceListChecker.Instance.DictionaryItemInit,
                        GeneratorParameters.SsdtInitReferenceListScriptFolder,
                        GeneratorParameters.SsdtInitReferenceListMainScriptName,
                        "delta_reference_lists.sql",
                        false);
                }

                if (generateProcedural) {
                    _schemaGenerator.GenerateSchemaScript(
                        _modelList,
                        GeneratorParameters.CrebasFile,
                        GeneratorParameters.UKFile,
                        GeneratorParameters.IndexFKFile,
                        GeneratorParameters.TypeFileName);


                    _schemaGenerator.GenerateListInitScript(
                        StaticListChecker.Instance.DictionaryItemInit,
                        GeneratorParameters.StaticListFile,
                        "delta_static_lists.sql",
                        true);
                    _schemaGenerator.GenerateListInitScript(
                        ReferenceListChecker.Instance.DictionaryItemInit,
                        GeneratorParameters.ReferenceListFile,
                        "delta_reference_lists.sql",
                        false);

                    var allClassList = _modelList.SelectMany(x => x.Namespaces).SelectMany(x => x.Value.ClassList);
                    var referenceList = allClassList.Where(x => x.IsReference && !x.IsStatique).ToList();
                    var staticList = allClassList.Where(x => x.IsStatique).ToList();

                    _schemaGenerator.GenerateReferenceTranslationList(staticList, GeneratorParameters.StaticListLabelFile, true);
                    _schemaGenerator.GenerateReferenceTranslationList(referenceList, GeneratorParameters.ReferenceListLabelFile, false);
                }
            }
        }

        /// <summary>
        /// Génère les fichiers Javascript.
        /// </summary>
        private void GenerateJavascript() {
            if (GeneratorParameters.GenerateJavascript) {
                _javascriptSchemaGenerator.Generate(_modelList, $"{GeneratorParameters.SpaAppPath}/{GeneratorParameters.JsModelRoot}");
                _javascriptResourceGenerator.Generate(_modelList, $"{GeneratorParameters.SpaAppPath}/{GeneratorParameters.JsResourceRoot}");
            }
        }
    }
}
