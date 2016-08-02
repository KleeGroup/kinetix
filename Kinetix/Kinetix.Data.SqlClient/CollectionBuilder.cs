using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Kinetix.ComponentModel;

namespace Kinetix.Data.SqlClient {

    /// <summary>
    /// Constructeur de collection.
    /// </summary>
    /// <typeparam name="TData">Type des données à construire.</typeparam>
    /// <typeparam name="TCollection">Type de collection à construire.</typeparam>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Nommage distinct non applicable ici.")]
    public static class CollectionBuilder<TData, TCollection>
        where TCollection : class
        where TData : class, TCollection, new() {
        /// <summary>
        /// Parse un DataReader et crèe une liste l'éléments.
        /// </summary>
        /// <param name="collection">Collection initiale.</param>
        /// <param name="cmd">Commande a éxecuter.</param>
        /// <returns>Liste l'éléments.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static ICollection<TCollection> ParseCommand(ICollection<TCollection> collection, IReadCommand cmd) {
            if (cmd == null) {
                throw new ArgumentNullException("cmd");
            }

            using (IDataReader reader = cmd.ExecuteReader()) {
                return ParseReader(collection, reader);
            }
        }

        /// <summary>
        /// Parse un DataReader et crèe une liste l'éléments.
        /// </summary>
        /// <param name="collection">Collection initiale.</param>
        /// <param name="cmd">Commande a éxecuter.</param>
        /// <returns>Liste l'éléments.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static ICollection<TCollection> ParseCommand(ICollection<TCollection> collection, IDbCommand cmd) {
            if (cmd == null) {
                throw new ArgumentNullException("cmd");
            }

            using (IDataReader reader = cmd.ExecuteReader()) {
                return ParseReader(collection, reader);
            }
        }

        /// <summary>
        /// Parse un DataReader et crèe une liste l'éléments.
        /// </summary>
        /// <param name="collection">Collection initiale.</param>
        /// <param name="reader">Reader à lire.</param>
        /// <returns>Liste l'éléments.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        private static ICollection<TCollection> ParseReader(ICollection<TCollection> collection, IDataReader reader) {
            IDataRecordAdapter<TData> adapter = null;
            ICollection<TCollection> list = collection ?? new List<TCollection>();
            while (reader.Read()) {
                if (adapter == null) {
                    adapter = DataRecordAdapterManager<TData>.CreateAdapter(reader);
                }

                list.Add(adapter.Read(new TData(), reader));
            }

            return list;
        }
    }

    /// <summary>
    /// Constructeur de collection.
    /// </summary>
    /// <typeparam name="T">Type de collection à construire.</typeparam>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Nommage distinct non applicable ici.")]
    public static class CollectionBuilder<T>
        where T : class, new() {
        /// <summary>
        /// Parse un DataReader et crèe une liste l'éléments.
        /// </summary>
        /// <param name="cmd">Commande a executer.</param>
        /// <returns>Liste l'éléments.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static ICollection<T> ParseCommand(IDbCommand cmd) {
            return ParseCommand(null, cmd);
        }

        /// <summary>
        /// Parse un DataReader et crèe une liste l'éléments.
        /// </summary>
        /// <param name="cmd">Commande a executer.</param>
        /// <returns>Liste l'éléments.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static ICollection<T> ParseCommand(IReadCommand cmd) {
            return ParseCommand(null, cmd);
        }

        /// <summary>
        /// Parse un DataReader et crèe une liste l'éléments.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="cmd">Commande a executer.</param>
        /// <returns>Liste l'éléments.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static ICollection<T> ParseCommand(ICollection<T> collection, IDbCommand cmd) {
            if (cmd == null) {
                throw new ArgumentNullException("cmd");
            }

            using (IDataReader reader = cmd.ExecuteReader()) {
                return ParseReader(collection, reader);
            }
        }

        /// <summary>
        /// Parse un DataReader et crèe une liste l'éléments.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="cmd">Commande a executer.</param>
        /// <returns>Liste l'éléments.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static ICollection<T> ParseCommand(ICollection<T> collection, IReadCommand cmd) {
            if (cmd == null) {
                throw new ArgumentNullException("cmd");
            }

            SqlServerCommand command = cmd as SqlServerCommand;
            if (command != null) {
                if (command.QueryParameters != null) {
                    command.QueryParameters.RemapSortColumn(typeof(T));
                }
            }

            using (IDataReader reader = cmd.ExecuteReader()) {
                return ParseReader(collection, reader);
            }
        }

        /// <summary>
        /// Parse un DataReader et crèe une liste l'éléments.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="reader">Reader à lire.</param>
        /// <returns>Liste l'éléments.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static ICollection<T> ParseReader(ICollection<T> collection, IDataReader reader) {
            if (reader == null) {
                throw new ArgumentNullException("reader");
            }

            IDataRecordAdapter<T> adapter = null;
            ICollection<T> list = collection ?? new List<T>();
            while (reader.Read()) {
                if (adapter == null) {
                    adapter = DataRecordAdapterManager<T>.CreateAdapter(reader);
                }

                list.Add(adapter.Read(null, reader));
            }

            return list;
        }

        /// <summary>
        /// Lit un objet dans le reader.
        /// </summary>
        /// <param name="cmd">Commande a executer.</param>
        /// <returns>Objet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static T ParseCommandForSingleObject(IDbCommand cmd) {
            return ParseCommandForSingleObject(null, cmd, false);
        }

        /// <summary>
        /// Lit un objet dans le reader.
        /// </summary>
        /// <param name="destination">Le bean à charger.</param>
        /// <param name="cmd">Commande a executer.</param>
        /// <returns>Objet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static T ParseCommandForSingleObject(T destination, IDbCommand cmd) {
            return ParseCommandForSingleObject(destination, cmd, false);
        }

        /// <summary>
        /// Lit un objet dans le reader.
        /// </summary>
        /// <param name="cmd">Commande a executer.</param>
        /// <returns>Objet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static T ParseCommandForSingleObject(IReadCommand cmd) {
            return ParseCommandForSingleObject(null, cmd, false);
        }

        /// <summary>
        /// Lit un objet dans le reader.
        /// </summary>
        /// <param name="destination">Le bean à charger.</param>
        /// <param name="cmd">Commande a executer.</param>
        /// <returns>Objet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static T ParseCommandForSingleObject(T destination, IReadCommand cmd) {
            return ParseCommandForSingleObject(destination, cmd, false);
        }

        /// <summary>
        /// Lit un objet dans le reader.
        /// </summary>
        /// <param name="cmd">Commande a executer.</param>
        /// <param name="returnNullIfZeroRow">Indique si une valeur nulle doit être retournée si il n'y a aucune ligne.</param>
        /// <returns>Objet.</returns>
        [SuppressMessage("Cpd", "Cpd", Justification = "Pas de factorisation possible.")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static T ParseCommandForSingleObject(IDbCommand cmd, bool returnNullIfZeroRow) {
            return ParseCommandForSingleObject(null, cmd, returnNullIfZeroRow);
        }

        /// <summary>
        /// Lit un objet dans le reader.
        /// </summary>
        /// <param name="destination">Le bean à charger.</param>
        /// <param name="cmd">Commande a executer.</param>
        /// <param name="returnNullIfZeroRow">Indique si une valeur nulle doit être retournée si il n'y a aucune ligne.</param>
        /// <returns>Objet.</returns>
        [SuppressMessage("Cpd", "Cpd", Justification = "Pas de factorisation possible.")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static T ParseCommandForSingleObject(T destination, IDbCommand cmd, bool returnNullIfZeroRow) {
            if (cmd == null) {
                throw new ArgumentNullException("cmd");
            }

            try {
                using (IDataReader reader = cmd.ExecuteReader()) {
                    return ParseReader(destination, returnNullIfZeroRow, reader);
                }
            } catch (NotSupportedException e) {
                StringBuilder parameters = new StringBuilder();
                foreach (IDataParameter parameter in cmd.Parameters) {
                    parameters.AppendFormat("{0} = {1}, ", parameter.ParameterName, parameter.Value);
                }

                throw new CollectionBuilderException("Paramètres de la commande: " + parameters, e);
            }
        }

        /// <summary>
        /// Lit un objet dans le reader.
        /// </summary>
        /// <param name="cmd">Commande a executer.</param>
        /// <param name="returnNullIfZeroRow">Indique si une valeur nulle doit être retournée si il n'y a aucune ligne.</param>
        /// <returns>Objet.</returns>
        [SuppressMessage("Cpd", "Cpd", Justification = "Pas de factorisation possible.")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static T ParseCommandForSingleObject(IReadCommand cmd, bool returnNullIfZeroRow) {
            return ParseCommandForSingleObject(null, cmd, returnNullIfZeroRow);
        }

        /// <summary>
        /// Lit un objet dans le reader.
        /// </summary>
        /// <param name="destination">Le bean à charger.</param>
        /// <param name="cmd">Commande a executer.</param>
        /// <param name="returnNullIfZeroRow">Indique si une valeur nulle doit être retournée si il n'y a aucune ligne.</param>
        /// <returns>Objet.</returns>
        [SuppressMessage("Cpd", "Cpd", Justification = "Pas de factorisation possible.")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        public static T ParseCommandForSingleObject(T destination, IReadCommand cmd, bool returnNullIfZeroRow) {
            if (cmd == null) {
                throw new ArgumentNullException("cmd");
            }

            try {
                using (IDataReader reader = cmd.ExecuteReader()) {
                    return ParseReader(destination, returnNullIfZeroRow, reader);
                }
            } catch (NotSupportedException e) {
                StringBuilder parameters = new StringBuilder();
                foreach (IDataParameter parameter in cmd.Parameters) {
                    parameters.AppendFormat("{0} = {1}, ", parameter.ParameterName, parameter.Value);
                }

                throw new CollectionBuilderException("Paramètres de la commande: " + parameters, e);
            }
        }

        /// <summary>
        /// Lit un objet dans le reader.
        /// </summary>
        /// <param name="destination">Le bean à charger.</param>
        /// <param name="returnNullIfZeroRow">Indique si une valeur nulle doit être retournée si il n'y a aucune ligne.</param>
        /// <param name="reader">Commande a executer.</param>
        /// <returns>Objet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Typage fort nécessaire.")]
        private static T ParseReader(T destination, bool returnNullIfZeroRow, IDataReader reader) {
            IDataRecordAdapter<T> adapter = null;
            while (reader.Read()) {
                if (adapter == null) {
                    adapter = DataRecordAdapterManager<T>.CreateAdapter(reader);
                } else {
                    throw new CollectionBuilderException("Too many rows selected !");
                }

                destination = adapter.Read(destination, reader);
            }

            if (adapter == null) {
                if (returnNullIfZeroRow) {
                    return default(T);
                }

                throw new CollectionBuilderException("Zero row selected !");
            }

            return destination;
        }
    }
}
