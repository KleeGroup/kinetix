using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.Tfs;

namespace Kinetix.ClassGenerator.Configuration {

    /// <summary>
    /// Chargeur de la configuration de génération du ClassGenerator.
    /// </summary>
    [SuppressMessage("Cpd", "Cpd", Justification = "Pas d'enjeu.")]
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Conceptuellement non statique")]
    public class ConfigurationLoader {

        /// <summary>
        /// Charge la configuration sur la liste des modèles.
        /// </summary>
        /// <param name="modelList">Liste des modèles.</param>
        public void LoadConfigurationFiles(ICollection<ModelRoot> modelList) {

            // Mise à plat des classe par nom de table.
            var classByTableMap = modelList
                .SelectMany(x => x.Namespaces.Values)
                .SelectMany(x => x.ClassList)
                .Where(x => x.IsTableGenerated)
                .ToDictionary(x => x.DataContract.Name);

            // Charge la configuration des default values.
            if (!string.IsNullOrEmpty(GeneratorParameters.DefaultValuesFile)) {
                LoadConfigurationDefaultValue(classByTableMap, GeneratorParameters.DefaultValuesFile);
            }

            // Charge la configuration des tables à ne pas générer.
            if (!string.IsNullOrEmpty(GeneratorParameters.NoTableFile)) {
                LoadConfigurationNoTable(classByTableMap, GeneratorParameters.NoTableFile);
            }

            // Charge la configuration de l'historique de création.
            if (!string.IsNullOrEmpty(GeneratorParameters.HistoriqueCreationFile)) {
                LoadConfigurationHistoriqueCreation(classByTableMap, GeneratorParameters.HistoriqueCreationFile);
            }
        }

        /// <summary>
        /// Charge la configuration des default values.
        /// </summary>
        /// <param name="classByTableMap">Map nom de table => classe de la table.</param>
        /// <param name="configurationFilePath">Chemin du fichier de configuration.</param>
        private static void LoadConfigurationDefaultValue(IDictionary<string, ModelClass> classByTableMap, string configurationFilePath) {
            if (!File.Exists(configurationFilePath)) {
                Console.WriteLine("Fichier de configuration absent : " + configurationFilePath);
                return;
            }

            Console.WriteLine("Chargement de la configuration des default values...");

            // Chargement du fichier de configuration.
            var xDoc = XDocument.Load(configurationFilePath);

            // Lecture et affectation des nullsparse.
            foreach (XElement tableNode in xDoc.Root.Elements()) {
                var tableName = tableNode.Attribute("name").Value;
                ModelClass clazz;
                if (!classByTableMap.TryGetValue(tableName, out clazz)) {
                    Console.WriteLine("Nom de table inconnu : " + tableName);
                    continue;
                }

                var propertyByColumnMap = clazz.PersistentPropertyList.ToDictionary(x => x.DataMember.Name);
                foreach (XElement columnNode in tableNode.Elements()) {
                    var columnName = columnNode.Attribute("name").Value;
                    ModelProperty property;
                    if (!propertyByColumnMap.TryGetValue(columnName, out property)) {
                        Console.WriteLine("Colonne inconnue : " + tableName + "." + columnName);
                        continue;
                    }

                    foreach (XElement defaultNode in columnNode.Elements()) {
                        // La colonne est dans le fichier de configuration, on active la default value.
                        property.DefaultValue = defaultNode.Value;
                    }
                }
            }

            Console.WriteLine("Chargement de la configuration des default values terminé.");
        }

        /// <summary>
        /// Charge la configuration des tables à ne pas générer.
        /// </summary>
        /// <param name="classByTableMap">Map nom de table => classe de la table.</param>
        /// <param name="configurationFilePath">Chemin du fichier de configuration.</param>
        private static void LoadConfigurationNoTable(IDictionary<string, ModelClass> classByTableMap, string configurationFilePath) {
            if (!File.Exists(configurationFilePath)) {
                Console.WriteLine("Fichier de configuration absent : " + configurationFilePath);
                return;
            }

            Console.WriteLine("Chargement de la configuration des tables à ne pas générer...");

            // Chargement du fichier de configuration.
            var xDoc = XDocument.Load(configurationFilePath);

            // Lecture et affectation des nullsparse.
            foreach (XElement tableNode in xDoc.Root.Elements()) {
                var tableName = tableNode.Attribute("name").Value;
                ModelClass clazz;
                if (!classByTableMap.TryGetValue(tableName, out clazz)) {
                    Console.WriteLine("Nom de table inconnu : " + tableName);
                    continue;
                }

                // Cas où on ne désactive la génération que de certains indexes.
                var indexList = tableNode.Elements("Index");
                if (indexList.Any()) {
                    foreach (string index in indexList.Select(x => x.Attribute("name").Value)) {
                        clazz.IndexNotGeneratedList.Add(index);
                    }

                    continue;
                }

                // Cas où on désactive la génération de toute la table.
                clazz.IsNoTable = true;
            }

            Console.WriteLine("Chargement de la configuration des tables à ne pas générer terminé.");
        }

        /// <summary>
        /// Charge la configuration de l'historique de création.
        /// </summary>
        /// <param name="classByTableMap">Map nom de table => classe de la table.</param>
        /// <param name="configurationFilePath">Chemin du fichier de configuration.</param>
        private static void LoadConfigurationHistoriqueCreation(IDictionary<string, ModelClass> classByTableMap, string configurationFilePath) {
            if (!File.Exists(configurationFilePath)) {
                Console.WriteLine("Fichier de configuration absent : " + configurationFilePath);
                return;
            }

            Console.WriteLine("Chargement de la configuration de l'historique de création des colonnes...");

            // Chargement du fichier de configuration.
            var xDoc = XDocument.Load(configurationFilePath);

            // Lecture et affectation des ordres de colonnes.
            var tablesNode = xDoc.Root.Element("Tables");
            var tableNodes = tablesNode.Elements().ToList();
            foreach (XElement tableNode in tableNodes) {
                var tableName = tableNode.Attribute("name").Value;
                ModelClass clazz;
                if (!classByTableMap.TryGetValue(tableName, out clazz)) {
                    Console.WriteLine("Nom de table inconnu : " + tableName);
                    continue;
                }

                var propertyByColumnMap = clazz.PersistentPropertyList.ToDictionary(x => x.DataMember.Name);
                var index = 0;
                foreach (XElement columnNode in tableNode.Elements()) {
                    var columnName = columnNode.Attribute("name").Value;

                    ModelProperty property;
                    if (!propertyByColumnMap.TryGetValue(columnName, out property)) {
                        Console.WriteLine("Colonne inconnue : " + tableName + "." + columnName);
                        continue;
                    }

                    // Stockage de l'ordre
                    index++;
                    property.DataMember.Order = index;
                }

                // Calcul de l'ordre pour les nouvelles colonnes.
                var addedColumnList = propertyByColumnMap.Values.Where(x => x.DataMember.Order == null).ToList();
                foreach (var property in addedColumnList) {
                    index++;
                    property.DataMember.Order = index;
                }

                // Ajout des nouvelles colonnes à l'XML de configuration, à la fin de la table.
                var addedElementList = addedColumnList.Select(x => new XElement(
                    "Column",
                    new XAttribute("name", x.DataMember.Name)));
                foreach (var newItem in addedElementList) {
                    tableNode.Add(newItem);
                }
            }

            // Créé les nouvelles tables.
            var tableNodeMap = tableNodes.ToDictionary(x => x.Attribute("name").Value);

            Dictionary<string, ModelClass> classByTableMapSansView = new Dictionary<string, ModelClass>();
            foreach (var kvp in classByTableMap) {
                if (!kvp.Value.IsView) {
                    classByTableMapSansView.Add(kvp.Key, kvp.Value);
                }
            }

            var addedTableNameList = classByTableMapSansView.Keys.Where(x => !tableNodeMap.ContainsKey(x)).ToList();

            var addedTableNodeList = addedTableNameList.Select(tableName => {
                var clazz = classByTableMap[tableName];
                XElement tableNode = new XElement("Table", new XAttribute("name", tableName));
                foreach (var column in clazz.PersistentPropertyList) {
                    var columnNode = new XElement("Column", new XAttribute("name", column.DataMember.Name));
                    tableNode.Add(columnNode);
                }
                return tableNode;
            }).ToList();

            // Ajoute les nouvelles tables aux anciennes.
            tableNodes.AddRange(addedTableNodeList);

            // Trie les tables par ordre alphabétiques.
            tableNodes = tableNodes.OrderBy(x => x.Attribute("name").Value).ToList();

            // Remplace la collection dans le document XML.
            tablesNode.RemoveNodes();
            tablesNode.Add(tableNodes.ToArray());

            Console.WriteLine("Chargement de la configuration de l'historique de création des colonnes terminé.");

            // Sauvegarde du fichier avec les nouvelles colonnes.
            try {
                Console.WriteLine("Sauvegarde de la configuration de l'historique de création des colonnes....");
                using (TextWriter tw = new TfsXmlFileWriter(configurationFilePath)) {
                    xDoc.Save(tw);
                }

                Console.WriteLine("Sauvegarde de la configuration de l'historique de création des colonnes terminé.");
            } catch (Exception ex) {
                Console.WriteLine("Echec de la sauvegarde de " + configurationFilePath);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
