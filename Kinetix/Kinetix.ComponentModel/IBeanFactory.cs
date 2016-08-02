namespace Kinetix.ComponentModel {

    /// <summary>
    /// Interface presentant les methodes accessibles sur BeanFactory.
    /// </summary>
    public interface IBeanFactory {
        /// <summary>
        /// Clone une bean.
        /// </summary>
        /// <param name="bean">Le bean à cloner.</param>
        /// <returns>Le bean cloné.</returns>
        object CloneBean(object bean);

        /// <summary>
        /// Clone un bean.
        /// </summary>
        /// <param name="source">Source de données.</param>
        /// <param name="target">Cible de la copie.</param>
        void CloneBean(object source, object target);

        /// <summary>
        /// Reset les valeurs du bean a null, clear des listes.
        /// </summary>
        /// <param name="bean">Le bean à reseter.</param>
        void ResetBean(object bean);
    }
}
