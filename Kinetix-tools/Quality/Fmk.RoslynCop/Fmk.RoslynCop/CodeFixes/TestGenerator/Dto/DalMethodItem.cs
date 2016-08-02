using System.Collections.Generic;
using System.Linq;

namespace Fmk.RoslynCop.CodeFixes.TestGenerator.Dto {

    /// <summary>
    /// Représente une méthode de DAL.
    /// </summary>
    public class DalMethodItem {

        /// <summary>
        /// Nom de l'assemblée qui contient la DAL.
        /// </summary>
        public string DalAssemblyName {
            get;
            set;
        }

        /// <summary>
        /// Nom de la classe de la DAL.
        /// </summary>
        public string DalClassName {
            get;
            set;
        }

        /// <summary>
        /// Nom de la méthode de la DAL.
        /// </summary>
        public string DalMethodName {
            get;
            set;
        }

        /// <summary>
        /// Espace de nom de la classe de la DAL.
        /// </summary>
        public string DalNamespace {
            get;
            set;
        }

        /// <summary>
        /// Liste des paramètres de la méthode de la DAL.
        /// </summary>
        public ICollection<DalMethodParam> Params {
            get;
            set;
        }

        /// <summary>
        /// Liste à plat des valeurs des paramètres.
        /// </summary>
        public string FlatParams {
            get {
                return string.Join(", ", this.Params.Select(x => x.Value));
            }
        }

        /// <summary>
        /// Liste des usings spécifiques aux projets.
        /// </summary>
        public ICollection<string> SpecificUsings {
            get;
            set;
        }
    }
}
