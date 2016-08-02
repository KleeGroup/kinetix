namespace Fmk.MsBuildCop.Core {

    /// <summary>
    /// Descripteur de diagnostic.
    /// </summary>
    public class DiagnosticDescriptor {

        /// <summary>
        /// Id du diagnostic.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Titre du diagnostic.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Catégorie du diagnostic.
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// Format du message.
        /// </summary>
        public string MessageFormat { get; private set; }

        /// <summary>
        /// Créé un descripteur.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <param name="title">Titre.</param>
        /// <param name="category">Catégorie.</param>
        /// <param name="messageFormat">Format du message.</param>
        /// <returns>Diagnostic.</returns>
        public static DiagnosticDescriptor Create(string id, string title, string category, string messageFormat) {
            return new DiagnosticDescriptor {
                Id = id,
                Title = title,
                Category = category,
                MessageFormat = messageFormat
            };
        }
    }
}