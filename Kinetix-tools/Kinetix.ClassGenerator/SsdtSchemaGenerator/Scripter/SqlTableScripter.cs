using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.SsdtSchemaGenerator.Contract;

namespace Kinetix.ClassGenerator.SsdtSchemaGenerator.Scripter {

    /// <summary>
    /// Scripter permettant d'écrire les scripts de création d'une table SQL avec :
    /// - sa structure
    /// - sa contrainte PK
    /// - ses contraintes FK
    /// - ses indexes FK
    /// - ses contraintes d'unicité sur colonne unique.
    /// </summary>
    public class SqlTableScripter : ISqlScripter<ModelClass> {

        /// <summary>
        /// Calcule le nom du script pour l'item.
        /// </summary>
        /// <param name="item">Item à scripter.</param>
        /// <returns>Nom du fichier de script.</returns>
        public string GetScriptName(ModelClass item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            return item.GetTableName() + ".sql";
        }

        /// <summary>
        /// Indique si l'item doit générer un script.
        /// </summary>
        /// <param name="item">Item candidat.</param>
        /// <returns><code>True</code> si un script doit être généré.</returns>
        public bool IsScriptGenerated(ModelClass item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            return item.IsTableGenerated;
        }

        /// <summary>
        /// Ecrit dans un flux le script pour l'item.
        /// </summary>
        /// <param name="writer">Flux d'écriture.</param>
        /// <param name="item">Table à scripter.</param>
        public void WriteItemScript(TextWriter writer, ModelClass item) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            if (item == null) {
                throw new ArgumentNullException("item");
            }

            // TODO : rendre paramétrable.
            bool useCompression = false;

            // Entête du fichier.
            WriteHeader(writer, item.GetTableName());

            // Ouverture du create table.
            WriteCreateTableOpening(writer, item);

            // Intérieur du create table.
            WriteInsideInstructions(writer, item);

            // Fin du create table.
            WriteCreateTableClosing(writer, item, useCompression);

            // Indexes sur les clés étrangères.
            GenerateIndexForeignKey(writer, item);

            // Définition
            WriteTableDescriptionProperty(writer, item);
        }

        /// <summary>
        /// Extrait les propriétés de type clés étrangères.
        /// </summary>
        /// <param name="classe">La table à ecrire.</param>
        /// <returns>Liste des propriétés étrangères persistentes.</returns>
        private static ICollection<ModelProperty> ExtractFkProperties(ModelClass classe) {
            ICollection<ModelProperty> fkPropertiesList = new List<ModelProperty>();
            foreach (ModelProperty property in classe.PersistentPropertyList) {
                if (!string.IsNullOrEmpty(property.DataDescription.ReferenceType)) {
                    fkPropertiesList.Add(property);
                }
            }

            return fkPropertiesList;
        }

        /// <summary>
        /// Génère les indexes portant sur les FK.
        /// </summary>
        /// <param name="writer">Flux d'écriture.</param>
        /// <param name="table">Table.</param>
        private static void GenerateIndexForeignKey(TextWriter writer, ModelClass table) {
            string tableName = table.GetTableName();
            var fkList = ExtractFkProperties(table);
            foreach (ModelProperty property in fkList.Where(prop => !prop.DataDescription.ReferenceClass.IsView)) {
                string propertyName = property.GetColumnName();
                string indexName = "IDX_" + tableName + "_" + propertyName + "_FK";

                // Cas où l'index ne doit pas être généré.
                if (table.IndexNotGeneratedList.Contains(indexName)) {
                    continue;
                }

                ModelClass referenceClass = property.DataDescription.ReferenceClass;
                writer.WriteLine("/* Index on foreign key column for " + tableName + "." + propertyName + " */");
                writer.WriteLine("create nonclustered index [" + indexName + "]");
                writer.Write("\ton [dbo].[" + tableName + "] (");
                string propertyConcat = "[" + propertyName + "] ASC";
                if (referenceClass.HasPartitionKey && property.Class.HasPartitionKey) {
                    propertyConcat += ", [" + property.Class.PartitionKey.GetColumnName() + "] ASC";
                }

                writer.Write(propertyConcat);
                writer.Write(")");
                if (!string.IsNullOrEmpty(property.Class.Storage)) {
                    writer.Write("on " + property.Class.Storage);
                }

                writer.WriteLine();
                writer.WriteLine("go");
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Ecrit le SQL pour une colonne.
        /// </summary>
        /// <param name="sb">Flux.</param>
        /// <param name="property">Propriété.</param>
        private static void WriteColumn(StringBuilder sb, ModelProperty property) {
            string persistentType = property.DeterminerSqlDataType();
            sb.Append("[").Append(property.GetColumnName()).Append("] ").Append(persistentType);
            if (property.DataMember.IsRequired && !property.DataDescription.IsPrimaryKey) {
                sb.Append(" not null");
            }

            if (!string.IsNullOrEmpty(property.DefaultValue)) {
                sb.Append(" default ").Append(property.DefaultValue);
            }
        }

        /// <summary>
        /// Génère la contrainte de clef étrangère.
        /// </summary>
        /// <param name="sb">Flux d'écriture.</param>
        /// <param name="property">Propriété portant la clef étrangère.</param>
        private static void WriteConstraintForeignKey(StringBuilder sb, ModelProperty property) {
            string tableName = property.Class.GetTableName();

            string propertyName = property.GetColumnName();
            ModelClass referenceClass = property.DataDescription.ReferenceClass;

            string constraintName = "FK_" + tableName + "_" + referenceClass.GetTableName() + "_" + propertyName;
            string propertyConcat = "[" + propertyName + "]";
            if (referenceClass.HasPartitionKey && property.Class.HasPartitionKey) {
                propertyConcat += ", [" + property.Class.PartitionKey.GetColumnName() + "]";
            }

            sb.Append("constraint [").Append(constraintName).Append("] foreign key (").Append(propertyConcat).Append(") ");
            sb.Append("references [dbo].[").Append(referenceClass.GetTableName()).Append("] (");

            int currentProperty = 0;
            int propertyCount = referenceClass.PrimaryKey.Count;
            foreach (ModelProperty targetPkProperty in referenceClass.PrimaryKey) {
                ++currentProperty;
                sb.Append("[").Append(targetPkProperty.GetColumnName()).Append("]");
                if (currentProperty < propertyCount) {
                    sb.Append(", ");
                }
            }

            sb.Append(")");
        }

        /// <summary>
        /// Ecrit le pied du script.
        /// </summary>
        /// <param name="writer">Flux.</param>
        /// <param name="classe">Classe de la table.</param>
        /// <param name="useCompression">Indique si on utilise la compression.</param>
        private static void WriteCreateTableClosing(TextWriter writer, ModelClass classe, bool useCompression) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            if (classe == null) {
                throw new ArgumentNullException("classe");
            }

            writer.WriteLine(")");
            if (!string.IsNullOrEmpty(classe.Storage)) {
                writer.WriteLine("on " + classe.Storage);
            }

            if (useCompression) {
                writer.WriteLine("WITH (DATA_COMPRESSION=PAGE)");
            }

            writer.WriteLine("go");
            writer.WriteLine();
        }

        /// <summary>
        /// Ecrit l'ouverture du create table.
        /// </summary>
        /// <param name="writer">Flux.</param>
        /// <param name="table">Table.</param>
        private static void WriteCreateTableOpening(TextWriter writer, ModelClass table) {
            writer.WriteLine("create table [dbo].[" + table.GetTableName() + "] (");
        }

        /// <summary>
        /// Ecrit l'entête du fichier.
        /// </summary>
        /// <param name="writer">Flux.</param>
        /// <param name="tableName">Nom de la table.</param>
        private static void WriteHeader(TextWriter writer, string tableName) {
            writer.WriteLine("-- ===========================================================================================");
            writer.WriteLine("--   Description		:	Création de la table " + tableName + ".");
            writer.WriteLine("-- ===========================================================================================");
            writer.WriteLine();
        }

        /// <summary>
        /// Ecrit les instructions à l'intérieur du create table.
        /// </summary>
        /// <param name="writer">Flux.</param>
        /// <param name="table">Table.</param>
        private static void WriteInsideInstructions(TextWriter writer, ModelClass table) {

            // Construction d'une liste de toutes les instructions.
            List<string> definitions = new List<string>();
            StringBuilder sb = new StringBuilder();

            // Colonnes
            foreach (ModelProperty property in table.OrderedPersistentPropertyList) {
                sb.Clear();
                WriteColumn(sb, property);
                definitions.Add(sb.ToString());
            }

            // Primary Key
            sb.Clear();
            WritePkLine(sb, table);
            definitions.Add(sb.ToString());

            // Foreign key constraints
            var fkList = ExtractFkProperties(table);
            foreach (ModelProperty property in fkList.Where(prop => !prop.DataDescription.ReferenceClass.IsView)) {
                sb.Clear();
                WriteConstraintForeignKey(sb, property);
                definitions.Add(sb.ToString());
            }

            // Unique constraints
            definitions.AddRange(WriteUniqueConstraint(table));

            // Ecriture de la liste concaténée.
            var separator = "," + Environment.NewLine;
            writer.Write(string.Join(separator, definitions.Select(x => "\t" + x)));
        }

        /// <summary>
        /// Ecrit la ligne de création de la PK.
        /// </summary>
        /// <param name="sb">Flux.</param>
        /// <param name="classe">Classe.</param>
        private static void WritePkLine(StringBuilder sb, ModelClass classe) {
            int pkCount = 0;
            sb.Append("constraint [PK_").Append(classe.GetTableName()).Append("] primary key clustered (");
            foreach (ModelProperty pkProperty in classe.PrimaryKey) {
                ++pkCount;
                sb.Append("[").Append(pkProperty.GetColumnName()).Append("] ASC");
                if (pkCount < classe.PrimaryKey.Count) {
                    sb.Append(", ");
                } else {
                    sb.Append(")");
                }
            }
        }

        /// <summary>
        /// Ecrit la création de la propriété de description de la table.
        /// </summary>
        /// <param name="writer">Writer.</param>
        /// <param name="classe">Classe de la table.</param>
        private static void WriteTableDescriptionProperty(TextWriter writer, ModelClass classe) {
            writer.WriteLine("/* Description property. */");
            writer.WriteLine("EXECUTE sp_addextendedproperty 'Description', '" + ScriptUtils.PrepareDataToSqlDisplay(classe.Label) + "', 'SCHEMA', 'dbo', 'TABLE', '" + classe.GetTableName() + "';");
        }

        /// <summary>
        /// Calcule la liste des déclarations de contraintes d'unicité.
        /// </summary>
        /// <param name="classe">Classe de la table.</param>
        /// <returns>Liste des déclarations de contraintes d'unicité.</returns>
        private static IList<string> WriteUniqueConstraint(ModelClass classe) {

            var constraintList = new List<string>();

            // Contrainte d'unicité sur une seule colonne.
            foreach (var columnProperty in classe.PersistentPropertyList) {
                if (columnProperty.IsUnique && !columnProperty.IsPrimaryKey) {
                    constraintList.Add("constraint [UK_" + classe.GetTableName() + '_' + columnProperty.Name.ToUpperInvariant() + "] unique nonclustered ([" + columnProperty.GetColumnName() + "] ASC)");
                }
            }

            // Contrainte d'unicité sur plusieurs colonnes.
            var columnList = classe.PersistentPropertyList.Where(x => x.IsUniqueMany);
            if (columnList.Any()) {
                constraintList.Add("constraint [UK_" + classe.GetTableName() + "_MULTIPLE] unique (" + string.Join(", ", columnList.Select(x => "[" + x.GetColumnName() + "]")) + ")");
            }

            return constraintList;
        }
    }
}
