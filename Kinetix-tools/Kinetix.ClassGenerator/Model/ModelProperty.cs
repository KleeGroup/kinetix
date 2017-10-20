using System;
using System.Collections.Generic;
using Kinetix.ClassGenerator.XmlParser;

namespace Kinetix.ClassGenerator.Model {

    /// <summary>
    /// Cette classe représente la propriété d'une classe.
    /// </summary>
    public sealed class ModelProperty : IModelObject {

        private readonly List<ModelAnnotation> _annotations = new List<ModelAnnotation>();

        /// <summary>
        /// Nom du modèle définissant l'objet.
        /// </summary>
        public string ModelFile {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit si la propriété vient d'une association.
        /// </summary>
        public bool IsFromAssociation {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit si la propriété vient d'une composition.
        /// </summary>
        public bool IsFromComposition {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le role de la propriété.
        /// </summary>
        public string Role {
            get;
            set;
        }

        /// <summary>
        /// Métadonnées.
        /// </summary>
        public string Metadata {
            get;
            set;
        }

        /// <summary>
        /// Retourne le nom de la propriété.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Retourne le commentaire de la propriété.
        /// </summary>
        public string Comment {
            get;
            set;
        }

        /// <summary>
        /// Retourne le type de la propriété.
        /// </summary>
        public string DataType {
            get;
            set;
        }

        /// <summary>
        /// Retourne les propriétés de l'attribut.
        /// </summary>
        public ModelDataMember DataMember {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le stéréotype de la propriété.
        /// </summary>
        public string Stereotype {
            get;
            set;
        }

        /// <summary>
        /// Retourne la description de l'attribut.
        /// </summary>
        public ModelDataDescription DataDescription {
            get;
            set;
        }

        /// <summary>
        /// Retourne si le type contenu est une collection.
        /// </summary>
        public bool IsCollection {
            get {
                return DataType.StartsWith("System.Collections.Generic.ICollection<", StringComparison.CurrentCulture) || DataType.StartsWith("ICollection<", StringComparison.CurrentCulture);
            }
        }

        /// <summary>
        /// Retourne si le type contenu est un DateTime.
        /// </summary>
        public bool IsDateTime {
            get {
                return DataType.StartsWith("System.DateTime", StringComparison.CurrentCulture);
            }
        }

        /// <summary>
        /// Retourne si le type est primitif.
        /// </summary>
        public bool IsPrimitive {
            get {
                return ParserHelper.IsPrimitiveType(DataType);
            }
        }

        /// <summary>
        /// Indique si la propriété est unique.
        /// </summary>
        public bool IsUnique {
            get;
            set;
        }

        /// <summary>
        /// Indique si la propriété est unique et nullable.
        /// </summary>
        public bool IsUniqueNullable {
            get;
            set;
        }

        /// <summary>
        /// Indique si la propriété est unique sur plusieur colonnes.
        /// </summary>
        public bool IsUniqueMany {
            get;
            set;
        }

        /// <summary>
        /// Indique si la propriété est utilisée uniquement pour la reprise de données.
        /// </summary>
        public bool IsReprise {
            get;
            set;
        }

        /// <summary>
        /// Indique si la propriété est un Id qui sera entré par l'utilisateur à la création de l'objet.
        /// </summary>
        public bool IsIdManuallySet {
            get;
            set;
        }

        /// <summary>
        /// Indique dans le cas d'une colonne de table, une valeur par défaut à définir.
        /// La chaîne correspond au SQL qui sera utilisé dans la déclaration de colonne sans mise en forme.
        /// Si <code>null</code>, la colonne n'aura pas de valeurs par défaut.
        /// </summary>
        public string DefaultValue {
            get;
            set;
        }

        /// <summary>
        /// Indique si la propriété est une clef primaire.
        /// </summary>
        public bool IsPrimaryKey {
            get {
                return DataDescription.IsPrimaryKey;
            }
        }

        /// <summary>
        /// Indique si la propriété est une clef de partition.
        /// </summary>
        public bool IsPartitionKey {
            get {
                return !string.IsNullOrEmpty(this.Class.PartitionKeyColumnName) && this.Class.PartitionKeyColumnName == this.DataMember.Name;
            }
        }

        /// <summary>
        /// Indique si la propriété est persistée mais non applicative (non visible dans le DTO).
        /// </summary>
        public bool IsDatabaseOnly {
            get {
                return Kinetix.ClassGenerator.Model.Stereotype.DatabaseOnly == this.Stereotype;
            }
        }

        /// <summary>
        /// Retourne la classe de la propriété.
        /// </summary>
        public ModelClass Class {
            get;
            set;
        }

        /// <summary>
        /// Indique si la propriété est persistente.
        /// </summary>
        public bool IsPersistent {
            get;
            set;
        }

        /// <summary>
        /// Indique si la propriété est non traductible (pour une liste de référence).
        /// </summary>
        public bool IsNonTranslatable {
            get;
            set;
        }

        /// <summary>
        /// Indique si la propriété est traduite.
        /// </summary>
        public bool IsTranslated {
            get {
                return this.Class.IsReference &&
                    !this.Class.IsNonTranslatable &&
                    !this.IsNonTranslatable &&
                    this.IsPersistent &&
                    this.IsPrimitive &&
                    !this.IsPrimaryKey &&
                    !this.DataDescription.IsForeignKey &&
                    this.DataDescription.Domain != null &&
                    this.DataDescription.Domain.IsTranslatable;
            }
        }

        /// <summary>
        /// Liste des annotations.
        /// </summary>
        public ICollection<ModelAnnotation> Annotations {
            get {
                return _annotations;
            }
        }

        /// <summary>
        /// Retourne true si le domaine de la propriété est du même type que domainCode.
        /// </summary>
        /// <param name="domainCode">Domaine à comparer.</param>
        /// <returns>Boolean.</returns>
        public bool IsDomain(string domainCode) {
            ModelDomain domain = DataDescription.Domain;
            return (domainCode == null && domain == null) ||
                (domain != null && domain.Code.Equals(domainCode));
        }

        /// <summary>
        /// Ajoute une annotation.
        /// </summary>
        /// <param name="annotation">Annotation.</param>
        public void AddAnnotation(ModelAnnotation annotation) {
            _annotations.Add(annotation);
        }
    }
}
