using System;
using System.Collections.Generic;
using System.IO;
using Kinetix.ClassGenerator.Model;
using Kinetix.ComponentModel;
using Kinetix.ServiceModel;

namespace Kinetix.ClassGenerator.SchemaGenerator {

    /// <summary>
    /// Générateur de PLSQL.
    /// </summary>
    public class OracleSchemaGenerator : AbstractSchemaGenerator {
        /// <summary>
        /// Suffixe utilisé pour les séquences.
        /// </summary>
        private const string SequenceSuffix = "_SEQ";

        /// <summary>
        /// Préfixe utilisé pour certaines contraintes (ex: vérification qu'un booléen appartient bien à {0,1}).
        /// </summary>
        private readonly string _checkPrefix = "CHK_";

        /// <summary>
        /// Séparateur de lots de commandes PL/SQL.
        /// </summary>
        protected override string BatchSeparator {
            get {
                return "/";
            }
        }

        /// <summary>
        /// Indique si le moteur de BDD visé supporte "primary key clustered ()".
        /// </summary>
        protected override bool SupportsClusteredKey {
            get {
                return false;
            }
        }

        /// <summary>
        /// Indique la limite de longueur d'un identifiant.
        /// </summary>
        protected override int IdentifierLengthLimit {
            get {
                return 30;
            }
        }

        /// <summary>
        /// Return concat operator.
        /// </summary>
        protected override string ConcatOperator {
            get {
                return " || ";
            }
        }

        /// <summary>
        /// Ecrit dans le writer le script de création du type.
        /// </summary>
        /// <param name="classe">Classe.</param>
        /// <param name="writerType">Writer.</param>
        protected override void WriteType(ModelClass classe, StreamWriter writerType) {
        }

        /// <summary>
        /// Gère l'auto-incrémentation des clés primaires.
        /// </summary>
        /// <param name="writerCrebas">Flux d'écriture création bases.</param>
        protected override void WriteIdentityColumn(StreamWriter writerCrebas) {
        }

        /// <summary>
        /// Ecrit les contraintes sur les booléens lors de la déclaration de la table.
        /// </summary>
        /// <param name="writerCrebas">Flux d'écriture vers le fichier de création de tables.</param>
        /// <param name="classe">Classe.</param>
        protected override void WriteBooleanConstraints(StreamWriter writerCrebas, ModelClass classe) {
            foreach (ModelProperty property in classe.PersistentPropertyList) {
                if (property.DataDescription.Domain.PersistentDataType == "BL") {
                    writerCrebas.WriteLine("\tconstraint " + CheckIdentifierLength(_checkPrefix + property.DataMember.Name) + " check (" + property.DataMember.Name + " in (0,1)),");
                }
            }
        }

        /// <summary>
        /// Crée une séquence après la déclaration de la table si une des colonnes doit être auto incrémentée.
        /// </summary>
        /// <param name="writerCrebas">Flux d'écriture vers le fichier de déclaration des tables.</param>
        /// <param name="classe">Table pour laquelle la séquence est créée.</param>
        protected override void CreateSequenceIfNeeded(StreamWriter writerCrebas, ModelClass classe) {
            foreach (ModelProperty property in classe.PersistentPropertyList) {
                if (property.IsPrimaryKey && property.DataDescription.Domain.Code == "DO_ID") {
                    writerCrebas.WriteLine("/**");
                    writerCrebas.WriteLine("  * Creation d'une séquence pour la clé " + property.DataMember.Name);
                    writerCrebas.WriteLine(" **/");
                    writerCrebas.WriteLine("create sequence " + GetSequenceName(classe) + " start with 2020 increment by 1 nocycle");
                    writerCrebas.WriteLine(BatchSeparator);
                    return;
                }
            }
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
                    if (property.DataType.Equals("bool")) {
                        // gestion d'un booléen sous Oracle: number appartenant à {0,1}
                        nameValueDict[property.DataMember.Name] = propertyValueStr.Equals("true") ? "1" : "0";
                    } else if (propertyDescriptor.PrimitiveType == typeof(string)) {
                        nameValueDict[property.DataMember.Name] = "'" + propertyValueStr.Replace("'", "''") + "'";
                    } else if (propertyDescriptor.PrimitiveType == typeof(DateTime)) {
                        nameValueDict[property.DataMember.Name] = "to_date('" + string.Format("{0:MM/dd/yy}", (DateTime)propertyValue) + "', 'DD/MM/YY')";
                    } else {
                        nameValueDict[property.DataMember.Name] = propertyValueStr;
                    }
                } else {
                    // cas de la clé primaire non spécifiée sous Oracle: on utilise une séquence pour auto incrémenter la valeur de la clé
                    nameValueDict[property.DataMember.Name] = GetSequenceName(modelClass) + ".nextval";
                }
            }

            return nameValueDict;
        }

        /// <summary>
        /// Génère le script de définition du tablespace d'une table.
        /// </summary>
        /// <param name="writerCrebas">Flux de sortie.</param>
        /// <param name="classe">Classe concernée.</param>
        protected override void WriteTableSpaceTable(StreamWriter writerCrebas, ModelClass classe) {
            WriteTableSpace(writerCrebas, classe, "&TABLESPACE_TABLE");
        }

        /// <summary>
        /// Génère le script de définition du tablespace d'un index.
        /// </summary>
        /// <param name="writerCrebas">Flux de sortie.</param>
        /// <param name="classe">Classe concernée.</param>
        protected override void WriteTableSpaceIndex(StreamWriter writerCrebas, ModelClass classe) {
            WriteTableSpace(writerCrebas, classe, "&TABLESPACE_INDEX");
        }

        private static void WriteTableSpace(StreamWriter writerCrebas, ModelClass classe, string defaultTableSpaceName) {
            writerCrebas.Write("TABLESPACE ");
            if (!string.IsNullOrEmpty(classe.Storage)) {
                writerCrebas.WriteLine(classe.Storage);
            } else {
                writerCrebas.WriteLine(' ' + defaultTableSpaceName + ' ');
            }
        }

        private static string GetSequenceName(ModelClass classe) {
            return classe.Trigram.ToUpperInvariant() + SequenceSuffix;
        }
    }
}
