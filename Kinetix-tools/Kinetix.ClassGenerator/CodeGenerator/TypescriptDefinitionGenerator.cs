using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.Templates;
using static Kinetix.ClassGenerator.Utils;

namespace Kinetix.ClassGenerator.CodeGenerator {

    /// <summary>
    /// Générateur de définitions Typescript.
    /// </summary>
    public class TypescriptDefinitionGenerator {

        /// <summary>
        /// Génère les définitions Typescript.
        /// </summary>
        /// <param name="modelRootList">La liste des modèles.</param>
        /// <param name="spaAppPath">Le chemin de la SPA.</param>
        /// <param name="rootNamespace">Le namespace de base de l'application.</param>
        public void Generate(ICollection<ModelRoot> modelRootList, string spaAppPath, string rootNamespace) {
            var nameSpaceMap = new Dictionary<string, List<ModelClass>>();
            foreach (var model in modelRootList) {
                foreach (var modelNameSpace in model.Namespaces.Values) {
                    var namespaceName = ToNamespace(modelNameSpace.Name);

                    if (!nameSpaceMap.ContainsKey(namespaceName)) {
                        nameSpaceMap.Add(namespaceName, new List<ModelClass>());
                    }

                    nameSpaceMap[namespaceName].AddRange(modelNameSpace.ClassList);
                }
            }

            var staticLists = new List<ModelClass>();

            foreach (var entry in nameSpaceMap) {
                foreach (var model in entry.Value) {
                    if (!model.IsStatique) {
                        var fileName = model.Name.ToDashCase();
                        Console.Out.WriteLine($"Generating Typescript file: {fileName}.ts ...");

                        fileName = $"{spaAppPath}/model/{entry.Key.ToDashCase(false)}/{fileName}.ts";
                        var fileInfo = new FileInfo(fileName);

                        var isNewFile = !fileInfo.Exists;

                        var directoryInfo = fileInfo.Directory;
                        if (!directoryInfo.Exists) {
                            Directory.CreateDirectory(directoryInfo.FullName);
                        }

                        var template = new TypescriptTemplate { RootNamespace = rootNamespace, Model = model };
                        var result = template.TransformText();
                        File.WriteAllText(fileName, result, Encoding.UTF8);
                    } else {
                        staticLists.Add(model);
                    }
                }
            }

            if (staticLists.Any()) {
                Console.Out.WriteLine($"Generating Typescript file: references.ts ...");
                var fileName = $"{spaAppPath}/model/references.ts";
                var fileInfo = new FileInfo(fileName);

                var isNewFile = !fileInfo.Exists;

                var directoryInfo = fileInfo.Directory;
                if (!directoryInfo.Exists) {
                    Directory.CreateDirectory(directoryInfo.FullName);
                }

                var template = new ReferenceTemplate { References = staticLists.OrderBy(r => r.Name) };
                var result = template.TransformText();
                File.WriteAllText(fileName, result, Encoding.UTF8);
            }
        }
    }
}
