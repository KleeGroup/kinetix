using System.Collections.Generic;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// API la base de stockage.
    /// </summary>
    internal interface IWritableHyperCube : IHyperCube {
        /// <summary>
        /// Ajout d'un processus clôtûré à la base.
        /// </summary>
        /// <param name="process">Processus obligatoirement clôtûré.</param>
        /// <returns>Retourne la durée du processus.</returns>
        long AddProcess(CounterProcess process);

        /// <summary>
        /// Exécute le garbage.
        /// </summary>
        /// <param name="collection">Liste des cubes modifiés depuis le dernier garbage.</param>
        void RunStorage(ICollection<Cube> collection);
    }
}
