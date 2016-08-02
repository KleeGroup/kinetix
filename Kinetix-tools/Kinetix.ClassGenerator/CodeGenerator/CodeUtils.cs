using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Kinetix.ClassGenerator.CodeGenerator {
    /// <summary>
    /// Classe utilitaire destinée à la génération de code.
    /// </summary>
    internal static class CodeUtils {

        private static readonly Regex RegNotNullableType = new Regex(@"^((u)?int|(u)?long|(s)?byte|(u)?short|bool|System.DateTime|System.TimeSpan|decimal|System.Guid)$");
        private static readonly Regex RegExVarChar = new Regex("^VA[0-9]*$");
        private static readonly Regex RegExChar = new Regex("^A[0-9]*$");
        private static readonly Regex RegExNumeric = new Regex("^N[0-9]*,[0-9]*$");
        private static readonly Regex RegExDecimal = new Regex("^DC[0-9]*,[0-9]*$");
        private static IDictionary<string, string> regType;

        /// <summary>
        /// Détermine si le type est un type de base C#
        /// non nullable.
        /// </summary>
        /// <param name="name">Nom du type à définir.</param>
        /// <returns>Vrai si le type est un type C#.</returns>
        public static bool IsNonNullableCSharpBaseType(string name) {
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name");
            }

            return RegNotNullableType.Match(name).Success;
        }

        /// <summary>
        /// Détermine si le type est un type de base C#.
        /// </summary>
        /// <param name="name">Nom du type à définir.</param>
        /// <returns>Vrai si le type est un type C#.</returns>
        public static bool IsCSharpBaseType(string name) {
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name");
            }

            if (regType == null) {
                InitializeRegType();
            }

            return regType.ContainsKey(name);
        }

        /// <summary>
        /// Donne la valeur par défaut d'un type de base C#.
        /// Renvoie null si le type n'est pas un type par défaut.
        /// </summary>
        /// <param name="name">Nom du type à définir.</param>
        /// <returns>Vrai si le type est un type C#.</returns>
        public static string GetCSharpDefaultValueBaseType(string name) {
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name");
            }

            if (regType == null) {
                InitializeRegType();
            }

            string res;
            regType.TryGetValue(name, out res);
            return res;
        }

        /// <summary>
        /// Retourne le nom de la variable membre a générer à partir du nom de la propriété.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <returns>Nom de la variable membre privée.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Nom de la propriété en CAMEL Case")]
        public static string LoadPrivateFieldName(string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException("propertyName");
            }

            return propertyName.Substring(0, 1).ToLowerInvariant() + propertyName.Substring(1);
        }

        /// <summary>
        /// Retourne le type contenu dans la collection.
        /// </summary>
        /// <param name="dataType">Type de données qualifié.</param>
        /// <returns>Nom du type de données contenu.</returns>
        public static string LoadInnerDataType(string dataType) {
            int beginIdx = dataType.LastIndexOf('<');
            int endIdx = dataType.LastIndexOf('>');
            if (beginIdx == -1 || endIdx == -1) {
                throw new NotSupportedException();
            }

            return dataType.Substring(beginIdx + 1, (endIdx - 1) - beginIdx);
        }

        /// <summary>
        /// Retourne le type de l'objet en notation minimaliste (suppression du namespace).
        /// </summary>
        /// <param name="dataType">Type de données.</param>
        /// <returns>Nom court.</returns>
        public static string LoadShortDataType(string dataType) {
            int idx = dataType.LastIndexOf('.');
            return idx != -1 ? dataType.Substring(idx + 1) : dataType;
        }

        /// <summary>
        /// Retourne le type SQL à partir d'un type persistent PowerDesigner.
        /// </summary>
        /// <param name="persistentDataType">Type persistent PowerDesigner.</param>
        /// <returns>Le type SQL associé.</returns>
        public static string PowerDesignerPersistentDataTypeToSqlDatType(string persistentDataType) {
            if (string.IsNullOrEmpty(persistentDataType)) {
                throw new ArgumentNullException("persistentDataType");
            }

            if (persistentDataType == "I") {
                return "int";
            }

            if (persistentDataType == "D") {
                if (GeneratorParameters.IsOracle) {
                    return "timestamp";
                }

                return "datetime2";
            }

            if (persistentDataType == "DT") {
                if (GeneratorParameters.IsOracle) {
                    return "timestamp";
                }

                return "datetime2";
            }

            if (persistentDataType == "BL") {
                if (GeneratorParameters.IsOracle) {
                    return "number(1)";
                }

                return "bit";
            }

            if (persistentDataType == "SI") {
                return "smallint";
            }

            if (persistentDataType == "T") {
                return "time";
            }

            if (persistentDataType == "TXT") {
                if (GeneratorParameters.IsOracle) {
                    return "clob";
                }

                return "text";
            }

            if (persistentDataType == "PIC") {
                if (GeneratorParameters.IsOracle) {
                    return "blob";
                } else {
                    return "image";
                }
            }

            if (RegExNumeric.IsMatch(persistentDataType)) {
                return "numeric";
            }

            if (RegExVarChar.IsMatch(persistentDataType)) {
                if (GeneratorParameters.IsOracle) {
                    return "nvarchar2";
                }

                return "nvarchar";
            }

            if (RegExChar.IsMatch(persistentDataType)) {
                return "nchar";
            }

            if (RegExDecimal.IsMatch(persistentDataType)) {
                return "decimal";
            }

            return persistentDataType;
        }

        /// <summary>
        /// Initialisation des types.
        /// </summary>
        private static void InitializeRegType() {
            regType = new Dictionary<string, string>();
            regType.Add("int", "0");
            regType.Add("uint", "0");
            regType.Add("float", "0.0f");
            regType.Add("double", "0.0");
            regType.Add("bool", "false");
            regType.Add("short", "0");
            regType.Add("ushort", "0");
            regType.Add("long", "0");
            regType.Add("ulong", "0");
            regType.Add("decimal", "0");
            regType.Add("byte", "0");
            regType.Add("sbyte", "0");
            regType.Add("string", "\"\"");
        }
    }
}
