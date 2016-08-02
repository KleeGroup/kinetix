using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// MethodBase extension method.
    /// </summary>
    public static class MethodBaseExtensions {

        /// <summary>
        /// Returns the list of custom attributes defined on current method or on any corresponding declaration in implemented interfaces.
        /// </summary>
        /// <typeparam name="T">Attribut type.</typeparam>
        /// <param name="method">Methode.</param>
        /// <param name="inherit">Inherit attributes from base class.</param>
        /// <returns>List of custom attributes.</returns>
        public static T[] GetCustomAttributes<T>(this MethodBase method, bool inherit) {
            return GetCustomAttributes(method, typeof(T), inherit).Select(attr => (T)attr).ToArray();
        }

        /// <summary>
        /// Returns the list of custom attributes defined on current method or on any corresponding declaration in implemented interfaces.
        /// </summary>
        /// <param name="method">Methode.</param>
        /// <param name="attributeType">Attribut type.</param>
        /// <param name="inherit">Inherit attributes from base class.</param>
        /// <returns>List of custom attributes.</returns>
        private static object[] GetCustomAttributes(MethodBase method, Type attributeType, bool inherit) {
            var attributeCollection = new Collection<object>();
            method.GetCustomAttributes(attributeType, inherit).Apply(attributeCollection.Add);

            foreach (var interfaceType in method.DeclaringType.GetInterfaces()) {
                MethodInfo interfaceMethod = interfaceType.GetMethod(method.Name);
                if (interfaceMethod != null) {
                    interfaceMethod.GetCustomAttributes(attributeType, inherit).Apply(attributeCollection.Add);
                }
            }

            var attributeArray = new object[attributeCollection.Count];
            attributeCollection.CopyTo(attributeArray, 0);
            return attributeArray;
        }

        /// <summary>
        /// Call Action on each collection item.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="enumerable">Collection.</param>
        /// <param name="function">Action.</param>
        private static void Apply<T>(this IEnumerable<T> enumerable, Action<T> function) {
            foreach (var item in enumerable) {
                function.Invoke(item);
            }
        }
    }
}
