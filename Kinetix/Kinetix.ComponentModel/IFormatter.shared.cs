namespace Kinetix.ComponentModel {

    /// <summary>
    /// Interface de définition d'un formateur. Un formateur est responsable
    /// de convertir un type de données primitif en string ou un string en
    /// type de données primitif.
    /// </summary>
    /// <typeparam name="T">Type des données à formater.</typeparam>
    public interface IFormatter<T> : IFormatter {

        /// <summary>
        /// Convertit un string en type primitif.
        /// </summary>
        /// <param name="text">Données sous forme string.</param>
        /// <returns>Donnée typée.</returns>
        /// <exception cref="System.FormatException">En cas d'erreur de convertion.</exception>
        T ConvertFromString(string text);

        /// <summary>
        /// Convertit un type primitif en string.
        /// </summary>
        /// <param name="value">Données typées.</param>
        /// <returns>Données sous forme de string.</returns>
        string ConvertToString(T value);
    }

    /// <summary>
    /// Interface de déinitiion d'un formateur.
    ///     - Unite
    ///     - Chaine de formattage.
    /// </summary>
    public interface IFormatter {

        /// <summary>
        /// Retourne l'unité associée au format.
        /// </summary>
        string Unit {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la chaine de formattage.
        /// </summary>
        string FormatString {
            get;
            set;
        }
    }
}
