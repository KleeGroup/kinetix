namespace Kinetix.ClassGenerator.Model {

    /// <summary>
    /// Représentation simple d'un élément d'une liste statique
    /// </summary>
    public class StaticListElement {

        /// <summary>
        /// Code de l'élèment
        /// </summary>
        public object Code {
            get;
            set;
        }

        /// <summary>
        /// Type du code de l'élément
        /// </summary>
        public string CodeType {
            get;
            set;
        }

        /// <summary>
        /// Libellé de l'élèment
        /// </summary>
        public string Libelle {
            get;
            set;
        }
    }
}
