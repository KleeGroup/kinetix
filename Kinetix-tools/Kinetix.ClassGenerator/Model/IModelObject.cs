namespace Kinetix.ClassGenerator.Model {

    /// <summary>
    /// Interface contractualisant les données d'un modele objet.
    /// </summary>
    internal interface IModelObject {

        /// <summary>
        /// Nom du modèle définissant l'objet.
        /// </summary>
        string ModelFile {
            get;
            set;
        }

        /// <summary>
        /// Le nom de la propriété.
        /// </summary>
        string Name {
            get;
            set;
        }
    }
}
