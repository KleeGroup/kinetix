namespace Kinetix.Monitoring.Manager {
    /// <summary>
    /// Module ou Composant.
    /// Un module possède un paramétrage et un état interne,
    /// ce qui lui permet d'offrir des services.
    ///
    /// L'usage du module permet d'enrichir des statistiques.
    ///
    /// Le module permet de représenter
    /// - ce qu'il est, ce qu'il fait (ex : module de cache permet de...)
    /// - les statistiques d'usage (ex: 99 % d'utilisation du cache)
    /// - son état interne (ex : 153 Mo utilisé dont 3 Mo sur disque)
    /// - son paramétrage. (ex : Implémentation eh cache avec les paramètres suivants ...)
    ///
    /// Les statistiques et l'état varient au fil du temps.
    /// Le paramétrage doit être stable et nécessite une reconfiguration. (Nouvelle version d'une application)
    /// Le contrat du composant (ce qu'il est, ce qu'il fait) doit évidemment être très stable.
    /// </summary>
    public interface IManager {
        /// <summary>
        /// Retourne un objet décrivant le service.
        /// </summary>
        IManagerDescription Description {
            get;
        }

        /// <summary>
        /// Libération des ressources consommées par le manager lors du undeploy.
        /// Exemples : connexions, thread, flux.
        /// </summary>
        void Close();
    }
}
