/*
 * Kinetix
 *
 * $Id: SqlServerSchemaGenerator.cs,v 1.33 2010/06/15 08:10:55 sezratty Exp $
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.MsBuild;
using Kinetix.ClassGenerator.SsdtSchemaGenerator.Contract;
using Kinetix.ClassGenerator.SsdtSchemaGenerator.Dto;
using Kinetix.ClassGenerator.SsdtSchemaGenerator.Scripter;
using Kinetix.ServiceModel;

namespace Kinetix.ClassGenerator.SsdtSchemaGenerator {

    /// <summary>
    /// Générateur de Transact-SQL (insertions de données) visant une structure de fichiers SSDT.
    /// </summary>
    public class SqlServerSsdtInsertGenerator : ISqlServerSsdtInsertGenerator {

        private readonly ISqlScriptEngine _engine = new SqlScriptEngine();

        /// <summary>
        /// Génère le script SQL d'initialisation des listes reference.
        /// </summary>
        /// <param name="initDictionary">Dictionnaire des initialisations.</param>
        /// <param name="insertScriptFolderPath">Chemin du dossier contenant les scripts.</param>        
        /// <param name="insertMainScriptName">Nom du script principal.</param>
        /// <param name="outputDeltaFileName">Nom du fichier de delta généré.</param>
        /// <param name="isStatic">True if generation for static list.</param>
        public void GenerateListInitScript(IDictionary<ModelClass, TableInit> initDictionary, string insertScriptFolderPath, string insertMainScriptName, string outputDeltaFileName, bool isStatic) {
            if (string.IsNullOrEmpty(insertScriptFolderPath)) {
                throw new ArgumentNullException("insertScriptFolderPath");
            }

            if (string.IsNullOrEmpty(outputDeltaFileName)) {
                throw new ArgumentNullException("outputDeltaFileName");
            }

            if (initDictionary == null) {
                throw new ArgumentNullException("initDictionary");
            }

            Console.Out.WriteLine("Generating init script " + insertScriptFolderPath);
            Directory.CreateDirectory(insertScriptFolderPath);

            // Construit la liste des Reference Class ordonnée.
            ModelClass[] orderList = OrderStaticTableList(initDictionary).Where(x => !x.IsView).ToArray();
            var referenceClassList =
                orderList.Select(x => new ReferenceClass {
                    Class = x,
                    Values = initDictionary[x],
                    IsStatic = isStatic
                }).ToList();
            ReferenceClassSet referenceClassSet = new ReferenceClassSet {
                ClassList = orderList.ToList(),
                ScriptName = insertMainScriptName
            };

            // Script un fichier par classe.            
            _engine.Write(new InitReferenceListScripter(), referenceClassList, insertScriptFolderPath, BuildActions.None);

            // Script le fichier appelant les fichiers dans le bon ordre.
            _engine.Write(new InitReferenceListMainScripter(), referenceClassSet, insertScriptFolderPath, BuildActions.None);

            // TODO : delta ?
        }

        /// <summary>
        /// Retourne un tableau ordonné des ModelClass pour gérer les FK entre les listes statiques.
        /// </summary>
        /// <param name="dictionnary">Dictionnaire des couples (ModelClass, StaticTableInit) correspondant aux tables de listes statiques. </param>
        /// <returns>ModelClass[] ordonné.</returns>
        private static ModelClass[] OrderStaticTableList(IDictionary<ModelClass, TableInit> dictionnary) {
            int nbTable = dictionnary.Count;
            ModelClass[] orderedList = new ModelClass[nbTable];
            dictionnary.Keys.CopyTo(orderedList, 0);

            int i = 0;
            while (i < nbTable) {
                bool canIterate = true;
                ModelClass currentModelClass = orderedList[i];

                // On récupère les ModelClass des tables pointées par la table
                ISet<ModelClass> pointedTableSet = new HashSet<ModelClass>();
                foreach (ModelProperty property in currentModelClass.PropertyList) {
                    if (property.IsFromAssociation) {
                        ModelClass pointedTable = property.DataDescription.ReferenceClass;
                        pointedTableSet.Add(pointedTable);
                    }
                }

                for (int j = i; j < nbTable; j++) {
                    if (pointedTableSet.Contains(orderedList[j])) {
                        ModelClass sauvegarde = orderedList[i];
                        orderedList[i] = orderedList[j];
                        orderedList[j] = sauvegarde;
                        canIterate = false;
                        break;
                    }
                }

                if (canIterate) {
                    i++;
                }
            }

            return orderedList;
        }
    }
}
