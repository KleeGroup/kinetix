namespace Fmk.MsBuildCop.Core {

    /// <summary>
    /// Diagnostic.
    /// </summary>
    public class Diagnostic {

        /// <summary>
        /// Descripteur de la règle du diagnostic.
        /// </summary>
        public DiagnosticDescriptor Descriptor { get; private set; }

        /// <summary>
        /// Localisation du diagnostic dans le projet.
        /// </summary>
        public Location Location { get; private set; }

        /// <summary>
        /// Arguments pour le message de diagnostic.
        /// </summary>
        public object[] MessageArgs { get; private set; }

        /// <summary>
        /// Créé un diagnostic.
        /// </summary>
        /// <param name="rule">Descripteur.</param>
        /// <param name="location">Localisation.</param>
        /// <param name="messageArgs">Arguments du message.</param>
        /// <returns>Diagnostic.</returns>
        public static Diagnostic Create(DiagnosticDescriptor rule, Location location, params object[] messageArgs) {
            return new Diagnostic {
                Descriptor = rule,
                Location = location,
                MessageArgs = messageArgs
            };
        }

        /// <summary>
        /// Retourne le message.
        /// </summary>
        /// <returns>Message.</returns>
        public string GetMessage() {
            return string.Format(
                this.Descriptor.MessageFormat,
                this.MessageArgs);
        }
    }
}