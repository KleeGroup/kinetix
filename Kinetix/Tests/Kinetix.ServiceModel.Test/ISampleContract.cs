using System.Collections.Generic;

namespace Kinetix.ServiceModel.Test {
    /// <summary>
    /// Contrat d'exemple pour le test de la factory de proxy.
    /// </summary>
    public interface ISampleContract {
        /// <summary>
        /// Méthode sans argurment.
        /// </summary>
        void Method1();

        /// <summary>
        /// Méthode avec un argument et un retour.
        /// </summary>
        /// <param name="a">Argument 1.</param>
        /// <returns>Retour.</returns>
        int Method2(int a);

        /// <summary>
        /// Méthode avec 5 arguments.
        /// </summary>
        /// <param name="s">Argument 1.</param>
        /// <param name="o">Argument 2.</param>
        /// <param name="o2">Argument 3.</param>
        /// <param name="o3">Argument 4.</param>
        /// <param name="o4">Argument 5.</param>
        /// <returns>Retour.</returns>
        object Method3(string s, object o, object o2, object o3, object o4);

        /// <summary>
        /// Retourne une liste d'objets.
        /// </summary>
        /// <returns>Liste d'objets.</returns>
        List<object> GetObjectList();

        /// <summary>
        /// Retourne une liste d'int.
        /// </summary>
        /// <returns>Liste d'int.</returns>
        ICollection<int> GetIntList();
    }
}
