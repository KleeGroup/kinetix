using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Formatteur de base à utiliser pour tous les formatteurs.
    /// </summary>
    /// <typeparam name="T">Type des données à formater.</typeparam>
    public abstract class AbstractFormatter<T> : TypeConverter, IFormatter<T>, IValueConverter {

        /// <summary>
        /// Retourne la chaine de formattage du formatteur.
        /// </summary>
        public string FormatString {
            get;
            set;
        }

        /// <summary>
        /// Retourne l'unité associée au format.
        /// </summary>
        public virtual string Unit {
            get;
            set;
        }

        /// <summary>
        /// Indique si un type peut être convertit.
        /// </summary>
        /// <param name="context">Contexte.</param>
        /// <param name="sourceType">Type source.</param>
        /// <returns>True si la conversion est possible.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(T);
        }

        /// <summary>
        /// Indique si un type peut être convertit.
        /// </summary>
        /// <param name="context">Contexte.</param>
        /// <param name="destinationType">Type destination.</param>
        /// <returns>True si la conversion est possible.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return destinationType == typeof(T);
        }

        /// <summary>
        /// Convertit depuis un type.
        /// </summary>
        /// <param name="context">Contexte.</param>
        /// <param name="culture">Culture.</param>
        /// <param name="value">Valeur source.</param>
        /// <returns>Valeur cible.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            return this.InternalConvertFromString((string)value);
        }

        /// <summary>
        /// Convertit vers un type.
        /// </summary>
        /// <param name="context">Contexte.</param>
        /// <param name="culture">Culture.</param>
        /// <param name="value">Valeur source.</param>
        /// <param name="destinationType">Type cible.</param>
        /// <returns>Valeur cible.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            return this.InternalConvertToString((T)value);
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The formatter parameter to use.</param>
        /// <param name="culture">The culture to use in the formatter.</param>
        /// <returns>A converted value. If the method returns nullNothingnullptra null reference (Nothing in Visual Basic), the valid null value is used.</returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Mapping des API.")]
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return this.InternalConvertToString((T)value);
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The formatter parameter to use.</param>
        /// <param name="culture">The culture to use in the formatter.</param>
        /// <returns>A converted value. If the method returns nullNothingnullptra null reference (Nothing in Visual Basic), the valid null value is used.</returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Mapping des API.")]
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return this.InternalConvertFromString((string)value);
        }

        /// <summary>
        /// Convertit un string en type primitif.
        /// </summary>
        /// <param name="text">Données sous forme string.</param>
        /// <returns>Donnée typée.</returns>
        /// <exception cref="System.FormatException">En cas d'erreur de convertion.</exception>
        T IFormatter<T>.ConvertFromString(string text) {
            return this.InternalConvertFromString(text);
        }

        /// <summary>
        /// Convertit un type primitif en string.
        /// </summary>
        /// <param name="value">Données typées.</param>
        /// <returns>Données sous forme de string.</returns>
        string IFormatter<T>.ConvertToString(T value) {
            return this.InternalConvertToString(value);
        }

        /// <summary>
        /// Convertit un string en type primitif.
        /// </summary>
        /// <param name="text">Données sous forme string.</param>
        /// <returns>Donnée typée.</returns>
        /// <exception cref="System.FormatException">En cas d'erreur de convertion.</exception>
        protected abstract T InternalConvertFromString(string text);

        /// <summary>
        /// Convertit un type primitif en string.
        /// </summary>
        /// <param name="value">Données typées.</param>
        /// <returns>Données sous forme de string.</returns>
        protected abstract string InternalConvertToString(T value);
    }
}
