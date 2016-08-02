using System.Collections.Generic;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// API la base de stockage.
    /// </summary>
    public interface IHyperCube {
        /// <summary>
        /// Set de tous les axes (non triés).
        /// </summary>
        ICollection<string> AllAxis {
            get;
        }

        /// <summary>
        /// Retourne la liste des définitions.
        /// </summary>
        ICollection<ICounterDefinition> AllDefinitions {
            get;
        }

        /// <summary>
        /// Nom de l'instance.
        /// </summary>
        string Name {
            get;
        }

        /// <summary>
        /// Indique si le cube peut être remis à zéro.
        /// </summary>
        bool IsResetable {
            get;
        }

        /// <summary>
        /// Retourne cube qui peut être null.
        /// </summary>
        /// <param name="key">CubeKey.</param>
        /// <returns>CounterCube.</returns>
        ICube GetCube(CubeKey key);

        /// <summary>
        /// Remet à zéro les compteurs.
        /// </summary>
        void Reset();
    }
}
