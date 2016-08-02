using System;
using System.Linq.Expressions;

namespace Kinetix.ComponentModel.Facets {

    /// <summary>
    /// Ligne de facettage.
    /// </summary>
    /// <typeparam name="TResource">Type de l'expression évaluée.</typeparam>
    public class Heading<TResource> {

        /// <summary>
        /// Valeur de la facette.
        /// </summary>
        public string Value {
            get;
            set;
        }

        /// <summary>
        /// Nombre d'éléments correspondant à la facette.
        /// </summary>
        public int MatchCount {
            get;
            set;
        }

        /// <summary>
        /// Eexpression pour évaluée.
        /// </summary>
        public Expression<Func<TResource, bool>> Expression {
            get;
            set;
        }
    }
}
