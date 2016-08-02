using System;
using System.Collections.Generic;
using Kinetix.ComponentModel;

namespace Kinetix.Data.SqlClient {

    /// <summary>
    /// Méthodes d'extensions pour les commandes SQL.
    /// </summary>
    public static class SqlCommandExtensions {

        /// <summary>
        /// Exécute la commande et liste une liste d'éléments.
        /// </summary>
        /// <typeparam name="T">Type de l'élément.</typeparam>
        /// <param name="cmd">Commande à exécuter.</param>
        /// <returns>Liste d'éléments.</returns>
        public static ICollection<T> ReadList<T>(this IReadCommand cmd)
            where T : class, new() {
            return CollectionBuilder<T>.ParseCommand(cmd);
        }

        /// <summary>
        /// Exécute une commande et lit un élement.
        /// </summary>
        /// <typeparam name="T">Type de l'élement.</typeparam>
        /// <param name="cmd">Commande à exécuter.</param>
        /// <returns>Objet.</returns>
        public static T ReadItem<T>(this IReadCommand cmd)
            where T : class, new() {
            return CollectionBuilder<T>.ParseCommandForSingleObject(cmd);
        }

        /// <summary>
        /// Exécute une commande et lit le booléen.
        /// </summary>
        /// <param name="cmd">Commande à exécuter.</param>
        /// <returns>Objet.</returns>
        public static bool ReadBoolean(this SqlServerCommand cmd) {
            return Convert.ToBoolean(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Exécute une commande et lit le scalar.
        /// </summary>
        /// <typeparam name="T">Type de l'élement.</typeparam>
        /// <param name="cmd">Commande à exécuter.</param>
        /// <returns>Objet.</returns>
        public static T ReadScalar<T>(this SqlServerCommand cmd) {
            object value = cmd.ExecuteScalar();

            /* Valeur non nulle : on la cast et on la renvoie. */
            if (value != null) {
                return (T)value;
            }

            /* Valeur null : on renvoie seulement si le type est nullable. */
            if (typeof(T).IsClass || Nullable.GetUnderlyingType(typeof(T)) != null) {
                return (T)value;
            }

            /* Valeur null pour un type non nullable : exception. */
            throw new NotSupportedException("Null result is not supported.");
        }
    }
}
