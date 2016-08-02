namespace Kinetix.ServiceModel {

    /// <summary>
    /// Classe encapsulant les données d'une initialisation d'un élément de liste statique.
    /// </summary>
    public sealed class ItemInit {

        /// <summary>
        /// Nom de la constante statique d'accès.
        /// </summary>
        public string VarName {
            get;
            set;
        }

        /// <summary>
        /// Bean initialisé pour l'insert SQL.
        /// </summary>
        public object Bean {
            get;
            set;
        }
    }
}
