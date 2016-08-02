using System.Collections.Generic;
using System.Diagnostics;
using Kinetix.ClassGenerator.Model;
using Kinetix.ComponentModel;

namespace Kinetix.ClassGenerator.Checker {

    /// <summary>
    /// Classe chargée de vérifier l'intégrité du modèle.
    /// </summary>
    internal sealed class ModelRootChecker : AbstractModelChecker {

        /// <summary>
        /// Récupère l'instance.
        /// </summary>
        public static readonly ModelRootChecker Instance = new ModelRootChecker();
        private static readonly Dictionary<string, string> TrigramDictionary = new Dictionary<string, string>();

        /// <summary>
        /// Obtient ou définit la liste des domaines.
        /// </summary>
        public ICollection<IDomain> DomainList {
            get;
            set;
        }

        /// <summary>
        /// Vérifie l'intégrité du modèle.
        /// </summary>
        /// <param name="objet">Le modèle à vérifier.</param>
        public override void Check(IModelObject objet) {
            ModelRoot root = objet as ModelRoot;
            Debug.Assert(root != null, "La racine du modèle est null.");

            if (string.IsNullOrEmpty(root.Name)) {
                RegisterBug(root, "Le nom du modèle n'est pas renseigné.");
            } else if (!IsPascalCaseWithDotValid(root.Name)) {
                RegisterCodeStyle(root, "Le nom du modèle est mal formatté.");
            }

            if (root.Namespaces.Count < 1) {
                RegisterBug(root, "Le modèle ne contient aucun namespace.");
            }

            if (root.CreatedDomains.Count > 0 && root.UsableDomains.Keys.Count != root.CreatedDomains.Count) {
                RegisterBug(objet, "Le fichier " + root.ModelFile + " définit de nouveaux domaines alors qu'il utilise des raccourcis.");
            }

            foreach (ModelDomain domain in root.CreatedDomains) {
                ModelDomainChecker.Instance.Check(domain);
            }

            foreach (IDomain domain in DomainList) {
                if (!root.HasDomainByCode(domain.Name)) {
                    RegisterBug(objet, "Le domaine " + domain.Name + " est déclaré dans la Factory mais pas dans le modèle.");
                }
            }

            foreach (string nsKey in root.Namespaces.Keys) {
                ModelNamespaceChecker.Instance.Check(root.Namespaces[nsKey]);
                foreach (ModelClass classe in root.Namespaces[nsKey].ClassList) {
                    string otherClasse = string.Empty;
                    if (classe.DataContract.IsPersistent) {
                        if (string.IsNullOrEmpty(classe.Trigram)) {
                            RegisterFatalError(classe, "La classe " + classe.Name + " est persistante mais ne définit pas de trigrame.");
                        } else {
                            if (TrigramDictionary.TryGetValue(classe.Trigram, out otherClasse)) {
                                RegisterBug(objet, "Le trigramme " + classe.Trigram + " est utilisé dans la classe " + classe.Name + " et dans la classe " + otherClasse);
                            } else {
                                TrigramDictionary.Add(classe.Trigram, classe.Name);
                            }
                        }
                    }
                }
            }
        }
    }
}
