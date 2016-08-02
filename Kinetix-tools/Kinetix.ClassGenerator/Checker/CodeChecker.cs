using System.Collections.Generic;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.NVortex;
using Kinetix.ComponentModel;

namespace Kinetix.ClassGenerator.Checker {

    /// <summary>
    /// Classe chargée de la vérification des différentes règles sur les classes.
    /// </summary>
    internal static class CodeChecker {

        /// <summary>
        /// Methode de controle du modele.
        /// </summary>
        /// <param name="modelList">Liste des modeles parsés.</param>
        /// <param name="domainList">Liste des domaines chargés depuis l'assembly de déclaration.</param>
        /// <returns>La liste des erreurs.</returns>
        public static ICollection<NVortexMessage> Check(ICollection<ModelRoot> modelList, ICollection<IDomain> domainList) {
            ModelRootChecker modelChecker = ModelRootChecker.Instance;
            modelChecker.DomainList = domainList;
            foreach (ModelRoot model in modelList) {
                modelChecker.Check(model);
            }

            return AbstractModelChecker.NVortexMessageList;
        }
    }
}
