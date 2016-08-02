using System.Collections.Generic;
using Kinetix.ClassGenerator.Model;

namespace Kinetix.ClassGenerator.SsdtSchemaGenerator.Contract {

    /// <summary>
    /// Contrat du générateur de Transact-SQL (structure) visant une structure de fichiers SSDT.
    /// </summary>
    public interface ISqlServerSsdtSchemaGenerator {

        /// <summary>
        /// Génère le script SQL.
        /// </summary>
        /// <param name="modelRootList">Liste des tous les modeles OOM analysés.</param>
        /// <param name="tableScriptFolder">Dossier contenant les fichiers de script des tables.</param>
        /// <param name="tableTypeScriptFolder">Dossier contenant les fichiers de script des types de table.</param>
        void GenerateSchemaScript(ICollection<ModelRoot> modelRootList, string tableScriptFolder, string tableTypeScriptFolder);
    }
}
