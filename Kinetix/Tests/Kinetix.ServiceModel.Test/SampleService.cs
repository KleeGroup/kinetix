using System;
using System.Collections.Generic;

namespace Kinetix.ServiceModel.Test {
    /// <summary>
    /// Implémentation de ISampleContract pour les tests.
    /// </summary>
    public class SampleService : ISampleContract {
        /// <summary>
        /// Méthode sans argurment.
        /// Retourne une exception.
        /// </summary>
        public void Method1() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Méthode avec un argument et un retour.
        /// Retourne la valeur du paramètre a.
        /// </summary>
        /// <param name="a">Argument 1.</param>
        /// <returns>Retour.</returns>
        public int Method2(int a) {
            return a;
        }

        /// <summary>
        /// Méthode avec 5 arguments.
        /// Retourne la valeur du paramètre o4.
        /// </summary>
        /// <param name="s">Argument 1.</param>
        /// <param name="o">Argument 2.</param>
        /// <param name="o2">Argument 3.</param>
        /// <param name="o3">Argument 4.</param>
        /// <param name="o4">Argument 5.</param>
        /// <returns>Retour.</returns>
        public object Method3(string s, object o, object o2, object o3, object o4) {
            return o4;
        }

        /// <summary>
        /// Retourne une liste d'objets.
        /// </summary>
        /// <returns>Liste d'objets.</returns>
        public List<object> GetObjectList() {
            List<object> list = new List<object>();
            list.Add(Guid.Empty);
            return list;
        }

        /// <summary>
        /// Retourne une liste d'int.
        /// </summary>
        /// <returns>Liste d'objets.</returns>
        public ICollection<int> GetIntList() {
            return new List<int>();
        }
    }
}
