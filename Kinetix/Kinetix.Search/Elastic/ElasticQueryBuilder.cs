using System.Globalization;
using System.Linq;
using System.Text;

namespace Kinetix.Search.Elastic {

    /// <summary>
    /// Builder de requête ElasticSearch.
    /// </summary>
    public class ElasticQueryBuilder {

        /// <summary>
        /// Caractères réservés de la syntaxe Query DSL à échapper.
        /// </summary>
        private static string[] elasticSpecialChars = new string[] { "\"", "<", ">", "=", "/", "\\", "+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", "~", "*", "?", ":", "." };

        /// <summary>
        /// Construit une requête pour une recherche textuelle.
        /// </summary>
        /// <param name="field">Champ de recherche.</param>
        /// <param name="text">Texte de recherche.</param>
        /// <returns>Requête.</returns>
        public string BuildFullTextSearch(string field, string text) {
            if (string.IsNullOrEmpty(text)) {
                return string.Empty;
            }

            /* Enlève les accents. */
            var withoutAccent = RemoveDiacritics(text);
            /* Passe en minsucule. */
            var lower = withoutAccent.ToLower(CultureInfo.CurrentCulture);
            /* Echappe les caractères réservés. */
            var escapedValue = EscapeLuceneSpecialChars(lower);
            /* Remplace les tirets et apostrophe par des espaces. */
            escapedValue = escapedValue.Replace('-', ' ').Replace('\'', ' ');
            /* Découpe en mot. */
            var subWords = escapedValue.Split(' ');
            /* Rajoute le joker à la fin. */
            /* Concatène en AND : tous les termes doivent matcher. */
            var andQuery = string.Join(" AND ", subWords.Select(x => x + "*"));
            var query = string.Format("{0}:({1})", field, andQuery);
            return query;
        }

        /// <summary>
        /// Construit une requête pour inclure une valeur parmi plusieurs.
        /// </summary>
        /// <param name="field">Champ.</param>
        /// <param name="codes">Liste de valeurs à inclure.</param>
        /// <returns>Requête.</returns>
        public string BuildInclusiveInclude(string field, string codes) {
            /* Echappe les caractères réservés. */
            var escapedValue = EscapeLuceneSpecialChars(codes);
            /* Découpe en mot. */
            var subWords = escapedValue.Split(' ');
            /* Concatène en OR : un seul match est suffisant. */
            var andQuery = string.Join(" OR ", subWords);
            /* Ajoute le nom du champ. */
            var query = string.Format("{0}:({1})", field, andQuery);
            return query;
        }

        /// <summary>
        /// Construit une requête pour exclure des valeurs.
        /// </summary>
        /// <param name="field">Champ.</param>
        /// <param name="codes">Liste de valeurs à exclure.</param>
        /// <returns>Requête.</returns>
        public string BuildExcludeQuery(string field, string codes) {
            /* Echappe les caractères réservés. */
            var escapedValue = EscapeLuceneSpecialChars(codes);
            /* Découpe en mot. */
            var subWords = escapedValue.Split(' ');
            /* Concatène en OR : un seul match est suffisant. */
            var andQuery = string.Join(" AND ", subWords.Select(x => $"-{x}"));
            /* Ajoute le nom du champ. */
            var query = string.Format("{0}:({1})", field, andQuery);
            return query;
        }

        /// <summary>
        /// Construit une requête pour le filtrage exacte (sélection de facette, ...).
        /// </summary>
        /// <param name="field">Champ.</param>
        /// <param name="value">Valeur.</param>
        /// <returns>Requête.</returns>
        public string BuildFilter(string field, string value) {
            /* Echappe les caractères réservés. */
            var escapedValue = EscapeLuceneSpecialChars(value);
            /* Ajoute le nom du champ. */
            var query = string.Format("{0}:({1})", field, escapedValue);
            return query;
        }

        /// <summary>
        /// Construit une requête pour un champ qui doit manquer (équivalent d'une valeur NULL).
        /// </summary>
        /// <param name="field">Champ.</param>
        /// <returns>Requête.</returns>
        public string BuildMissingField(string field) {
            return string.Format("NOT (_exists_:{0})", field);
        }

        /// <summary>
        /// Construit une requête avec des AND sur des sous-requêtes.
        /// </summary>
        /// <param name="subQueries">Sous-requêtes.</param>
        /// <returns>Requête.</returns>
        public string BuildAndQuery(params string[] subQueries) {
            if (subQueries == null) {
                return null;
            }

            return string.Join(
                " AND ",
                subQueries
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => "(" + x + ")"));
        }

        /// <summary>
        /// Construit une requête avec des OR sur des sous-requêtes.
        /// </summary>
        /// <param name="subQueries">Sous-requêtes.</param>
        /// <returns>Requête.</returns>
        public string BuildOrQuery(params string[] subQueries) {
            if (subQueries == null) {
                return null;
            }

            return string.Join(
                " OR ",
                subQueries
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => "(" + x + ")"));
        }

        /// <summary>
        /// Échappe les caractères spéciaux ElasticSearch.
        /// </summary>
        /// <param name="value">Texte à traiter.</param>
        /// <returns>Chaîne échappée.</returns>
        private static string EscapeLuceneSpecialChars(string value) {
            var sb = new StringBuilder(value);
            foreach (var specialChar in elasticSpecialChars) {
                sb.Replace(specialChar, @"\" + specialChar);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Remplace les caractères avec accents par les caractères correspondants sans accents.
        /// </summary>
        /// <param name="raw">Chaîne brute.</param>
        /// <returns>Chaîne traitée.</returns>
        private static string RemoveDiacritics(string raw) {
            string normalizedString = raw.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in normalizedString) {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
