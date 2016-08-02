using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Kinetix.ComponentModel;
using Kinetix.ComponentModel.DataAnnotations;

namespace Kinetix.Reporting {

    /// <summary>
    /// Helper permettant la production de fichiers plats.
    /// </summary>
    /// <remarks>
    /// S'appuie sur la présence de l'attribut ExportFixedSize sur le bean exporté.
    /// </remarks>
    public static class ReportToFlatFile {

        /// <summary>
        /// Produit un fichier plat à partir d'une source de données.
        /// </summary>
        /// <param name="dataSource">Source de données.</param>
        /// <param name="separator">Le séparateur.</param>
        /// <returns>Contenu du fichier.</returns>
        public static byte[] CreateFlatFile(IEnumerable dataSource, char separator) {
            return CreateFlatFile(dataSource, Encoding.Unicode, separator);
        }

        /// <summary>
        /// Produit un fichier plat à partir d'une source de données.
        /// </summary>
        /// <param name="dataSource">Source de données.</param>
        /// <param name="encoding">Encodage du fichier.</param>
        /// <param name="separator">Le separateur.</param>
        /// <param name="isWithHeader">Flag pour afficher les headers avec un ";".</param>
        /// <param name="isWithSeparator">Met un separateur entre les champs.</param>
        /// <param name="isFixedColumn">Indique si le fichier est plat est taille de colonne fixe.</param>
        /// <returns>Contenu du fichier.</returns>
        public static byte[] CreateFlatFile(IEnumerable dataSource, Encoding encoding, char separator, bool isWithHeader = true, bool isWithSeparator = true, bool isFixedColumn = true) {
            if (dataSource == null) {
                throw new ArgumentNullException("dataSource");
            }

            if (encoding == null) {
                throw new ArgumentNullException("encoding");
            }

            StringBuilder sb = new StringBuilder();
            IDictionary<int, EncodingFlatFile> propertyMapSortedByPosition = BuildSortedDictionnary(dataSource);

            if (isWithHeader) {
                AddHeader(sb, propertyMapSortedByPosition);
            }

            foreach (object valeur in dataSource) {
                bool first = true;
                foreach (EncodingFlatFile encodingFlatFile in propertyMapSortedByPosition.Values) {
                    FlatFileField attr = encodingFlatFile.Attr;
                    object propertyValue = encodingFlatFile.Property.GetValue(valeur);
                    string propertyValueString;
                    if (propertyValue is DateTime) {
                        propertyValueString = ((DateTime)propertyValue).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                    } else {
                        propertyValueString = (propertyValue ?? string.Empty).ToString();
                    }

                    propertyValueString = propertyValueString.Substring(0, Math.Min(propertyValueString.Length, attr.Length));
                    string paddedValue;

                    if (isFixedColumn) {
                        switch (attr.PaddingDirection) {
                            case PaddingPosition.Right:
                                paddedValue = propertyValueString.PadRight(attr.Length);
                                break;
                            case PaddingPosition.Left:
                                paddedValue = propertyValueString.PadLeft(attr.Length);
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                    } else {
                        paddedValue = propertyValueString;
                    }

                    if (!first && isWithSeparator) {
                        sb.Append(separator);
                    }

                    sb.Append(paddedValue);

                    first = false;
                }

                sb.Append(Environment.NewLine);
            }

            return encoding.GetBytes(sb.ToString());
        }

        /// <summary>
        /// Produit un fichier plat à partir d'une source de données.
        /// </summary>
        /// <param name="dataSource">Source de données.</param>
        /// <param name="encoding">Encodage du fichier</param>
        /// <param name="isWithHeader">Flag pour afficher les headers avec un ";".</param>
        /// <returns>Le fichier plat.</returns>
        public static byte[] CreateFlatFileFixedWidth(IEnumerable dataSource, Encoding encoding, bool isWithHeader = true) {
            return CreateFlatFile(dataSource, encoding, ' ', isWithHeader, false, true);
        }

        /// <summary>
        /// Produit un fichier plat à partir d'une source de données.
        /// </summary>
        /// <param name="dataSource">Source de données.</param>
        /// <param name="encoding">Encodage du fichier</param>
        /// <param name="separator">Le séparateur.</param>
        /// <param name="isWithHeader">Flag pour afficher les headers avec un ";".</param>
        /// <returns>Le fichier plat.</returns>
        public static byte[] CreateFlatFileColumnSeparator(IEnumerable dataSource, Encoding encoding, char separator, bool isWithHeader = true) {
            return CreateFlatFile(dataSource, encoding, separator, isWithHeader, true, false);
        }

        /// <summary>
        /// Ajout l'entête des colonnes du fichier plat dans le flux de caractères.
        /// </summary>
        /// <param name="sb">Flux de caractères.</param>
        /// <param name="propertyMapSortedByPosition">Dictionnaire des colonnes de fichiers plats indexées par position.</param>
        private static void AddHeader(StringBuilder sb, IDictionary<int, EncodingFlatFile> propertyMapSortedByPosition) {
            StringBuilder sbHeader = new StringBuilder();
            bool first = true;
            foreach (EncodingFlatFile encodingFlatFile in propertyMapSortedByPosition.Values) {
                if (!first) {
                    sbHeader.Append(';');
                }

                sbHeader.Append(encodingFlatFile.Property.Description);
                first = false;
            }

            sb.Append(sbHeader);
            sb.Append("\r\n");
        }

        /// <summary>
        /// Renvoie à partir d'une collection de bean un dictionnaire associant à une position de colonne dans le fichier plat la propriété correspondante du bean.
        /// </summary>
        /// <param name="dataSource">Collection de bean.</param>
        /// <returns>Dictionnaire des colonnes de fichiers plat indéxées par leur position.</returns>
        private static IDictionary<int, EncodingFlatFile> BuildSortedDictionnary(IEnumerable dataSource) {
            BeanDefinition beanDefinition = BeanDescriptor.GetCollectionDefinition(dataSource);
            BeanPropertyDescriptorCollection properties = beanDefinition.Properties;
            IDictionary<int, EncodingFlatFile> propertyMapSortedByPosition = new SortedDictionary<int, EncodingFlatFile>(Comparer<int>.Default);
            foreach (BeanPropertyDescriptor property in properties) {
                PropertyInfo basePropertyInfo = beanDefinition.BeanType.GetProperty(property.PropertyName);
                object[] attrs = basePropertyInfo.GetCustomAttributes(typeof(FlatFileField), false);
                if (attrs != null && attrs.Length > 0) {
                    EncodingFlatFile encodingFlatFile = new EncodingFlatFile {
                        Property = property,
                        Attr = (FlatFileField)attrs[0]
                    };
                    propertyMapSortedByPosition.Add(encodingFlatFile.Attr.Position, encodingFlatFile);
                }
            }

            if (propertyMapSortedByPosition.Count == 0) {
                throw new NotSupportedException("No exportable properties");
            }

            if (propertyMapSortedByPosition.Keys.Min() != 0) {
                throw new NotSupportedException("The position must start at 0");
            }

            if ((propertyMapSortedByPosition.Keys.Max() - propertyMapSortedByPosition.Keys.Min() + 1) != propertyMapSortedByPosition.Keys.Count) {
                throw new NotSupportedException("Inconsistency between the total number of exportable properties and the min and max position, there is some hole in your positions");
            }

            return propertyMapSortedByPosition;
        }

        /// <summary>
        /// Classe temporaire pour stocker dans le dictionnaire.
        /// </summary>
        private class EncodingFlatFile {

            /// <summary>
            /// La propriété.
            /// </summary>
            public BeanPropertyDescriptor Property {
                get;
                set;
            }

            /// <summary>
            /// L'attribut.
            /// </summary>
            public FlatFileField Attr {
                get;
                set;
            }
        }
    }
}
