using System;
using System.Diagnostics.CodeAnalysis;
using Kinetix.ClassGenerator.CodeGenerator;
using Kinetix.ClassGenerator.Model;

namespace Kinetix.ClassGenerator.SsdtSchemaGenerator {

    /// <summary>
    /// Classe utilitaire pour écritre du SQL.
    /// </summary>
    [SuppressMessage("Cpd", "Cpd", Justification = "Pas d'enjeu.")]
    public static class ScriptUtils {

        /// <summary>
        /// Détermine le nom du type T-SQL avec la précision.
        /// </summary>
        /// <param name="property">Propriété de classe.</param>
        /// <returns>Nom du type T-SQL.</returns>
        public static string DeterminerSqlDataType(this ModelProperty property) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }

            IPersistenceData persistenceData =
                property.IsDatabaseOnly ?
                    property.DataMember : // Propriété non applicative : les données de persistence sont portées par le champ directement.
                    (IPersistenceData)property.DataDescription.Domain; // Propriété applicative : les données de persistences sont portées par le domaine.

            string persistentType = CodeUtils.PowerDesignerPersistentDataTypeToSqlDatType(persistenceData.PersistentDataType);

            bool hasLength = persistenceData.PersistentLength.HasValue;
            bool hasPrecision = persistenceData.PersistentPrecision.HasValue;

            if (hasLength) {
                persistentType += "(" + persistenceData.PersistentLength;
                if (hasPrecision) {
                    persistentType += "," + persistenceData.PersistentPrecision;
                }

                persistentType += ")";
            } else if (property.DataDescription.IsPrimaryKey && property.DataDescription.Domain.Code == "DO_ID") {
                persistentType += " identity";
            } else if (persistentType == "nvarchar") {
                persistentType += "(MAX)";
            }

            return persistentType;
        }

        /// <summary>
        /// Retourne le nom de la table SQL correspondant à la classe.
        /// </summary>
        /// <param name="classe">Classe.</param>
        /// <returns>Nom de la table SQL.</returns>
        public static string GetTableName(this ModelClass classe) {
            if (classe == null) {
                throw new ArgumentNullException("classe");
            }

            return GeneratorParameters.IsProjetUesl ? classe.DataContract.Name : classe.DataContract.Name.ToUpperInvariant();
        }

        /// <summary>
        /// Retourne le nom de la colonne SQL correspondant à la propriété.
        /// </summary>
        /// <param name="property">Propriété.</param>
        /// <returns>Nom de la colonne SQL.</returns>
        public static string GetColumnName(this ModelProperty property) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }

            return GeneratorParameters.IsProjetUesl ? property.DataMember.Name : property.DataMember.Name.ToUpperInvariant();
        }

        /// <summary>
        /// Retourne le nom du type de table SQL correspondant à la classe.
        /// </summary>
        /// <param name="classe">Classe.</param>
        /// <returns>Nom du type de table.</returns>
        public static string GetTableTypeName(this ModelClass classe) {
            if (classe == null) {
                throw new ArgumentNullException("classe");
            }

            return GeneratorParameters.IsProjetUesl ? classe.GetTableName() + "TableType" : classe.GetTableName() + "_TABLE_TYPE";
        }

        /// <summary>
        /// Prépare une chaîne de caractères à être écrite dans un script SQL.
        /// </summary>
        /// <param name="raw">La chaîne à préparer.</param>
        /// <returns>La chaîne de caractère équivalente, mise au format SQL.</returns>
        public static string PrepareDataToSqlDisplay(string raw) {
            if (string.IsNullOrEmpty(raw)) {
                return string.Empty;
            }

            return raw.Replace("'", "''");
        }
    }
}
