using System;
using System.Diagnostics.CodeAnalysis;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Attribut spécifiant le TypeConverter.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "TypeConverter déja accessible via ConverterTypeName.")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CustomTypeConverterAttribute : Attribute {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="typeConverter">Type du convertisseur de type.</param>
        public CustomTypeConverterAttribute(Type typeConverter) {
            if (typeConverter == null) {
                throw new ArgumentNullException("typeConverter");
            }

            this.ConverterTypeName = typeConverter.AssemblyQualifiedName;
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="converterTypeName">Type du convertisseur appelé.</param>
        public CustomTypeConverterAttribute(string converterTypeName) {
            if (string.IsNullOrEmpty("converterTypeName")) {
                throw new ArgumentNullException("converterTypeName");
            }

            this.ConverterTypeName = converterTypeName;
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="typeConverter">Type du convertisseur.</param>
        /// <param name="formatString">Chaine de formattage du convertisseur.</param>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Choix de nommage admis.")]
        public CustomTypeConverterAttribute(Type typeConverter, string formatString)
            : this(typeConverter) {
            if (string.IsNullOrEmpty(formatString)) {
                throw new ArgumentNullException("formatString");
            }

            this.FormatString = formatString;
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="converterTypeName">AssemblyQualifiedName du convertisseur de type appelé.</param>
        /// <param name="formatString">Chaine de formattage.</param>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Choix de nommage admis.")]
        public CustomTypeConverterAttribute(string converterTypeName, string formatString)
            : this(converterTypeName) {
            if (string.IsNullOrEmpty(formatString)) {
                throw new ArgumentNullException("formatString");
            }

            this.FormatString = formatString;
        }

        /// <summary>
        /// Obtient le type de converter appelé.
        /// </summary>
        public string ConverterTypeName {
            get;
            private set;
        }

        /// <summary>
        /// Obtient la chaine de formattage.
        /// </summary>
        public string FormatString {
            get;
            private set;
        }

        /// <summary>
        /// Obtient ou définit l'unité du domaine.
        /// </summary>
        public string Unit {
            get;
            set;
        }
    }
}
