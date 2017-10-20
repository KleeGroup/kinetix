namespace Kinetix.ComponentModel {

    /// <summary>
    /// Classe de base pour la création d'une énum Type Safe
    /// </summary>
    public abstract class TypeSafeEnum {

        /// <summary>
        /// Valeur de l'élément de l'enum
        /// </summary>
        public string Value {
            get;
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="value">La valeur de l'enum.</param>
        protected TypeSafeEnum(string value) {
            Value = value;
        }

        /// <summary>
        /// Vérifie l'égalité entre la valeur de l'enum et celle de l'objet
        /// </summary>
        /// <param name="value">La valeur testée</param>
        /// <returns></returns>
        public bool Check(string value) {
            return Value == value;
        }
    }
}
