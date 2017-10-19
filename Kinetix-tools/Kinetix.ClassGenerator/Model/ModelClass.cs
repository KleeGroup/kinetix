using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GenStereotype = Kinetix.ClassGenerator.Model.Stereotype;

namespace Kinetix.ClassGenerator.Model {

    /// <summary>
    /// Cette classe représente une classe d'une modèle.
    /// </summary>
    public sealed class ModelClass : IModelObject {

        private readonly IList<ModelProperty> _propertyList = new List<ModelProperty>();
        private readonly HashSet<string> _usingList = new HashSet<string>();
        private readonly IList<string> _indexNotGenerated = new List<string>();

        /// <summary>
        /// Constructeur;.
        /// </summary>
        public ModelClass() {
            this.ConstValues = new Dictionary<string, StaticListElement>();
            this.IgnoreReferenceToReference = false;
            this.IndexNotGeneratedList = new List<string>();
        }

        /// <summary>
        /// Objet indiquant si la table est une référence externe.
        /// </summary>
        public bool IsExternal {
            get;
            set;
        }


        /// <summary>
        /// Retourne la liste des using de la classe.
        /// </summary>
        public HashSet<string> UsingList {
            get {
                return new HashSet<string>(_usingList);
            }
        }

        /// <summary>
        /// Objet présentant les const.
        /// </summary>
        public Dictionary<string, StaticListElement> ConstValues {
            get;
            private set;
        }

        /// <summary>
        /// Métadonnées.
        /// </summary>
        public string Metadata {
            get;
            set;
        }

        /// <summary>
        /// Indique le trigram de la classe.
        /// </summary>
        public string Trigram {
            get;
            set;
        }

        /// <summary>
        /// Indique si la classe doit être sérializable.
        /// </summary>
        public bool IsSerializable {
            get;
            set;
        }

        /// <summary>
        /// Retoure le libellé de la classe.
        /// </summary>
        public string Label {
            get;
            set;
        }

        /// <summary>
        /// Retoure le nom de la classe.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Retourne le nom du fichier contenant la classe.
        /// </summary>
        public string ModelFile {
            get;
            set;
        }

        /// <summary>
        /// Retourne le commentaire de la classe.
        /// </summary>
        public string Comment {
            get;
            set;
        }

        /// <summary>
        /// Indique si l'audit des modifications est activée.
        /// </summary>
        public bool AuditEnabled {
            get;
            set;
        }

        /// <summary>
        /// Indique si la gestion des exports en delta est activée.
        /// </summary>
        public bool ExportDeltaEnabled {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le nom du filegroup à utiliser pour la table.
        /// </summary>
        public string Storage {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le nom du schéma de partitionnement à utiliser pour la table.
        /// </summary>
        public string PartitionSchema {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la colonne utilisée pour le partionnement.
        /// </summary>
        public string PartitionKeyColumnName {
            get;
            set;
        }

        /// <summary>
        /// Retourne le contrat de la classe.
        /// </summary>
        public ModelDataContract DataContract {
            get;
            set;
        }

        /// <summary>
        /// Retourne la liste des propriétés de la classe.
        /// </summary>
        public ReadOnlyCollection<ModelProperty> PropertyList {
            get {
                return new ReadOnlyCollection<ModelProperty>(_propertyList);
            }
        }

        /// <summary>
        /// Indique si la classe contient des propriétés nécessitant une initialisation.
        /// </summary>
        public bool NeedsInitialization {
            get {
                return PropertyList.Count(p => (!p.IsPrimitive && !p.IsCollection) || p.IsCollection) != 0;
            }
        }

        /// <summary>
        /// Renvoie le nom qualifié de la classe générée.
        /// </summary>
        public string FullyQualifiedName {
            get {
                return this.Namespace.Model.Name + "." + this.Namespace.Name + "." + this.Name;
            }
        }

        /// <summary>
        /// Retourne la liste des propriétés persistantes de la classe.
        /// </summary>
        public ReadOnlyCollection<ModelProperty> PersistentPropertyList {
            get {
                IList<ModelProperty> persistentPropertyList = new List<ModelProperty>();
                foreach (ModelProperty modelProperty in PropertyList) {
                    if (modelProperty.IsPersistent) {
                        persistentPropertyList.Add(modelProperty);
                    }
                }

                return new ReadOnlyCollection<ModelProperty>(persistentPropertyList);
            }
        }

        /// <summary>
        /// Retourne la liste des propriétés persistantes de la classe (correspondent à une colonne en base), ordonnée.
        /// </summary>
        public ReadOnlyCollection<ModelProperty> OrderedPersistentPropertyList {
            get {
                IList<ModelProperty> persistentPropertyList = new List<ModelProperty>();
                foreach (ModelProperty modelProperty in PropertyList) {
                    if (modelProperty.IsPersistent) {
                        persistentPropertyList.Add(modelProperty);
                    }
                }

                return new ReadOnlyCollection<ModelProperty>(persistentPropertyList.OrderBy(x => x.DataMember.Order).ToList());
            }
        }

        /// <summary>
        /// Retourne la liste des propriétés non persistantes de la classe.
        /// </summary>
        public ReadOnlyCollection<ModelProperty> NonPersistentPropertyList {
            get {
                IList<ModelProperty> nonPersistentPropertyList = new List<ModelProperty>();
                foreach (ModelProperty modelProperty in PropertyList) {
                    if (!modelProperty.IsPersistent) {
                        nonPersistentPropertyList.Add(modelProperty);
                    }
                }

                return new ReadOnlyCollection<ModelProperty>(nonPersistentPropertyList);
            }
        }

        /// <summary>
        /// Classe parente (pour héritage).
        /// </summary>
        public ModelClass ParentClass {
            get;
            set;
        }

        /// <summary>
        /// Stéréotype de la classe.
        /// </summary>
        public string Stereotype {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la propriété par défaut de la classe.
        /// </summary>
        public string DefaultProperty {
            get;
            set;
        }

        /// <summary>
        /// Default class property.
        /// </summary>
        public ModelProperty DefaultModelProperty {
            get;
            set;
        }

        /// <summary>
        /// Default order class property.
        /// </summary>
        public ModelProperty DefaultOrderModelProperty {
            get;
            set;
        }

        /// <summary>
        /// Retourne le namespace de la classe.
        /// </summary>
        public ModelNamespace Namespace {
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
        /// Indique si la classe est historisée.
        /// </summary>
        public bool IsHistorized {
            get;
            set;
        }

        /// <summary>
        /// Tue if this class is migrated from SAP.
        /// </summary>
        public bool IsOriginSap {
            get;
            set;
        }

        /// <summary>
        /// Retourne si la classe doit implémenter un IBeanState.
        /// </summary>
        public bool IsBeanStatable {
            get {
                return "N-N".Equals(Stereotype);
            }
        }

        /// <summary>
        /// Retourne si le code de la classe ne doit pas être généré.
        /// </summary>
        public bool IsTable {
            get {
                return "TABLE".Equals(Stereotype);
            }
        }

        /// <summary>
        /// Retourne si la classe contient une collection générique.
        /// </summary>
        public bool HasCollection {
            get {
                foreach (ModelProperty item in PropertyList) {
                    if (item.IsCollection) {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Retourne si la classe peut être référencée par un objet référentiel même si elle-même n'est pas référentielle.
        /// </summary>
        public bool IgnoreReferenceToReference {
            get;
            set;
        }

        /// <summary>
        /// Retourne si la classe contient un Champ DateTime.
        /// </summary>
        public bool HasDateTime {
            get {
                foreach (ModelProperty item in PropertyList) {
                    if (item.IsDateTime) {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Retourne si la classe est une liste de référence (statique ou administrable).
        /// </summary>
        public bool IsReference {
            get {
                switch (this.Stereotype) {
                    case GenStereotype.Reference:
                    case GenStereotype.Statique:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Retourne si la classe est une liste statique.
        /// </summary>
        public bool IsStatique {
            get {
                switch (this.Stereotype) {
                    case GenStereotype.Statique:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Retourne si la classe contient des colonnes traduites.
        /// </summary>
        public bool IsTranslated {
            get {
                return this.PropertyList.Any(x => x.IsTranslated);
            }
        }

        /// <summary>
        /// Retourne si la classe contient un Champ portant l'attribut Domain.
        /// </summary>
        public bool HasDomainAttribute {
            get {
                foreach (ModelProperty item in PropertyList) {
                    if (item.DataDescription.Domain != null) {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Retourne la collection de clés primaires de la classe.
        /// </summary>
        public ICollection<ModelProperty> PrimaryKey {
            get {
                ICollection<ModelProperty> primaryKey = new Collection<ModelProperty>();
                foreach (ModelProperty property in PropertyList) {
                    if (property.DataDescription.IsPrimaryKey) {
                        primaryKey.Add(property);
                    }
                }

                return primaryKey;
            }
        }

        /// <summary>
        /// Obtient la propriété utilisée pour le partitionnement.
        /// </summary>
        public ModelProperty PartitionKey {
            get {
                foreach (ModelProperty property in PropertyList) {
                    if (property.IsPartitionKey) {
                        return property;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Indique si la classe a une clé primaire.
        /// </summary>
        public bool HasPrimaryKey {
            get {
                return PrimaryKey.Count > 0;
            }
        }

        /// <summary>
        /// Indique si la classe a une clé primaire.
        /// </summary>
        public bool HasPartitionKey {
            get {
                return PartitionKey != null;
            }
        }

        /// <summary>
        /// Indique si on désactive le script de création de la table.
        /// </summary>
        public bool IsNoTable {
            get;
            set;
        }

        /// <summary>
        /// Indique si une table doit être générée pour cette classe.
        /// </summary>
        public bool IsTableGenerated {
            get {
                return this.DataContract.IsPersistent && !this.IsNoTable;
            }
        }

        /// <summary>
        /// Indique si la classe correspond à une vue.
        /// </summary>
        public bool IsView {
            get;
            set;
        }

        /// <summary>
        /// Desactive la génération automatique de l'implementation du service de chargement d'une liste de référence.
        /// </summary>
        public bool DisableAccessorImplementation {
            get;
            set;
        }

        /// <summary>
        /// Liste des indexes à ne pas générer sur la table.
        /// </summary>
        public IList<string> IndexNotGeneratedList {
            get;
            private set;
        }

        /// <summary>
        /// Ajoute un namespace dans la liste des using de la classe.
        /// </summary>
        /// <param name="nmspace">Le namespace à ajouter.</param>
        public void AddUsing(ModelNamespace nmspace) {
            if (nmspace == null) {
                throw new ArgumentNullException("nmspace");
            }

            if (string.IsNullOrEmpty(nmspace.Name)) {
                throw new ArgumentNullException("nmspace", "nmspace.Name is null.");
            }

            if (nmspace.Model == null) {
                throw new ArgumentNullException("nmspace", "nmspace.Model is null.");
            }

            if (string.IsNullOrEmpty(nmspace.Model.Name)) {
                throw new ArgumentNullException("nmspace", "nmspace.Model.Name is null.");
            }

            if (!string.IsNullOrEmpty(nmspace.Name) && !nmspace.Name.Equals(Namespace.Name) && !nmspace.IsExternal) {
                _usingList.Add(nmspace.Model.Name + "." + nmspace.Name);
            }
        }

        /// <summary>
        /// Ajoute une propriété à la classe.
        /// </summary>
        /// <param name="property">La propriété à ajouter.</param>
        public void AddProperty(ModelProperty property) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }

            if (property.DataDescription.IsPrimaryKey) {
                if (PrimaryKey.Count != 0) {
                    Trigram = property.Name;
                }
            }

            _propertyList.Add(property);
        }

        /// <summary>
        /// Retourne si la classe contient un Champ avec le domaine concerné.
        /// </summary>
        /// <param name="domain">Domaine recherché.</param>
        /// <returns>True si la classe contient un champs de ce domaine.</returns>
        public bool HasDomain(string domain) {
            foreach (ModelProperty item in PropertyList) {
                if (item.IsDomain(domain)) {
                    return true;
                }
            }
            
            return false;
        }
    }
}
