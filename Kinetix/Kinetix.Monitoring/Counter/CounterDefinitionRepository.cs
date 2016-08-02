using System.Collections.Generic;
using System.Globalization;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Contient la définition de tous les compteurs.
    /// Les compteurs sont définis indépendamment des bases de données.
    /// </summary>
    internal sealed class CounterDefinitionRepository {
        private readonly IDictionary<string, CounterDefinition> _counterDefinitionMap = new Dictionary<string, CounterDefinition>();

        /// <summary>
        /// Liste des instances de CounterDefinition créée par createDefinition.
        /// </summary>
        internal ICollection<CounterDefinition> Values {
            get {
                return _counterDefinitionMap.Values;
            }
        }

        /// <summary>
        /// Liste des instances de CounterDefinition créée par createDefinition.
        /// </summary>
        internal ICollection<CounterDefinition> SortValues {
            get {
                List<CounterDefinition> list = new List<CounterDefinition>(_counterDefinitionMap.Values);
                list.Sort();
                return list;
            }
        }

        /// <summary>
        /// Crée une instance de CounterDefinition en la conservant en cache.
        /// </summary>
        /// <param name="label">Libellé à afficher.</param>
        /// <param name="code">Clé de l'instance dans le cache.</param>
        /// <param name="warningThreshold">Seuil d'alerte premier niveau (peut être null).</param>
        /// <param name="criticalThreshold">Seuil d'alerte seconde niveau (peut être null).</param>
        /// <param name="priority">Priorité d'affichage du compteur (minimum en premier).</param>
        /// <param name="counterDefinition">Définition du compteur.</param>
        /// <returns>Indique si un nouveau compteur a été créé.</returns>
        internal bool CreateDefinition(string label, string code, long warningThreshold, long criticalThreshold, int priority, out CounterDefinition counterDefinition) {
            counterDefinition = new CounterDefinition(label, code, warningThreshold, criticalThreshold, priority);
            string key = code.ToUpper(CultureInfo.InvariantCulture);
            if (_counterDefinitionMap.ContainsKey(key)) {
                return false;
            }

            _counterDefinitionMap.Add(key, counterDefinition);
            return true;
        }

        /// <summary>
        /// Retourne l'instance de CounterDefinition en cache selon sa clé.
        /// </summary>
        /// <param name="code">Clé de l'instance dans le cache.</param>
        /// <returns>CounterDefinition.</returns>
        internal CounterDefinition ValueOf(string code) {
            CounterDefinition counter;
            _counterDefinitionMap.TryGetValue(code.ToUpper(CultureInfo.InvariantCulture), out counter);
            return counter;
        }
    }
}
