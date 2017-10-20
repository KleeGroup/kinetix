using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Kinetix.ClassGenerator.Model;
using Kinetix.ComponentModel;
using Kinetix.ServiceModel;

namespace Kinetix.ClassGenerator.SchemaGenerator {

    /// <summary>
    /// Générateur de Transact-SQL.
    /// </summary>
    public class SqlServerSchemaGenerator : AbstractSchemaGenerator {

        /// <summary>
        /// Séparateur de lots de commandes Transact-SQL.
        /// </summary>
        protected override string BatchSeparator {
            get {
                return "go";
            }
        }

        /// <summary>
        /// Indique si le moteur de BDD visé supporte "primary key clustered ()".
        /// </summary>
        protected override bool SupportsClusteredKey {
            get {
                return true;
            }
        }

        /// <summary>
        /// Indique la limite de longueur d'un identifiant.
        /// </summary>
        protected override int IdentifierLengthLimit {
            get {
                return 128;
            }
        }

        /// <summary>
        /// Return concat operator.
        /// </summary>
        protected override string ConcatOperator {
            get {
                return " + ";
            }
        }

        /// <summary>
        /// Génère le script de définition du tablespace d'une table.
        /// </summary>
        /// <param name="writerCrebas">Flux de sortie.</param>
        /// <param name="classe">Classe concernée.</param>
        protected override void WriteTableSpaceTable(StreamWriter writerCrebas, ModelClass classe) {
            if (!string.IsNullOrEmpty(classe.Storage)) {
                writerCrebas.WriteLine("on \"" + classe.Storage + "\"");
            }
        }

        /// <summary>
        /// Génère le script de définition du tablespace d'un index.
        /// </summary>
        /// <param name="writerCrebas">Flux de sortie.</param>
        /// <param name="classe">Classe concernée.</param>
        protected override void WriteTableSpaceIndex(StreamWriter writerCrebas, ModelClass classe) {
            WriteTableSpaceTable(writerCrebas, classe);
        }

        /// <summary>
        /// Crée un dictionnaire { nom de la propriété => valeur } pour un item à insérer.
        /// </summary>
        /// <param name="modelClass">Modele de la classe.</param>
        /// <param name="initItem">Item a insérer.</param>
        /// <param name="isPrimaryKeyIncluded">True si le script d'insert doit comporter la clef primaire.</param>
        /// <returns>Dictionnaire contenant { nom de la propriété => valeur }.</returns>
        protected override Dictionary<string, string> CreatePropertyValueDictionary(ModelClass modelClass, ItemInit initItem, bool isPrimaryKeyIncluded) {
            Dictionary<string, string> nameValueDict = new Dictionary<string, string>();
            BeanDefinition definition = BeanDescriptor.GetDefinition(initItem.Bean);
            foreach (ModelProperty property in modelClass.PersistentPropertyList) {
                if (!property.DataDescription.IsPrimaryKey || isPrimaryKeyIncluded) {
                    BeanPropertyDescriptor propertyDescriptor = definition.Properties[property.Name];
                    object propertyValue = propertyDescriptor.GetValue(initItem.Bean);
                    string propertyValueStr = propertyValue == null ? string.Empty : propertyValue.ToString();
                    if (property.DataType == "byte[]") {
                        nameValueDict[property.DataMember.Name] = GetBulkColumn(propertyValueStr);
                    } else if (propertyDescriptor.PrimitiveType == typeof(string)) {
                        nameValueDict[property.DataMember.Name] = "'" + propertyValueStr.Replace("'", "''") + "'";
                    } else {
                        nameValueDict[property.DataMember.Name] = propertyValueStr;
                    }
                }
            }

            return nameValueDict;
        }

        /// <summary>
        /// Ecrit dans le writer le script de création du type.
        /// </summary>
        /// <param name="classe">Classe.</param>
        /// <param name="writerType">Writer.</param>
        protected override void WriteType(ModelClass classe, StreamWriter writerType) {
            string typeName = classe.DataContract.Name.ToUpperInvariant() + "_TABLE_TYPE";
            writerType.WriteLine("/**");
            writerType.WriteLine("  * Création du type " + classe.DataContract.Name.ToUpperInvariant() + "_TABLE_TYPE");
            writerType.WriteLine(" **/");
            writerType.WriteLine("If Exists (Select * From sys.types st Join sys.schemas ss On st.schema_id = ss.schema_id Where st.name = N'" + typeName + "')");
            writerType.WriteLine("Drop Type " + typeName + '\n');
            writerType.WriteLine("Create type " + typeName + " as Table (");
        }

        /// <summary>
        /// Gère l'auto-incrémentation des clés primaires en ajoutant identity à la colonne.
        /// </summary>
        /// <param name="writerCrebas">Flux d'écriture création bases.</param>
        protected override void WriteIdentityColumn(StreamWriter writerCrebas) {
            writerCrebas.Write(" identity(2020, 1)");
        }

        /// <summary>
        /// Ecrit les contraintes sur les booléens lors de la déclaration de la table.
        /// </summary>
        /// <param name="writerCrebas">Flux d'écriture vers le fichier de création de tables.</param>
        /// <param name="classe">Classe.</param>
        protected override void WriteBooleanConstraints(StreamWriter writerCrebas, ModelClass classe) {
            // aucune contrainte pour SqlServer, le type "bit" utilisé contraint déjà les valeurs entre 0 et 1.
        }

        /// <summary>
        /// Crée une séquence après la déclaration de la table si une des colonnes doit être auto incrémentée.
        /// </summary>
        /// <param name="writerCrebas">Flux d'écriture vers le fichier de déclaration des tables.</param>
        /// <param name="classe">Table pour laquelle la séquence est créée.</param>
        protected override void CreateSequenceIfNeeded(StreamWriter writerCrebas, ModelClass classe) {
        }

        /// <summary>
        /// Returns the select getting an image to put in the database.
        /// </summary>
        /// <param name="imagePath">Path of the image to insert.</param>
        /// <returns>Select string.</returns>
        private static string GetBulkColumn(string imagePath) {
            return "(SELECT BulkColumn FROM OPENROWSET (BULK '" + imagePath + "', SINGLE_BLOB) as x)";
        }

        /// <summary>
        /// Dans une requête SQL à exécuter localement, remplace les variables SQLCMD définies dans le script de configuration par les valeurs locales.
        /// </summary>
        /// <param name="rawCmd">Commande SQL à modifier.</param>
        /// <returns>Commande SQL modifiée.</returns>
        private static string ReplaceConfigurationVariables(string rawCmd) {
            var varValueDic = new Dictionary<string, string> {
                { "dbEchangeDirectory", @"c:\SSIS" }
            };
            StringBuilder sb = new StringBuilder(rawCmd);
            foreach (KeyValuePair<string, string> pair in varValueDic) {
                string varName = string.Format(CultureInfo.InvariantCulture, "$({0})", pair.Key);
                string varValue = pair.Value;
                sb.Replace(varName, varValue);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Retourne l'instruction d'update de l'item dans la calsse ModelClass.
        /// </summary>
        /// <param name="modelClass">Table.</param>
        /// <param name="initItem">Item.</param>
        /// <returns>Nombre de lignes mises à jour.</returns>
        private static string GetUpdateLine(ModelClass modelClass, ItemInit initItem) {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE " + modelClass.DataContract.Name + " SET ");
            BeanDefinition definition = BeanDescriptor.GetDefinition(initItem.Bean);
            int persistentPropertyCount = definition.Properties.Count;
            int idx = 0;
            BeanPropertyDescriptorCollection propertyList = definition.Properties;

            ModelProperty key = null;
            foreach (ModelProperty property in modelClass.PersistentPropertyList) {
                if (propertyList.Contains(property.Name)) {
                    ++idx;
                    if (property.DataDescription.IsPrimaryKey
                        || (key == null && property.IsUnique)) {
                        key = property;
                    } else {
                        sb.Append(property.DataMember.Name);
                        sb.Append(" = ");
                        object propertyValue = propertyList[property.Name].GetValue(initItem.Bean);
                        string propertyValueStr = propertyValue == null ? string.Empty : propertyValue.ToString();
                        if (property.DataType == "byte[]") {
                            sb.Append(GetBulkColumn(propertyValueStr));
                        } else if (propertyList[property.Name].PrimitiveType == typeof(string)) {
                            sb.Append("'" + propertyValueStr.Replace("'", "''") + "'");
                        } else {
                            sb.Append(propertyValueStr);
                        }

                        if (idx < persistentPropertyCount) {
                            sb.Append(", ");
                        }
                    }
                }
            }

            sb.Append(" WHERE ");
            sb.Append(key.DataMember.Name);
            sb.Append(" = ");
            if (propertyList[key.Name].PrimitiveType == typeof(string)) {
                sb.Append("'" + propertyList[key.Name].GetValue(initItem.Bean).ToString().Replace("'", "''") + "'");
            } else {
                sb.Append(propertyList[key.Name].GetValue(initItem.Bean).ToString());
            }

            sb.Append(";");
            return sb.ToString();
        }
    }
}
