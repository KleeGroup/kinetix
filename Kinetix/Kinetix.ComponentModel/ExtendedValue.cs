using System;
using System.Diagnostics.CodeAnalysis;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Valeur et métadonnées associées.
    /// </summary>
    public sealed class ExtendedValue : IComparable {
        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="value">Valeur.</param>
        /// <param name="metadata">Métadonnées.</param>
        public ExtendedValue(object value, object metadata) {
            this.Value = value;
            this.Metadata = metadata;
        }

        /// <summary>
        /// Valeur principale.
        /// </summary>
        public object Value {
            get;
            set;
        }

        /// <summary>
        /// Métadonnées.
        /// </summary>
        public object Metadata {
            get;
            set;
        }

        /// <summary>
        /// Test l'égalité.
        /// </summary>
        /// <param name="source">Opérande de gauche.</param>
        /// <param name="test">Opérande de droite.</param>
        /// <returns>True si équivalent, False sinon.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Les valeurs nulles sont autorisées dans l'implémentation de '=='.")]
        public static bool operator ==(ExtendedValue source, ExtendedValue test) {
            if (object.Equals(source, null) && object.Equals(test, null)) {
                return object.Equals(source, test);
            }

            if (object.Equals(source, null) && !object.Equals(test, null)) {
                return test.Equals(source);
            }

            return source.Equals(test);
        }

        /// <summary>
        /// Test l'inégalité.
        /// </summary>
        /// <param name="source">Opérande de gauche.</param>
        /// <param name="test">Opérande de droite.</param>
        /// <returns>True si équivalent, False sinon.</returns>
        public static bool operator !=(ExtendedValue source, ExtendedValue test) {
            return !(source == test);
        }

        /// <summary>
        /// Test l'inferiorité.
        /// </summary>
        /// <param name="source">Opérande de gauche.</param>
        /// <param name="test">Opérande de droite.</param>
        /// <returns>True si équivalent, False sinon.</returns>
        public static bool operator <(ExtendedValue source, ExtendedValue test) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            return source.CompareTo(test) < 0;
        }

        /// <summary>
        /// Test la supériorité.
        /// </summary>
        /// <param name="source">Opérande de gauche.</param>
        /// <param name="test">Opérande de droite.</param>
        /// <returns>True si équivalent, False sinon.</returns>
        public static bool operator >(ExtendedValue source, ExtendedValue test) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            return source.CompareTo(test) > 0;
        }

        /// <summary>
        /// Indique si l'ExtendedValue est égale a un objet donné.
        /// </summary>
        /// <param name="obj">L'objet à comparer.</param>
        /// <returns>Indique si les objets sont égaux.</returns>
        public override bool Equals(object obj) {
            ExtendedValue other = obj as ExtendedValue;
            return other == null ?
                base.Equals(obj) :
                object.Equals(this.Value, other.Value) && object.Equals(this.Metadata, other.Metadata);
        }

        /// <summary>
        /// Retourne le hash de l'instance.
        /// </summary>
        /// <returns>Le hash.</returns>
        /// <remarks>Requis par l'implémentation d'Equals.</remarks>
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <summary>
        /// Comparaison.
        /// </summary>
        /// <param name="obj">L'objet.</param>
        /// <returns>Retourne 0 si équivalent, -1 si inférieur, 1 si supérieur.</returns>
        public int CompareTo(object obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            ExtendedValue value = (ExtendedValue)obj;
            if (object.Equals(this.Value, value.Value) && object.Equals(this.Metadata, value.Metadata)) {
                return 0;
            }

            if (this.Value == null) {
                return -1;
            }

            return decimal.Compare((decimal)this.Value, (decimal)value.Value);
        }
    }
}
