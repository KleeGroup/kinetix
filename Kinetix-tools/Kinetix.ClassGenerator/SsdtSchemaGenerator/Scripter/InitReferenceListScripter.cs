using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.SsdtSchemaGenerator.Contract;
using Kinetix.ClassGenerator.SsdtSchemaGenerator.Dto;
using Kinetix.ComponentModel;
using Kinetix.ServiceModel;

namespace Kinetix.ClassGenerator.SsdtSchemaGenerator.Scripter {

    /// <summary>
    /// Scripter permettant d'écrire les scripts d'initialisation des valeurs de listes de référence.
    /// </summary>
    public class InitReferenceListScripter : ISqlScripter<ReferenceClass> {

        /// <summary>
        /// Calcule le nom du script pour l'item.
        /// </summary>
        /// <param name="item">Item à scripter.</param>
        /// <returns>Nom du fichier de script.</returns>
        public string GetScriptName(ReferenceClass item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            return item.Class.GetTableName() + ".insert.sql";
        }

        /// <summary>
        /// Indique si l'item doit générer un script.
        /// </summary>
        /// <param name="item">Item candidat.</param>
        /// <returns><code>True</code> si un script doit être généré.</returns>
        public bool IsScriptGenerated(ReferenceClass item) {
            return true;
        }

        /// <summary>
        /// Ecrit dans un flux le script pour l'item.
        /// </summary>
        /// <param name="writer">Flux d'écriture.</param>
        /// <param name="item">Table à scripter.</param>
        public void WriteItemScript(TextWriter writer, ReferenceClass item) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            if (item == null) {
                throw new ArgumentNullException("item");
            }

            string tableName = item.Class.GetTableName();

            // Entête du fichier.
            WriteHeader(writer, tableName);

            // Ecrit les inserts.
            WriteInsertLines(writer, item);

            WriteFooter(writer, tableName);
        }

        /// <summary>
        /// Retourne la ligne d'insert.
        /// </summary>
        /// <param name="modelClass">Modele de la classe.</param>
        /// <param name="initItem">Item a insérer.</param>
        /// <param name="siPrimaryKeyIncluded">True si le script d'insert doit comporter la clef primaire.</param>
        /// <returns>Requête.</returns>
        private static string GetInsertLine(ModelClass modelClass, ItemInit initItem, bool siPrimaryKeyIncluded) {

            // Remplissage d'un dictionnaire nom de colonne => valeur.
            BeanDefinition definition = BeanDescriptor.GetDefinition(initItem.Bean);
            Dictionary<string, string> nameValueDict = new Dictionary<string, string>();
            foreach (ModelProperty property in modelClass.PersistentPropertyList) {
                if (!property.DataDescription.IsPrimaryKey || siPrimaryKeyIncluded || property.DataDescription.Domain.Code == "DO_CD") {
                    BeanPropertyDescriptor propertyDescriptor = definition.Properties[property.Name];
                    object propertyValue = propertyDescriptor.GetValue(initItem.Bean);
                    string propertyValueStr = propertyValue == null ? "NULL" : propertyValue.ToString();
                    if (property.DataType == "byte[]") {
                        nameValueDict[property.DataMember.Name] = propertyValueStr;
                    } else if (propertyDescriptor.PrimitiveType == typeof(string)) {
                        nameValueDict[property.DataMember.Name] = propertyValue == null ? "NULL" : "N'" + ScriptUtils.PrepareDataToSqlDisplay(propertyValueStr) + "'";
                    } else {
                        nameValueDict[property.DataMember.Name] = propertyValue == null ? "NULL" : propertyValueStr;
                    }
                }
            }

            // Création de la requête.
            StringBuilder sb = new StringBuilder();
            sb.Append("\tINSERT INTO " + modelClass.DataContract.Name + "(");
            bool isFirst = true;
            foreach (string columnName in nameValueDict.Keys) {
                if (!isFirst) {
                    sb.Append(", ");
                }

                isFirst = false;
                sb.Append(columnName);
            }

            sb.Append(") VALUES(");

            isFirst = true;
            foreach (string value in nameValueDict.Values) {
                if (!isFirst) {
                    sb.Append(", ");
                }

                isFirst = false;
                sb.Append(value);
            }

            sb.Append(");");
            return sb.ToString();
        }

        /// <summary>
        /// Ecrit le pied du fichier.
        /// </summary>
        /// <param name="writer">Flux.</param>
        /// <param name="tableName">Nom de la table.</param>
        private static void WriteFooter(TextWriter writer, string tableName) {
            writer.WriteLine("\tINSERT INTO " + GeneratorParameters.LogScriptTableName + "(" + GeneratorParameters.LogScriptVersionField + ", " + GeneratorParameters.LogScriptDateField + ") VALUES (@SCRIPT_NAME, GETDATE());");
            writer.WriteLine("\tCOMMIT TRANSACTION");
            writer.WriteLine("END");
            writer.WriteLine("GO");
        }

        /// <summary>
        /// Ecrit l'entête du fichier.
        /// </summary>
        /// <param name="writer">Flux.</param>
        /// <param name="tableName">Nom de la table.</param>
        private static void WriteHeader(TextWriter writer, string tableName) {
            writer.WriteLine("-- ===========================================================================================");
            writer.WriteLine("--   Description		:	Insertion des valeurs de la table " + tableName + ".");
            writer.WriteLine("-- ===========================================================================================");
            writer.WriteLine();
            writer.WriteLine("DECLARE @SCRIPT_NAME varchar(100)");
            writer.WriteLine();
            writer.WriteLine("SET @SCRIPT_NAME = '" + tableName + ".insert'");
            writer.WriteLine("IF not exists(Select 1 From " + GeneratorParameters.LogScriptTableName + " WHERE " + GeneratorParameters.LogScriptVersionField + " = @SCRIPT_NAME)");
            writer.WriteLine("BEGIN");
            writer.WriteLine("\tPRINT 'Appling script ' + @SCRIPT_NAME;");
            writer.WriteLine("\tSET XACT_ABORT ON");
            writer.WriteLine("\tBEGIN TRANSACTION");
            writer.WriteLine();
        }

        /// <summary>
        /// Ecrit les lignes d'insertion pour la liste des valeurs.
        /// </summary>
        /// <param name="writer">Flux.</param>
        /// <param name="item">Liste de références.</param>
        private static void WriteInsertLines(TextWriter writer, ReferenceClass item) {
            foreach (ItemInit initItem in item.Values.ItemInitList) {
                writer.WriteLine(GetInsertLine(item.Class, initItem, item.IsStatic));
            }

            writer.WriteLine();
        }
    }
}
