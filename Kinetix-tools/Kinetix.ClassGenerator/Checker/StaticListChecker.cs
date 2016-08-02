using System;
using System.Collections.Generic;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.NVortex;
using Kinetix.ComponentModel;
using Kinetix.ServiceModel;

namespace Kinetix.ClassGenerator.Checker {

    /// <summary>
    /// Vérifie la cohérence des données issus de la factory d'initialisation des listes statiques.
    /// </summary>
    internal sealed class StaticListChecker : InitListChecker {

        /// <summary>
        /// Pattern Singleton.
        /// </summary>
        public static readonly StaticListChecker Instance = new StaticListChecker();

        /// <summary>
        /// Vérifie les éléments d'initialisation statiques.
        /// </summary>
        /// <param name="item">Data of the table.</param>
        /// <param name="classe">Table.</param>
        /// <param name="messageList">List of the error message.</param>
        protected override void CheckSpecific(TableInit item, ModelClass classe, ICollection<NVortexMessage> messageList) {
            CheckStereotype(item, classe, messageList);
            CheckPropertyExists(item, classe, messageList);
            CheckFkNoBoucle(item, classe, messageList);
        }

        /// <summary>
        /// Retourne le descripteur de propriété pour la primary key de la classe.
        /// </summary>
        /// <param name="classe">La classe en question.</param>
        /// <param name="beanDefinition">La définition du bean d'initialisation.</param>
        /// <returns>Descripteur de la PK.</returns>
        protected override BeanPropertyDescriptor GetReferenceKeyDescriptor(ModelClass classe, BeanDefinition beanDefinition) {
            if (classe.PrimaryKey.Count != 1) {
                throw new NotSupportedException();
            }

            ModelProperty pkProperty = ((IList<ModelProperty>)classe.PrimaryKey)[0];
            return beanDefinition.Properties[pkProperty.Name];
        }

        /// <summary>
        /// Get the factory stereotype waited.
        /// </summary>
        /// <returns>The factory stereotype waited.</returns>
        protected override string GetStereotype() {
            return Stereotype.Statique;
        }

        /// <summary>
        /// Vérifie que la classe ne pointe pas sur elle même (directement ou indirectement).
        /// </summary>
        /// <param name="item">Model class.</param>
        /// <param name="classe">La classe considérée.</param>
        /// <param name="messageList">Liste des potentiels messages d'erreur.</param>
        private static void CheckFkNoBoucle(TableInit item, ModelClass classe, ICollection<NVortexMessage> messageList) {
            if (IsLinked(classe, classe)) {
                messageList.Add(new NVortexMessage() {
                    Category = Category.Error,
                    IsError = true,
                    Description = "La liste de référence statique de type " + item.ClassName + " pointe sur elle même (directement ou indirectement).",
                    FileName = classe.Namespace.Model.ModelFile
                });
            }
        }

        /// <summary>
        /// Méthode récursive retournant true si la classe de départ est reliée à la classe d'arrivée.
        /// </summary>
        /// <param name="classeDepart">Classe de départ. </param>
        /// <param name="classeArrivee">Classe d'arrivée. </param>
        /// <returns>True or false.</returns>
        private static bool IsLinked(ModelClass classeDepart, ModelClass classeArrivee) {
            foreach (ModelProperty property in classeDepart.PropertyList) {
                if (property.IsFromAssociation) {
                    ModelClass pointedClass = property.DataDescription.ReferenceClass;
                    if (pointedClass.Equals(classeArrivee) || IsLinked(pointedClass, classeArrivee)) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
