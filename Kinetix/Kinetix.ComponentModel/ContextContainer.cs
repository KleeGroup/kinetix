using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Container for current Context.
    /// </summary>
    public sealed class ContextContainer {

        /// <summary>
        /// Instance.
        /// </summary>
        private static readonly ContextContainer _instance = new ContextContainer();

        [ThreadStatic]
        private IDictionary _items = null;

        /// <summary>
        /// Current context.
        /// </summary>
        public static ContextContainer Current {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Map container.
        /// </summary>
        private IDictionary Items {
            get {
                if (HttpContext.Current != null) {
                    return HttpContext.Current.Items;
                }

                if (_items == null) {
                    _items = new Dictionary<object, object>();
                }

                return _items;
            }
        }

        /// <summary>
        /// Get or set a value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Value.</returns>
        public object this[object key] {
            get {
                return Items[key];
            }

            set {
                Items[key] = value;
            }
        }
    }
}
