using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Kinetix.ClassGenerator.Model;

namespace Kinetix.ClassGenerator.Checker {

    /// <summary>
    /// Classe chargée de vérifier l'intégrité d'une classe du modèle.
    /// </summary>
    internal sealed class ModelClassChecker : AbstractModelChecker {

        /// <summary>
        /// Récupère l'instance.
        /// </summary>
        public static readonly ModelClassChecker Instance = new ModelClassChecker();

        /// <summary>
        /// Constructeur.
        /// </summary>
        public ModelClassChecker() {
            ClassNameList = new Collection<string>();
        }

        /// <summary>
        /// Collection des noms de classe.
        /// </summary>
        public ICollection<string> ClassNameList {
            get;
            private set;
        }

        /// <summary>
        /// Enregistre la classe.
        /// </summary>
        /// <param name="classe">La classe a enregistre.</param>
        /// <returns>Retourne <code>False</code> si la classe est deja enregistrée.</returns>
        public bool RegisterClass(ModelClass classe) {
            if (ClassNameList.Contains(classe.Name)) {
                return false;
            }

            ClassNameList.Add(classe.Name);
            return true;
        }

        /// <summary>
        /// Vérifie l'intégrité de classe.
        /// </summary>
        /// <param name="objet">La classe a vérifier.</param>
        public override void Check(IModelObject objet) {
            ModelClass classe = objet as ModelClass;
            Debug.Assert(classe != null, "La classe est null.");
            CheckNaming(classe);
            CheckComment(classe);
            CheckStereotype(classe);
            CheckPersistence(classe);
            CheckProperties(classe);
        }

        /// <summary>
        /// Vérifie le commentaire de la classe.
        /// </summary>
        /// <param name="classe">La classe en question.</param>
        private static void CheckComment(ModelClass classe) {
            if (string.IsNullOrEmpty(classe.Comment)) {
                RegisterDoc(classe, "Pas de commentaire.");
            } else if (!IsCommentValid(classe.Comment)) {
                RegisterDoc(classe, "La documentation de la classe n'est pas valide.");
            }
        }

        /// <summary>
        /// Vérifie le stéréotype de la classe.
        /// </summary>
        /// <param name="classe">La classe en question.</param>
        private static void CheckStereotype(ModelClass classe) {
            if (classe.Stereotype == Stereotype.Statique) {
                ICollection<ModelProperty> pkList = classe.PrimaryKey;
                foreach (ModelProperty property in pkList) {
                    if (!property.Name.EndsWith("Code", StringComparison.Ordinal)) {
                        RegisterCodeStyle(classe, "La classe " + classe.Name + " est de stéréotype Statique. Les propriétés définissant la clé primaire doivent se terminer par \"Code\".");
                    }
                }
            }
        }

        /// <summary>
        /// Vérifie les propriétés liées à la persistence.
        /// </summary>
        /// <param name="classe">La classe en question.</param>
        private static void CheckPersistence(ModelClass classe) {
            if (classe.Namespace.HasPersistentClasses && !classe.DataContract.IsPersistent) {
                RegisterBug(classe, "Cette classe doit être persistée.");
            }

            if (classe.DataContract.IsPersistent && classe.PropertyList.Count < 1) {
                RegisterBug(classe, "La classe doit contenir au moins 1 propriété.");
            }

            bool isPersistante = classe.Namespace.Name.EndsWith("DataContract", StringComparison.Ordinal);
            bool isFonctionnel = !classe.Namespace.Name.EndsWith("DataContract", StringComparison.Ordinal) && classe.Namespace.Name.EndsWith("Contract", StringComparison.Ordinal);
            if (isPersistante) {
                if (!classe.DataContract.IsPersistent) {
                    RegisterBug(classe, "Cette classe est dans un namespace persitant, elle doit être persistante.");
                }
            } else if (isFonctionnel) {
                if (classe.DataContract.IsPersistent) {
                    RegisterBug(classe, "Cette classe est dans un namespace fonctionnel, elle doit être non persistante.");
                }
            } else {
                RegisterBug(classe, "La classe est dans un namespace non valide (le code doit être suffixé par DataContract ou Contract).");
            }

            if (isPersistante && classe.DataContract.IsPersistent) {
                if (string.IsNullOrEmpty(classe.DataContract.Name)) {
                    RegisterDoc(classe, "Le nom persistent n'est pas renseigné.");
                } else if (!IsDataBaseTableNameCaseValid(classe.DataContract.Name)) {
                    RegisterCodeStyle(classe, "Le nom persistent de la table [" + classe.DataContract.Name + "] n'est pas valide.");
                }

                ICollection<string> fieldNameList = new Collection<string>();
                foreach (ModelProperty property in classe.PropertyList) {
                    if (property.IsPersistent) {
                        if (fieldNameList.Contains(property.DataMember.Name)) {
                            RegisterBug(classe, "Le champ persistent [" + property.Name + "] est déjà utilisé.");
                        } else {
                            fieldNameList.Add(property.DataMember.Name);
                        }
                    }
                }

                if (!classe.HasPrimaryKey) {
                    RegisterBug(classe, "Aucune clé primaire n'est définie pour cette classe persistente.");
                }

                if (classe.ParentClass != null) {
                    RegisterBug(classe, "Cette classe persistente n'a pas le droit d'hériter de la classe [" + classe.ParentClass.Name + "].");
                }
            } else {
                foreach (ModelProperty property in classe.PrimaryKey) {
                    if (property.DataDescription.IsPrimaryKey && property.IsPersistent) {
                        RegisterBug(classe, "Une clé primaire est définie pour cette classe non persistente.");
                    }
                }
            }
        }

        /// <summary>
        /// Vérifie les propriétés de la classe.
        /// </summary>
        /// <param name="classe">La classe en question.</param>
        private static void CheckProperties(ModelClass classe) {
            ICollection<string> codeNameList = new Collection<string>();
            Dictionary<string, ModelProperty> dictionnary = new Dictionary<string, ModelProperty>();
            var referencePropertyWithNoRoleMap = new Dictionary<string, ModelProperty>();

            foreach (ModelProperty property in classe.PropertyList) {
                if (codeNameList.Contains(property.Name)) {
                    RegisterBug(classe, "La propriété [" + property.Name + "] est déjà utilisée.");
                } else {
                    codeNameList.Add(property.Name);
                }

                ModelPropertyChecker.Instance.Check(property);

                string referenceType = property.DataDescription.ReferenceType;
                bool hasRole = !string.IsNullOrEmpty(property.Role);
                if (hasRole) {
                    if (property.Role.StartsWith(classe.Name, StringComparison.Ordinal)) {
                        RegisterCodeStyle(classe, "Le role de propriété [" + property.Name + "] ne doit pas commencer par le nom de la classe.");
                    }

                    if (dictionnary.ContainsKey(referenceType)) {
                        dictionnary[referenceType] = null;
                    } else {
                        dictionnary.Add(referenceType, property);
                    }
                } else {
                    if (!string.IsNullOrEmpty(referenceType)) {
                        referencePropertyWithNoRoleMap.Add(referenceType, property);
                    }

                    if (property.DataDescription.ReferenceClass == classe) {
                        if (property.IsFromComposition && !property.IsCollection) {
                            RegisterFatalError(classe, "La propriété [" + property.Name + "] compose l'objet lui-même et provoque une récursivité infinie.");
                        } else {
                            RegisterCodeStyle(classe, "La propriété [" + property.Name + "] référence la même classe " + referenceType + ", elle doit avoir un rôle.");
                        }
                    }
                }

                ModelClass parent = SearchParentClassWithIdenticalPropertyName(property, classe);
                if (parent != null) {
                    RegisterFatalError(classe, "La propriété [" + property.Name + "] est déjà définie dans la classe parente " + parent.Name + ", il faut la renommer avec un nom différent.");
                }
            }

            /* On autorise une propriété avec un rôle et une propriété sans rôle. */
            foreach (ModelProperty property in dictionnary.Values) {
                if (property != null && property.DataDescription.ReferenceClass != classe &&
                        !IsParentGetIdenticalReference(property.DataDescription.ReferenceClass, classe) && !referencePropertyWithNoRoleMap.ContainsKey(property.DataDescription.ReferenceType)) {
                    RegisterCodeStyle(classe, "La propriété [" + property.Name + "] est la seule à référencer la classe " + property.DataDescription.ReferenceType + ", elle ne doit pas avoir de rôle.");
                }
            }
        }

        /// <summary>
        /// Return true if a parent of classe has a reference to referencedClasse.
        /// </summary>
        /// <param name="referencedClasse">ReferencedClasse.</param>
        /// <param name="classe">Current class.</param>
        /// <returns>Boolean.</returns>
        private static bool IsParentGetIdenticalReference(ModelClass referencedClasse, ModelClass classe) {
            if (classe.ParentClass == null) {
                return false;
            }

            foreach (ModelProperty prop in classe.ParentClass.PropertyList) {
                if (prop.DataDescription.ReferenceClass == referencedClasse) {
                    return true;
                }
            }

            return IsParentGetIdenticalReference(referencedClasse, classe.ParentClass);
        }

        /// <summary>
        /// Search if a class ancestor of a class has a property with the same name of a given property.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="classe">Current class.</param>
        /// <returns>The parent class with the same property, <code>null</code> if not found.</returns>
        private static ModelClass SearchParentClassWithIdenticalPropertyName(ModelProperty property, ModelClass classe) {
            if (classe.ParentClass == null) {
                return null;
            }

            ModelClass parent = classe.ParentClass;
            foreach (ModelProperty prop in parent.PropertyList) {
                if (prop.Name == property.Name) {
                    return parent;
                }
            }

            return SearchParentClassWithIdenticalPropertyName(property, parent);
        }

        /// <summary>
        /// Vérifie le nomage de la classe.
        /// </summary>
        /// <param name="classe">La classe en question.</param>
        private void CheckNaming(ModelClass classe) {
            if (string.IsNullOrEmpty(classe.Name)) {
                RegisterDoc(classe, "Le nom de la classe n'est pas renseignée.");
            } else if (!IsPascalCaseValid(classe.Name)) {
                RegisterCodeStyle(classe, "Le code de la classe est mal formaté.");
            }

            if (!RegisterClass(classe)) {
                RegisterBug(classe, "Cette classe existe déjà.");
            }
        }
    }
}
