using System.Collections.Generic;

namespace Fmk.RoslynCop.CodeFixes.TestGenerator.Dto {

    /// <summary>
    /// Paramètre d'une méthode de DAL.
    /// </summary>
    public class DalMethodParam {

        /// <summary>
        /// Nom du paramètre.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Expression de la valeur du paramètre.
        /// Rendue telle quelle dans le template de la classe de test.
        /// </summary>
        public string Value {
            get;
            set;
        }

        /// <summary>
        /// Indique si le paramètre est de type primitif (entier / string / etc.).
        /// </summary>
        public bool IsPrimitive {
            get;
            set;
        }

        /// <summary>
        /// Liste des usings spécifiques aux projets.
        /// </summary>
        public ICollection<string> SpecificUsings {
            get;
            set;
        }

            = new List<string>();
    }
}
