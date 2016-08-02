using System;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Attribut marquant une colonne de table gérant la traduction, dans les listes de référence.
    /// <para>Le ClassGenerator pose cet attribut sur les propriétés de DTO en fonction des domaines et des
    /// annotations dans le modèle.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TranslatableAttribute : Attribute {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public TranslatableAttribute() {
        }
    }
}