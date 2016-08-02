using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Kinetix.ClassGenerator.Model;

namespace Kinetix.ClassGenerator.Checker {

    /// <summary>
    /// Classe chargée de vérifier l'intégrité d'un Namespace du modèle.
    /// </summary>
    internal sealed class ModelNamespaceChecker : AbstractModelChecker {

        /// <summary>
        /// Récupère l'instance.
        /// </summary>
        public static readonly ModelNamespaceChecker Instance = new ModelNamespaceChecker();

        /// <summary>
        /// Constructeur.
        /// </summary>
        private ModelNamespaceChecker() {
            NamespaceNameList = new Collection<string>();
        }

        /// <summary>
        /// Collection des noms de namespace.
        /// </summary>
        public ICollection<string> NamespaceNameList {
            get;
            private set;
        }

        /// <summary>
        /// Vérifie l'intégrité du namespace.
        /// </summary>
        /// <param name="objet">Le namespace à vérifier.</param>
        public override void Check(IModelObject objet) {
            ModelNamespace nmspace = objet as ModelNamespace;
            Debug.Assert(nmspace != null, "Le namespace est null.");
            if (string.IsNullOrEmpty(nmspace.Name)) {
                RegisterBug(nmspace, "Le nom du namespace n'est pas renseigné.");
            } else if (!IsPascalCaseValid(nmspace.Name)) {
                RegisterCodeStyle(nmspace, "Le nom du namespace est mal formatté.");
            }

            if (!RegisterNamespace(nmspace)) {
                RegisterBug(nmspace, "Le namespace \"" + nmspace.Name + "\" est déjà enregistré.");
            }

            if (!nmspace.Name.EndsWith("Contract", StringComparison.Ordinal)) {
                RegisterBug(nmspace, "Le nom du namespace n'est pas valide.");
            }

            if (nmspace.ClassList.Count < 1) {
                RegisterBug(nmspace, "Le namespace n'a pas de classes.");
            }

            foreach (ModelClass classe in nmspace.ClassList) {
                ModelClassChecker.Instance.Check(classe);
            }
        }

        /// <summary>
        /// Enregistre le namespace.
        /// </summary>
        /// <param name="nmspace">Le namespace à enregistrer.</param>
        /// <returns><code>False</code>si le namespace est déjà enregistré.</returns>
        private bool RegisterNamespace(ModelNamespace nmspace) {
            if (NamespaceNameList.Contains(nmspace.Name)) {
                return false;
            }

            NamespaceNameList.Add(nmspace.Name);
            return true;
        }
    }
}
