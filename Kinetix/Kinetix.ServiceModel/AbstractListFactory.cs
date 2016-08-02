using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Classe de base pour l'initialisation des listes statiques.
    /// </summary>
    public abstract class AbstractListFactory {

        /// <summary>
        /// Map contenant en :
        ///     * Clef : le nom du Type.
        ///     * Valeur : une map avec en clef le nom de la constante statique, en valeur l'objet initialisé pour générér le script d'insert.
        /// </summary>
        private readonly Dictionary<string, TableInit> _mapInitialisation = new Dictionary<string, TableInit>();

        /// <summary>
        /// Obtient l'ensemble des éléments d'initialisation définis.
        /// </summary>
        public ReadOnlyCollection<TableInit> Items {
            get {
                return new ReadOnlyCollection<TableInit>(_mapInitialisation.Keys.Select(itemKey => _mapInitialisation[itemKey]).ToList());
            }
        }

        /// <summary>
        /// Returns true if initialize static list.
        /// </summary>
        /// <returns>Boolean.</returns>
        public abstract bool IsStatic {
            get;
        }

        /// <summary>
        /// Initialise les listes statiques.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Ajoute l'initialisation d'une table.
        /// </summary>
        /// <param name="tableName">Nom de la table à initialiser.</param>
        /// <returns>La table initialisée.</returns>
        protected TableInit AddTable(string tableName) {
            if (string.IsNullOrEmpty(tableName)) {
                throw new ArgumentNullException("tableName");
            }

            if (_mapInitialisation.ContainsKey(tableName)) {
                throw new NotSupportedException();
            }

            foreach (string itemTableName in
                _mapInitialisation.Keys.Where(itemTableName => string.Compare(itemTableName, tableName, StringComparison.OrdinalIgnoreCase) > 0)) {
                throw new NotSupportedException("L'initialisation des listes statiques doit être effectuée dans l'ordre alphabétique, l'élément " + itemTableName + " précède l'élément " + tableName + ".");
            }

            _mapInitialisation.Add(tableName, new TableInit { ClassName = tableName, FactoryName = this.GetType().Name });
            return _mapInitialisation[tableName];
        }
    }
}
