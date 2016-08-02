using System;
using System.Collections.Generic;
using System.IO;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.Tfs;

namespace Kinetix.ClassGenerator.CodeGenerator {

    /// <summary>
    /// Abstract class tio generate JS scripts.
    /// </summary>
    public abstract class AbstractJavascriptGenerator {

#pragma warning disable SA1310

        /// <summary>
        /// Tab character.
        /// </summary>
        public const string TAB = "    ";

        /// <summary>
        /// Open bracket character.
        /// </summary>
        public const string OPEN_BRACKET = ": {";

        /// <summary>
        /// Closing bracket character.
        /// </summary>
        public const string CLOSE_BRACKET = "}";

        /// <summary>
        /// Double quote character.
        /// </summary>
        public const string DOUBLE_QUOTE = "\"";

        /// <summary>
        /// Comma character.
        /// </summary>
        public const string COMA = ",";

        /// <summary>
        /// Comma point character.
        /// </summary>
        public const string COMA_POINT = ";";

        /// <summary>
        /// JS require and open bracket character.
        /// </summary>
        public const string OPEN_REQUIRE = ": require(\"";

        /// <summary>
        /// JS close require character.
        /// </summary>
        public const string CLOSE_REQUIRE = "\")";

#pragma warning restore SA1310

        /// <summary>
        /// Ecrit dans le flux de sortie la fermeture du noeud courant.
        /// </summary>
        /// <param name="writer">Flux de sortie.</param>
        /// <param name="indentionLevel">Idention courante.</param>
        /// <param name="isLast">Si true, on n'ajoute pas de virgule à la fin.</param>
        public static void WriteCloseBracket(TextWriter writer, int indentionLevel, bool isLast) {
            for (int i = 0; i < indentionLevel; i++) {
                writer.Write(TAB);
            }

            writer.Write(CLOSE_BRACKET);
            writer.WriteLine(!isLast ? COMA : string.Empty);
        }

        /// <summary>
        /// Formate le nom en javascript.
        /// </summary>
        /// <param name="name">Nom a formatter.</param>
        /// <returns>Nom formatté.</returns>
        public static string FormatJsName(string name) {
            return FirstToLower(name);
        }

        /// <summary>
        /// Formate le nom en javascript.
        /// </summary>
        /// <param name="name">Nom a formatter.</param>
        /// <returns>Nom formatté.</returns>
        public static string FormatJsPropertyName(string name) {
            return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
        }

        /// <summary>
        /// Génère le code des classes.
        /// </summary>
        /// <param name="modelRootList">Liste des modeles.</param>
        /// <param name="outputDirectory">Nom du script.</param>
        public void Generate(ICollection<ModelRoot> modelRootList, string outputDirectory) {
            if (modelRootList == null) {
                throw new ArgumentNullException("modelRootList");
            }

            IDictionary<string, List<ModelClass>> nameSpaceMap = new Dictionary<string, List<ModelClass>>();
            foreach (ModelRoot model in modelRootList) {
                foreach (ModelNamespace modelNameSpace in model.Namespaces.Values) {
                    string namespaceName = modelNameSpace.Name;
                    if (namespaceName.EndsWith("DataContract")) {
                        namespaceName = namespaceName.Substring(0, namespaceName.Length - 12);
                    } else if (namespaceName.EndsWith("Contract")) {
                        namespaceName = namespaceName.Substring(0, namespaceName.Length - 8);
                    }

                    if (!nameSpaceMap.ContainsKey(namespaceName)) {
                        nameSpaceMap.Add(namespaceName, new List<ModelClass>());
                    }

                    nameSpaceMap[namespaceName].AddRange(modelNameSpace.ClassList);
                }
            }

            int i = 1;
            foreach (KeyValuePair<string, List<ModelClass>> entry in nameSpaceMap) {
                var isLast = nameSpaceMap.Count == i++;
                var dirInfo = Directory.CreateDirectory(outputDirectory);
                var fileName = FirstToLower(entry.Key);
                WriteNameSpaceNode(dirInfo.FullName + "/" + fileName + ".js", entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Génère le noeud de la proprité.
        /// </summary>
        /// <param name="writer">Flux de sortie.</param>
        /// <param name="property">Propriété.</param>
        /// <param name="isLast">True s'il s'agit du dernier noeud de la classe.</param>
        protected abstract void WritePropertyNode(TextWriter writer, ModelProperty property, bool isLast);

        /// <summary>
        /// Set the first character to lower.
        /// </summary>
        /// <param name="value">String to edit.</param>
        /// <returns>Parser string.</returns>
        private static string FirstToLower(string value) {
            return value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
        }

        /// <summary>
        /// Générère le noeud de namespace.
        /// </summary>
        /// <param name="outputFileNameJavascript">Nom du fichier de sortie..</param>
        /// <param name="namespaceName">Nom du namespace.</param>
        /// <param name="modelClassList">Liste des classe du namespace.</param>
        private void WriteNameSpaceNode(string outputFileNameJavascript, string namespaceName, ICollection<ModelClass> modelClassList) {
            using (TextWriter writerJs = new TfsJsFileWriter(outputFileNameJavascript)) {

                writerJs.WriteLine($"export const {FirstToLower(namespaceName)} = {{");
                int i = 1;
                foreach (ModelClass classe in modelClassList) {
                    WriteClasseNode(writerJs, classe, modelClassList.Count == i++);
                }

                writerJs.WriteLine(CLOSE_BRACKET + COMA_POINT);
            }
        }

        /// <summary>
        /// Générère le noeus de classe.
        /// </summary>
        /// <param name="writer">Flux de sortie.</param>
        /// <param name="classe">Classe.</param>
        /// <param name="isLast">True s'il s'agit de al dernière classe du namespace.</param>
        private void WriteClasseNode(TextWriter writer, ModelClass classe, bool isLast) {
            writer.WriteLine(TAB + FormatJsName(classe.Name) + OPEN_BRACKET);
            int i = 1;
            foreach (ModelProperty property in classe.PropertyList) {
                WritePropertyNode(writer, property, classe.PropertyList.Count == i++);
            }

            WriteCloseBracket(writer, 1, isLast);
        }
    }
}
