using System;
using System.Collections.Generic;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.NVortex;
using Kinetix.ComponentModel;
using Kinetix.ServiceModel;

namespace Kinetix.ClassGenerator.Checker {

    /// <summary>
    /// Classe abstraite permettant l'initialisation des listes statiques.
    /// </summary>
    internal abstract class InitListChecker {

        /// <summary>
        /// Dictionnaire présentant en Clef la classe reference et en valeur les initialisations liées à cette table.
        /// </summary>
        public Dictionary<ModelClass, TableInit> DictionaryItemInit {
            get;
            set;
        }

        /// <summary>
        /// Vérifie les éléments d'initialisation statiques.
        /// </summary>
        /// <param name="modelRootList">Liste des modeles parsés..</param>
        /// <param name="tableInitList">Liste des éléments d'initialisation des listes statiques.</param>
        /// <returns>Liste des erreurs.</returns>
        public ICollection<NVortexMessage> Check(ICollection<ModelRoot> modelRootList, ICollection<TableInit> tableInitList) {
            if (modelRootList == null) {
                throw new ArgumentNullException("modelRootList");
            }

            ICollection<NVortexMessage> messageList = new List<NVortexMessage>();
            if (tableInitList == null) {
                return messageList;
            }

            DictionaryItemInit = new Dictionary<ModelClass, TableInit>();
            foreach (TableInit item in tableInitList) {
                ModelClass classe = GetModelClassByName(item.ClassName, modelRootList);
                if (classe == null) {
                    messageList.Add(HandleClassNotExists(item.ClassName, string.Empty, item.FactoryName));
                } else {
                    CheckSpecific(item, classe, messageList);
                    if (messageList.Count == 0) {
                        HandleCorrectInit(classe, item);
                    }
                }
            }

            return messageList;
        }

        /// <summary>
        /// Construit un NVortexMessage si la classe du bean d'init n'existe pas.
        /// </summary>
        /// <param name="className">Nom de la classe.</param>
        /// <param name="fileName">Nom du modele objet.</param>
        /// <param name="factoryName">Nom de la classe de définition des éléments de la liste.</param>
        /// <returns>Le message correctement formaté pour CruiseControl.</returns>
        protected static NVortexMessage HandleClassNotExists(string className, string fileName, string factoryName) {
            return new NVortexMessage() {
                Category = Category.Error,
                IsError = true,
                Description = factoryName + " définit une initialisation pour le type " + className + " qui n'existe pas.",
                FileName = fileName
            };
        }

        /// <summary>
        /// Vérifie que toutes les propriétés sont saisis dans le bean d'init et que le bean d'init ne définit pas des propriétés inexistantes.
        /// </summary>
        /// <param name="item">Le bean d'init.</param>
        /// <param name="classe">La classe considérée.</param>
        /// <param name="messageList">Liste des potentiels messages d'erreurs.</param>
        protected static void CheckPropertyExists(TableInit item, ModelClass classe, ICollection<NVortexMessage> messageList) {
            foreach (ItemInit itemInit in item.ItemInitList) {
                BeanDefinition definition = BeanDescriptor.GetDefinition(itemInit.Bean);
                Dictionary<string, bool> visitedProperties = new Dictionary<string, bool>();
                foreach (ModelProperty property in classe.PersistentPropertyList) {
                    if (definition.Properties.Contains(property.Name) || property.DataDescription.IsPrimaryKey) {
                        visitedProperties.Add(property.Name, true);
                    } else {
                        messageList.Add(new NVortexMessage() {
                            Category = Category.Error,
                            IsError = true,
                            Description = item.FactoryName + " définit une initialisation pour le type " + item.ClassName + " mais ne précise pas la valeur de la propriété " + property.Name + ".",
                            FileName = classe.Namespace.Model.ModelFile
                        });
                    }
                }

                foreach (BeanPropertyDescriptor property in definition.Properties) {
                    if (!visitedProperties.ContainsKey(property.PropertyName)) {
                        messageList.Add(new NVortexMessage() {
                            Category = Category.Error,
                            IsError = true,
                            Description = item.FactoryName + " définit une initialisation pour le type " + item.ClassName + " avec la propriété " + property.PropertyName + " qui n'existe pas.",
                            FileName = classe.Namespace.Model.ModelFile
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Vérifie que le bean d'init concerne une liste de reference.
        /// </summary>
        /// <param name="item">Le bean d'init.</param>
        /// <param name="classe">La classe considérée.</param>
        /// <param name="messageList">Liste des potentiels messages d'erreur.</param>
        protected void CheckStereotype(TableInit item, ModelClass classe, ICollection<NVortexMessage> messageList) {
            if (classe.Stereotype != GetStereotype()) {
                messageList.Add(new NVortexMessage() {
                    Category = Category.Error,
                    IsError = true,
                    Description = item.FactoryName + " définit une initialisation pour le type " + item.ClassName + " qui n'est pas une liste de référence.",
                    FileName = classe.Namespace.Model.ModelFile
                });
            }
        }

        /// <summary>
        /// Get the factory stereotype waited.
        /// </summary>
        /// <returns>The factory stereotype waited.</returns>
        protected abstract string GetStereotype();

        /// <summary>
        /// Prend en charge l'alimentation de la classe pour les valeurs d'initialisation.
        /// </summary>
        /// <param name="classe">Classe concernée.</param>
        /// <param name="item">Bean d'initialisation.</param>
        protected void HandleCorrectInit(ModelClass classe, TableInit item) {
            if (!DictionaryItemInit.ContainsKey(classe)) {
                DictionaryItemInit.Add(classe, item);
            }

            foreach (ItemInit itemInit in item.ItemInitList) {
                BeanDefinition definition = BeanDescriptor.GetDefinition(itemInit.Bean);
                BeanPropertyDescriptor propertyDescriptor = GetReferenceKeyDescriptor(classe, definition);
                object propertyValue = propertyDescriptor.GetValue(itemInit.Bean);
                if (propertyDescriptor.PrimitiveType == typeof(string)) {
                    propertyValue = "\"" + propertyValue + "\"";
                }

                BeanPropertyDescriptor libelleDescriptor = GetLibelleDescriptor(classe, definition);
                string libelle = null;
                if (libelleDescriptor != null) {
                    libelle = (string)libelleDescriptor.GetValue(itemInit.Bean);
                } else {
                    libelle = itemInit.VarName;
                }

                classe.ConstValues.Add(itemInit.VarName, new StaticListElement() { Code = propertyValue, Libelle = libelle, CodeType = propertyDescriptor.PrimitiveType.ToString() });
            }
        }

        private BeanPropertyDescriptor GetLibelleDescriptor(ModelClass classe, BeanDefinition beanDefinition) {

            foreach (BeanPropertyDescriptor prop in beanDefinition.Properties) {
                if (prop.PropertyName.Equals("Libelle") || prop.PropertyName.Equals("Description")) {
                    return prop;
                }
            }

            return null;
        }

        /// <summary>
        /// Methode a surcharger. 
        /// </summary>
        /// <param name="classe">Classe.</param>
        /// <param name="definition">Definition.</param>
        /// <returns>Returns property reference.</returns>
        protected abstract BeanPropertyDescriptor GetReferenceKeyDescriptor(ModelClass classe, BeanDefinition definition);

        /// <summary>
        /// To override.
        /// </summary>
        /// <param name="item">Data of the table.</param>
        /// <param name="classe">Table.</param>
        /// <param name="messageList">List of the error message.</param>
        protected abstract void CheckSpecific(TableInit item, ModelClass classe, ICollection<NVortexMessage> messageList);

        /// <summary>
        /// Retourne une classe a partir de son nom dans le modele.
        /// </summary>
        /// <param name="name">Nom de la classe.</param>
        /// <param name="modelRootList">Liste des modeles objet.</param>
        /// <returns>La classe si elle existe, <code>Null</code> sinon.</returns>
        private static ModelClass GetModelClassByName(string name, IEnumerable<ModelRoot> modelRootList) {
            foreach (ModelRoot modelRoot in modelRootList) {
                foreach (string nsKey in modelRoot.Namespaces.Keys) {
                    foreach (ModelClass classe in modelRoot.Namespaces[nsKey].ClassList) {
                        if (classe.Name == name) {
                            return classe;
                        }
                    }
                }
            }

            return null;
        }
    }
}
