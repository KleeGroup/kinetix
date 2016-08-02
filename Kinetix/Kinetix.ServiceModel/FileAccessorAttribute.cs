using System;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Attribut indiquant qu'une méthode permet la récupération d'un fichier.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class FileAccessorAttribute : Attribute {
    }
}
