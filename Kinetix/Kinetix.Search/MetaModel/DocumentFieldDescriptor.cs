﻿using System;
using System.ComponentModel;
using Kinetix.Search.ComponentModel;

namespace Kinetix.Search.MetaModel {

    /// <summary>
    /// Classe de description d'une propriété.
    /// </summary>
    [Serializable]
    public sealed class DocumentFieldDescriptor {

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <param name="fieldName">Nom du champ dans le document.</param>
        /// <param name="propertyType">Type de la propriété.</param>
        /// <param name="category">Domaine de la propriété.</param>
        internal DocumentFieldDescriptor(string propertyName, string fieldName, Type propertyType, SearchFieldCategory category) {
            this.PropertyName = propertyName;
            this.FieldName = fieldName;
            this.PropertyType = propertyType;
            this.Category = category;
        }

        /// <summary>
        /// Obtient le nom de la propriété.
        /// </summary>
        public string PropertyName {
            get;
            private set;
        }

        /// <summary>
        /// Nom du champ dans le document (camel case).
        /// </summary>
        public string FieldName {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le type de la propriété.
        /// </summary>
        public Type PropertyType {
            get;
            private set;
        }

        /// <summary>
        /// Catégorie de field.
        /// </summary>
        public SearchFieldCategory Category {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la valeur de la propriété pour un objet.
        /// </summary>
        /// <param name="bean">Objet.</param>
        /// <returns>Valeur.</returns>
        public object GetValue(object bean) {
            object value = TypeDescriptor.GetProperties(bean)[this.PropertyName].GetValue(bean);
            return value;
        }

        /// <summary>
        /// Définit la valeur de la propriété pour un objet.
        /// </summary>
        /// <param name="bean">Objet.</param>
        /// <param name="value">Valeur.</param>
        public void SetValue(object bean, object value) {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(bean)[this.PropertyName];
            descriptor.SetValue(bean, value);
        }

        /// <summary>
        /// Retourne une chaîne de caractère représentant l'objet.
        /// </summary>
        /// <returns>Chaîne de caractère représentant l'objet.</returns>
        public override string ToString() {
            return this.PropertyName;
        }
    }
}
