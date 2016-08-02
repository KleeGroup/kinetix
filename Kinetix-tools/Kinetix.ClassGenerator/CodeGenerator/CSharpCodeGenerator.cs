using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using Kinetix.ClassGenerator.CodeGenerator;
using Kinetix.ClassGenerator.Model;
using Kinetix.ComponentModel.DataAnnotations;

namespace Kinetix.ClassGenerator {

    /// <summary>
    /// Générateur de code C#.
    /// </summary>
    internal class CSharpCodeGenerator : AbstractCodeGenerator {

        private const string IndentValue = "    ";

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="directory">Répertoire de destination.</param>
        public CSharpCodeGenerator(string directory)
            : base(directory) {
        }

        /// <summary>
        /// Obtient ou définit l'extension des fichiers associés.
        /// </summary>
        public override string FileExtension {
            get {
                return "cs";
            }
        }

        /// <summary>
        /// Obtient la valeur de l'attribut permettant de spécifier que la classe est sérialisable.
        /// </summary>
        public override string SerializableAttribute {
            get {
                return "[Serializable]";
            }
        }

        /// <summary>
        /// Obtient la valeur de la chaine de caractère précisant que la ligne est en commentaire.
        /// </summary>
        public override string Comment {
            get {
                return "//";
            }
        }

        /// <summary>
        /// Retourne le code associé à la déclaration d'une propriété publique.
        /// </summary>
        /// <param name="propertyType">Type de la propriété.</param>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <param name="propertyBackingField">Dans le cas de PostSharp désactivé, on passe la variable qui stoque la valeur du champ.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadPublicPropertyCode(string propertyType, string propertyName, string propertyBackingField) {

            StringBuilder sb = new StringBuilder("public ");
            sb.Append(propertyType);
            sb.Append(' ');
            sb.Append(propertyName);
            sb.Append(" {\r\n");
            sb.Append(IndentValue);
            if (propertyBackingField != null) {
                sb.Append("get {\r\n");
                sb.Append(IndentValue);
                sb.Append(IndentValue);
                sb.Append("return ");
                sb.Append(propertyBackingField);
                sb.Append(";\r\n");
                sb.Append(IndentValue);
                sb.Append("}\r\n");
                sb.Append(IndentValue);
                sb.Append("set {\r\n");
                sb.Append(IndentValue);
                sb.Append(IndentValue);
                sb.Append("if (value != ");
                sb.Append(propertyBackingField);
                sb.Append(") {\r\n");
                sb.Append(IndentValue);
                sb.Append(IndentValue);
                sb.Append(IndentValue);
                sb.Append(propertyBackingField);
                sb.Append(" = value;\r\n");
                sb.Append(IndentValue);
                sb.Append(IndentValue);
                sb.Append(IndentValue);
                sb.Append("OnPropertyChanged(\"");
                sb.Append(propertyName);
                sb.Append("\");\r\n");
                sb.Append(IndentValue);
                sb.Append(IndentValue);
                sb.Append("}\r\n");
                sb.Append(IndentValue);
                sb.Append("}\r\n");
                sb.Append("}");
            } else {
                sb.Append("get;\r\n    set;\r\n}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Retourne le code associé à la déclaration d'une propriété obligatoire et publique issue d'une composition.
        /// </summary>
        /// <param name="propertyType">Type de la propriété.</param>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadPublicRequiredCompositionPropertyCode(string propertyType, string propertyName) {
            return "public " + propertyType + " " + propertyName + " {\r\n    get;\r\n    private set;\r\n}";
        }

        /// <summary>
        /// Retourne le code associé à la déclaration d'ouverture d'un constructeur.
        /// </summary>
        /// <param name="type">Type de l'objet.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadBeginConstructor(string type) {
            return "public " + type + "() {";
        }

        /// <summary>
        /// Retourne le code associé à la fin de déclaration d'un constructeur.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadEndConstructor() {
            return "}";
        }

        /// <summary>
        /// Retourne le code associé à la déclaration d'une interface.
        /// </summary>
        /// <param name="name">Nom de l'interface.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadBeginInterfaceDeclaration(string name) {
            return "public partial interface " + name + " {";
        }

        /// <summary>
        /// Retourne le code associé à la déclaration d'un namespace.
        /// </summary>
        /// <param name="value">Valeur du namespace.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadBeginNamespace(string value) {
            return "namespace " + value + " {";
        }

        /// <summary>
        /// Retourne le code associé à la fin de déclaration de namespace.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadEndNamespace() {
            return "}";
        }

        /// <summary>
        /// Retourne le code associé à la déclaration.
        /// </summary>
        /// <param name="name">Nom de la classe.</param>
        /// <param name="inheritedClass">Classe parente.</param>
        /// <param name="ifList">Liste des interfaces implémentées.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadBeginClassDeclaration(string name, string inheritedClass, ICollection<string> ifList) {
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name");
            }

            if (ifList == null) {
                throw new ArgumentNullException("ifList");
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("public partial class ");
            sb.Append(name);
            if (!string.IsNullOrEmpty(inheritedClass) || (ifList != null && ifList.Count > 0)) {
                sb.Append(" : ");
                if (!string.IsNullOrEmpty(inheritedClass)) {
                    sb.Append(inheritedClass);
                    if (ifList.Count > 0) {
                        sb.Append(", ");
                    }
                }

                if (ifList.Count > 0) {
                    IEnumerator<string> enumerator = ifList.GetEnumerator();
                    for (int i = 0; i < ifList.Count; ++i) {
                        if (!enumerator.MoveNext()) {
                            throw new NotSupportedException();
                        }

                        sb.Append(enumerator.Current);
                        if (i < ifList.Count - 1) {
                            sb.Append(", ");
                        }
                    }
                }
            }

            sb.Append(" {");
            return sb.ToString();
        }

        /// <summary>
        /// Retourne le code associé à la fin de déclaration d'une classe.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadEndClassDeclaration() {
            return "}";
        }

        /// <summary>
        /// Retourne le code associé à la déclaration d'un Using.
        /// </summary>
        /// <param name="nsName">Nom de la classe/namespace à importer.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadUsing(string nsName) {
            if (string.IsNullOrEmpty("nsName")) {
                throw new ArgumentNullException("nsName");
            }

            return "using " + nsName + ";";
        }

        /// <summary>
        /// Retourne l'attribut DataMember.
        /// </summary>
        /// <param name="modelProperty">La propriété liée.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadDataMemberAttribute(ModelProperty modelProperty) {
            if (modelProperty == null) {
                throw new ArgumentNullException("modelProperty");
            }

            if (modelProperty.DataMember.IsRequired) {
                return "[DataMember(IsRequired = true)]";
            }

            return "[DataMember]";
        }

        /// <summary>
        /// Retourne l'attribut Column.
        /// </summary>
        /// <param name="property">Propriété du modèle.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadColumnAttribute(ModelProperty property) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }

            return "[Column(\"" + property.DataMember.Name + "\")]";
        }

        /// <summary>
        /// Retourne l'attribut DatabaseGenerated.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadDatabaseGeneratedAttribute() {
            return "[DatabaseGenerated(DatabaseGeneratedOption.Identity)]";
        }

        /// <summary>
        /// Retourne l'attribut Key.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadKeyAttribute() {
            return "[Key]";
        }

        /// <summary>
        /// Retourne l'attribut OperationContract.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadOperationContractAttribute() {
            return "[OperationContract]";
        }

        /// <summary>
        /// Retourne l'attribut ReferenceAccessor.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadReferenceAccessorAttribute() {
            return "[ReferenceAccessor]";
        }

        /// <summary>
        /// Retourne le code associé au cors de l'implémentation d'un service de type ReferenceAccessor.
        /// </summary>
        /// <param name="className">Nom du type chargé par le ReferenceAccessor.</param>
        /// <param name="defaultProperty">Propriété par defaut de la classe.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadReferenceAccessorBody(string className, ModelProperty defaultProperty) {
            string queryParameter = string.Empty;
            if (defaultProperty != null) {
                queryParameter = "new Kinetix.Data.SqlClient.QueryParameter(" + className + ".Cols." + defaultProperty.DataMember.Name + ", Kinetix.Data.SqlClient.SortOrder.Asc)";
            }

            return "return BrokerManager.GetBroker<" + className + ">().GetAll(" + queryParameter + ");";
        }

        /// <summary>
        /// Retourne le code de déclaration d'un service de type ReferenceAccessor.
        /// </summary>
        /// <param name="className">Nom du type de l'entité chargée par le ReferenceAccessor.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadReferenceAccessorDeclaration(string className) {
            return "ICollection<" + className + "> Load" + className + "List();";
        }

        /// <summary>
        /// Retourne l'attribut ReferenceAccessor.
        /// </summary>
        /// <param name="isStatic"><code>True</code> si la liste est statique, <code>False</code> sinon.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadReferenceAttribute(bool isStatic) {
            return isStatic ? "[Reference(true)]" : "[Reference]";
        }

        /// <summary>
        /// Retourne l'attribut DefaultProperty.
        /// </summary>
        /// <param name="defaultProperty">La propriété DefaultProperty.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadDefaultPropertyAttribute(string defaultProperty) {
            return "[DefaultProperty(\"" + defaultProperty + "\")]";
        }

        /// <summary>
        /// Retourne l'attribut Translatable.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadTranslatableAttribute() {
            return "[Translatable]";
        }

        /// <summary>
        /// Retourne l'attribut Display.
        /// </summary>
        /// <param name="displayName">Valeur de l'attribut Name de Display.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadDisplayAttribute(string displayName) {
            return "[Display(Name = \"" + displayName + "\")]";
        }

        /// <summary>
        /// Retourne l'attribut DataContract.
        /// </summary>
        /// <param name="dataContract">Données du DataContract.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadDataContractAttribute(ModelDataContract dataContract) {
            if (dataContract == null) {
                throw new ArgumentNullException("dataContract");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("[DataContract(");
            sb.Append("Namespace = \"http://" + dataContract.Namespace.Replace('.', '/') + "/\"");
            sb.Append(")]");

            return sb.ToString();
        }

        /// <summary>
        /// Retourne l'attribut Table.
        /// </summary>
        /// <param name="dataContract">Données du DataContract.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadTableAttribute(ModelDataContract dataContract) {
            if (dataContract == null) {
                throw new ArgumentNullException("dataContract");
            }

            return "[Table(\"" + dataContract.Name + "\")]";
        }

        /// <summary>
        /// Retourne le commentaire du summary formatté.
        /// </summary>
        /// <param name="summary">Contenu du commentaire.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadSummary(string summary) {
            if (string.IsNullOrEmpty(summary)) {
                throw new ArgumentNullException("summary");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("/// <summary>\r\n");
            sb.Append("/// " + summary.Replace("\r\n", "\r\n/// "));
            if (!summary.EndsWith(".", StringComparison.OrdinalIgnoreCase)) {
                sb.Append('.');
            }

            sb.Append("\r\n/// </summary>");
            return sb.ToString();
        }

        /// <summary>
        /// Retourne le commentaire du param formatté.
        /// </summary>
        /// <param name="paramName">Nom du paramètre.</param>
        /// <param name="value">Description du paramètre.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadParam(string paramName, string value) {
            if (string.IsNullOrEmpty(paramName)) {
                throw new ArgumentNullException("paramName");
            }

            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentNullException("value");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("/// <param name=\"");
            sb.Append(paramName);
            sb.Append("\">");
            sb.Append(value);
            if (!value.EndsWith(".", StringComparison.OrdinalIgnoreCase)) {
                sb.Append('.');
            }

            sb.Append("</param>");
            return sb.ToString();
        }

        /// <summary>
        /// Retourne le commentaire inheritdoc.
        /// </summary>
        /// <param name="interfaceName">Nom de l'interface où se trouve le service dont on hérite de la documentation.</param>
        /// <param name="serviceName">Nom du service dont on hérite de la documentation.</param>
        /// <returns>Code généré.</returns>
        protected override string Loadinheritdoc(string interfaceName, string serviceName) {
            return "/// <inheritdoc cref=\"" + interfaceName + "." + serviceName + "\"/>";
        }

        /// <summary>
        /// Retourne le code associé à l'instanciation d'un membre privé.
        /// </summary>
        /// <param name="fieldName">Nom de la variable membre privée.</param>
        /// <param name="dataType">Type de données.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadInitPrivateField(string fieldName, string dataType) {
            string res = "this." + fieldName + " = ";
            if (CodeUtils.IsCSharpBaseType(dataType)) {
                res += CodeUtils.GetCSharpDefaultValueBaseType(dataType) + ";";
            } else {
                res += "new " + dataType + "();";
            }

            return res;
        }

        /// <summary>
        /// Retourne le code associé à l'instanciation d'un membre public.
        /// </summary>
        /// <param name="fieldName">Nom de la variable membre privée.</param>
        /// <param name="dataType">Type de données.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadInitPublicField(string fieldName, string dataType) {
            string res = "this." + fieldName + " = ";
            if (CodeUtils.IsCSharpBaseType(dataType)) {
                res += CodeUtils.GetCSharpDefaultValueBaseType(dataType) + ";";
            } else {
                res += "new " + dataType + "();";
            }

            return res;
        }

        /// <summary>
        /// Déclare une variable membre privée.
        /// </summary>
        /// <param name="name">Nom de la variable.</param>
        /// <param name="dataType">Type de données.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadDeclarePrivateField(string name, string dataType) {
            return "private readonly " + dataType + " _" + name + ";";
        }

        /// <summary>
        /// Retourne le code associé à la déclaration d'une propriété public read-only.
        /// </summary>
        /// <param name="propertyType">Type de la propriété.</param>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <param name="privateFieldName">Nom de la variable membre pré-initialisée dans le constructeur.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadPublicReadOnlyPropertyCode(string propertyType, string propertyName, string privateFieldName) {
            return "public " + propertyType + " " + propertyName + " {\r\n    get;\r\n    private set;\r\n}";
        }

        /// <summary>
        /// Retourne le code associé à l'initialisation d'une constante statique.
        /// </summary>
        /// <param name="fieldName">Nom de la constante.</param>
        /// <param name="dataType">Type de la constante.</param>
        /// <param name="value">Valeur de la constante.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadPublicConstField(string fieldName, string dataType, string value) {
            return "public const " + dataType + " " + fieldName + " = " + value + ";";
        }

        /// <summary>
        /// Retourne l'attribut TypeDescriptionProvider pour la classe.
        /// </summary>
        /// <param name="classe">La classe en question.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadTypeDescriptionProviderAttribute(ModelClass classe) {
            if (classe == null) {
                throw new ArgumentNullException("classe");
            }

            return "[TypeDescriptionProvider(typeof(BeanTypeDescriptionProvider<" + classe.Name + ">))]";
        }

        /// <summary>
        /// Retourne le code de déclaration d'un enum.
        /// </summary>
        /// <param name="enumName">Nom de l'enum.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadBeginPublicEnum(string enumName) {
            return "public enum " + enumName + " {";
        }

        /// <summary>
        /// Retourne le code associé au début de l'implémentation d'un service de type ReferenceAccessor.
        /// </summary>
        /// <param name="className">Nom du type chargé par le ReferenceAccessor.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadBeginReferenceAccessorImplementation(string className) {
            return "public ICollection<" + className + "> Load" + className + "List() {";
        }

        /// <summary>
        /// Retourne le code généré pour la fin d'un enum.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadEndEnum() {
            return "}";
        }

        /// <summary>
        /// Retourne le code de déclaration d'un item.
        /// </summary>
        /// <param name="enumValue">Nom de l'enum.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadEnumItem(string enumValue) {
            return enumValue + ",";
        }

        /// <summary>
        /// Génère l'attribut de validation issu du domaine.
        /// </summary>
        /// <param name="validationAttribute">Attribut de validation.</param>
        /// <returns>Attribut de validation formatté.</returns>
        protected override string LoadValidationAttribute(ValidationAttribute validationAttribute) {
            if (validationAttribute == null) {
                throw new ArgumentNullException("validationAttribute");
            }

            RangeAttribute rangeAttr = validationAttribute as RangeAttribute;
            if (rangeAttr != null) {
                return "[Range(" + rangeAttr.Minimum.ToString() + ", " + rangeAttr.Maximum.ToString() + ")]";
            }

            EmailAttribute emailAttr = validationAttribute as EmailAttribute;
            if (emailAttr != null) {
                return "[Email(" + emailAttr.MaximumLength + ")]";
            }

            StringLengthAttribute strLenAttr = validationAttribute as StringLengthAttribute;
            if (strLenAttr != null) {
                StringBuilder sb = new StringBuilder("[StringLength(");
                sb.Append(strLenAttr.MaximumLength);
                if (strLenAttr.MinimumLength > 0) {
                    sb.Append(" , ");
                    sb.Append("MinimumLength = ");
                    sb.Append(strLenAttr.MinimumLength);
                }

                sb.Append(")]");
                return sb.ToString();
            }

            RegularExpressionAttribute regexAttr = validationAttribute as RegularExpressionAttribute;
            if (regexAttr != null) {
                return "[RegularExpression(\"" + regexAttr.Pattern + "\")]";
            }

            DateAttribute dateAttr = validationAttribute as DateAttribute;
            if (dateAttr != null) {
                return string.Format(CultureInfo.InvariantCulture, "[Date({0})]", dateAttr.Precision);
            }

            NumeroSiretAttribute siretAttr = validationAttribute as NumeroSiretAttribute;
            if (siretAttr != null) {
                return "[NumeroSiret]";
            }

            throw new NotSupportedException("Recopie d'attribut de validation par aspect non géré pour : " + validationAttribute.GetType().ToString());
        }

        /// <summary>
        /// Retourne l'attribut Required.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadRequiredAttribute() {
            return "[Required]";
        }

        /// <summary>
        /// Retourne le commentaire du returns formatté.
        /// </summary>
        /// <param name="value">Description de la valeur retournée.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadReturns(string value) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentNullException("value");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("/// <returns>");
            sb.Append(value);
            if (!value.EndsWith(".", StringComparison.OrdinalIgnoreCase)) {
                sb.Append('.');
            }

            sb.Append("</returns>");
            return sb.ToString();
        }

        /// <summary>
        /// Charge l'attribut ServiceBehavior.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadServiceBehaviorAttribute() {
            return "[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]";
        }

        /// <summary>
        /// Charge l'attribut ServiceContract.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadServiceContractAttribute() {
            return "[ServiceContract]";
        }

        /// <summary>
        /// Charge l'attribut SuppressMessageAttribute.
        /// </summary>
        /// <param name="category">Categorie.</param>
        /// <param name="checkId">Identifiant.</param>
        /// <param name="justification">Justification.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadSuppressMessageAttribute(string category, string checkId, string justification) {
            return string.Format(CultureInfo.InvariantCulture, "[SuppressMessage(\"{0}\", \"{1}\", Justification = \"{2}\")]", category, checkId, justification);
        }

        /// <summary>
        /// Retourne l'attribut Range.
        /// </summary>
        /// <param name="min">Valeur minimale.</param>
        /// <param name="max">Valeur maximale.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadRangeAttribute(int min, int max) {
            return "[Range(" + min + ", " + max + ")]";
        }

        /// <summary>
        /// Retourne un constructeur privé.
        /// </summary>
        /// <param name="className">Nom de la classe.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadPrivateConstructor(string className) {
            return "private " + className + "() { }";
        }

        /// <summary>
        /// Retourne un champ privé.
        /// </summary>
        /// <param name="fieldType">Type du champ.</param>
        /// <param name="fieldName">Nom du champ.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadPrivateField(string fieldType, string fieldName) {
            return "private " + fieldType + ' ' + fieldName + ";";
        }

        /// <summary>
        /// Retourne l'attribut Browsable.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadBrowsableAttribute() {
            return "[Browsable(false)]";
        }

        /// <summary>
        /// Retourne l'attribut EditorBrowsable.
        /// </summary>
        /// <returns>Code généré.</returns>
        protected override string LoadEditorBrowsableAttribute() {
            return "[EditorBrowsable(EditorBrowsableState.Never)]";
        }

        /// <summary>
        /// Retourne l'attribut Domain.
        /// </summary>
        /// <param name="domainCode">Nom du domaine.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadDomainAttribute(string domainCode) {
            return "[Domain(\"" + domainCode + "\")]";
        }

        /// <summary>
        /// Retourne l'attribut Association.
        /// </summary>
        /// <param name="typeName">Type cible.</param>
        /// <returns>Code généré.</returns>
        protected override string LoadReferencedTypeAttribute(string typeName) {
            return "[ReferencedType(typeof(" + typeName + "))]";
        }
    }
}
