using System.Collections.Generic;
using System.Diagnostics;
using Kinetix.ClassGenerator.Model;

namespace Kinetix.ClassGenerator.Checker {

    /// <summary>
    /// Classe chargée de vérifier l'intégrité d'un Domain du modèle.
    /// </summary>
    internal sealed class ModelDomainChecker : AbstractModelChecker {

        /// <summary>
        /// Récupère l'instance.
        /// </summary>
        public static readonly ModelDomainChecker Instance = new ModelDomainChecker();

        /// <summary>
        /// Constructeur.
        /// </summary>
        private ModelDomainChecker() {
            DomainList = new Dictionary<string, ModelDomain>();
        }

        /// <summary>
        /// Collection des noms de trigrams.
        /// </summary>
        public IDictionary<string, ModelDomain> DomainList {
            get;
            private set;
        }

        /// <summary>
        /// Enregistre le dommaine.
        /// </summary>
        /// <param name="domaine">Le domaine a enregistrer.</param>
        /// <returns>Retourne <code>false</code> si le domaine est déja enregistré.</returns>
        public bool RegisterDomain(ModelDomain domaine) {
            if (DomainList.ContainsKey(domaine.Code)) {
                return false;
            }

            DomainList.Add(domaine.Code, domaine);
            return true;
        }

        /// <summary>
        /// Vérifie l'intégrité du domaine.
        /// </summary>
        /// <param name="objet">Le domaine à vérifier.</param>
        public override void Check(IModelObject objet) {
            ModelDomain domaine = objet as ModelDomain;
            Debug.Assert(domaine != null, "Le domaine est null.");
            if (string.IsNullOrEmpty(domaine.Code)) {
                RegisterBug(domaine, "Le code du domaine n'est pas renseigné.");
            }

            if (!IsDomaineNameValid(domaine.Code)) {
                RegisterCodeStyle(domaine, "Le code de la classe est mal formaté.");
            }

            if (!RegisterDomain(domaine)) {
                RegisterBug(domaine, "Le domaine existe déjà.");
            }
        }
    }
}
