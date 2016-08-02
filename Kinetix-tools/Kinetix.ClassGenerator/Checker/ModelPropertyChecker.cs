using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Kinetix.ClassGenerator.Model;
using Kinetix.ComponentModel;

namespace Kinetix.ClassGenerator.Checker {

    /// <summary>
    /// Classe chargée de vérifier l'intégrité d'un Propery du modèle.
    /// </summary>
    internal sealed class ModelPropertyChecker : AbstractModelChecker {

        /// <summary>
        /// Récupère l'instance.
        /// </summary>
        public static readonly ModelPropertyChecker Instance = new ModelPropertyChecker();

        private static readonly Regex ConstRegexList = new Regex("^ICollection<[A-Za-z0-9]+>$");

        /// <summary>
        /// Vérifie l'intégrité de la propriété.
        /// </summary>
        /// <param name="objet">La propriété à vérifier.</param>
        public override void Check(IModelObject objet) {
            ModelProperty property = objet as ModelProperty;
            Debug.Assert(property != null, "La propriété est null.");
            CheckComment(property);
            CheckPersistentProperty(property);
            CheckLibelle(property);
            CheckNaming(property);
            CheckDomain(property);
        }

        /// <summary>
        /// Vérifie la présence d'un commentaire sur la propriété.
        /// </summary>
        /// <param name="property">Le propriété en question.</param>
        private static void CheckComment(ModelProperty property) {
            if (string.IsNullOrEmpty(property.Name)) {
                RegisterBug(property.Class, "Le nom de la propriété n'est pas renseigné.");
            } else if ((property.DataDescription.Domain == null || property.DataDescription.Domain.Code != DomainManager.AliasDomain) && !IsPascalCaseValid(property.Name)) {
                RegisterCodeStyle(property.Class, "Le code de la propriété [" + property.Name + "] n'est pas valide.");
            } else if ((property.DataDescription.Domain != null && property.DataDescription.Domain.Code == DomainManager.AliasDomain) && !IsAliasPropertyNameValid(property.Name)) {
                RegisterCodeStyle(property.Class, "Le code de la propriété [" + property.Name + "] n'est pas valide.");
            }
        }

        private static bool IsAliasPropertyNameValid(string property) {
            string[] s = property.Split('_');
            return s.Length == 2 && IsPascalCaseValid(s[0]) && IsPascalCaseValid(s[0]);
        }

        /// <summary>
        /// Vérifie les propriétés de persistence sur la propriété.
        /// </summary>
        /// <param name="property">La propriété en question.</param>
        private static void CheckPersistentProperty(ModelProperty property) {
            if (property.Class.DataContract.IsPersistent && property.IsPersistent) {
                if (string.IsNullOrEmpty(property.DataMember.Name)) {
                    RegisterDoc(property.Class, "Le champ persistent de la propriété [" + property.Name + "] n'est pas renseigné.");
                } else if (!IsDataBaseFieldNameCaseValid(property.DataMember.Name)) {
                    RegisterBug(property.Class, "Le champ persistent [" + property.DataMember.Name + "] de la propriété [" + property.Name + "] est mal formaté.");
                }
            }
        }

        /// <summary>
        /// Vérifie la présence du libellé sur la propriété.
        /// </summary>
        /// <param name="property">La propriété en question.</param>
        private static void CheckLibelle(ModelProperty property) {
            if (string.IsNullOrEmpty(property.DataDescription.Libelle)) {
                RegisterBug(property.Class, "Le libellé de la propriété [" + property.Name + "] n'est pas renseigné.");
            }
        }

        /// <summary>
        /// Vérifie le nommage de la propriété.
        /// </summary>
        /// <param name="property">La propriété en question.</param>
        private static void CheckNaming(ModelProperty property) {
            if (ConstRegexList.IsMatch(property.DataType)) {
                if (!property.Name.EndsWith("List", StringComparison.Ordinal)) {
                    RegisterCodeStyle(property.Class, "La propriété [" + property.Name + "] retourne une collection. Elle doit se terminer par \"List\".");
                }
            }

            if (property.DataType == "bool" && ((!GeneratorParameters.IsProjetUesl && !(property.Name.StartsWith("Is", StringComparison.Ordinal) || (!property.IsPersistent && property.Name.StartsWith("Has", StringComparison.Ordinal)))) || (GeneratorParameters.IsProjetUesl && !property.Name.StartsWith("Flag", StringComparison.Ordinal)))) {
                RegisterCodeStyle(property.Class, "Le propriété [" + property.Name + "] de type " + property.DataType + " devrait commencer par " + (GeneratorParameters.IsProjetUesl ? "Flag" : "Is") + ".");
            }
        }

        /// <summary>
        /// Vérifie le domaine de la propriété.
        /// </summary>
        /// <param name="property">La propriété en question.</param>
        private static void CheckDomain(ModelProperty property) {
            if (property.DataDescription.Domain == null && property.IsPrimitive) {
                RegisterBug(property.Class, "La propriété [" + property.Name + "] est de type primitif et n'a pas de domaine.");
            } else if (property.DataDescription.Domain != null) {
                if (!ModelDomainChecker.Instance.DomainList.ContainsKey(property.DataDescription.Domain.Code)) {
                    RegisterBug(property.Class, "Le domaine de la propriété [" + property.Name + "] n'existe pas.");
                } else {
                    ModelDomain domaine = ModelDomainChecker.Instance.DomainList[property.DataDescription.Domain.Code];
                    if (!domaine.DataType.Equals(property.DataType) && !("ICollection<" + domaine.DataType + ">").Equals(property.DataType)) {
                        RegisterBug(property.Class, "Le datatype de la propriété [" + property.Name + "] ne correspond pas au datatype du domaine [" + domaine.Code + "].");
                    }
                }
            }
        }
    }
}
