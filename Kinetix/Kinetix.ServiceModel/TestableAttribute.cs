using System;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Indique qu'un test unitaire doit être généré pour la méthode décorée.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestableAttribute : Attribute {
    }
}
