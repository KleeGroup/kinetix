using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;

namespace Kinetix.ClassGenerator.Configuration {

    /// <summary>
    /// Chargeur du fichier de configuration du modèle.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Conceptuellement non statique")]
    public class ModelConfigurationLoader {

        private const string VortexFileTag = "VortexFile";
        private const string CrebasFileTag = "CrebasFile";
        private const string RootNamespace = "RootNamespace";
        private const string SpaAppPath = "SpaAppPath";
        private const string JsModelRootTag = "JsModelRoot";
        private const string JsResourceRootTag = "JsResourceRoot";
        private const string IsFocus4Tag = "IsFocus4";
        private const string UkFileTag = "UkFile";
        private const string IndexFkFileTag = "IndexFkFile";
        private const string TypeFileTag = "TypeFile";
        private const string SynonymFileTag = "SynonymFile";
        private const string StaticListFileTag = "StaticListFile";
        private const string ReferenceListFileTag = "ReferenceListFile";
        private const string StaticListLabelFileTag = "StaticListLabelFile";
        private const string ReferenceListLabelFileTag = "ReferenceListLabelFile";

        private const string DefaultValuesFileTag = "DefaultValuesFile";
        private const string NoTableFileTag = "NoTableFile";
        private const string HistoriqueCreationFileTag = "HistoriqueCreationFile";
        private const string OutputDirectoryTag = "OutputDirectory";
        private const string DomainFactoryAssemblyTag = "DomainFactoryAssembly";
        private const string ListFactoryAssemblyTag = "ListFactoryAssembly";
        private const string DbContextModelTag = "DbContextModel";
        private const string IsEntityFrameworkTag = "IsEntityFrameworkTag";

        private const string ModelTypeTag = "ModelType";
        private const string IsSpaTag = "IsSpa";
        private const string DomainModelFileTag = "DomainModelFile";
        private const string ModelFilesTag = "ModelFiles";
        private const string ModelFileTag = "ModelFile";

        private const string ExtModelFilesTag = "ExtModelFiles";

        private const string SourceRepositoryTag = "SourceRepository";
        private const string LogScriptTableNameTag = "LogScriptTableName";
        private const string LogScriptVersionFieldTag = "LogScriptVersionField";
        private const string LogScriptDateFieldTag = "LogScriptDateField";
        private const string DbTypeTag = "DbType";

        private const string SsdtProjFileNameTag = "SsdtProjFileName";
        private const string SsdtTableScriptFolderTag = "SsdtTableScriptFolder";
        private const string SsdtTableTypeScriptFolderTag = "SsdtTableTypeScriptFolder";
        private const string SsdtInitReferenceListScriptFolderTag = "SsdtInitReferenceListScriptFolder";
        private const string SsdtInitStaticListScriptFolderTag = "SsdtInitStaticListScriptFolder";
        private const string SsdtInitReferenceListMainScriptNameTag = "SsdtInitReferenceListMainScriptName";
        private const string SsdtInitStaticListMainScriptNameTag = "SsdtInitStaticListMainScriptName";

        /// <summary>
        /// Charge la configuration du fichier XML.
        /// </summary>
        /// <param name="xmlPath">Chemin du fichier XML de configuration.</param>
        public void LoadModelConfiguration(string xmlPath) {

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            LoadModelFileNames(doc);

            // Paramètre pour l'OOM de domaines.
            GeneratorParameters.DomainModelFile = LoadValueFromXml(doc, DomainModelFileTag);

            // Paramètre pour le fichier d'erreurs.
            GeneratorParameters.VortexFile = LoadValueFromXml(doc, VortexFileTag);

            // Parametre pour connaitre le type du modèle à parser.
            GeneratorParameters.ModelType = LoadValueFromXml(doc, ModelTypeTag);
            GeneratorParameters.IsSpa = bool.Parse(LoadValueFromXml(doc, IsSpaTag));

            // Paramètre pour la génération des fichiers Javascript.
            GeneratorParameters.RootNamespace = TryLoadValueFromXml(doc, RootNamespace);
            GeneratorParameters.SpaAppPath = TryLoadValueFromXml(doc, SpaAppPath);
            GeneratorParameters.JsModelRoot = TryLoadValueFromXml(doc, JsModelRootTag);
            GeneratorParameters.JsResourceRoot = TryLoadValueFromXml(doc, JsResourceRootTag);
            GeneratorParameters.IsFocus4 = bool.Parse(LoadValueFromXml(doc, IsFocus4Tag));

            // Paramètres pour la génération de scripts d'initialisation pour le SQL.
            GeneratorParameters.CrebasFile = TryLoadValueFromXml(doc, CrebasFileTag);
            GeneratorParameters.UKFile = TryLoadValueFromXml(doc, UkFileTag);
            GeneratorParameters.IndexFKFile = TryLoadValueFromXml(doc, IndexFkFileTag);
            GeneratorParameters.TypeFileName = TryLoadValueFromXml(doc, TypeFileTag);
            GeneratorParameters.StaticListFile = TryLoadValueFromXml(doc, StaticListFileTag);
            GeneratorParameters.ReferenceListFile = TryLoadValueFromXml(doc, ReferenceListFileTag);
            GeneratorParameters.StaticListLabelFile = TryLoadValueFromXml(doc, StaticListLabelFileTag);
            GeneratorParameters.ReferenceListLabelFile = TryLoadValueFromXml(doc, ReferenceListLabelFileTag);

            // Paramètres pour la génération de fichiers SSDT pour le SQL (configuration).
            GeneratorParameters.DefaultValuesFile = TryLoadValueFromXml(doc, DefaultValuesFileTag);
            GeneratorParameters.NoTableFile = TryLoadValueFromXml(doc, NoTableFileTag);
            GeneratorParameters.HistoriqueCreationFile = TryLoadValueFromXml(doc, HistoriqueCreationFileTag);

            // Paramètres pour la génération de fichiers SSDT pour le SQL (fichiers cibles).
            GeneratorParameters.SsdtProjFileName = TryLoadValueFromXml(doc, SsdtProjFileNameTag);
            GeneratorParameters.SsdtTableScriptFolder = TryLoadValueFromXml(doc, SsdtTableScriptFolderTag);
            GeneratorParameters.SsdtTableTypeScriptFolder = TryLoadValueFromXml(doc, SsdtTableTypeScriptFolderTag);
            GeneratorParameters.SsdtInitReferenceListScriptFolder = TryLoadValueFromXml(doc, SsdtInitReferenceListScriptFolderTag);
            GeneratorParameters.SsdtInitStaticListScriptFolder = TryLoadValueFromXml(doc, SsdtInitStaticListScriptFolderTag);
            GeneratorParameters.SsdtInitReferenceListMainScriptName = TryLoadValueFromXml(doc, SsdtInitReferenceListMainScriptNameTag);
            GeneratorParameters.SsdtInitStaticListMainScriptName = TryLoadValueFromXml(doc, SsdtInitStaticListMainScriptNameTag);

            // Paramètres pour la génération des scripts d'initialisation pour SSDT à passage unique et loggés.
            GeneratorParameters.LogScriptTableName = LoadValueFromXml(doc, LogScriptTableNameTag);
            GeneratorParameters.LogScriptVersionField = LoadValueFromXml(doc, LogScriptVersionFieldTag);
            GeneratorParameters.LogScriptDateField = LoadValueFromXml(doc, LogScriptDateFieldTag);

            // Paramètres pour la génération des classes C#
            GeneratorParameters.OutputDirectory = LoadValueFromXml(doc, OutputDirectoryTag);
            GeneratorParameters.DomainFactoryAssembly = Path.GetFullPath(LoadValueFromXml(doc, DomainFactoryAssemblyTag));
            GeneratorParameters.ListFactoryAssembly = Path.GetFullPath(LoadValueFromXml(doc, ListFactoryAssemblyTag));
            GeneratorParameters.IsEntityFrameworkUsed = bool.Parse(TryLoadValueFromXml(doc, IsEntityFrameworkTag));
            GeneratorParameters.DbContext = TryLoadValueFromXml(doc, DbContextModelTag);

            // Paramètre pour le type de base de données cible
            GeneratorParameters.IsOracle = TryLoadValueFromXml(doc, DbTypeTag) == "oracle";

            // Paramètre de repository CVS.
            GeneratorParameters.SourceRepository = TryLoadValueFromXml(doc, SourceRepositoryTag);
        }

        /// <summary>
        /// Retourne la valeur d'un noeud unique du document XML.
        /// </summary>
        /// <param name="doc">Document XML.</param>
        /// <param name="paramName">Nom du noaud.</param>
        /// <returns>Contenu du noeud.</returns>
        private static string LoadValueFromXml(XmlDocument doc, string paramName) {
            return GetUniqueNodeByName(doc, paramName).InnerText;
        }

        /// <summary>
        /// Retourne le contenu d'un noeud unique s'il existe dans le document XML, null sinon.
        /// </summary>
        /// <param name="doc">Document XML.</param>
        /// <param name="paramName">Nom du noeud.</param>
        /// <returns>Contenu du noeud.</returns>
        private static string TryLoadValueFromXml(XmlDocument doc, string paramName) {
            XmlNodeList nodeList = doc.GetElementsByTagName(paramName);
            if (nodeList.Count != 1) {
                return null;
            }

            return nodeList.Item(0).InnerText;
        }

        /// <summary>
        /// Retourne un noeud unique du document XML.
        /// </summary>
        /// <param name="doc">Document XML.</param>
        /// <param name="paramName">Nom du noeud.</param>
        /// <returns>Noeud.</returns>
        private static XmlNode GetUniqueNodeByName(XmlDocument doc, string paramName) {
            XmlNodeList nodeList = doc.GetElementsByTagName(paramName);
            if (nodeList.Count != 1) {
                throw new XmlException("Paramètre obligatoire non-renseigné : " + paramName);
            }

            return nodeList.Item(0);
        }

        /// <summary>
        /// Charge les nom des modèles de données depuis le fichier de configuration.
        /// </summary>
        /// <param name="doc">Document XML de configuration.</param>
        private static void LoadModelFileNames(XmlDocument doc) {
            XmlNodeList nodeList = GetUniqueNodeByName(doc, ModelFilesTag).ChildNodes;
            foreach (XmlNode node in nodeList) {
                if (node.Name == ModelFileTag) {
                    GeneratorParameters.ModelFiles.Add(node.InnerText);
                }
            }

            nodeList = GetUniqueNodeByName(doc, ExtModelFilesTag).ChildNodes;
            foreach (XmlNode node in nodeList) {
                if (node.Name == ModelFileTag) {
                    GeneratorParameters.ExtModelFiles.Add(node.InnerText);
                }
            }
        }
    }
}
