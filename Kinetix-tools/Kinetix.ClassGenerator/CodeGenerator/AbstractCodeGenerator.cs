using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.Writer;
using Kinetix.ComponentModel;

namespace Kinetix.ClassGenerator.CodeGenerator {

    /// <summary>
    /// Classe abstraite de génération de code.
    /// </summary>
    public abstract class AbstractCodeGenerator {

        private readonly string _outputDirectory;
        private FileWriter _currentWriter;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="directory">Répertoire de génération des sources.</param>
        protected AbstractCodeGenerator(string directory) {
            _outputDirectory = directory;
        }

        /// <summary>
        /// Obtient ou définit l'extension des fichiers associés.
        /// </summary>
        public abstract string FileExtension {
            get;
        }

        /// <summary>
        /// Obtient la valeur de l'attribut permettant de spécifier que la classe est sérialisable.
        /// </summary>
        public abstract string SerializableAttribute {
            get;
        }

        /// <summary>
        /// Obtient la valeur de la chaine de caractère précisant que la ligne est en commentaire.
        /// </summary>
        public abstract string Comment {
            get;
        }

        /// <summary>
        /// Génère le code des classes.
        /// </summary>
        /// <param name="modelRootList">Liste des modeles.</param>
        public void Generate(ICollection<ModelRoot> modelRootList) {
            if (modelRootList == null) {
                throw new ArgumentNullException(nameof(modelRootList));
            }

            if (GeneratorParameters.IsEntityFrameworkUsed) {
                GenerateDbContext(modelRootList);
            }

            foreach (ModelRoot model in modelRootList) {
                if (model.Namespaces != null && model.Namespaces.Values.Count > 0) {
                    foreach (ModelNamespace ns in model.Namespaces.Values) {
                        if (!Directory.Exists(ns.Name)) {

                            var directoryForModelClass = GetDirectoryForModelClass(ns.HasPersistentClasses, model.Name, ns.Name);
                            var projectDirectory = GetDirectoryForProject(ns.HasPersistentClasses, model.Name, ns.Name);
                            var csprojFileName = Path.Combine(projectDirectory, model.Name + "." + ns.Name + ".csproj");

                            foreach (ModelClass item in ns.ClassList) {
                                var currentDirectory = GetDirectoryForModelClass(item.DataContract.IsPersistent, model.Name, item.Namespace.Name);
                                Directory.CreateDirectory(currentDirectory);
                                using (_currentWriter = new CsharpFileWriter(GetFilenameFromModelClass(item, model.Name), csprojFileName)) {
                                    Console.Out.WriteLine("Generating class " + ns.Name + "." + item.Name);
                                    GenerateClass(item, ns, model.Name);
                                }
                            }
                        }
                    }
                }
            }

            GenerateReferenceAccessors(modelRootList);
        }

        /// <summary>
        /// Retourne le code associé à la déclaration.
        /// </summary>
        /// <param name="name">Nom de la classe.</param>
        /// <param name="inheritedClass">Classe parente.</param>
        /// <param name="ifList">Liste des interfaces implémentées.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadBeginClassDeclaration(string name, string inheritedClass, ICollection<string> ifList);

        /// <summary>
        /// Retourne le code associé à la déclaration d'ouverture d'un constructeur.
        /// </summary>
        /// <param name="type">Type de l'objet.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadBeginConstructor(string type);

        /// <summary>
        /// Retourne le code associé à la déclaration d'une interface.
        /// </summary>
        /// <param name="name">Nom de l'interface.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadBeginInterfaceDeclaration(string name);

        /// <summary>
        /// Retourne le code associé à la déclaration d'un namespace.
        /// </summary>
        /// <param name="value">Valeur du namespace.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadBeginNamespace(string value);

        /// <summary>
        /// Retourne le code associé à la déclaration de l'énum des colonnes.
        /// </summary>
        /// <param name="enumName">Nom de l'enum.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadBeginPublicEnum(string enumName);

        /// <summary>
        /// Retourne le code associé au début de l'implémentation d'un service de type ReferenceAccessor.
        /// </summary>
        /// <param name="className">Nom du type chargé par le ReferenceAccessor.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadBeginReferenceAccessorImplementation(string className);

        /// <summary>
        /// Retourne l'attribut Browsable.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadBrowsableAttribute();

        /// <summary>
        /// Retourne l'attribut Column.
        /// </summary>
        /// <param name="prop">Propriété du modèle.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadColumnAttribute(ModelProperty prop);

        /// <summary>
        /// Retourne l'attribut DataContract.
        /// </summary>
        /// <param name="dataContract">Données du DataContract.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadDataContractAttribute(ModelDataContract dataContract);

        /// <summary>
        /// Retourne l'attribut DataMember.
        /// </summary>
        /// <param name="modelProperty">Propriété liée.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadDataMemberAttribute(ModelProperty modelProperty);

        /// <summary>
        /// Charge l'attribut DatabaseGenerated.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadDatabaseGeneratedAttribute();

        /// <summary>
        /// Déclare une variable membre privée.
        /// </summary>
        /// <param name="name">Nom de la variable.</param>
        /// <param name="dataType">Type de données.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadDeclarePrivateField(string name, string dataType);

        /// <summary>
        /// Retourne l'attribut DefaultProperty.
        /// </summary>
        /// <param name="defaultProperty">La propriété DefaultProperty.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadDefaultPropertyAttribute(string defaultProperty);

        /// <summary>
        /// Retourne l'attribut Display.
        /// </summary>
        /// <param name="displayName">Valeur de l'attribut Name de Display.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadDisplayAttribute(string displayName);

        /// <summary>
        /// Retourne l'attribut Domain.
        /// </summary>
        /// <param name="domainCode">Nom du domaine.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadDomainAttribute(string domainCode);

        /// <summary>
        /// Retourne l'attribut EditorBrowsable.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadEditorBrowsableAttribute();

        /// <summary>
        /// Retourne le code associé à la fin de déclaration d'une classe.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadEndClassDeclaration();

        /// <summary>
        /// Retourne le code associé à la fin de déclaration d'un constructeur.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadEndConstructor();

        /// <summary>
        /// Retourne le code associé à la fin de déclaration d'un enum.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadEndEnum();

        /// <summary>
        /// Retourne le code associé à la fin de déclaration de namespace.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadEndNamespace();

        /// <summary>
        /// Retourne le code associé à la déclaration d'une valeur d'un enum.
        /// </summary>
        /// <param name="enumValue">Nom de l'enum.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadEnumItem(string enumValue);

        /// <summary>
        /// Retourne l'attribut DatabaseGenerated set à None.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadGeneratedNoneAttribute();

        /// <summary>
        /// Retourne le code associé à l'instanciation d'un membre privé.
        /// </summary>
        /// <param name="fieldName">Nom de la variable membre privée.</param>
        /// <param name="dataType">Type de données.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadInitPrivateField(string fieldName, string dataType);

        /// <summary>
        /// Retourne le code associé à l'instanciation d'un membre public.
        /// </summary>
        /// <param name="fieldName">Nom de la variable membre privée.</param>
        /// <param name="dataType">Type de données.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadInitPublicField(string fieldName, string dataType);

        /// <summary>
        /// Retourne l'attribut Key.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadKeyAttribute();

        /// <summary>
        /// Retourne l'attribut OperationContract.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadOperationContractAttribute();

        /// <summary>
        /// Retourne le commentaire du param formatté.
        /// </summary>
        /// <param name="paramName">Nom du paramètre.</param>
        /// <param name="value">Description du paramètre.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadParam(string paramName, string value);

        /// <summary>
        /// Retourne un constructeur privé.
        /// </summary>
        /// <param name="className">Nom de la classe.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadPrivateConstructor(string className);

        /// <summary>
        /// Retourne un champ privé.
        /// </summary>
        /// <param name="fieldType">Type du champ.</param>
        /// <param name="fieldName">Nom du champ.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadPrivateField(string fieldType, string fieldName);

        /// <summary>
        /// Retourne le code associé à la déclaration d'une variable constante publique.
        /// </summary>
        /// <param name="fieldName">Nom de la variable.</param>
        /// <param name="dataType">Type de données associé.</param>
        /// <param name="value">Valeur de la variable.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadPublicConstField(string fieldName, string dataType, string value);

        /// <summary>
        /// Retourne le code associé à la déclaration d'une propriété publique.
        /// </summary>
        /// <param name="propertyType">Type de la propriété.</param>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <param name="propertyBackingField">Dans le cas de PostSharp désactivé, on passe la variable qui stoque la valeur du champ.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadPublicPropertyCode(string propertyType, string propertyName, string propertyBackingField);

        /// <summary>
        /// Retourne le code associé à la déclaration d'une propriété public read-only.
        /// </summary>
        /// <param name="propertyType">Type de la propriété.</param>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <param name="privateFieldName">Nom de la variable membre pré-initialisée dans le constructeur.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadPublicReadOnlyPropertyCode(string propertyType, string propertyName, string privateFieldName);

        /// <summary>
        /// Retourne le code associé à la déclaration d'une propriété obligatoire et publique issue d'une composition.
        /// </summary>
        /// <param name="propertyType">Type de la propriété.</param>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadPublicRequiredCompositionPropertyCode(string propertyType, string propertyName);

        /// <summary>
        /// Charge l'attribut Range.
        /// </summary>
        /// <param name="min">Valeur minimale.</param>
        /// <param name="max">Valeur maximale.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadRangeAttribute(int min, int max);

        /// <summary>
        /// Retourne l'attribut ReferenceAccessor.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadReferenceAccessorAttribute();

        /// <summary>
        /// Retourne le code associé au cors de l'implémentation d'un service de type ReferenceAccessor.
        /// </summary>
        /// <param name="className">Nom du type chargé par le ReferenceAccessor.</param>
        /// <param name="defaultProperty">Propriété par defaut de la classe.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadReferenceAccessorBody(string className, ModelProperty defaultProperty);

        /// <summary>
        /// Retourne le code de déclaration d'un service de type ReferenceAccessor.
        /// </summary>
        /// <param name="className">Nom du type de l'entité chargée par le ReferenceAccessor.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadReferenceAccessorDeclaration(string className);

        /// <summary>
        /// Retourne l'attribut ReferenceAccessor.
        /// </summary>
        /// <param name="isStatic"><code>True</code> si la liste est statique, <code>False</code> sinon.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadReferenceAttribute(bool isStatic);

        /// <summary>
        /// Retourne l'attribut Association.
        /// </summary>
        /// <param name="typeName">Nom du type.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadReferencedTypeAttribute(string typeName);

        /// <summary>
        /// Charge l'attribut Required.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadRequiredAttribute();

        /// <summary>
        /// Retourne le commentaire du returns formatté.
        /// </summary>
        /// <param name="value">Description de la valeur retournée.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadReturns(string value);

        /// <summary>
        /// Charge l'attribut ServiceBehavior.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadServiceBehaviorAttribute();

        /// <summary>
        /// Charge l'attribut ServiceContract.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected abstract string LoadServiceContractAttribute();

        /// <summary>
        /// Retourne le commentaire du summary formatté.
        /// </summary>
        /// <param name="summary">Contenu du commentaire.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadSummary(string summary);

        /// <summary>
        /// Charge l'attribut SuppressMessageAttribute.
        /// </summary>
        /// <param name="category">Categorie.</param>
        /// <param name="checkId">Identifiant.</param>
        /// <param name="justification">Justification.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadSuppressMessageAttribute(string category, string checkId, string justification);

        /// <summary>
        /// Retourne l'attribut Table.
        /// </summary>
        /// <param name="dataContract">Données du DataContract.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadTableAttribute(ModelDataContract dataContract);

        /// <summary>
        /// Retourne l'attribut TypeDescriptionProvider pour la classe.
        /// </summary>
        /// <param name="classe">La classe en question.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadTypeDescriptionProviderAttribute(ModelClass classe);

        /// <summary>
        /// Retourne le code associé à la déclaration d'un Using.
        /// </summary>
        /// <param name="nsName">Nom de la classe/namespace à importer.</param>
        /// <returns>Code généré.</returns>
        protected abstract string LoadUsing(string nsName);

        /// <summary>
        /// Génère l'attribut de validation issu du domaine.
        /// </summary>
        /// <param name="validationAttribute">Attribut de validation.</param>
        /// <returns>Attribut de validation formatté.</returns>
        protected abstract string LoadValidationAttribute(ValidationAttribute validationAttribute);

        /// <summary>
        /// Retourne le commentaire inheritdoc.
        /// </summary>
        /// <param name="interfaceName">Nom de l'interface où se trouve le service dont on hérite de la documentation.</param>
        /// <param name="serviceName">Nom du service dont on hérite de la documentation.</param>
        /// <returns>Code généré.</returns>
        protected abstract string Loadinheritdoc(string interfaceName, string serviceName);

        private static string ExtractModuleMetier(string nameSpace) {
            const string DataContractSuffix = "DataContract";
            const string ContractSuffix = "Contract";
            if (nameSpace.EndsWith(DataContractSuffix, StringComparison.InvariantCultureIgnoreCase)) {
                return nameSpace.Substring(0, nameSpace.Length - DataContractSuffix.Length);
            }

            if (nameSpace.EndsWith(ContractSuffix, StringComparison.InvariantCultureIgnoreCase)) {
                return nameSpace.Substring(0, nameSpace.Length - ContractSuffix.Length);
            }

            return nameSpace;
        }

        /// <summary>
        /// Retourne le nom des backing field.
        /// </summary>
        /// <param name="property">Propriété.</param>
        /// <returns>String.</returns>
        private static string GetBackingFieldName(ModelProperty property) {
            if (GeneratorParameters.IsNotifyPropertyChangeEnabled) {
                var sb = new StringBuilder("_");
                sb.Append(property.Name[0].ToString().ToLower(CultureInfo.InvariantCulture));
                if (sb.Length > 1) {
                    sb.Append(property.Name.Substring(1));
                }

                return sb.ToString();
            }

            return null;
        }

        /// <summary>
        /// Retourne le namespace du projet "Common" de Business.
        /// </summary>
        /// <param name="projectName">Nom du projet.</param>
        /// <returns>Namespace du projet "Common" de Business.</returns>
        private static string GetBusinessCommonNamespace(string projectName) {
            return string.Format(CultureInfo.InvariantCulture, "{0}.Business.Common", projectName);
        }

        /// <summary>
        /// Retourne le namespace complet d'une class.
        /// </summary>
        /// <param name="root">Racine du namespace.</param>
        /// <param name="localNamespace">Namespace local de la classe.</param>
        /// <returns>Namespace complet.</returns>
        private static string GetClassNamespace(string root, string localNamespace) {
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", root, localNamespace);
        }

        /// <summary>
        /// Supprime les points de la chaîne.
        /// </summary>
        /// <param name="dottedString">Chaîne avec points.</param>
        /// <returns>Chaîne sans points.</returns>
        private static string RemoveDots(string dottedString) {
            if (string.IsNullOrEmpty(dottedString)) {
                return dottedString;
            }

            return dottedString.Replace(".", string.Empty);
        }

        /*/// <summary>
        /// Indique si la classe en paramètre à des propriétés à initialiser dans le constructor.
        /// </summary>
        /// <param name="item">La classe concernée.</param>
        /// <returns><code>True</code> si classe possède des propriétés à initialiser, <code>False</code> sinon.</returns>
        private static bool HasConstructor(ModelClass item) {
            return item.PropertyList.Any(property => property.IsCollection || !property.IsPrimitive);
        }*/

        /// <summary>
        /// Méthode générant le code d'une classe.
        /// </summary>
        /// <param name="item">Classe concernée.</param>
        /// <param name="ns">Namespace.</param>
        /// <param name="root">Racine du nom du namespace.</param>
        private void GenerateClass(ModelClass item, ModelNamespace ns, string root) {
            GenerateUsing(item);
            WriteEmptyLine();
            var classNamespace = GetClassNamespace(root, ns.Name);
            WriteLine(LoadBeginNamespace(classNamespace));
            WriteEmptyLine();
            GenerateComment(item);
            GenerateClassDeclaration(item);
            WriteLine(LoadEndNamespace());
        }

        /// <summary>
        /// Génération de la déclaration de la classe.
        /// </summary>
        /// <param name="item">Classe à générer.</param>
        private void GenerateClassDeclaration(ModelClass item) {
            if (item.Stereotype == Stereotype.Reference) {
                WriteLine(1, LoadReferenceAttribute(false));
                //// WriteLine(1, LoadDefaultPropertyAttribute("Libelle"));
            } else if (item.Stereotype == Stereotype.Statique) {
                WriteLine(1, LoadReferenceAttribute(true));
                //// WriteLine(1, LoadDefaultPropertyAttribute("Libelle"));
            }

            if (GeneratorParameters.IsPostSharpDisabled) {
                WriteLine(1, "[Serializable]");
                WriteLine(1, "[DataContract]");
            }

            if (!string.IsNullOrEmpty(item.DefaultProperty)) {
                WriteLine(1, LoadDefaultPropertyAttribute(item.DefaultProperty));
            }

            if (item.DataContract.IsPersistent && !item.IsView) {
                WriteLine(1, LoadTableAttribute(item.DataContract));
            }

            ICollection<string> ifList = new List<string>();
            if (GeneratorParameters.IsPostSharpDisabled) {
                if (item.ParentClass == null) {
                    if (item.DataContract.IsPersistent) {
                        ifList.Add("IBeanState");
                    }

                    if (GeneratorParameters.IsNotifyPropertyChangeEnabled) {
                        ifList.Add("INotifyPropertyChanged");
                    }
                }

                if (item.Stereotype == Stereotype.Reference) {
                    ifList.Add("IReferenceBean");
                } else if (item.Stereotype == Stereotype.Statique) {
                    ifList.Add("IStaticBean");
                }
            }

            if (item.AuditEnabled) {
                ifList.Add("IOptimisticLocking");
            }

            if (item.ExportDeltaEnabled) {
                ifList.Add("IDeltaExportable");
            }

            WriteLine(1, LoadBeginClassDeclaration(item.Name, item.ParentClass == null ? string.Empty : item.ParentClass.Name, ifList));

            GenerateConstProperties(item);

            GenerateConstructors(item);

            if (item.DataContract.IsPersistent && !item.IsView && item.PersistentPropertyList.Count > 0) {
                WriteEmptyLine();
                WriteLine(2, "#region Meta données");
                GenerateEnumCols(item);
                WriteEmptyLine();
                WriteLine(2, "#endregion");
            }

            GenerateProperties(item);
            GenerateExtensibilityMethods(item);

            if (GeneratorParameters.IsPostSharpDisabled) {
                if (item.ParentClass == null && item.DataContract.IsPersistent) {
                    GenerateStateMethod();
                }
            }

            if (GeneratorParameters.IsNotifyPropertyChangeEnabled) {
                GenerateNotifyPropertyChangeField(item);
                if (item.ParentClass == null) {
                    GenerateOnPropertyChangeMethod();
                }
            }

            WriteLine(1, LoadEndClassDeclaration());

            if (GeneratorParameters.UseTypeSafeConstValues) {
                GenerateConstPropertiesClass(item);
            }
        }

        /// <summary>
        /// Génération du commentaire d'entete d'objet.
        /// </summary>
        /// <param name="item">Classe concernée.</param>
        private void GenerateComment(ModelClass item) {
            WriteSummary(1, item.Comment);
        }

        /// <summary>
        /// Génération des constantes statiques.
        /// </summary>
        /// <param name="item">La classe générée.</param>
        private void GenerateConstProperties(ModelClass item) {
            int nbConstValues = item.ConstValues.Count;
            if (nbConstValues != 0) {

                WriteEmptyLine();

                int i = 0;
                foreach (string constFieldName in item.ConstValues.Keys.ToList().OrderBy(x => x)) {
                    ++i;
                    StaticListElement valueLibelle = item.ConstValues[constFieldName];
                    ModelProperty property = null;
                    if (item.Stereotype == Stereotype.Reference) {
                        foreach (ModelProperty prop in item.PropertyList) {
                            if (prop.IsUnique) {
                                property = prop;
                                break;
                            }
                        }
                    } else {
                        property = ((IList<ModelProperty>)item.PrimaryKey)[0];
                    }

                    WriteSummary(2, valueLibelle.Libelle);

                    if (GeneratorParameters.UseTypeSafeConstValues) {
                        WriteLine(2, String.Format("public static readonly {2}Code {0} = new {2}Code({1});", constFieldName, valueLibelle.Code, item.Name));
                    } else {
                        WriteLine(2, String.Format("public const string {0} = {1};", constFieldName, valueLibelle.Code));
                    }

                    if (i < nbConstValues) {
                        WriteEmptyLine();
                    }
                }
            }
        }

        /// <summary>
        /// Génération des constantes statiques.
        /// </summary>
        /// <param name="item">La classe générée.</param>
        private void GenerateConstPropertiesClass(ModelClass item) {
            int nbConstValues = item.ConstValues.Count;
            if (nbConstValues != 0) {

                WriteEmptyLine();
                WriteLine("#pragma warning disable SA1402");
                WriteEmptyLine();
                WriteSummary(1, $"Type des valeurs pour {item.Name}");
                WriteLine(1, $"public sealed class {item.Name}Code : TypeSafeEnum {{");
                WriteEmptyLine();

                WriteLine(2, $"private static readonly Dictionary<string, {item.Name}Code> Instance = new Dictionary<string, {item.Name}Code>();");
                WriteEmptyLine();

                WriteSummary(2, "Constructeur");
                WriteParam(2, "value", "Valeur");
                WriteLine(2, $"public {item.Name}Code(string value)");
                WriteLine(3, ": base(value) {");
                WriteLine(3, "Instance[value] = this;");
                WriteLine(2, "}");
                WriteEmptyLine();

                WriteLine(2, $"public static explicit operator {item.Name}Code(string value) {{");
                WriteLine(3, "if (Instance.TryGetValue(value, out var result)) {");
                WriteLine(4, "return result;");
                WriteLine(3, "} else {");
                WriteLine(4, "throw new InvalidCastException();");
                WriteLine(3, "}");
                WriteLine(2, "}");
                WriteLine(1, "}");
            }
        }

        /// <summary>
        /// Génère les constructeurs.
        /// </summary>
        /// <param name="item">La classe générée.</param>
        private void GenerateConstructors(ModelClass item) {
            WriteDefaultConstructor(item);
            WriteCopyConstructor(item);
            if (item.ParentClass != null) {
                WriteBaseCopyConstructor(item);
            }
        }

        /// <summary>
        /// Génère l'objectcontext spécialisé pour le schéma.
        /// </summary>
        /// <remarks>Support de Linq2Sql.</remarks>
        /// <param name="modelRootList">Liste des modeles.</param>
        private void GenerateDbContext(IEnumerable<ModelRoot> modelRootList) {
            Console.Out.WriteLine("Generating DbContext");

            // Si le tag est présent, tout mettre dans un seul Db Context.
            ModelRoot refModel = modelRootList.SingleOrDefault(x => x.ModelFile == GeneratorParameters.DbContext);

            string projectName = "";
            string strippedProjectName = "";
            string destDirectory = "";

            if (refModel != null) {
                projectName = refModel.Name;
                strippedProjectName = RemoveDots(projectName);
                destDirectory = GetImplementationDirectoryName(projectName);
            }

            IEnumerator<ModelRoot> enumerator = modelRootList.GetEnumerator();
            enumerator.MoveNext();
            if (refModel == null) {
                projectName = enumerator.Current.Name;
                strippedProjectName = RemoveDots(projectName);
                destDirectory = GetImplementationDirectoryName(projectName);
            }

            Directory.CreateDirectory(destDirectory);

            string targetFileName = Path.Combine(destDirectory, strippedProjectName + "DbContext.cs");
            using (_currentWriter = new CsharpFileWriter(targetFileName)) {
                WriteLine("using System.Data.Entity;");
                WriteLine("using System.Diagnostics.CodeAnalysis;");
                WriteLine("using System.Transactions;");
                WriteLine("using Fmk.Data.SqlClient;");

                List<string> listNs = new List<string>();
                foreach (ModelRoot model in modelRootList) {
                    //foreach (ModelNamespace ns in model.Namespaces.Values)
                    //{
                    //    if (ns.HasPersistentClasses)
                    //    {
                    //        listNs.Add(ns.Model.Name + "." + ns.Name);
                    //    }
                    //}
                }
                listNs.Sort();
                foreach (string ns in listNs.Distinct()) {
                    WriteLine("using " + ns + ";");
                }

                WriteEmptyLine();
                WriteLine("namespace " + GetBusinessCommonNamespace(projectName) + " {");
                //WriteEmptyLine();
                WriteSummary(1, "DbContext généré pour Entity-Framework.");
                WriteLine(1, "[SuppressMessage(\"Microsoft.Maintainability\", \"CA1506:AvoidExcessiveClassCoupling\", Justification = \"EF4.1\")]");
                WriteLine(1, "public partial class " + strippedProjectName + "DbContext : DbContext {");
                WriteEmptyLine();
                WriteSummary(2, "Constructeur par défaut.");
                WriteLine(2, "public " + strippedProjectName + "DbContext(TransactionScope transScope)");
                WriteLine(3, ": base(SqlServerManager.Instance.ObtainConnection(\"default\"), false) {");
                WriteLine(2, "}");

                foreach (ModelRoot model in modelRootList) {
                    foreach (ModelNamespace ns in model.Namespaces.Values) {
                        foreach (ModelClass classe in ns.ClassList) {
                            if (classe.DataContract.IsPersistent) {
                                WriteEmptyLine();
                                WriteSummary(2, "Accès à l'entité " + classe.Name);
                                WriteLine(2, "public DbSet<" + classe.FullyQualifiedName + "> " + Pluralize(classe.Name) + " { get; set; }");
                            }
                        }
                    }
                }

                WriteEmptyLine();
                WriteSummary(2, "Hook pour l'ajout de configuration sur EF (précision des champs, etc).");
                WriteParam(2, "modelBuilder", "L'objet de construction du modèle");
                WriteLine(2, "protected override void OnModelCreating(DbModelBuilder modelBuilder)");
                WriteLine(2, "{");
                WriteLine(3, "base.OnModelCreating(modelBuilder);");

                WriteEmptyLine();

                foreach (ModelRoot model in modelRootList) {
                    foreach (ModelNamespace ns in model.Namespaces.Values) {
                        foreach (ModelClass classe in ns.ClassList) {
                            if (classe.DataContract.IsPersistent) {
                                foreach (ModelProperty property in classe.PropertyList) {
                                    if (property.DataType == "decimal" && property.DataDescription.Domain.PersistentLength.HasValue && property.DataDescription.Domain.PersistentPrecision.HasValue) {
                                        WriteLine(3,
                                            string.Format("modelBuilder.Entity<{0}>().Property(x => x.{1}).HasPrecision({2}, {3});",
                                                classe.FullyQualifiedName,
                                                property.Name,
                                                property.DataDescription.Domain.PersistentLength.Value,
                                                property.DataDescription.Domain.PersistentPrecision.Value
                                            )
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
                WriteEmptyLine();

                WriteLine(3, "OnModelCreatingCustom(modelBuilder);");

                WriteLine(2, "}");

                WriteEmptyLine();
                WriteSummary(2, "Hook pour l'ajout de configuration custom sur EF (view, etc).");
                WriteParam(2, "modelBuilder", "L'objet de construction du modèle");
                WriteLine(2, "partial void OnModelCreatingCustom(DbModelBuilder modelBuilder);");

                WriteLine(1, "}");
                WriteLine("}");
            }
        }

        /// <summary>
        /// Génère le type énuméré présentant les colonnes persistentes.
        /// </summary>
        /// <param name="item">La classe générée.</param>
        private void GenerateEnumCols(ModelClass item) {
            if (item.DataContract.IsPersistent) {
                WriteEmptyLine();
                WriteSummary(2, "Type énuméré présentant les noms des colonnes en base.");
                WriteLine(2, "[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Microsoft.Design\", \"CA1034:NestedTypesShouldNotBeVisible\", Justification = \"A corriger\")]");
                WriteLine(2, LoadBeginPublicEnum("Cols"));
                foreach (ModelProperty property in item.PersistentPropertyList) {
                    WriteEmptyLine();
                    WriteSummary(3, "Nom de la colonne en base associée à la propriété " + property.Name + ".");
                    WriteLine(3, "[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Microsoft.Naming\", \"CA1709:IdentifiersShouldBeCasedCorrectly\", Justification = \"Correspondance schéma persistence\")]");
                    WriteLine(3, "[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Microsoft.Naming\", \"CA1707:IdentifiersShouldNotContainUnderscores\", Justification = \"Correspondance schéma persistence\")]");
                    WriteLine(3, LoadEnumItem(property.DataMember.Name));
                }

                WriteLine(2, LoadEndEnum());
            }
        }

        /// <summary>
        /// Génère les méthodes d'extensibilité.
        /// </summary>
        /// <param name="item">Classe générée.</param>
        private void GenerateExtensibilityMethods(ModelClass item) {
            if (item.NeedsInitialization) {
                WriteEmptyLine();
                WriteSummary(2, "Initialisation des fields privés.");
                WriteLine(2, "private void Initialize() {");
                foreach (ModelProperty property in item.PropertyList.Where(p => !p.IsPrimitive && !p.IsCollection)) {
                    WriteLine(3, LoadInitPublicField(property.Name, property.DataType));
                }

                foreach (ModelProperty property in item.PropertyList.Where(p => p.IsCollection)) {
                    WriteLine(3, LoadInitPrivateField(property.Name, "List<" + CodeUtils.LoadInnerDataType(property.DataType) + ">"));
                }

                WriteLine(2, "}");
            }

            WriteEmptyLine();
            WriteSummary(2, "Methode d'extensibilité possible pour les constructeurs.");
            WriteLine(2, "partial void OnCreated();");
            WriteEmptyLine();
            WriteSummary(2, "Methode d'extensibilité possible pour les constructeurs par recopie.");
            WriteParam(2, "bean", "Source.");
            WriteLine(2, "partial void OnCreated(" + item.Name + " bean);");
        }

        /// <summary>
        ///  Génère les attributs pour la gestion de notification de modification de propriété (PostSharp désactivé).
        /// </summary>
        /// <param name="item">La classe générée.</param>
        private void GenerateNotifyPropertyChangeField(ModelClass item) {
            if (item.ParentClass == null) {
                WriteEmptyLine();
                WriteSummary(2, "Stocke les informations concernant les modifications des champs de l'objet.");
                WriteLine(2, LoadBrowsableAttribute());
                WriteLine(2, LoadEditorBrowsableAttribute());
                WriteLine(2, "public event PropertyChangedEventHandler PropertyChanged;");
            }

            int propertyCount = 0;
            if (item.PropertyList.Count > 0) {
                foreach (ModelProperty property in item.PropertyList) {
                    if (property.IsCollection || !property.IsPrimitive) {
                        continue;
                    }

                    if (propertyCount == 0) {
                        WriteEmptyLine();
                    }

                    ++propertyCount;
                    var dataType = CodeUtils.LoadShortDataType(property.DataType);
                    if (property.IsPrimitive && CodeUtils.IsNonNullableCSharpBaseType(property.DataType)) {
                        dataType += "?";
                    }

                    WriteLine(2, LoadPrivateField(dataType, GetBackingFieldName(property)));
                }
            }
        }

        /// <summary>
        /// Génère la méthode OnPropertyChanged lorsque PostSharp est désactivé.
        /// </summary>
        private void GenerateOnPropertyChangeMethod() {
            WriteEmptyLine();
            WriteSummary(2, "Gestion du changement de l'état de la propriété.");
            WriteParam(2, "propertyName", "Nom de la propriété.");
            WriteLine(2, "protected void OnPropertyChanged(string propertyName) {");
            WriteLine(3, "if (this.PropertyChanged != null) {");
            WriteLine(4, "this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));");
            WriteLine(3, "}");
            WriteLine(2, "}");
        }

        /// <summary>
        /// Génère les propriétés.
        /// </summary>
        /// <param name="item">La classe générée.</param>
        private void GenerateProperties(ModelClass item) {
            if (item.PropertyList.Count > 0) {
                foreach (ModelProperty property in item.PersistentPropertyList.Where(prop => !prop.IsReprise)) {
                    WriteEmptyLine();
                    GenerateProperty(property);
                }

                foreach (ModelProperty propertyNonPersistent in item.NonPersistentPropertyList.Where(prop => !prop.IsReprise)) {
                    WriteEmptyLine();
                    GenerateProperty(propertyNonPersistent);
                }
            }
        }

        /// <summary>
        /// Génère la propriété concernée.
        /// </summary>
        /// <param name="property">La propriété générée.</param>
        private void GenerateProperty(ModelProperty property) {
            WriteSummary(2, property.Comment);

            if (!GeneratorParameters.IsSpa) {
                WriteLine(2, LoadDisplayAttribute(property.DataDescription.Libelle));
            }

            if (!property.Class.IsView && property.IsPersistent && property.DataMember != null) {
                WriteLine(2, LoadColumnAttribute(property));
            }

            if (property.DataMember.IsRequired && !property.DataDescription.IsPrimaryKey) {
                WriteLine(2, LoadRequiredAttribute());
            }

            if (property.DataDescription != null) {
                if (!string.IsNullOrEmpty(property.DataDescription.ReferenceType) && !property.DataDescription.ReferenceClass.IsExternal) {
                    string referencedType = property.DataDescription.ReferenceType;

                    WriteLine(2, LoadReferencedTypeAttribute(referencedType));
                }

                if (property.DataDescription.Domain != null) {
                    WriteLine(2, LoadDomainAttribute(property.DataDescription.Domain.Code));
                }
            }

            if (property.DataDescription.IsPrimaryKey /*&& (GeneratorParameters.IsPostSharpDisabled || GeneratorParameters.IsEntityFrameworkUsed)*/) {
                WriteLine(2, LoadKeyAttribute());
                if (property.IsIdManuallySet) {
                    WriteLine(2, LoadGeneratedNoneAttribute());
                }
            }

            if (GeneratorParameters.IsPostSharpDisabled) {
                WriteLine(2, LoadDataMemberAttribute(property));
                if (property.DataDescription.Domain != null) {
                    ModelDomain domain = property.DataDescription.Domain;
                    foreach (ValidationAttribute va in ((IDomainChecker)DomainManager.Instance.GetDomain(domain.Code)).ValidationAttributes) {
                        if (va != null) {
                            WriteLine(2, LoadValidationAttribute(va));
                        }
                    }
                }
            }

            if (property.IsPrimitive) {
                var dataType = CodeUtils.LoadShortDataType(property.DataType);
                if (CodeUtils.IsNonNullableCSharpBaseType(property.DataType)) {
                    dataType += "?";
                }

                WriteLine(2, LoadPublicPropertyCode(dataType, property.Name, GetBackingFieldName(property)));
            } else {
                if (property.IsCollection) {
                    WriteLine(2, LoadPublicReadOnlyPropertyCode(CodeUtils.LoadShortDataType(property.DataType), property.Name, CodeUtils.LoadPrivateFieldName(property.Name)));
                } else {
                    WriteLine(2, LoadPublicRequiredCompositionPropertyCode(property.DataType, property.Name));
                }
            }
        }

        /// <summary>
        /// Génère les ReferenceAccessor pour un namespace.
        /// </summary>
        /// <param name="nameSpace">Namespace.</param>
        /// <param name="projectName">Projet.</param>
        private void GenerateReferenceAccessor(ModelNamespace nameSpace, string projectName) {
            if (!nameSpace.HasPersistentClasses) {
                return;
            }

            ICollection<ModelClass> classList = nameSpace.ClassList
                .Where(x => x.DataContract.IsPersistent)
                .Where(x => x.Stereotype == Stereotype.Reference || x.Stereotype == Stereotype.Statique)
                .OrderBy(x => x.Name)
                .ToList();

            if (classList.Count == 0) {
                return;
            }

            string nameSpaceName = nameSpace.Name;
            GenerateReferenceAccessorsInterface(classList, projectName, nameSpaceName);

            classList = classList.Where(x => x.DisableAccessorImplementation == false).ToList();
            GenerateReferenceAccessorsImplementation(classList, projectName, nameSpaceName);
        }

        /// <summary>
        /// Génère les ReferenceAccessor.
        /// </summary>
        /// <param name="modelRootList">Liste de ModelRoot.</param>
        private void GenerateReferenceAccessors(ICollection<ModelRoot> modelRootList) {

            var enumerator = modelRootList.GetEnumerator();
            enumerator.MoveNext();
            var projectName = enumerator.Current.Name;
            var strippedProjectName = RemoveDots(projectName);

            foreach (ModelRoot model in modelRootList) {
                foreach (ModelNamespace ns in model.Namespaces.Values) {
                    GenerateReferenceAccessor(ns, strippedProjectName);
                }
            }
        }

        /// <summary>
        /// Génère l'implémentation des ReferenceAccessors.
        /// </summary>
        /// <param name="classList">Liste de ModelClass.</param>
        /// <param name="projectName">Nom du projet.</param>
        /// <param name="nameSpaceName">Namespace.</param>
        private void GenerateReferenceAccessorsImplementation(ICollection<ModelClass> classList, string projectName, string nameSpaceName) {
            var nameSpacePrefix = nameSpaceName.Replace("DataContract", string.Empty);
            string implementationName = "Service" + nameSpacePrefix + "Accessors";
            string interfaceName = 'I' + implementationName;
            string nameSpaceContract = nameSpacePrefix + "Contract";
            string nameSpaceImplementation = nameSpacePrefix + "Implementation";
            var moduleMetier = ExtractModuleMetier(nameSpaceName);
            var projectDir = Path.Combine(_outputDirectory, moduleMetier, projectName + "." + nameSpaceImplementation);
            var csprojFileName = Path.Combine(projectDir, projectName + "." + nameSpaceImplementation + ".csproj");
            Console.Out.WriteLine("Generating class " + implementationName + " implementing " + interfaceName);

            var implementationFileName = Path.Combine(projectDir, "generated", implementationName + ".cs");
            using (_currentWriter = new CsharpFileWriter(implementationFileName, csprojFileName)) {
                WriteLine(LoadUsing("System.Collections.Generic"));
                WriteLine(LoadUsing("System.Diagnostics.CodeAnalysis"));
                WriteLine(LoadUsing("System.ServiceModel"));
                WriteLine(LoadUsing(projectName + "." + nameSpaceContract));
                WriteLine(LoadUsing(projectName + "." + nameSpaceName));
                WriteLine(LoadUsing("Kinetix.Broker"));

                WriteEmptyLine();
                WriteLine(LoadBeginNamespace(projectName + "." + nameSpaceImplementation));
                WriteEmptyLine();

                WriteSummary(1, "This interface was automatically generated. It contains all the operations to load the reference lists declared in namespace " + nameSpaceName + ".");
                WriteLine(1, LoadSuppressMessageAttribute("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", "Contains " + classList.Count + " reference accessors."));

                WriteLine(1, LoadServiceBehaviorAttribute());
                WriteLine(1, LoadBeginClassDeclaration(implementationName, null, new List<string> { interfaceName }));

                foreach (ModelClass classe in classList) {
                    string serviceName = "Load" + classe.Name + "List";
                    WriteEmptyLine();
                    WriteLine(2, Loadinheritdoc(interfaceName, serviceName));
                    WriteLine(2, LoadBeginReferenceAccessorImplementation(classe.Name));
                    WriteLine(3, LoadReferenceAccessorBody(classe.Name, classe.DefaultOrderModelProperty));
                    WriteLine(2, "}");
                }

                WriteLine(1, "}");
                WriteLine("}");
            }
        }

        /// <summary>
        /// Génère l'interface déclarant les ReferenceAccessors d'un namespace.
        /// </summary>
        /// <param name="classList">Liste de ModelClass.</param>
        /// <param name="projectName">Nom du projet.</param>
        /// <param name="nameSpaceName">Namespace.</param>
        private void GenerateReferenceAccessorsInterface(ICollection<ModelClass> classList, string projectName, string nameSpaceName) {
            var nameSpacePrefix = nameSpaceName.Replace("DataContract", string.Empty);
            string interfaceName = "IService" + nameSpacePrefix + "Accessors";
            string nameSpaceContract = nameSpacePrefix + "Contract";

            Console.Out.WriteLine("Generating interface " + interfaceName + " containing reference accessors for namesSpace " + nameSpaceName);

            var projectDir = Path.Combine(GetDirectoryForProject(false, projectName, nameSpaceContract));
            var csprojFileName = Path.Combine(projectDir, projectName + "." + nameSpaceContract + ".csproj");
            var interfaceFileName = Path.Combine(projectDir, "generated", interfaceName + ".cs");

            using (_currentWriter = new CsharpFileWriter(interfaceFileName, csprojFileName)) {
                WriteLine(LoadUsing("System.Collections.Generic"));
                WriteLine(LoadUsing("System.Diagnostics.CodeAnalysis"));
                WriteLine(LoadUsing("System.ServiceModel"));
                WriteLine(LoadUsing(projectName + "." + nameSpaceName));
                WriteLine(LoadUsing("Kinetix.ServiceModel"));

                WriteEmptyLine();
                WriteLine(LoadBeginNamespace(projectName + "." + nameSpaceContract));
                WriteEmptyLine();
                WriteSummary(1, "This interface was automatically generated. It contains all the operations to load the reference lists declared in namespace " + nameSpaceName + ".");
                WriteLine(1, LoadSuppressMessageAttribute("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", "Contains " + classList.Count + " reference accessors."));
                WriteLine(1, LoadServiceContractAttribute());
                WriteLine(1, LoadBeginInterfaceDeclaration(interfaceName));

                foreach (ModelClass classe in classList) {
                    WriteEmptyLine();
                    WriteSummary(2, "Reference accessor for type " + classe.Name);
                    WriteReturns(2, "List of " + classe.Name);
                    WriteLine(2, LoadReferenceAccessorAttribute());
                    WriteLine(2, LoadOperationContractAttribute());
                    WriteLine(2, LoadReferenceAccessorDeclaration(classe.Name));
                }

                WriteLine(1, "}");
                WriteLine("}");
            }
        }

        /// <summary>
        /// Génère la méthode State lorsque PostSharp est désactivé.
        /// </summary>
        private void GenerateStateMethod() {
            WriteEmptyLine();
            WriteSummary(2, "Gestion du changement de l'état de l'objet.");
            WriteLine(2, "public ChangeAction State {");
            WriteLine(3, "get;");
            WriteLine(3, "set;");
            WriteLine(2, "}");
        }

        /// <summary>
        /// Génération des imports.
        /// </summary>
        /// <param name="item">Classe concernée.</param>
        private void GenerateUsing(ModelClass item) {
            WriteLine(LoadUsing("System"));
            if (item.HasCollection || (item.ConstValues != null && item.ConstValues.Count > 0)) {
                WriteLine(LoadUsing("System.Collections.Generic"));
            }
            if (!string.IsNullOrEmpty(item.DefaultProperty)
                || GeneratorParameters.IsNotifyPropertyChangeEnabled
                || item.Stereotype == Stereotype.Reference
                || item.Stereotype == Stereotype.Statique) {
                WriteLine(LoadUsing("System.ComponentModel"));
            }

            if (item.PropertyList.Count > 0) {
                WriteLine(LoadUsing("System.ComponentModel.DataAnnotations"));
                WriteLine(LoadUsing("System.ComponentModel.DataAnnotations.Schema"));
            }

            if (GeneratorParameters.IsPostSharpDisabled && item.DataContract.IsPersistent) {
                WriteLine(this.LoadUsing("System.Data.Linq"));
            }

            if (GeneratorParameters.IsPostSharpDisabled) {
                WriteLine(LoadUsing("System.Runtime.Serialization"));
            }

            // Pas besoin du using du DataContract si inutilisé
            var usingList = new List<string>();
            foreach (string value in item.UsingList) {
                usingList.Add(value);
            }

            if (item.HasDomainAttribute || (GeneratorParameters.IsPostSharpDisabled && item.ParentClass == null)) {
                usingList.Add("Kinetix.ComponentModel");
            }

            if (GeneratorParameters.IsPostSharpDisabled &&
                (item.HasDomain("DO_DATE") || item.HasDomain("DO_TIMESTAMP") || item.HasDomain("DO_EMAIL"))) {
                usingList.Add("Kinetix.ComponentModel.DataAnnotations");
            }

            usingList.Sort();
            foreach (string import in usingList) {
                WriteLine(LoadUsing(import));
            }
        }

        /// <summary>
        /// Retourne le nom du répertoire dans lequel placer la classe générée à partir du ModelClass fourni.
        /// </summary>
        /// <param name="isPersistent">Trie s'il s'agit du domaine persistant.</param>
        /// <param name="projectName">Nom du projet.</param>
        /// <param name="nameSpace">Namespace de la classe.</param>
        /// <returns>Emplacement dans lequel placer la classe générée à partir du ModelClass fourni.</returns>
        private string GetDirectoryForModelClass(bool isPersistent, string projectName, string nameSpace) {
            var moduleMetier = ExtractModuleMetier(nameSpace);
            var basePath = _outputDirectory;
            var localPath = Path.Combine(moduleMetier, projectName + "." + nameSpace);
            string path = isPersistent ? Path.Combine(basePath, localPath) : Path.Combine(basePath, localPath, "Dto");
            return Path.Combine(path, "generated");
        }

        /// <summary>
        /// Retourne le nom du répertoire du projet d'une classe.
        /// </summary>
        /// <param name="isPersistent">Trie s'il s'agit du domaine persistant.</param>
        /// <param name="projectName">Nom du projet.</param>
        /// <param name="nameSpace">Namespace de la classe.</param>
        /// <returns>Nom du répertoire contenant le csproj.</returns>
        private string GetDirectoryForProject(bool isPersistent, string projectName, string nameSpace) {
            var moduleMetier = ExtractModuleMetier(nameSpace);
            var basePath = _outputDirectory;
            var localPath = Path.Combine(moduleMetier, projectName + "." + nameSpace);
            return Path.Combine(basePath, localPath);
        }


        /// <summary>
        /// Retourne le sous-répertoire de Business.
        /// </summary>
        /// <param name="projectName">Nom du projet.</param>
        /// <returns>Sous-répertoire de Business.</returns>
        private string GetBusinessSubdirectoryName(string projectName) {
            return Path.Combine(_outputDirectory, projectName);
        }

        /// <summary>
        /// Retourne le répertoire dans lequel générer les contrats.
        /// </summary>
        /// <param name="projectName">Nom du projet.</param>
        /// <returns>Répertoire dans lequel générer les contrats.</returns>
        private string GetContractDirectoryName(string projectName) {
            return Path.Combine(GetBusinessSubdirectoryName(projectName), "Contract");
        }

        /// <summary>
        /// Retourne le répertoire dans lequel générer les objets persistants.
        /// </summary>
        /// <param name="projectName">Nom du projet.</param>
        /// <returns>Répertoire dans lequel générer les objets persistants.</returns>
        private string GetDataContractDirectoryName(string projectName) {
            return Path.Combine(GetBusinessSubdirectoryName(projectName), "DataContract");
        }

        /// <summary>
        /// Retourne l'emplacement du fichier généré à partir du ModelClass fourni.
        /// </summary>
        /// <param name="modelClass">ModelClass.</param>
        /// <param name="projectName">Nom du projet.</param>
        /// <returns>Emplacement du fichier généré à partir du ModelClass fourni.</returns>
        private string GetFilenameFromModelClass(ModelClass modelClass, string projectName) {
            return Path.Combine(GetDirectoryForModelClass(modelClass.DataContract.IsPersistent, projectName, modelClass.Namespace.Name), modelClass.Name + "." + FileExtension);
        }

        /// <summary>
        /// Retourne le répertoire d'implémentation des contrats.
        /// </summary>
        /// <param name="projectName">Nom du projet.</param>
        /// <returns>Répertoire dans lequel générer les objets persistants.</returns>
        private string GetImplementationDirectoryName(string projectName) {
            return Path.Combine(GetBusinessSubdirectoryName(projectName), "Implementation");
        }

        private string Pluralize(string className) {
            return className.EndsWith("s") ? className : className + "s";
        }

        /// <summary>
        /// Génère le constructeur par recopie d'un type base.
        /// </summary>
        /// <param name="item">Classe générée.</param>
        private void WriteBaseCopyConstructor(ModelClass item) {
            WriteEmptyLine();
            WriteSummary(2, "Constructeur par base class.");
            WriteParam(2, "bean", "Source.");
            WriteLine(2, "public " + item.Name + "(" + item.ParentClass.Name + " bean)");
            WriteLine(3, " : base(bean) {");
            if (item.NeedsInitialization) {
                WriteLine(3, "this.Initialize();");
            }

            WriteLine(3, "this.OnCreated();");
            WriteLine(2, LoadEndConstructor());
        }

        /// <summary>
        /// Génère le constructeur par recopie.
        /// </summary>
        /// <param name="item">Classe générée.</param>
        private void WriteCopyConstructor(ModelClass item) {
            WriteEmptyLine();
            WriteSummary(2, "Constructeur par recopie.");
            WriteParam(2, "bean", "Source.");
            if (item.ParentClass != null) {
                WriteLine(2, "public " + item.Name + "(" + item.Name + " bean)");
                WriteLine(3, " : base(bean) {");
            } else {
                WriteLine(2, "public " + item.Name + "(" + item.Name + " bean) {");
            }

            WriteLine(3, "if (bean == null) {");
            WriteLine(4, "throw new ArgumentNullException(nameof(bean));");
            WriteLine(3, "}");
            WriteLine(0, string.Empty);
            foreach (ModelProperty property in item.PropertyList.Where(p => !p.IsPrimitive && !p.IsCollection)) {
                WriteLine(3, "this." + property.Name + " = new " + property.DataType + "(bean." + property.Name + ");");
            }

            foreach (ModelProperty property in item.PropertyList.Where(p => p.IsCollection)) {
                WriteLine(3, "this." + property.Name + " = new List<" + CodeUtils.LoadInnerDataType(property.DataType) + ">(bean." + property.Name + ");");
            }

            foreach (ModelProperty property in item.PropertyList.Where(p => p.IsPrimitive)) {
                WriteLine(3, "this." + property.Name + " = bean." + property.Name + ";");
            }

            WriteLine(3, "this.OnCreated(bean);");
            WriteLine(2, LoadEndConstructor());
        }

        /// <summary>
        /// Génère le constructeur par défaut.
        /// </summary>
        /// <param name="item">Classe générée.</param>
        private void WriteDefaultConstructor(ModelClass item) {
            WriteEmptyLine();
            WriteSummary(2, "Constructeur.");
            if (item.ParentClass == null) {
                WriteLine(2, LoadBeginConstructor(item.Name));
            } else {
                WriteLine(2, "public " + item.Name + "() ");
                WriteLine(3, " : base() {");
            }

            if (item.NeedsInitialization) {
                WriteLine(3, "this.Initialize();");
            }

            WriteLine(3, "this.OnCreated();");
            WriteLine(2, LoadEndConstructor());
        }

        /// <summary>
        /// Ecrit une ligne vide.
        /// </summary>
        private void WriteEmptyLine() {
            WriteLine(string.Empty);
        }

        /// <summary>
        /// Ecrit la chaine de caractère dans le flux.
        /// </summary>
        /// <param name="value">Valeur à écrire dans le flux.</param>
        private void WriteLine(string value) {
            WriteLine(0, value);
        }

        /// <summary>
        /// Ecrit la chaine avec le niveau indenté.
        /// </summary>
        /// <param name="indentationLevel">Niveau d'indentation.</param>
        /// <param name="value">Valeur à écrire dans le flux.</param>
        private void WriteLine(int indentationLevel, string value) {
            string indentValue = string.Empty;
            for (int i = 0; i < indentationLevel; ++i) {
                indentValue += "    ";
            }

            value = value.Replace("\r\n", "\r\n" + indentValue);
            _currentWriter.WriteLine(indentValue + value);
        }

        /// <summary>
        /// Ecrit  le commentaire de parametre.
        /// </summary>
        /// <param name="indentationLevel">Niveau d'indention.</param>
        /// <param name="paramName">Nom du paramètre.</param>
        /// <param name="value">Valeur du paramètre.</param>
        private void WriteParam(int indentationLevel, string paramName, string value) {
            if (!string.IsNullOrEmpty(paramName) && !string.IsNullOrEmpty(value)) {
                WriteLine(indentationLevel, LoadParam(paramName, value));
            }
        }

        /// <summary>
        /// Ecrit  le commentaire de returns.
        /// </summary>
        /// <param name="indentationLevel">Niveau d'indention.</param>
        /// <param name="value">Description du returns.</param>
        private void WriteReturns(int indentationLevel, string value) {
            if (!string.IsNullOrEmpty(value)) {
                WriteLine(indentationLevel, LoadReturns(value));
            }
        }

        /// <summary>
        /// Ecrit la valeur du résumé du commentaire..
        /// </summary>
        /// <param name="indentationLevel">Niveau d'indentation.</param>
        /// <param name="value">Valeur à écrire.</param>
        private void WriteSummary(int indentationLevel, string value) {
            if (!string.IsNullOrEmpty(value)) {
                WriteLine(indentationLevel, LoadSummary(value));
            }
        }
    }
}
