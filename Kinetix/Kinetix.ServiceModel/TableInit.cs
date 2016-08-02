using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Classe encapsulant les données d'une initialisation de liste statique.
    /// </summary>
    public sealed class TableInit {

        private readonly Dictionary<string, ItemInit> _dictionary = new Dictionary<string, ItemInit>();

        /// <summary>
        /// Nom de la classe statique cible.
        /// </summary>
        public string ClassName {
            get;
            set;
        }

        /// <summary>
        /// Retourne ou définit la factory contenant l'initialisation de la liste.
        /// </summary>
        public string FactoryName {
            get;
            set;
        }

        /// <summary>
        /// Liste des items d'initialisation de la table.
        /// </summary>
        public ReadOnlyCollection<ItemInit> ItemInitList {
            get {
                return new ReadOnlyCollection<ItemInit>(_dictionary.Values.ToList());
            }
        }

        /// <summary>
        /// Ajoute l'élément d'initialisation à la table.
        /// </summary>
        /// <param name="varName">Le nom de la constante statique.</param>
        /// <param name="bean">Le bean d'initialisation.</param>
        /// <returns>Le TableInit pour appel chaîné.</returns>
        /// <exception cref="System.NotSupportedException">Si la table contient déja un élément d'initialisation pour la valeur.</exception>
        public TableInit AddItem(string varName, object bean) {
            if (string.IsNullOrEmpty(varName)) {
                throw new ArgumentNullException("varName");
            }

            if (bean == null) {
                throw new ArgumentNullException("bean");
            }

            if (_dictionary.ContainsKey(varName)) {
                throw new NotSupportedException("La table contient déja une initialisation nommée " + varName);
            }

            _dictionary.Add(varName, new ItemInit { VarName = varName, Bean = bean });

            return this;
        }
    }
}
