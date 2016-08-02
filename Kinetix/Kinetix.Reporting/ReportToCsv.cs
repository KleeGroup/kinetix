using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Kinetix.ComponentModel;

namespace Kinetix.Reporting {

    /// <summary>
    /// Classe permettant de générer des fichiers CSV.
    /// </summary>
    public static class ReportToCsv {
        /// <summary>
        /// Création d'un document CSV à partir d'une collection et de ses propriétés à exporter.
        /// </summary>
        /// <param name="properties">Liste des proprités à exporter.</param>
        /// <param name="dataSourceList">Liste des données à exporter.</param>
        /// <param name="showHeader">Affiche ou pas un header.</param>
        /// <returns>Le fichier binaire.</returns>
        public static byte[] CreateCsvFile(ICollection<ExportPropertyDefinition> properties, object dataSourceList, bool showHeader = true) {
            if (dataSourceList == null) {
                throw new ArgumentNullException("dataSourceList");
            }

            if (properties == null) {
                throw new ArgumentNullException("properties");
            }

            IList<string> colonnes = new List<string>(properties.Count);
            IList<string> headers = new List<string>(properties.Count);

            BeanDefinition definition = BeanDescriptor.GetCollectionDefinition((ICollection)dataSourceList);
            foreach (ExportPropertyDefinition propertyDefinition in properties) {
                colonnes.Add(propertyDefinition.PropertyPath);
                if (!string.IsNullOrEmpty(propertyDefinition.PropertyLabel)) {
                    headers.Add(propertyDefinition.PropertyLabel);
                } else {
                    if (propertyDefinition.PropertyPath.IndexOf('.') != -1) {
                        throw new ArgumentException("La composition n'est pas supporté");
                    }

                    headers.Add(definition.Properties[propertyDefinition.PropertyPath].Description);
                }
            }

            return CreateCsvFile(colonnes, dataSourceList, headers, showHeader);
        }

        /// <summary>
        /// Création d'un document CSV à partir d'un IDataReader.
        /// </summary>
        /// <param name="header">Chaine de caractères représentant le header.</param>
        /// <param name="reader">DataReader.</param>
        /// <returns>Fichier binaire.</returns>
        public static byte[] CreateCsvFileFromDataReader(string header, IDataReader reader) {
            if (reader == null) {
                throw new ArgumentNullException("reader");
            }

            StringBuilder sb = new StringBuilder();
            if (header != null) {
                sb.Append(header);
                sb.Append("\r\n");
            }

            using (reader) {
                while (reader.Read()) {
                    for (int i = 0; i < reader.FieldCount; ++i) {
                        object value = reader.GetValue(i);
                        if (value != null) {
                            sb.Append(value.ToString());
                        }

                        if (i != reader.FieldCount - 1) {
                            sb.Append(';');
                        }
                    }

                    sb.Append("\r\n");
                }
            }

            return Encoding.Default.GetBytes(sb.ToString());
        }

        /// <summary>
        /// Création d'un document CSV à partir d'une collection et de ses colonnes à exporter.
        /// </summary>
        /// <param name="colonnes">Colonnes à exporter.</param>
        /// <param name="dataSourceList">Liste des données à exporter.</param>
        /// <param name="headers">Liste des headers(si null alors on utilise les colonnes).</param>
        /// <param name="showHeader">Affiche ou pas un header.</param>
        /// <returns>Fichier binaire.</returns>
        private static byte[] CreateCsvFile(IList<string> colonnes, object dataSourceList, IList<string> headers, bool showHeader = true) {
            if (dataSourceList == null) {
                throw new ArgumentNullException("dataSourceList");
            }

            if (colonnes == null) {
                throw new ArgumentNullException("colonnes");
            }

            StringBuilder sb = new StringBuilder();
            BeanDefinition beanDefinition = BeanDescriptor.GetCollectionDefinition(dataSourceList);
            BeanPropertyDescriptorCollection properties = beanDefinition.Properties;
            if (showHeader) {
                bool first = true;
                if (headers == null) {
                    foreach (string colonne in colonnes) {
                        BeanPropertyDescriptor descriptor = properties[colonne];
                        if (!first) {
                            sb.Append(';');
                        }

                        sb.Append(descriptor.Description);
                        first = false;
                    }
                } else {
                    if (headers.Count != colonnes.Count) {
                        throw new ArgumentException("colonnes et headers n'ont pas la même taille");
                    }

                    foreach (string header in headers) {
                        if (!first) {
                            sb.Append(';');
                        }

                        sb.Append(header);
                        first = false;
                    }
                }

                sb.Append("\r\n");
            }

            foreach (object valeur in (ICollection)dataSourceList) {
                bool first = true;
                foreach (string colonne in colonnes) {
                    BeanPropertyDescriptor descriptor = properties[colonne];
                    if (!first) {
                        sb.Append(';');
                    }

                    sb.Append(descriptor.GetValue(valeur));
                    first = false;
                }

                sb.Append("\r\n");
            }

            return Encoding.Default.GetBytes(sb.ToString());
        }
    }
}
