using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;

namespace Kinetix.Data.SqlClient.Test {
    /// <summary>
    /// Collection de paramètres pour les commandes de test.
    /// </summary>
    public sealed class TestDbParameterCollection : DbParameterCollection {

        private readonly List<TestDbParameter> _list = new List<TestDbParameter>();
        private readonly Dictionary<string, TestDbParameter> _index = new Dictionary<string, TestDbParameter>();

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        internal TestDbParameterCollection() {
        }

        /// <summary>
        /// Retourne le nombre d'éléments de la collection.
        /// </summary>
        public override int Count {
            get {
                return _list.Count;
            }
        }

        /// <summary>
        /// Indique si la collection a une taille fixe.
        /// </summary>
        public override bool IsFixedSize {
            get {
                return false;
            }
        }

        /// <summary>
        /// Indique si la collection est en lecture seule.
        /// </summary>
        public override bool IsReadOnly {
            get {
                return false;
            }
        }

        /// <summary>
        /// Indique si la collection est synchronisée.
        /// </summary>
        public override bool IsSynchronized {
            get {
                return false;
            }
        }

        /// <summary>
        /// Retourne la racine de synchronisation.
        /// </summary>
        public override object SyncRoot {
            get {
                return null;
            }
        }

        /// <summary>
        /// Retourne un énumérateur sur la collection.
        /// </summary>
        /// <returns>Enumrérateur sur la collection.</returns>
        public override IEnumerator GetEnumerator() {
            return ((IList)_list).GetEnumerator();
        }

        /// <summary>
        /// Ajoute une liste de paramètres.
        /// </summary>
        /// <param name="values">Paramètres.</param>
        public override void AddRange(Array values) {
            foreach (object param in values) {
                this.Add(param);
            }
        }

        /// <summary>
        /// Ajoute un paramètre la collection.
        /// </summary>
        /// <param name="parameter">Nouveau paramètre.</param>
        /// <returns>Paramètre ajouté.</returns>
        public TestDbParameter Add(TestDbParameter parameter) {
            _list.Add(parameter);
            _index.Add(parameter.ParameterName, parameter);
            return parameter;
        }

        /// <summary>
        /// Ajoute un paramètre.
        /// </summary>
        /// <param name="value">Parametre.</param>
        /// <returns>Position.</returns>
        public override int Add(object value) {
            TestDbParameter parameter = (TestDbParameter)value;
            this.Add(parameter);
            return _list.Count - 1;
        }

        /// <summary>
        /// Efface tous paramètres.
        /// </summary>
        public override void Clear() {
            _list.Clear();
            _index.Clear();
        }

        /// <summary>
        /// Indique si la collection contient un paramètre.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <returns>True si la collection contient le paramètre.</returns>
        public override bool Contains(string parameterName) {
            return _index.ContainsKey(parameterName);
        }

        /// <summary>
        /// Indique si la collection contient un paramètre.
        /// </summary>
        /// <param name="item">Paramètre.</param>
        /// <returns>True si la collection contient le paramètre.</returns>
        public override bool Contains(object item) {
            return _list.Contains((TestDbParameter)item);
        }

        /// <summary>
        /// Copie la collection dans un tableau d'objet.
        /// Cette méthode n'est pas supportée.
        /// </summary>
        /// <param name="array">Tableau de sortie.</param>
        /// <param name="arrayIndex">Index de début de copie.</param>
        public override void CopyTo(Array array, int arrayIndex) {
            ((IList)_list).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Retourne la position d'un paramètre dans la collection.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <returns>Postion du paramètre ou -1 si il est absent de la collection.</returns>
        public override int IndexOf(string parameterName) {
            return _list.IndexOf(_index[parameterName]);
        }

        /// <summary>
        /// Retourne la position d'un paramètre dans la collection.
        /// </summary>
        /// <param name="item">Paramètre.</param>
        /// <returns>Postion du paramètre ou -1 si il est absent de la collection.</returns>
        public override int IndexOf(object item) {
            return _list.IndexOf((TestDbParameter)item);
        }

        /// <summary>
        /// Ajoute un paramètre à la collection.
        /// </summary>
        /// <param name="index">Index d'insertion (0 pour insérer en première position).</param>
        /// <param name="item">Paramètre.</param>
        public override void Insert(int index, object item) {
            TestDbParameter parameter = (TestDbParameter)item;
            _list.Insert(index, parameter);
            _index.Add(parameter.ParameterName, parameter);
        }

        /// <summary>
        /// Supprime un paramètre de la collection.
        /// </summary>
        /// <param name="item">Paramètre.</param>
        public override void Remove(object item) {
            TestDbParameter parameter = (TestDbParameter)item;
            _list.Remove(parameter);
            _index.Remove(parameter.ParameterName);
        }

        /// <summary>
        /// Supprime un paramètre de la collection.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        public override void RemoveAt(string parameterName) {
            int index = IndexOf(parameterName);
            _index.Remove(parameterName);
            _list.RemoveAt(index);
        }

        /// <summary>
        /// Supprime un paramètre de la collection.
        /// </summary>
        /// <param name="index">Indice du paramètre.</param>
        public override void RemoveAt(int index) {
            this.Remove(_list[index]);
        }

        /// <summary>
        /// Définit le paramètre à l'index Index.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="value">Paramètre.</param>
        protected override void SetParameter(int index, DbParameter value) {
            TestDbParameter param = _list[index];
            _list[index] = (TestDbParameter)value;
            _index.Remove(param.ParameterName);
            _index.Add(value.ParameterName, (TestDbParameter)value);
        }

        /// <summary>
        /// Définit un paramètre à partir de son nom.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <param name="value">Paramètre.</param>
        protected override void SetParameter(string parameterName, DbParameter value) {
            int index = _list.IndexOf((TestDbParameter)value);
            _list[index] = (TestDbParameter)value;
            _index[parameterName] = (TestDbParameter)value;
        }

        /// <summary>
        /// Retourne un paramètre à partir de l'index.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <returns>Paramètre.</returns>
        protected override DbParameter GetParameter(int index) {
            return _list[index];
        }

        /// <summary>
        /// Retourne un paramètre à partir de son nom.
        /// </summary>
        /// <param name="parameterName">Nom.</param>
        /// <returns>Paramètre.</returns>
        protected override DbParameter GetParameter(string parameterName) {
            return _index[parameterName];
        }
    }
}
