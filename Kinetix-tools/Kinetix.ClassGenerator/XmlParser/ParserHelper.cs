using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Kinetix.ClassGenerator.Model;

namespace Kinetix.ClassGenerator.XmlParser {

    /// <summary>
    /// Classe abstraite définissant le comportement des helper pour le parsing de fichiers.
    /// </summary>
    internal class ParserHelper {

#pragma warning disable SA1401, SA1600

        protected static ParserHelper _instance = new ParserHelper();
        protected IDictionary<string, string> _conversionMap;

#pragma warning restore SA1401, SA1600

        /// <summary>
        /// Constructeur static.
        /// </summary>
        protected ParserHelper() {
            _conversionMap = new Dictionary<string, string>();
            _conversionMap.Add("&lt;", "<");
            _conversionMap.Add("&gt;", ">");
            _conversionMap.Add("é", "e");
            _conversionMap.Add("è", "e");
            _conversionMap.Add("ê", "e");
            _conversionMap.Add("a", "a");
            _conversionMap.Add("â", "a");
            _conversionMap.Add("ô", "o");
        }

        /// <summary>
        /// Convertit les caractères spéciaux de la chaine en paramètre.
        /// </summary>
        /// <param name="chaine">La chaine à convertir.</param>
        /// <returns>La chaine convertie.</returns>
        internal static string Convert(string chaine) {
            return _instance.InternalConvert(chaine);
        }

        /// <summary>
        /// Génère le nom de la table SQL à partir d'un nom de classe avec la syntaxe C#.
        /// </summary>
        /// <param name="className">Nom de la classe C#.</param>
        /// <param name="isView">True si la table est en fait une vue.</param>
        /// <returns>Nom de la table SQL.</returns>
        internal static string GetSqlTableNameFromClassName(string className, bool isView) {
            return (isView ? "V_" : "T_") + className;
        }

        /// <summary>
        /// Convertit un nom avec la syntaxe C#.
        /// </summary>
        /// <param name="name">Nom au format C#.</param>
        /// <returns>Nom base de données.</returns>
        internal static string ConvertCsharp2Bdd(string name) {
            StringBuilder sb = new StringBuilder();
            char[] c = name.ToCharArray();
            bool lastIsUp = true;
            bool anteLastIsUp = false;
            for (int i = 0; i < c.Length; ++i) {
                string upperChar = new string(c[i], 1).ToUpper(CultureInfo.CurrentCulture);
                if (i > 0) {
                    bool isLastCaracter = i == c.Length - 1;
                    bool nextIsMinus = !isLastCaracter && !new string(c[i + 1], 1).ToUpper(CultureInfo.CurrentCulture).Equals(new string(c[i + 1], 1));

                    if (upperChar.Equals(new string(c[i], 1))) {
                        if ((!lastIsUp || anteLastIsUp) ||
                            (!lastIsUp && isLastCaracter) ||
                            (lastIsUp && nextIsMinus)) {
                            sb.Append('_');
                            anteLastIsUp = false;
                            lastIsUp = true;
                        } else {
                            anteLastIsUp = lastIsUp;
                            lastIsUp = true;
                        }
                    } else {
                        anteLastIsUp = lastIsUp;
                        lastIsUp = false;
                    }
                }

                sb.Append(upperChar);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Créé la propriété référencant la clé étrangère d'une classe source.
        /// </summary>
        /// <param name="sourceClass">La classe source.</param>
        /// <param name="targetClass">La classe cible portant la propriété.</param>
        /// <param name="multiplicity">La multiplicité de l'association.</param>
        /// <param name="role">Le rôle de l'association.</param>
        /// <param name="name">Nom de l'association.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>La propriété relative à l'association.</returns>
        internal static ModelProperty BuildClassAssociationProperty(ModelClass sourceClass, ModelClass targetClass, string multiplicity, string role, string name, string columnName = null) {
            ModelProperty classSourceproperty = GetPrimaryKeyProperty(sourceClass);
            if (classSourceproperty == null) {
                throw new NotSupportedException("La classe (" + sourceClass.Name + " - " + sourceClass.ModelFile + ") référencée par " + targetClass.Name + " n'a pas de clé primaire.");
            }

            ModelProperty property = new ModelProperty() {
                IsFromAssociation = true,
                DataDescription = new ModelDataDescription() {
                    IsPrimaryKey = false,
                    Libelle = name.Trim().Length != 0 ? name.Trim() : sourceClass.Name,
                    ReferenceType = sourceClass.Namespace.Model.Name + "." + sourceClass.Namespace.Name + "." + sourceClass.Name,
                    ReferenceClass = sourceClass,
                    Domain = classSourceproperty.DataDescription.Domain
                },
                Role = role
            };

            string codeId;
            string dataType = classSourceproperty.DataDescription.Domain.DataType;
            switch (dataType) {
                case "int":
                    codeId = "Id";
                    break;
                case "string":
                    codeId = "Code";
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Type de clé étrangère non-géré : {0} ({1} référence {2})", dataType, targetClass.Name, sourceClass.Name));
            }

            string dmName = columnName;
            if (string.IsNullOrEmpty(dmName)) {
                dmName = classSourceproperty.DataMember.Name;
                if (!string.IsNullOrEmpty(role)) {
                    dmName += "_" + role.Replace(" ", "_").Replace("-", "_").ToUpper(CultureInfo.CurrentCulture);
                    dmName = DeleteAccents(dmName);
                }
            }

            property.DataMember = new ModelDataMember() {
                Name = dmName,
                IsRequired = AbstractParser.Multiplicity11.Equals(multiplicity)
            };
            property.Name = sourceClass.Name + codeId + property.Role.Replace(" ", string.Empty).Replace("-", string.Empty);
            property.Comment = "Identifiant de l'objet " + sourceClass.Name + (string.IsNullOrEmpty(role) ? string.Empty : " " + role) + ".";
            property.DataType = classSourceproperty.DataType;
            property.IsPersistent = classSourceproperty.IsPersistent && targetClass.DataContract.IsPersistent;
            return property;
        }

        /// <summary>
        /// Créé la propriété référencant une liste de la clé étrangère d'une classe source.
        /// </summary>
        /// <param name="sourceClass">La classe source.</param>
        /// <param name="targetClass">La classe cible portant la propriété.</param>
        /// <param name="role">Le rôle de l'association.</param>
        /// <param name="name">Nom de l'association.</param>
        /// <returns>ModelProperty.</returns>
        internal static ModelProperty BuildClassAssociationListProperty(ModelClass sourceClass, ModelClass targetClass, string role, string name) {
            ModelProperty classSourceproperty = GetPrimaryKeyProperty(sourceClass);
            if (classSourceproperty == null) {
                throw new NotSupportedException("La classe (" + sourceClass.Name + " - " + sourceClass.ModelFile + ") référencée par " + targetClass.Name + " n'a pas de clé primaire.");
            }

            ModelProperty property = new ModelProperty() {
                IsFromAssociation = true,
                DataDescription = new ModelDataDescription() {
                    IsPrimaryKey = false,
                    Libelle = name.Trim().Length != 0 ? name.Trim() : sourceClass.Name
                },
                Role = role
            };

            string codeId;
            string dataType = classSourceproperty.DataDescription.Domain.DataType;
            switch (dataType) {
                case "int":
                    codeId = "IdList";
                    break;
                case "string":
                    codeId = "CodeList";
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Type de clé étrangère non-géré : {0} ({1} référence {2})", dataType, targetClass.Name, sourceClass.Name));
            }

            property.DataMember = new ModelDataMember() {
                IsRequired = false
            };
            property.Name = sourceClass.Name + codeId + property.Role.Replace(" ", string.Empty).Replace("-", string.Empty);
            property.Comment = "Liste des identifiants de l'objet " + sourceClass.Name + (string.IsNullOrEmpty(role) ? string.Empty : " " + role) + ".";
            property.DataType = "ICollection<" + dataType + ">";
            property.IsPersistent = false;
            return property;
        }

        /// <summary>
        /// Construit la propriété contenant la composition.
        /// </summary>
        /// <param name="containedClass">La classe contenu.</param>
        /// <param name="multiplicity">La multiplicité de la composition.</param>
        /// <param name="name">Nom de la composition.</param>
        /// <param name="code">Code de la composition.</param>
        /// <param name="comment">Commentaire de la composition.</param>
        /// <returns>La propriété à ajouter dans la classe conteneur.</returns>
        internal static ModelProperty BuildClassCompositionProperty(ModelClass containedClass, string multiplicity, string name, string code, string comment) {
            return new ModelProperty() {
                IsFromComposition = true,
                DataDescription = new ModelDataDescription() {
                    IsPrimaryKey = false,
                    Libelle = name.Trim().Length != 0 ? name.Trim() : containedClass.Name,
                    ReferenceClass = containedClass
                },
                DataType = AbstractParser.Multiplicity11.Equals(multiplicity) || AbstractParser.Multiplicity01.Equals(multiplicity) ? containedClass.Name : "ICollection<" + containedClass.Name + ">",
                Name = code,
                Comment = comment,
                IsPersistent = false,
                DataMember = new ModelDataMember() {
                    IsRequired = AbstractParser.Multiplicity11.Equals(multiplicity)
                }
            };
        }

        /// <summary>
        /// Renvoie un entier contenu dans le texte d'un noeud XML.
        /// Si la valeur n'est pas un entier, lève une exception.
        /// Méthode utilitaire.
        /// </summary>
        /// <param name="xmlValue">Noeud XML contenant la valeur.</param>
        /// <returns>Null si vide ou l'entier correspondant à la valeur.</returns>
        internal static int? GetXmlInt(XmlNode xmlValue) {
            if (xmlValue == null || string.IsNullOrEmpty(xmlValue.InnerText)) {
                return null;
            }

            return int.Parse(xmlValue.InnerText, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Renvoie la valeur d'un noeud xml ou "" si vide.
        /// Méthode utilitaire.
        /// </summary>
        /// <param name="xmlValue">Noeud ou attribut XML dont on cherche la valeur.</param>
        /// <returns>Contenu texte de l'élement XML.</returns>
        internal static string GetXmlValue(XmlNode xmlValue) {
            if (xmlValue == null || xmlValue.InnerText == null) {
                return string.Empty;
            }

            return xmlValue.InnerText;
        }

        /// <summary>
        /// Supprime les accents d'une chaine de caractère.
        /// </summary>
        /// <param name="source">La chaine de caractère contenant des accents.</param>
        /// <returns>La chaine de caractères non accentuée.</returns>
        internal static string DeleteAccents(string source) {
            byte[] tab = Encoding.GetEncoding(1251).GetBytes(source);
            return Encoding.ASCII.GetString(tab);
        }

        /// <summary>
        /// Retourne la clé primaire d'une classe.
        /// </summary>
        /// <param name="classe">La classe.</param>
        /// <returns>La propriété clé primaire de la classe.</returns>
        internal static ModelProperty GetPrimaryKeyProperty(ModelClass classe) {
            if (!classe.HasPrimaryKey) {
                ModelClass parentClass = classe.ParentClass;
                while (parentClass != null) {
                    if (parentClass.HasPrimaryKey) {
                        return ((IList<ModelProperty>)parentClass.PrimaryKey)[0];
                    }

                    parentClass = parentClass.ParentClass;
                }

                return null;
            }

            return ((IList<ModelProperty>)classe.PrimaryKey)[0];
        }

        /// <summary>
        /// Vérifie que les types de données sont équivalents.
        /// </summary>
        /// <param name="dataType">Type de données issu du domaine.</param>
        /// <param name="parsedDataType">Type de données issu du modele.</param>
        /// <returns><code>True</code> si les deux type sont identiques, <code>False</code> sinon.</returns>
        internal static bool IsSameDataType(Type dataType, string parsedDataType) {
            if (dataType.ToString() != parsedDataType) {
                if (dataType.ToString() == "System.Byte[]" && parsedDataType == "byte[]") {
                    return true;
                }

                if (dataType.ToString() == "System.Collections.Generic.ICollection`1[System.String]" && parsedDataType == "System.Collections.Generic.ICollection<string>") {
                    return true;
                }

                if (dataType.ToString() == "System.Collections.Generic.ICollection`1[System.Int32]" && parsedDataType == "System.Collections.Generic.ICollection<int>") {
                    return true;
                }

                if (dataType.ToString() == "System.String" && parsedDataType == "string") {
                    return true;
                }

                if (dataType.ToString() == "System.Int32" && parsedDataType == "int") {
                    return true;
                }

                if (dataType.ToString() == "System.Boolean" && parsedDataType == "bool") {
                    return true;
                }

                if (dataType.ToString() == "System.Int16" && parsedDataType == "short") {
                    return true;
                }

                if (dataType.ToString() == "System.Decimal" && parsedDataType == "decimal") {
                    return true;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Retourne si le type concerné est un type primitif.
        /// </summary>
        /// <param name="type">Le type concerné.</param>
        /// <returns><code>True</code> si le type est primitif, <code>False</code> sinon.</returns>
        internal static bool IsPrimitiveType(string type) {
            return type == "int" ||
                type == "short" ||
                type == "string" ||
                type == "char" ||
                type == "long" ||
                type == "bool" ||
                type == "byte" ||
                type == "byte[]" ||
                type == "System.Collections.Generic.ICollection<string>" ||
                type == "System.Collections.Generic.ICollection<int>" ||
                type == "float" ||
                type == "decimal" ||
                type == "System.Guid" ||
                type == "System.DateTime" ||
                type == "System.TimeSpan";
        }

        /// <summary>
        /// Convertit les caractères spéciaux de la chaine en paramètre.
        /// </summary>
        /// <param name="chaine">La chaine à convertir.</param>
        /// <returns>La chaine convertie.</returns>
        private string InternalConvert(string chaine) {
            if (string.IsNullOrEmpty(chaine)) {
                return null;
            }

            ICollection<string> replaceList = _conversionMap.Keys;
            string ret = chaine;
            foreach (string strReplace in replaceList) {
                ret = ret.Replace(strReplace, _conversionMap[strReplace]);
            }

            return ret;
        }
    }
}
