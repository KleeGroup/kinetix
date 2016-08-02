using System;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Attribut précisant que cet objet est supprimé logiquement et non physiquement.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class LogicalDeleteAttribute : Attribute {
    }
}
