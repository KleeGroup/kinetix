using System;
using System.Reflection;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Accesseur sur une méthode.
    /// </summary>
    public class Accessor {
        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="contractType">Contrat.</param>
        /// <param name="method">Méthode.</param>
        /// <param name="referenceType">Type de liste de référence.</param>
        public Accessor(Type contractType, MethodInfo method, Type referenceType)
            : this(contractType, method, referenceType, referenceType, null) {
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="contractType">Contrat.</param>
        /// <param name="method">Méthode.</param>
        /// <param name="referenceType">Type de liste de référence.</param>
        /// <param name="name">Nom de l'accesseur.</param>
        public Accessor(Type contractType, MethodInfo method, Type referenceType, string name)
            : this(contractType, method, referenceType, referenceType, name) {
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="contractType">Contrat.</param>
        /// <param name="method">Méthode.</param>
        /// <param name="referenceType">Type de liste de référence.</param>
        /// <param name="returnType">Type retourné par l'accesseur.</param>
        public Accessor(Type contractType, MethodInfo method, Type referenceType, Type returnType)
            : this(contractType, method, referenceType, returnType, string.Empty) {
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="contractType">Contrat.</param>
        /// <param name="method">Méthode.</param>
        /// <param name="referenceType">Type de liste de référence.</param>
        /// <param name="returnType">Type retourné par l'accesseur.</param>
        /// <param name="name">Nom de l'accesseur.</param>
        public Accessor(Type contractType, MethodInfo method, Type referenceType, Type returnType, string name) {
            this.ContractType = contractType;
            this.Method = method;
            this.ReferenceType = referenceType;
            this.ReturnType = returnType;
            this.Name = name;
        }

        /// <summary>
        /// Nom de l'accesseur.
        /// </summary>
        public string Name {
            get;
            private set;
        }

        /// <summary>
        /// Contrat.
        /// </summary>
        public Type ContractType {
            get;
            private set;
        }

        /// <summary>
        /// Méthode.
        /// </summary>
        public MethodInfo Method {
            get;
            private set;
        }

        /// <summary>
        /// Type de la liste de référence.
        /// </summary>
        public Type ReferenceType {
            get;
            private set;
        }

        /// <summary>
        /// Obtient ou définit le type retourné par l'accesseur.
        /// </summary>
        public Type ReturnType {
            get;
            private set;
        }
    }
}
