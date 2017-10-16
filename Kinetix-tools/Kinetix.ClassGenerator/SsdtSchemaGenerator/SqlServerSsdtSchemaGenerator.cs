using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.MsBuild;
using Kinetix.ClassGenerator.SsdtSchemaGenerator.Contract;
using Kinetix.ClassGenerator.SsdtSchemaGenerator.Scripter;

namespace Kinetix.ClassGenerator.SsdtSchemaGenerator {

    /// <summary>
    /// Générateur de Transact-SQL (structure) visant une structure de fichiers SSDT.
    /// </summary>
    public class SqlServerSsdtSchemaGenerator : ISqlServerSsdtSchemaGenerator {

        private readonly ISqlScriptEngine _engine = new SqlScriptEngine();

        /// <summary>
        /// Génère le script SQL.
        /// </summary>
        /// <param name="modelRootList">Liste des tous les modeles OOM analysés.</param>
        /// <param name="tableScriptFolder">Dossier contenant les fichiers de script des tables.</param>
        /// <param name="tableTypeScriptFolder">Dossier contenant les fichiers de script des types de table.</param>
        public void GenerateSchemaScript(ICollection<ModelRoot> modelRootList, string tableScriptFolder, string tableTypeScriptFolder) {
            if (modelRootList == null) {
                throw new ArgumentNullException("modelRootList");
            }

            Console.Out.WriteLine("Generating schema script");

            // Charge le modèle issu de l'OOM.
            List<ModelClass> tableList = new List<ModelClass>();
            InitCollection(modelRootList, tableList);

            // Script de table.
            _engine.Write(new SqlTableScripter(), tableList, tableScriptFolder, BuildActions.Build);

            // Script de type table.
            _engine.Write(new SqlTableTypeScripter(), tableList, tableTypeScriptFolder, BuildActions.Build);
        }

        /// <summary>
        /// Initialise les collections.
        /// </summary>
        /// <param name="modelRootList">Liste des modeles.</param>
        /// <param name="tableList">Listes de tables.</param>
        private static void InitCollection(ICollection<ModelRoot> modelRootList, List<ModelClass> tableList) {
            // Sélection des classes avec persistance.
            tableList.AddRange(
                modelRootList
                .SelectMany(x => x.Namespaces.Values)
                .SelectMany(x => x.ClassList)
                .Where(x => x.DataContract.IsPersistent && !x.IsView));
        }
    }
}
