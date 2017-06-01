using System;

namespace Kinetix.TestUtils.Cases {

    /// <summary>
    /// Cas de test pour couvrir la combinatoire d'un critère.
    /// </summary>
    /// <typeparam name="TCritere">Type du critère.</typeparam>
    /// <remarks>
    /// On fournit au test un initialisateur de critère, et une liste d'actions qui agissent sur le critère.
    /// Pour chaque action, la méthode Execute est appelée.
    /// </remarks>
    public class CriteriaCoverageCase<TCritere>
        where TCritere : class, new() {

        /// <summary>
        /// Créé une instance de CriteriaCoverageCase.
        /// </summary>
        public CriteriaCoverageCase() {
            this.Init = () => new TCritere();
        }

        /// <summary>
        /// Action à exécuter pour effectuer le test.
        /// </summary>
        public Action<TCritere> Execute {
            get;
            set;
        }

        /// <summary>
        /// Fonction pour initialiser le critère.
        /// Par défaut, appelle le constructeur par défaut.
        /// </summary>
        public Func<TCritere> Init {
            get;
            set;
        }

        /// <summary>
        /// Inputs pour la combinatoire du critère.
        /// </summary>
        public Inputs<TCritere> Inputs {
            get;
            set;
        }

        /// <summary>
        /// Démarre le cas de test.
        /// </summary>
        public void Start() {

            if (this.Execute == null) {
                throw new NotSupportedException("Il faut définit la propriété Execute.");
            }

            if (this.Inputs == null) {
                throw new NotSupportedException("Il faut définir la propriété Inputs.");
            }

            // Pour chaque input de combinatoire
            foreach (var input in this.Inputs) {

                // Arrange
                var crit = this.Init();
                input(crit);

                // Act
                this.Execute(crit);
            }
        }
    }
}
