using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.NVortex;

namespace Kinetix.ClassGenerator.Checker {

    /// <summary>
    /// Classe proposant des outils permettant l'analyse du modèle objet.
    /// </summary>
    internal abstract class AbstractModelChecker {
        /// <summary>
        /// Récupère l'ensemble des erreurs du vérificateur de modèle.
        /// </summary>
        public static readonly ICollection<NVortexMessage> NVortexMessageList = new Collection<NVortexMessage>();

        private static readonly Regex RegexPascal = new Regex(@"^([A-Z][a-zA-Z0-9]+)$");
        private static readonly Regex RegexPascalWithDot = new Regex(@"^([A-Z][a-zA-Z0-9]*(\.[a-zA-Z0-9]+)*)$");
        private static readonly Regex RegexDataBaseTableName = new Regex(@"^([A-Z][_A-Z0-9]+)$");
        private static readonly Regex RegexDataBaseTableNameUesl = new Regex(@"^[TV]_[A-Z][a-zA-Z0-9]+$");
        private static readonly Regex RegexDataBaseFieldName = new Regex(@"^[A-Z0-9]{3}([_A-Z0-9]+)$");
        private static readonly Regex RegexDataBaseFieldNameUesl = new Regex(@"^[A-Z]{0,3}[A-Z][a-zA-Z0-9]+$");
        private static readonly Regex RegexDomaineName = new Regex(@"^DO([_A-Z0-9]+)$");
        private static readonly string SourceDtoDirectory = GeneratorParameters.OutputDirectory +
                Path.DirectorySeparatorChar + "Kinetix.Dto" + Path.DirectorySeparatorChar;

        /// <summary>
        /// Vérifie l'objet passé en paramètres.
        /// </summary>
        /// <param name="objet">L'objet à vérifier.</param>
        public abstract void Check(IModelObject objet);

        /// <summary>
        /// Enregistre une erreur de type bloquant.
        /// </summary>
        /// <param name="objet">L'objet en erreur.</param>
        /// <param name="error">Le message d'erreur.</param>
        protected static void RegisterFatalError(IModelObject objet, string error) {
            RegisterError(objet, Category.Error, error, true);
        }

        /// <summary>
        /// Enregistre une erreur de type bug.
        /// </summary>
        /// <param name="objet">L'objet en erreur.</param>
        /// <param name="error">Le message d'erreur.</param>
        protected static void RegisterBug(IModelObject objet, string error) {
            RegisterError(objet, Category.Bug, error, false);
        }

        /// <summary>
        /// Enregistre une erreur de type code style.
        /// </summary>
        /// <param name="objet">L'objet en erreur.</param>
        /// <param name="error">Le message d'erreur.</param>
        protected static void RegisterCodeStyle(IModelObject objet, string error) {
            RegisterError(objet, Category.CodeStyle, error, false);
        }

        /// <summary>
        /// Enregistre une erreur de type doc.
        /// </summary>
        /// <param name="objet">L'objet en erreur.</param>
        /// <param name="error">Le message d'erreur.</param>
        protected static void RegisterDoc(IModelObject objet, string error) {
            RegisterError(objet, Category.Doc, error, false);
        }

        /// <summary>
        /// Vérfie la casse autorisée en pascal case.
        /// </summary>
        /// <param name="value">Valeur à vérfier.</param>
        /// <returns>Vrai si casse Pascal.</returns>
        protected static bool IsPascalCaseValid(string value) {
            return string.IsNullOrEmpty(value) ? true : RegexPascal.IsMatch(value);
        }

        /// <summary>
        /// Vérfie la casse autorisée en pascal case avec éventuellement des points.
        /// </summary>
        /// <param name="value">Valeur à vérfier.</param>
        /// <returns>Vrai si casse Pascal avec éventuellement des points.</returns>
        protected static bool IsPascalCaseWithDotValid(string value) {
            return string.IsNullOrEmpty(value) ? true : RegexPascalWithDot.IsMatch(value);
        }

        /// <summary>
        /// Vérfie la casse autorisée en BDD pour une table.
        /// </summary>
        /// <param name="value">Valeur à vérfier.</param>
        /// <returns>Vrai si casse BDD.</returns>
        protected static bool IsDataBaseTableNameCaseValid(string value) {
            return string.IsNullOrEmpty(value) ? true : GeneratorParameters.IsProjetUesl ? RegexDataBaseTableNameUesl.IsMatch(value) : RegexDataBaseTableName.IsMatch(value);
        }

        /// <summary>
        /// Vérfie la casse autorisée en BDD pour une table.
        /// </summary>
        /// <param name="value">Valeur à vérfier.</param>
        /// <returns>Vrai si casse BDD.</returns>
        protected static bool IsDataBaseFieldNameCaseValid(string value) {
            return string.IsNullOrEmpty(value) ? true : GeneratorParameters.IsProjetUesl ? RegexDataBaseFieldNameUesl.IsMatch(value) : RegexDataBaseFieldName.IsMatch(value);
        }

        /// <summary>
        /// Vérifie la casse autorisée pour les domaines.
        /// </summary>
        /// <param name="value">La chaine de caractère à vérifier.</param>
        /// <returns><code>False</code> si le nom du domaine n'est pas valide <code>true</code> sinon.</returns>
        protected static bool IsDomaineNameValid(string value) {
            return string.IsNullOrEmpty(value) ? true : RegexDomaineName.IsMatch(value);
        }

        /// <summary>
        /// Vérifie que le commentaire est valide.
        /// </summary>
        /// <param name="commentaire">Le commentaire à vérifier.</param>
        /// <returns>True si le commentaire est valide.</returns>
        protected static bool IsCommentValid(string commentaire) {
            if (string.IsNullOrEmpty(commentaire)) {
                return false;
            }

            if (!commentaire.Trim().Substring(0, 1).ToUpper(CultureInfo.InvariantCulture).Equals(commentaire.Trim().Substring(0, 1))) {
                return false;
            }

            if (!commentaire.Trim().EndsWith(".", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Enregistre un message de type erreur.
        /// </summary>
        /// <param name="objet">L'objet en erreur.</param>
        /// <param name="category">La catégorie de l'erreur.</param>
        /// <param name="error">Le message d'erreur.</param>
        /// <param name="isFatal">Indique si l'erreur est bloquante.</param>
        private static void RegisterError(IModelObject objet, Category category, string error, bool isFatal) {
            NVortexMessage message = new NVortexMessage();
            ModelClass itemClass = objet as ModelClass;
            ModelProperty itemProperty = objet as ModelProperty;
            if (itemClass != null) {
                message.FileName = SourceDtoDirectory + itemClass.Namespace.Name + Path.DirectorySeparatorChar + itemClass.Name + ".cs";
            }

            if (itemProperty != null) {
                message.FileName = SourceDtoDirectory + itemProperty.Class.Namespace.Name + Path.DirectorySeparatorChar + itemProperty.Class.Name + ".cs";
            }

            message.IsError = isFatal;
            message.Category = category;
            message.Description = objet.ModelFile + " " + string.Concat(GetObjectType(objet), objet.Name == null ? string.Empty : " " + objet.Name) + " : " + error;
            NVortexMessageList.Add(message);
        }

        /// <summary>
        /// Retourne le nom de l'objet.
        /// </summary>
        /// <param name="objet">L'objet.</param>
        /// <returns>Le nom de l'objet.</returns>
        private static string GetObjectType(IModelObject objet) {
            switch (objet.GetType().Name) {
                case "ModelRoot":
                    return "Modèle";
                case "ModelDomain":
                    return "Domaine";
                case "ModelNamespace":
                    return "Namespace";
                case "ModelClass":
                    return "Classe";
                case "ModelProperty":
                    return "Propriété";
                default:
                    return null;
            }
        }
    }
}
