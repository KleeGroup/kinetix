using System;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Interface d'un provider de métadonnées.
    /// </summary>
    public interface IMetadataTypeProvider {
        /// <summary>
        /// Retourne le type de la classe portant les métadonnées pour une instance.
        /// </summary>
        /// <param name="instance">Instance à décorer.</param>
        /// <returns>Type de la classe de métadonnées.</returns>
        Type GetMetadataType(object instance);
    }

    /// <summary>
    /// Interface d'un provider de métadonnées.
    /// </summary>
    /// <typeparam name="T">Type de la classe à décorer.</typeparam>
    public interface IMetadataTypeProvider<T> : IMetadataTypeProvider {
        /// <summary>
        /// Retourne le type de la classe portant les métadonnées pour une instance de T.
        /// </summary>
        /// <param name="instance">Instance à décorer.</param>
        /// <returns>Type de la classe de métadonnées.</returns>
        Type GetMetadataType(T instance);
    }
}
