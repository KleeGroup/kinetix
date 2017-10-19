using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kinetix.ClassGenerator.Model {

    /// <summary>
    /// Cette classe représente un namespace du modèle.
    /// </summary>
    public sealed class ModelNamespace : IModelObject {

        /// <summary>
        /// Constructeur de namespace.
        /// </summary>
        public ModelNamespace() {
            ClassList = new Collection<ModelClass>();
        }

        /// <summary>
        /// Objet indiquant si le namespace est une référence externe.
        /// </summary>
        public bool IsExternal {
            get;
            set;
        }

        /// <summary>
        /// Nom du modèle définissant l'objet.
        /// </summary>
        public string ModelFile {
            get;
            set;
        }

        /// <summary>
        /// Retourne le nom du namespace.
        /// </summary>
        public string Label {
            get;
            set;
        }

        /// <summary>
        /// Retourne le nom (code) du namespace.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Retourne le commentaire associé au package.
        /// </summary>
        public string Comment {
            get;
            set;
        }

        /// <summary>
        /// Retourne l'utilisateur créateur du namespace.
        /// </summary>
        public string Creator {
            get;
            set;
        }

        /// <summary>
        /// Liste des classes du namespace.
        /// </summary>
        public ICollection<ModelClass> ClassList {
            get;
            private set;
        }

        /// <summary>
        /// Le modele du namespace.
        /// </summary>
        public ModelRoot Model {
            get;
            set;
        }

        /// <summary>
        /// Indique si les classes du namespaces doivent être persistentes.
        /// </summary>
        public bool HasPersistentClasses {
            get {
                return Name.EndsWith("DataContract", StringComparison.CurrentCulture);
            }
        }

        /// <summary>
        /// Ajoute une classe à la liste.
        /// </summary>
        /// <param name="classe">La classe à ajouter.</param>
        /// <exception cref="System.ArgumentNullException">Si la classe fournie en paramètre est null.</exception>
        public void AddClass(ModelClass classe) {
            if (classe == null) {
                throw new ArgumentNullException("classe");
            }
            
            ClassList.Add(classe);
        }
    }
}
