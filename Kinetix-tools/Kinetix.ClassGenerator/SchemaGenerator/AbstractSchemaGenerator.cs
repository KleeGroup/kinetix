using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kinetix.ClassGenerator.CodeGenerator;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.NVortex;
using Kinetix.ComponentModel;
using Kinetix.ServiceModel;

namespace Kinetix.ClassGenerator.SchemaGenerator {

    /// <summary>
    /// Classe abstraite de génération des sripts de création.
    /// </summary>
    public abstract class AbstractSchemaGenerator {

        /// <summary>
        /// Nom pour l'insert en bulk.
        /// </summary>
        protected const string InsertKeyName = "InsertKey";

        /// <summary>
        /// Séparateur de lots de commandes SQL.
        /// </summary>
        protected abstract string BatchSeparator {
            get;
        }

        /// <summary>
        /// Indique si le moteur de BDD visé supporte "primary key clustered ()".
        /// </summary>
        protected abstract bool SupportsClusteredKey {
            get;
        }

        /// <summary>
        /// Indique la limite de longueur d'un identifiant.
        /// </summary>
        protected abstract int IdentifierLengthLimit {
            get;
        }

        /// <summary>
        /// Return concat operator.
        /// </summary>
        protected abstract string ConcatOperator {
            get;
        }

        /// <summary>
        /// Génère le script SQL.
        /// </summary>
        /// <param name="modelRootList">Liste des tous les modeles OOM analysés.</param>
        /// <param name="outputFileNameCrebas">Nom du fichier de creation des tables en sortie.</param>
        /// <param name="outputFileNameUK">Nom du fichier de creation des clefs uniques.</param>
        /// <param name="outputFileNameIndex">Nom du fichier de création des index et fk en sortie.</param>/// <param name="outputFileNameType">Nom du ficheir de création des types.</param>
        public void GenerateSchemaScript(ICollection<ModelRoot> modelRootList, string outputFileNameCrebas, string outputFileNameUK, string outputFileNameIndex, string outputFileNameType) {
            if (modelRootList == null) {
                throw new ArgumentNullException("modelRootList");
            }

            Console.Out.WriteLine("Generating schema script");
            DeleteFileIfExists(outputFileNameCrebas);
            DeleteFileIfExists(outputFileNameIndex);
            DeleteFileIfExists(outputFileNameType);

            List<List<ModelProperty>> fkList = new List<List<ModelProperty>>();

            using (StreamWriter writerCrebas = File.CreateText(outputFileNameCrebas),
                    writerUk = File.CreateText(outputFileNameUK),
                    writerType = File.CreateText(outputFileNameType)) {
                writerCrebas.WriteLine("-- =========================================================================================== ");
                writerCrebas.WriteLine("--   Application Name	:	Kinetix ");
                writerCrebas.WriteLine("--   Script Name		:	" + outputFileNameCrebas);
                writerCrebas.WriteLine("--   Description		:	Script de création des tables.");
                writerCrebas.WriteLine("-- =========================================================================================== ");

                writerUk.WriteLine("-- =========================================================================================== ");
                writerUk.WriteLine("--   Application Name	:	Kinetix");
                writerUk.WriteLine("--   Script Name		:	" + outputFileNameUK);
                writerUk.WriteLine("--   Description		:	Script de création des indexs uniques.");
                writerUk.WriteLine("-- =========================================================================================== ");

                writerType.WriteLine("-- =========================================================================================== ");
                writerType.WriteLine("--   Application Name	:	Kinetix ");
                writerType.WriteLine("--   Script Name		:	" + outputFileNameType);
                writerType.WriteLine("--   Description		:	Script de création des types. ");
                writerType.WriteLine("-- =========================================================================================== ");

                foreach (ModelRoot modelRoot in modelRootList) {
                    List<ModelProperty> fkProperties = new List<ModelProperty>();
                    foreach (string nsKey in modelRoot.Namespaces.Keys) {
                        foreach (ModelClass classe in modelRoot.Namespaces[nsKey].ClassList) {
                            if (classe.DataContract.IsPersistent) {
                                fkProperties.AddRange(WriteTableDeclaration(classe, writerCrebas, writerUk, writerType));
                            }
                        }
                    }

                    fkList.Add(fkProperties);
                }
            }

            using (StreamWriter writer = File.CreateText(outputFileNameIndex)) {
                writer.WriteLine("-- =========================================================================================== ");
                writer.WriteLine("--   Application Name	:	Kinetix ");
                writer.WriteLine("--   Script Name		:	" + outputFileNameIndex);
                writer.WriteLine("--   Description		:	Script de création des indexes et des clef étrangères. ");
                writer.WriteLine("-- =========================================================================================== ");
                foreach (List<ModelProperty> fkProperties in fkList) {
                    foreach (ModelProperty fkProperty in fkProperties) {
                        GenerateIndexForeignKey(writer, fkProperty);
                        GenerateConstraintForeignKey(fkProperty, writer);
                    }
                }
            }
        }

        /// <summary>
        /// Génère le script SQL d'initialisation des listes reference.
        /// </summary>
        /// <param name="initDictionary">Dictionnaire des initialisations.</param>
        /// <param name="outputFileName">Nom du fichier généré.</param>
        /// <param name="outputDeltaFileName">Nom du fichier de delta généré.</param>
        /// <param name="isStatic">True if generation for static list.</param>
        public void GenerateListInitScript(Dictionary<ModelClass, TableInit> initDictionary, string outputFileName, string outputDeltaFileName, bool isStatic) {
            if (string.IsNullOrEmpty(outputFileName)) {
                throw new ArgumentNullException("outputFileName");
            }

            if (string.IsNullOrEmpty(outputFileName)) {
                throw new ArgumentNullException("outputDeltaFileName");
            }

            Console.Out.WriteLine("Generating init script " + outputFileName);
            DeleteFileIfExists(outputFileName);

            using (StreamWriter writerInsert = File.CreateText(outputFileName)) {
                writerInsert.WriteLine("-- =========================================================================================== ");
                writerInsert.WriteLine("--   Application Name	:	Kinetix ");
                writerInsert.WriteLine("--   Script Name		:	" + outputFileName);
                writerInsert.WriteLine("--   Description		:	Script d'insertion des données de références" + (!isStatic ? " non " : " ") + "statiques. ");
                writerInsert.WriteLine("-- ===========================================================================================");

                if (initDictionary != null) {
                    IEnumerable<ModelClass> orderList = OrderStaticTableList(initDictionary);
                    foreach (ModelClass modelClass in orderList) {
                        WriteInsert(writerInsert, initDictionary[modelClass], modelClass, isStatic);
                    }
                }
            }
        }

        /// <summary>
        /// Vérifie la liste des identifiants du modèle afin qu'ils ne dépassent pas la longueur maximale acceptée dans la base de données.
        /// </summary>
        /// <param name="modelRootList">Modèle.</param>
        /// <returns>Liste des messages d'erreurs créés.</returns>
        public ICollection<NVortexMessage> CheckAllIdentifiersNames(ICollection<ModelRoot> modelRootList) {
            if (modelRootList == null) {
                throw new ArgumentNullException("modelRootList");
            }

            List<NVortexMessage> messageList = new List<NVortexMessage>();
            foreach (ModelRoot modelRoot in modelRootList) {
                foreach (string namespaceKey in modelRoot.Namespaces.Keys) {
                    foreach (ModelClass modelClass in modelRoot.Namespaces[namespaceKey].ClassList) {
                        CheckIdentifierLength(messageList, modelClass.DataContract.Name);
                        foreach (ModelProperty property in modelClass.PersistentPropertyList) {
                            CheckIdentifierLength(messageList, property.DataMember.Name);
                        }
                    }
                }
            }

            return messageList;
        }

        /// <summary>
        /// Génère le script SQL d'initialisation des listes reference.
        /// </summary>
        /// <param name="tableList">Liste des classes de référence.</param>
        /// <param name="outputFileName">Nom du fichier généré.</param>
        /// <param name="isStatic">True if generation for static list.</param>
        public void GenerateReferenceTranslationList(ICollection<ModelClass> tableList, string outputFileName, bool isStatic) {
            if (string.IsNullOrEmpty(outputFileName)) {
                throw new ArgumentNullException("outputFileName");
            }

            if (tableList == null) {
                throw new ArgumentNullException("tableList");
            }

            Console.Out.WriteLine("Generating init translation script " + outputFileName);
            DeleteFileIfExists(outputFileName);
            using (StreamWriter writerInsert = File.CreateText(outputFileName)) {
                writerInsert.WriteLine("-- ===========================================================================================");
                writerInsert.WriteLine("--   Application Name	:	Kinetix");
                writerInsert.WriteLine("--   Script Name		:	" + outputFileName);
                writerInsert.WriteLine("--   Description		:	Script d'insertion des traductions des données de références" + (!isStatic ? " non " : " ") + "statiques.");
                writerInsert.WriteLine("-- ===========================================================================================");

                var sortedList = tableList.OrderBy(x => x.Name);
                foreach (ModelClass modelClass in sortedList) {
                    if (modelClass.IsTranslated) {
                        /* if (isStatic) {
                            WriteInsertLibelleRessource(writerInsert, modelClass);
                        } else { */
                        WriteInsertTraductionReference(writerInsert, modelClass);
                    }
                }
            }
        }

        /// <summary>
        /// Ecrit dans le writer le script de création du type.
        /// </summary>
        /// <param name="classe">Classe.</param>
        /// <param name="writerType">Writer.</param>
        protected abstract void WriteType(ModelClass classe, StreamWriter writerType);

        /// <summary>
        /// Gère l'auto-incrémentation des clés primaires.
        /// </summary>
        /// <param name="writerCrebas">Flux d'écriture création bases.</param>
        protected abstract void WriteIdentityColumn(StreamWriter writerCrebas);

        /// <summary>
        /// Ecrit les contraintes sur les booléens lors de la déclaration de la table.
        /// </summary>
        /// <param name="writerCrebas">Flux d'écriture vers le fichier de création de tables.</param>
        /// <param name="classe">Classe.</param>
        protected abstract void WriteBooleanConstraints(StreamWriter writerCrebas, ModelClass classe);

        /// <summary>
        /// Crée une séquence après la déclaration de la table si une des colonnes doit être auto incrémentée.
        /// </summary>
        /// <param name="writerCrebas">Flux d'écriture vers le fichier de déclaration des tables.</param>
        /// <param name="classe">Table pour laquelle la séquence est créée.</param>
        protected abstract void CreateSequenceIfNeeded(StreamWriter writerCrebas, ModelClass classe);

        /// <summary>
        /// Crée un dictionnaire { nom de la propriété => valeur } pour un item à insérer.
        /// </summary>
        /// <param name="modelClass">Modele de la classe.</param>
        /// <param name="initItem">Item a insérer.</param>
        /// <param name="isPrimaryKeyIncluded">True si le script d'insert doit comporter la clef primaire.</param>
        /// <returns>Dictionnaire contenant { nom de la propriété => valeur }.</returns>
        protected abstract Dictionary<string, string> CreatePropertyValueDictionary(ModelClass modelClass, ItemInit initItem, bool isPrimaryKeyIncluded);

        /// <summary>
        /// Génère le script de définition du tablespace d'une table.
        /// </summary>
        /// <param name="writerCrebas">Flux de sortie.</param>
        /// <param name="classe">Classe concernée.</param>
        protected abstract void WriteTableSpaceTable(StreamWriter writerCrebas, ModelClass classe);

        /// <summary>
        /// Génère le script de définition du tablespace d'un index.
        /// </summary>
        /// <param name="writerCrebas">Flux de sortie.</param>
        /// <param name="classe">Classe concernée.</param>
        protected abstract void WriteTableSpaceIndex(StreamWriter writerCrebas, ModelClass classe);

        /// <summary>
        /// Lève une ArgumentException si l'identifiant est trop long.
        /// </summary>
        /// <param name="identifier">Identifiant à vérifier.</param>
        /// <returns>Identifiant passé en paramètre.</returns>
        protected string CheckIdentifierLength(string identifier) {
            if (identifier.Length > IdentifierLengthLimit) {
                throw new ArgumentException(
                    "Le nom " + identifier + " est trop long ("
                    + identifier.Length + " caractères). Limite: "
                    + IdentifierLengthLimit + " caractères.");
            }

            return identifier;
        }

        /// <summary>
        /// Supprime le fichier s'il existe déja.
        /// </summary>
        /// <param name="outputFileName">Nom du fichier.</param>
        private static void DeleteFileIfExists(string outputFileName) {
            if (File.Exists(outputFileName)) {
                try {
                    File.Delete(outputFileName);
                } catch (IOException e) {
                    Console.Error.WriteLine("Le fichier " + outputFileName + " existe déja, erreur lors de la suppression : " + e.Message);
                    Environment.Exit(-1);
                }
            }
        }

        /// <summary>
        /// Retourne un tableau ordonné des ModelClass pour gérer les FK entre les listes statiques.
        /// </summary>
        /// <param name="dictionnary">Dictionnaire des couples (ModelClass, StaticTableInit) correspondant aux tables de listes statiques. </param>
        /// <returns>ModelClass[] ordonné.</returns>
        private static IEnumerable<ModelClass> OrderStaticTableList(Dictionary<ModelClass, TableInit> dictionnary) {
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

        /// <summary>
        /// Generate insert label.
        /// </summary>
        /// <param name="writerInsert">Writer.</param>
        /// <param name="modelClass">Reference Class.</param>
        private void WriteInsertTraductionReference(StreamWriter writerInsert, ModelClass modelClass) {
            var translatedPropList = modelClass.PropertyList.Where(x => x.IsTranslated);
            string propConcat = string.Join("\",\"", translatedPropList.Select(x => x.Name));
            writerInsert.WriteLine("-- Table " + modelClass.DataContract.Name);
            writerInsert.WriteLine(
@"Insert into TRADUCTION_REFERENCE (
    TDR_ID,
    TDR_TABLE,
    TDR_CODE,
    TDR_VALEUR,
    LAN_CODE
)
Select
    TDR_SEQ.NEXTVAL,
    tmp.TDR_TABLE, 
    tmp.TDR_CODE, 
    tmp.TDR_VALEUR,
    tmp.LAN_CODE
From
    (Select 
        '" + modelClass.FullyQualifiedName + @"_.' " + ConcatOperator + @" flat.ColumnName TDR_TABLE,
        flat.""PrimaryKey"" TDR_CODE, 
        case when lan.LAN_IS_DEFAULT = 1 then  flat.ColumnValue else '['" + ConcatOperator + @" lan.LAN_CODE" + ConcatOperator + @"']' " + ConcatOperator + @" flat.ColumnValue " + ConcatOperator + @" '[/'" + ConcatOperator + @" lan.LAN_CODE" + ConcatOperator + @"']' end TDR_VALEUR,
        lan.LAN_CODE
    From 	
        (Select ");
            foreach (var prop in translatedPropList) {
                writerInsert.WriteLine(
@"			cast(" + prop.DataMember.Name + " as nvarchar2(2000)) \"" + prop.Name + "\",");
            }

            writerInsert.WriteLine(
@"			cast(cast(" + modelClass.PrimaryKey.First().DataMember.Name + " as varchar2(250)) as nvarchar2(250)) \"PrimaryKey\" ");
            writerInsert.WriteLine(
@"		From 
            " + modelClass.DataContract.Name + @") p	
        unpivot (ColumnValue FOR ColumnName IN (""" + propConcat + @""")) flat	
        cross join LANG lan
    ) tmp
    left join TRADUCTION_REFERENCE tdr
        on tdr.TDR_TABLE = tmp.TDR_TABLE
        and tdr.TDR_CODE = tmp.TDR_CODE
        and tdr.LAN_CODE = tmp.LAN_CODE
Where
    tdr.TDR_ID is null;");
            writerInsert.WriteLine();
        }

        /// <summary>
        /// Ajoute une erreur à la liste des messages si l'identifiant est trop long.
        /// </summary>
        /// <param name="messageList">Liste des messages d'erreur/d'avertissement.</param>
        /// <param name="identifier">Identifiant à vérfier.</param>
        private void CheckIdentifierLength(ICollection<NVortexMessage> messageList, string identifier) {
            try {
                CheckIdentifierLength(identifier);
            } catch (ArgumentException exception) {
                messageList.Add(new NVortexMessage {
                    Code = "IDENTIFIER_TOO_LONG",
                    IsError = true,
                    Category = Category.Error,
                    FileName = string.Empty,
                    Description = exception.Message
                });
            }
        }

        /// <summary>
        /// Retourne les informations de taille de champ.
        /// </summary>
        /// <param name="property">Propriété.</param>
        /// <param name="persistentType">Nom du type persistant.</param>
        /// <returns>Informations de taille.</returns>
        /// <todo who="ADE" type="BUG">Gérer proprement la précision du datetime2.</todo>
        private string GetLengthInformation(ModelProperty property, string persistentType) {
            bool hasLength = property.DataDescription.Domain.PersistentLength.HasValue;
            bool hasPrecision = property.DataDescription.Domain.PersistentPrecision.HasValue;
            if (hasLength) {
                persistentType += "(" + property.DataDescription.Domain.PersistentLength;
                if (hasPrecision) {
                    persistentType += "," + property.DataDescription.Domain.PersistentPrecision;
                }

                persistentType += ")";
            } else if (persistentType == "datetime2") {
                persistentType += "(" + DomainManager.Instance.GetDomain(property.DataDescription.Domain.Code).Length + ")";
            }

            return persistentType;
        }

        /// <summary>
        /// Déclaration de la table.
        /// </summary>
        /// <param name="classe">La table à ecrire.</param>
        /// <param name="writerCrebas">Flux d'écriture crebas.</param>
        /// <param name="writerUk">Flux d'écriture Unique Key.</param>
        /// <param name="writerType">Flux d'écritures des types.</param>
        /// <returns>Liste des propriétés étrangères persistentes.</returns>
        private IEnumerable<ModelProperty> WriteTableDeclaration(ModelClass classe, StreamWriter writerCrebas, StreamWriter writerUk, StreamWriter writerType) {
            ICollection<ModelProperty> fkPropertiesList = new List<ModelProperty>();

            string tableName = CheckIdentifierLength(classe.DataContract.Name.ToUpperInvariant());

            writerCrebas.WriteLine("/**");
            writerCrebas.WriteLine("  * Création de la table " + tableName);
            writerCrebas.WriteLine(" **/");
            writerCrebas.WriteLine("create table " + tableName + " (");

            bool isContainsInsertKey = classe.PersistentPropertyList.Where(p => p.Name == InsertKeyName).Count() > 0;
            if (isContainsInsertKey) {
                WriteType(classe, writerType);
            }

            int nbPropertyCount = classe.PersistentPropertyList.Count;
            int t = 0;
            bool hasUniqueMultipleProperties = false;
            foreach (ModelProperty property in classe.PersistentPropertyList) {
                string persistentType = CodeUtils.PowerDesignerPersistentDataTypeToSqlDatType(property.DataDescription.Domain.PersistentDataType);
                persistentType = GetLengthInformation(property, persistentType);
                writerCrebas.Write("\t" + CheckIdentifierLength(property.DataMember.Name) + " " + persistentType);
                if (property.DataDescription.IsPrimaryKey && property.DataDescription.Domain.Code == "DO_ID") {
                    WriteIdentityColumn(writerCrebas);
                }

                if (isContainsInsertKey && !property.DataDescription.IsPrimaryKey && property.Name != InsertKeyName) {
                    if (t > 0) {
                        writerType.Write(",");
                        writerType.WriteLine();
                    }

                    writerType.Write("\t" + property.DataMember.Name + " " + persistentType);
                    t++;
                }

                if (property.DataMember.IsRequired) {
                    writerCrebas.Write(" not null");
                }

                writerCrebas.Write(",");
                writerCrebas.WriteLine();

                if (!string.IsNullOrEmpty(property.DataDescription.ReferenceType)) {
                    fkPropertiesList.Add(property);
                }

                if (property.IsUniqueMany) {
                    hasUniqueMultipleProperties = true;
                }

                if (property.IsUnique) {
                    writerUk.WriteLine("alter table " + classe.DataContract.Name.ToUpperInvariant() + " add constraint " + CheckIdentifierLength("UK_" + classe.DataContract.Name.ToUpperInvariant() + '_' + property.Name.ToUpperInvariant()) + " unique (" + property.DataMember.Name + ")");
                    writerUk.WriteLine(BatchSeparator);
                    writerUk.WriteLine();
                }
            }

            WriteBooleanConstraints(writerCrebas, classe);
            WritePrimaryKeyConstraint(writerCrebas, classe);
            WriteTableSpaceTable(writerCrebas, classe);
            writerCrebas.WriteLine(BatchSeparator);

            CreateSequenceIfNeeded(writerCrebas, classe);

            if (hasUniqueMultipleProperties) {
                WriteUniqueMultipleProperties(classe, writerUk);
            }

            writerCrebas.WriteLine();

            if (isContainsInsertKey) {
                if (t > 0) {
                    writerType.Write(',');
                    writerType.WriteLine();
                }

                writerType.WriteLine('\t' + classe.Trigram + '_' + "INSERT_KEY int");
                writerType.WriteLine();
                writerType.WriteLine(")");
                writerType.WriteLine(BatchSeparator);
                writerType.WriteLine();
            }

            // Histo table
            if (classe.IsHistorized) {
                string histTableName = "HIST_" + tableName;
                string triggerCommandInsert = "INSERT INTO " + histTableName + "(";
                string triggerCommandValue = "VALUES (";

                // Table histo.
                writerCrebas.WriteLine("/**");
                writerCrebas.WriteLine("  * Création de la table " + histTableName);
                writerCrebas.WriteLine(" **/");
                writerCrebas.WriteLine("create table " + histTableName + " (");
                foreach (ModelProperty property in classe.PersistentPropertyList) {
                    triggerCommandInsert += property.DataMember.Name + ", ";
                    triggerCommandValue += ":new." + property.DataMember.Name + ", ";
                    string persistentType = CodeUtils.PowerDesignerPersistentDataTypeToSqlDatType(property.DataDescription.Domain.PersistentDataType);
                    persistentType = GetLengthInformation(property, persistentType);
                    writerCrebas.Write("\t" + CheckIdentifierLength(property.DataMember.Name) + " " + persistentType);

                    writerCrebas.Write(",");
                    writerCrebas.WriteLine();
                }

                writerCrebas.WriteLine("\tDATE_EDIT  TIMESTAMP(6) NOT NULL,");
                writerCrebas.WriteLine("\tIS_CREATION NUMBER(1,0) NOT NULL");

                writerCrebas.WriteLine(")");
                WriteTableSpaceTable(writerCrebas, classe);
                writerCrebas.WriteLine(BatchSeparator);
                writerCrebas.WriteLine();

                WriteTrigger(writerCrebas, tableName, triggerCommandInsert, triggerCommandValue, true);
                WriteTrigger(writerCrebas, tableName, triggerCommandInsert, triggerCommandValue, false);
            }

            return fkPropertiesList;
        }

        private void WriteTrigger(StreamWriter writer, string tableName, string triggerCommandInsert, string triggerCommandValue, bool isInsert) {
            string operation = isInsert ? "INSERT" : "UPDATE";
            writer.WriteLine("create or replace  TRIGGER " + tableName + "_AFTER_" + operation + " AFTER " + operation + " ON " + tableName + " FOR EACH ROW BEGIN");
            writer.WriteLine(triggerCommandInsert + "DATE_EDIT, IS_CREATION)");
            writer.WriteLine(triggerCommandValue + "sysdate, " + (isInsert ? "1" : "0") + ");");
            writer.WriteLine("END;");
            writer.WriteLine(BatchSeparator);
            writer.WriteLine();
        }

        /// <summary>
        /// Ajoute les contraintes de clés primaires.
        /// </summary>
        /// <param name="writerCrebas">Writer.</param>
        /// <param name="classe">Classe.</param>
        private void WritePrimaryKeyConstraint(StreamWriter writerCrebas, ModelClass classe) {
            int pkCount = 0;
            writerCrebas.Write("\tconstraint PK_" + classe.DataContract.Name.ToUpperInvariant() + " primary key ");
            if (SupportsClusteredKey) {
                writerCrebas.Write("clustered ");
            }

            writerCrebas.Write("(");
            foreach (ModelProperty pkProperty in classe.PrimaryKey) {
                ++pkCount;
                writerCrebas.Write(pkProperty.DataMember.Name.ToUpperInvariant());
                if (pkCount < classe.PrimaryKey.Count) {
                    writerCrebas.Write(",");
                } else {
                    writerCrebas.WriteLine(")");
                }
            }

            writerCrebas.WriteLine(")");
        }

        /// <summary>
        /// Génère la contrainte de clef étrangère.
        /// </summary>
        /// <param name="property">Propriété portant la clef étrangère.</param>
        /// <param name="writer">Flux d'écriture.</param>
        private void GenerateConstraintForeignKey(ModelProperty property, StreamWriter writer) {
            string tableName = property.Class.DataContract.Name.ToUpperInvariant();
            string propertyName = property.DataMember.Name.ToUpperInvariant();
            writer.WriteLine("/**");
            writer.WriteLine("  * Génération de la contrainte de clef étrangère pour " + tableName + "." + propertyName);
            writer.WriteLine(" **/");
            writer.WriteLine("alter table " + tableName);
            string constraintName = "FK_" + property.Class.Trigram + "_" + propertyName;

            writer.WriteLine("\tadd constraint " + constraintName + " foreign key (" + propertyName + ")");
            writer.Write("\t\treferences " + property.DataDescription.ReferenceClass.DataContract.Name.ToUpperInvariant() + " (");

            int currentProperty = 0;
            int propertyCount = property.DataDescription.ReferenceClass.PrimaryKey.Count;
            foreach (ModelProperty targetPkProperty in property.DataDescription.ReferenceClass.PrimaryKey) {
                ++currentProperty;
                writer.Write(targetPkProperty.DataMember.Name.ToUpperInvariant());
                if (currentProperty < propertyCount) {
                    writer.Write(", ");
                }
            }

            writer.WriteLine(")");
            writer.WriteLine(BatchSeparator);
            writer.WriteLine();
        }

        /// <summary>
        /// Génère l'index portant sur la clef étrangère.
        /// </summary>
        /// <param name="writer">Flux d'écriture.</param>
        /// <param name="property">Propriété cible de l'index.</param>
        private void GenerateIndexForeignKey(StreamWriter writer, ModelProperty property) {
            string tableName = property.Class.DataContract.Name.ToUpperInvariant();
            string propertyName = property.DataMember.Name.ToUpperInvariant();
            writer.WriteLine("/**");
            writer.WriteLine("  * Création de l'index de clef étrangère pour " + tableName + "." + propertyName);
            writer.WriteLine(" **/");
            writer.WriteLine("create index IDX_" + property.Class.Trigram + "_" + propertyName + "_FK on " + tableName + " (");
            writer.WriteLine("\t" + propertyName + " ASC");
            writer.WriteLine(")");
            WriteTableSpaceIndex(writer, property.Class);
            writer.WriteLine(BatchSeparator);
            writer.WriteLine();
        }

        /// <summary>
        /// Ajoute les contraintes d'unicité.
        /// </summary>
        /// <param name="classe">Classe.</param>
        /// <param name="writerUk">Writer.</param>
        private void WriteUniqueMultipleProperties(ModelClass classe, StreamWriter writerUk) {
            writerUk.Write("alter table " + classe.DataContract.Name.ToUpperInvariant() + " add constraint UK_" + classe.DataContract.Name.ToUpperInvariant() + "_MULTIPLE unique (");
            int i = 0;
            foreach (ModelProperty property in classe.PersistentPropertyList) {
                if (!property.IsUniqueMany) {
                    continue;
                }

                if (i > 0) {
                    writerUk.Write(", ");
                }

                writerUk.Write(property.DataMember.Name);
                ++i;
            }

            writerUk.WriteLine(")");
            writerUk.WriteLine(BatchSeparator);
            writerUk.WriteLine();
        }

        /// <summary>
        /// Ecrit dans le writer le script d'insertion dans la table staticTable ayant pour model modelClass.
        /// </summary>
        /// <param name="writer">Writer.</param>
        /// <param name="staticTable">Classe de reference statique.</param>
        /// <param name="modelClass">Modele de la classe.</param>
        /// <param name="isStatic">True if generation for static list.</param>
        private void WriteInsert(StreamWriter writer, TableInit staticTable, ModelClass modelClass, bool isStatic) {
            writer.WriteLine("/**\t\tInitialisation de la table " + modelClass.Name + "\t\t**/");
            foreach (ItemInit initItem in staticTable.ItemInitList) {
                writer.WriteLine(GetInsertLine(modelClass, initItem, isStatic));
            }

            writer.WriteLine();
        }

        /// <summary>
        /// Retourne la ligne d'insert.
        /// </summary>
        /// <param name="modelClass">Modele de la classe.</param>
        /// <param name="initItem">Item a insérer.</param>
        /// <param name="isPrimaryKeyIncluded">True si le script d'insert doit comporter la clef primaire.</param>
        /// <returns>Requête.</returns>
        private string GetInsertLine(ModelClass modelClass, ItemInit initItem, bool isPrimaryKeyIncluded) {
            Dictionary<string, string> propertyValueDict = CreatePropertyValueDictionary(modelClass, initItem, isPrimaryKeyIncluded);
            return GetInsertLine(modelClass.DataContract.Name, propertyValueDict);
        }

        /// <summary>
        /// Retourne la ligne d'insert.
        /// </summary>
        /// <param name="tableName">Nom de la table dans laquelle ajouter la ligne.</param>
        /// <param name="propertyValuePairs">Dictionnaire au format {nom de la propriété => valeur}.</param>
        /// <returns>La requête "INSERT INTO ..." générée.</returns>
        private string GetInsertLine(string tableName, Dictionary<string, string> propertyValuePairs) {
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO " + tableName + "(");
            bool isFirst = true;
            foreach (string columnName in propertyValuePairs.Keys) {
                if (!isFirst) {
                    sb.Append(", ");
                }

                isFirst = false;
                sb.Append(columnName);
            }

            sb.Append(") VALUES(");

            isFirst = true;
            foreach (string value in propertyValuePairs.Values) {
                if (!isFirst) {
                    sb.Append(", ");
                }

                isFirst = false;
                sb.Append(value);
            }

            sb.Append(");");
            return sb.ToString();
        }
    }
}
