using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Kinetix.ClassGenerator.Model;
using Kinetix.ClassGenerator.NVortex;
using Kinetix.ComponentModel;

namespace Kinetix.ClassGenerator.XmlParser.EapReader {

    /// <summary>
    /// Cette classe représente l'implémentation d'un parser Eap (modèle objet EnterpriseArchitect).
    /// </summary>
    internal class EapParser : AbstractParser {

        private const string NodeModel = "/xmi:XMI/uml:Model";
        private const string NodeExtension = "/xmi:XMI/xmi:Extension";
        private const string NodePackageModel = "packagedElement";
        private const string NodeDomainsDefinition = NodeModel + "/thecustomprofile:Domains";
        private const string NodeDomainsByIdRef = NodeModel + "/packagedElement/packagedElement[@xmi:id='{0}']";
        private const string NodeExtensionElements = NodeExtension + "/elements";
        private const string NodeExtensionConnectors = NodeExtension + "/connectors";
        private const string NodeElementByIdRef = "element[@xmi:idref='{0}']";
        private const string NodeNamespaceDefinition = NodeModel + "/thecustomprofile:Namespace";
        private const string NodeNamespaceByIdRef = NodeModel + "/packagedElement/packagedElement[@xmi:id='{0}']";
        private const string NodeProperties = "properties";
        private const string NodeStereotype = "stereotype";
        private const string NodeExtendedProperties = "extendedProperties";
        private const string NodeProject = "project";
        private const string NodeClasses = "packagedElement[@xmi:type='uml:Class']";
        private const string NodePrimaryKey = "attributes/attribute/stereotype[@stereotype='PrimaryKey']";
        private const string NodeAttributes = "attributes";
        private const string NodeBounds = "bounds";
        private const string NodeStyle = "style";
        private const string NodeComment = "documentation";
        private const string NodeGeneralizations = "connector/properties[@ea_type='Generalization']";
        private const string NodeAssociations = "connector/properties[@ea_type='Association' or @ea_type='Aggregation']";
        private const string NodeSource = "source";
        private const string NodeTarget = "target";
        private const string NodeLabels = "labels";
        private const string NodeType = "type";
        private const string NodeRole = "role";
        private const string NodeTags = "tags";
        private const string NodeTagColumnName = NodeTags + "/tag[@name='ColumnName']";
        private const string NodeTagColumnType = NodeTags + "/tag[@name='ColumnType']";
        private const string NodeTagUnique = NodeTags + "/tag[@name='Unique']";
        private const string NodeTagView = NodeTags + "/tag[@name='View']";

        private const string PropertyName = "name";
        private const string PropertyId = "xmi:id";
        private const string PropertyIdRef = "xmi:idref";
        private const string PropertyAlias = "alias";
        private const string PropertyComment = "documentation";
        private const string PropertyBasePackage = "base_Package";
        private const string PropertyAuthor = "author";
        private const string PropertyStereotype = "stereotype";
        private const string PropertyPersistent = "persistence";
        private const string PropertyStyleEx = "styleex";
        private const string PropertyValue = "value";
        private const string PropertyMultiplicityLower = "lower";
        private const string PropertyMultiplicityUpper = "upper";
        private const string PropertyType = "type";
        private const string PropertyAssociationName = "mt";
        private const string PropertyMultiplicity = "multiplicity";
        private const string PropertyEAType = "ea_type";
        private const string PropertyAggregation = "aggregation";

        private const string StereotypeDefaultProperty = "DefaultProperty";
        private const string StereotypePrimaryKey = "PrimaryKey";
        private const string StereotypeReference = "Reference";
        private const string StereotypeStatique = "Statique";

        private const string AssociationComposition = "composite";

        private const string EATypeAssociation = "Association";
        private const string EATypeAggregation = "Aggregation";

        private const string PersistencePersistent = "Persistent";
        private const string PersistenceTransient = "Transient";

        private readonly Regex _regexICollection = new Regex(@"^ICollection<([A-Za-z0-9]+)>$");
        private readonly Regex _regexPropertyPersistance = new Regex(@"^.*volatile=([01]);.*$");
        private readonly Regex _regexAssociationName = new Regex(@"^.*alias=([A-Za-z0-9]+);.*$");
        private readonly Regex _regexPersistentLength = new Regex(@"^[A-Z]+([0-9]+).*$");
        private readonly Regex _regexPersistentPrecision = new Regex(@"^[A-Z]+[0-9]+,([0-9]+)$");

        private readonly IDictionary<string, ModelClass> _classByNameMap = new Dictionary<string, ModelClass>();
        private readonly IDictionary<string, ModelClass> _classByIdMap = new Dictionary<string, ModelClass>(); // identifiants de type XXXX_XXXXXXXX_XXXX_XXXX_XXXX_XXXXXXXXXXXX

        private XmlDocument _currentEap;
        private XmlNamespaceManager _currentNsManager;
        private ModelRoot _currentModelRoot;
        private ICollection<ModelRoot> _modelRootList;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="modelFiles">Liste des modèles à analyser.</param>
        /// <param name="domainList">Liste des domaines.</param>
        public EapParser(ICollection<string> modelFiles, ICollection<IDomain> domainList)
            : base(modelFiles, domainList) {
        }

        /// <summary>
        /// Peuple la structure du modèle objet en paramètre à partir d'un fichier Eap.
        /// </summary>
        /// <returns>La collection de message d'erreurs au format NVortex.</returns>
        public override ICollection<ModelRoot> Parse() {
            ErrorList.Clear();
            _modelRootList = new List<ModelRoot>();
            foreach (string modelFile in ModelFiles) {
                DisplayMessage("Analyse du fichier " + modelFile);
                using (FileStream stream = new FileStream(modelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    _currentEap = new XmlDocument();
                    _currentEap.Load(stream);
                }

                _currentNsManager = new XmlNamespaceManager(_currentEap.NameTable);
                _currentNsManager.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");
                _currentNsManager.AddNamespace("uml", "http://schema.omg.org/spec/UML/2.1");
                _currentNsManager.AddNamespace("xmi", "http://schema.omg.org/spec/XMI/2.1");
                _currentNsManager.AddNamespace("thecustomprofile", "http://www.sparxsystems.com/profiles/thecustomprofile/1.0");

                XmlNode extensionNode = _currentEap.SelectSingleNode(NodeExtension, _currentNsManager);
                XmlNode modelNode = _currentEap.SelectSingleNode(NodeModel, _currentNsManager);

                string modelNodeName = modelNode.SelectSingleNode(NodePackageModel, _currentNsManager).Attributes[PropertyName].Value;
                ModelRoot root = new ModelRoot() {
                    ModelFile = modelFile,
                    Label = modelNodeName,
                    Name = ConvertModelName(modelNodeName)
                };
                _currentModelRoot = root;

                DisplayMessage("==> Parsing du modèle " + root.Label + "(" + root.Name + ")");

                DisplayMessage("--> Lecture des domaines.");
                string domainsId = _currentEap.SelectSingleNode(NodeDomainsDefinition, _currentNsManager).Attributes[PropertyBasePackage].Value;
                XmlNode domainsNode = _currentEap.SelectSingleNode(string.Format(NodeDomainsByIdRef, domainsId), _currentNsManager);
                XmlNode extensionElementsNode = _currentEap.SelectSingleNode(NodeExtensionElements, _currentNsManager);
                BuildModelDomains(domainsNode, extensionElementsNode, modelFile);

                DisplayMessage("--> Lecture des namespaces.");
                List<string> namespaceNodeIdList = new List<string>();
                foreach (XmlNode namespaceIdNode in _currentEap.SelectNodes(NodeNamespaceDefinition, _currentNsManager)) {
                    namespaceNodeIdList.Add(namespaceIdNode.Attributes[PropertyBasePackage].Value);
                }

                BuildModelNamespaces(extensionElementsNode, namespaceNodeIdList, modelFile);

                DisplayMessage("--> Lecture des héritages.");
                XmlNode extensionConnectorsNode = _currentEap.SelectSingleNode(NodeExtensionConnectors, _currentNsManager);
                BuildModelNamespaceGeneralizations(extensionConnectorsNode, modelFile);

                DisplayMessage("--> Lecture des associations.");
                BuildModelNamespaceAssociations(extensionConnectorsNode, modelFile);

                _modelRootList.Add(root);
            }

            return _modelRootList;
        }

        /// <summary>
        /// Convertit le nom du modèle de l'Eap.
        /// Supprime les "_".
        /// </summary>
        /// <param name="modelName">Nom de modèle au format EnterpriseArchitect.</param>
        /// <returns>Nom de modèle pour .Net.</returns>
        private static string ConvertModelName(string modelName) {
            if (string.IsNullOrEmpty(modelName)) {
                return modelName;
            }

            return modelName.Replace("_", string.Empty);
        }

        /// <summary>
        /// Construit les objets de domaine.
        /// </summary>
        /// <param name="domainsDefinition">Le noeud XML de définition des domaines.</param>
        /// <param name="elementsExtension">Le noeud XML contenant les extentions d'éléments.</param>
        /// <param name="modelFile">Fichier modèle.</param>
        private void BuildModelDomains(XmlNode domainsDefinition, XmlNode elementsExtension, string modelFile) {
            foreach (XmlNode domainNode in domainsDefinition.ChildNodes) {
                string id = domainNode.Attributes[PropertyId].Value;
                XmlNode domaineExtensionNode = elementsExtension.SelectSingleNode(string.Format(NodeElementByIdRef, id), _currentNsManager);

                string persistentDataType = domaineExtensionNode.SelectSingleNode(NodeProperties, _currentNsManager).Attributes[PropertyAlias].Value;
                int? persistentLength = null;
                if (_regexPersistentLength.IsMatch(persistentDataType)) {
                    persistentLength = int.Parse(_regexPersistentLength.Replace(persistentDataType, "$1"));
                }

                int? persistentPrecision = null;
                if (_regexPersistentPrecision.IsMatch(persistentDataType)) {
                    persistentPrecision = int.Parse(_regexPersistentPrecision.Replace(persistentDataType, "$1"));
                }

                ModelDomain domaine = new ModelDomain() {
                    Name = domainNode.Attributes[PropertyName].Value,
                    ModelFile = modelFile,
                    Code = domainNode.Attributes[PropertyName].Value,
                    DataType = domaineExtensionNode.SelectSingleNode(NodeProperties, _currentNsManager).Attributes[PropertyStereotype].Value,
                    PersistentDataType = persistentDataType,
                    PersistentLength = persistentLength,
                    PersistentPrecision = persistentPrecision,
                    Model = _currentModelRoot
                };
                _currentModelRoot.AddDomain(id, domaine);
                CheckDomain(domaine);
            }
        }

        /// <summary>
        /// Construit les namespaces.
        /// </summary>
        /// <param name="elementsExtension">Le noeud XML contenant les extentions d'éléments.</param>
        /// <param name="namespaceNodeIdList">Liste des id des noeuds des namespaces.</param>
        /// <param name="modelFile">Nom du modèle contenant la classe.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = "Non internationalisé")]
        private void BuildModelNamespaces(XmlNode elementsExtension, List<string> namespaceNodeIdList, string modelFile) {
            foreach (string namespaceNodeId in namespaceNodeIdList) {
                XmlNode namespaceExtensionNode = elementsExtension.SelectSingleNode(string.Format(NodeElementByIdRef, namespaceNodeId), _currentNsManager);
                XmlNode namespaceNode = _currentEap.SelectSingleNode(string.Format(NodeNamespaceByIdRef, namespaceNodeId), _currentNsManager);

                XmlAttribute authorAttribute = namespaceExtensionNode.SelectSingleNode(NodeProject, _currentNsManager).Attributes[PropertyAuthor];
                ModelNamespace nmspace = new ModelNamespace() {
                    Label = namespaceExtensionNode.Attributes[PropertyName].Value,
                    Name = namespaceExtensionNode.SelectSingleNode(NodeProperties, _currentNsManager).Attributes[PropertyAlias].Value,
                    Comment = namespaceExtensionNode.SelectSingleNode(NodeProperties, _currentNsManager).Attributes[PropertyComment].Value,
                    Creator = (authorAttribute == null) ? string.Empty : authorAttribute.Value,
                    Model = _currentModelRoot
                };
                if (string.IsNullOrEmpty(nmspace.Comment)) {
                    RegisterError(Category.Doc, "Le package [" + nmspace.Label + "] n'a pas de commentaire.");
                }

                BuildNamespaceClasses(nmspace, namespaceNode, elementsExtension, modelFile);
                _currentModelRoot.AddNamespace(nmspace);
            }

            foreach (string nsName in _currentModelRoot.Namespaces.Keys) {
                ModelNamespace nmsp = _currentModelRoot.Namespaces[nsName];
                foreach (ModelClass classe in nmsp.ClassList) {
                    foreach (ModelProperty property in classe.PropertyList) {
                        if (_regexICollection.IsMatch(property.DataType)) {
                            string innerType = _regexICollection.Replace(property.DataType, "$1");
                            if (!string.IsNullOrEmpty(innerType)) {
                                if (!_classByNameMap.ContainsKey(innerType)) {
                                    if (!ParserHelper.IsPrimitiveType(innerType)) {
                                        RegisterError(Category.Bug, "Ajout des usings dans la classe [" + classe.Name + "] : la classe [" + innerType + "] n'a pas été trouvée.");
                                    }
                                } else {
                                    ModelClass usingClasse = _classByNameMap[innerType];
                                    classe.AddUsing(usingClasse.Namespace);
                                }
                            } else {
                                RegisterError(Category.Bug, "Impossible de récupérer le type générique de la collection pour la propriété [" + property.Name + "] de la classe [" + property.Class.Name + "].");
                            }
                        }

                        if (classe.ParentClass != null) {
                            classe.AddUsing(classe.ParentClass.Namespace);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Construit les objets de classe.
        /// </summary>
        /// <param name="nmspace">Namespace courant.</param>
        /// <param name="namespaceNode">Le noeud XML de définition du namespace courant.</param>
        /// <param name="elementsExtension">Le noeud XML contenant les extentions d'éléments.</param>
        /// <param name="modelFile">Nom du modèle.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = "Non internationalisé")]
        private void BuildNamespaceClasses(ModelNamespace nmspace, XmlNode namespaceNode, XmlNode elementsExtension, string modelFile) {
            foreach (XmlNode classNode in namespaceNode.SelectNodes(NodeClasses, _currentNsManager)) {
                string classNodeId = classNode.Attributes[PropertyId].Value;
                XmlNode classExtensionNode = elementsExtension.SelectSingleNode(string.Format(NodeElementByIdRef, classNodeId), _currentNsManager);

                XmlAttribute stereotypeAttribute = classExtensionNode.SelectSingleNode(NodeProperties, _currentNsManager).Attributes[PropertyStereotype];
                XmlNode viewNode = classExtensionNode.SelectSingleNode(NodeTagView, _currentNsManager);
                ModelClass classe = new ModelClass() {
                    Label = classNode.Attributes[PropertyName].Value,
                    Name = classExtensionNode.SelectSingleNode(NodeProperties, _currentNsManager).Attributes[PropertyAlias].Value,
                    Comment = classExtensionNode.SelectSingleNode(NodeProperties, _currentNsManager).Attributes[PropertyComment].Value,
                    Stereotype = (stereotypeAttribute != null) ? stereotypeAttribute.Value : null,
                    Namespace = nmspace,
                    ModelFile = modelFile,
                    IsView = viewNode != null
                };

                if (!string.IsNullOrEmpty(classe.Stereotype) && classe.Stereotype != StereotypeReference && classe.Stereotype != StereotypeStatique) {
                    RegisterError(Category.Error, "Classe " + classe.Name + " : seuls les stéréotypes '" + StereotypeReference + "' et '" + StereotypeStatique + "' sont acceptés.");
                }

                XmlAttribute persistenceAttribute = classExtensionNode.SelectSingleNode(NodeExtendedProperties, _currentNsManager).Attributes[PropertyPersistent];
                bool isPersistent = false;
                if (persistenceAttribute == null) {
                    RegisterError(Category.Error, "Classe " + classe.Name + " : la classe doit être définie comme persistante ou non-persistante.");
                } else {
                    if (PersistencePersistent.Equals(persistenceAttribute.Value)) {
                        isPersistent = true;
                    } else if (PersistenceTransient.Equals(persistenceAttribute.Value)) {
                        isPersistent = false;
                    } else {
                        RegisterError(Category.Error, "Classe " + classe.Name + " : seuls les persistances '" + PersistencePersistent + "' et '" + PersistenceTransient + "' sont acceptés.");
                    }
                }

                string persistentCode = string.Empty;
                if (isPersistent) {
                    persistentCode = GeneratorParameters.IsProjetUesl ? ParserHelper.GetSqlTableNameFromClassName(classe.Name, classe.IsView) : ParserHelper.ConvertCsharp2Bdd(classe.Name);
                }

                classe.DataContract = new ModelDataContract() {
                    Name = persistentCode,
                    Namespace = nmspace.Model.Name + "." + nmspace.Name,
                    IsPersistent = isPersistent
                };

                // Chargement et création des propriétés de la classe
                BuildClassProperties(classe, classNode, classExtensionNode, modelFile);

                // Détermine la DefaultProperty de la classe
                foreach (ModelProperty property in classe.PropertyList) {
                    if (!string.IsNullOrEmpty(property.Stereotype) && StereotypeDefaultProperty.Equals(property.Stereotype)) {
                        if (!string.IsNullOrEmpty(classe.DefaultProperty)) {
                            RegisterError(Category.Bug, "La classe " + classe.Name + " a déjà une valeur pour la propriété DefaultProperty.");
                        } else {
                            classe.DefaultProperty = property.Name;
                        }
                    }
                }

                nmspace.AddClass(classe);

                _classByIdMap.Add(modelFile + ":" + classNode.Attributes[PropertyId].Value, classe);

                if (_classByNameMap.ContainsKey(classe.Name)) {
                    RegisterError(Category.Bug, "Doublons dans le nommage des classes du modèle : la classe [" + classe.Name + "] existe déjà.");
                } else {
                    _classByNameMap.Add(classe.Name, classe);
                }
            }
        }

        /// <summary>
        /// Construit les propriétés d'une classe.
        /// </summary>
        /// <param name="classe">Classe considérée.</param>
        /// <param name="classNode">Le noeud caractérisant la classe parsée.</param>
        /// <param name="classExtensionNode">Le noeud XML contenant l'extention de la classe.</param>
        /// <param name="modelFile">Fichier modèle.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = "Non internationalisé")]
        private void BuildClassProperties(ModelClass classe, XmlNode classNode, XmlNode classExtensionNode, string modelFile) {
            ICollection<string> pkRefList = GetClassPrimaryKeysIdList(classExtensionNode);
            XmlNode attributesNode = classExtensionNode.SelectSingleNode(NodeAttributes, _currentNsManager);
            if (attributesNode != null) {
                foreach (XmlNode propertyNode in attributesNode.ChildNodes) {
                    string propertyId = propertyNode.Attributes[PropertyIdRef].Value;
                    string styleEx = propertyNode.SelectSingleNode(PropertyStyleEx, _currentNsManager).Attributes[PropertyValue].Value;
                    bool isPersistent = "0".Equals(_regexPropertyPersistance.Replace(styleEx, "$1"));
                    string multiplicityLower = propertyNode.SelectSingleNode(NodeBounds, _currentNsManager).Attributes[PropertyMultiplicityLower].Value;
                    string multiplicityUpper = propertyNode.SelectSingleNode(NodeBounds, _currentNsManager).Attributes[PropertyMultiplicityUpper].Value;
                    if ("-1".Equals(multiplicityUpper)) {
                        multiplicityUpper = "*";
                    }

                    string multiplicity = multiplicityLower + ".." + multiplicityUpper;
                    ModelDomain domain = GetDomainByCode(propertyNode.SelectSingleNode(NodeProperties, _currentNsManager).Attributes[PropertyType].Value);

                    XmlAttribute stereotypeAttribute = propertyNode.SelectSingleNode(NodeStereotype, _currentNsManager).Attributes[PropertyStereotype];
                    XmlNode uniqueNode = propertyNode.SelectSingleNode(NodeTagUnique, _currentNsManager);

                    ModelProperty property = new ModelProperty() {
                        Name = propertyNode.SelectSingleNode(NodeStyle, _currentNsManager).Attributes[PropertyValue].Value,
                        Comment = propertyNode.SelectSingleNode(NodeComment, _currentNsManager).Attributes[PropertyValue].Value,
                        IsPersistent = isPersistent,
                        IsUnique = uniqueNode != null,
                        DataType = domain.DataType,
                        Stereotype = (stereotypeAttribute == null) ? string.Empty : stereotypeAttribute.Value,
                        Class = classe,
                        ModelFile = modelFile,
                        DataDescription = new ModelDataDescription() {
                            Libelle = propertyNode.Attributes[PropertyName].Value,
                            Domain = domain,
                            IsPrimaryKey = pkRefList.Contains(propertyId)
                        }
                    };

                    if (!ParserHelper.IsPrimitiveType(property.DataType)) {
                        RegisterError(Category.Error, "Propriété " + classe.Name + "." + property.Name + " : le type de la propriété " + property.DataType + " n'est pas primitif, il faut utiliser les liens de composition.");
                        continue;
                    }

                    string dataMemberName = string.Empty;
                    if (property.IsPersistent) {
                        if (property.DataDescription.IsPrimaryKey) {
                            classe.Trigram = property.DataDescription.Libelle;
                        }

                        string columnName = null;
                        XmlNode columnNameNode = propertyNode.SelectSingleNode(NodeTagColumnName, _currentNsManager);
                        if (columnNameNode != null) {
                            columnName = columnNameNode.Attributes[PropertyValue].Value;
                        }

                        if (string.IsNullOrEmpty(columnName)) {
                            XmlNode columnTypeNode = propertyNode.SelectSingleNode(NodeTagColumnType, _currentNsManager);
                            string columnType = string.Empty;
                            if (columnTypeNode != null) {
                                columnType = columnTypeNode.Attributes[PropertyValue].Value;
                            }

                            if (property.DataDescription.IsPrimaryKey) {
                                columnName = columnType + classe.Name;
                            } else {
                                columnName = columnType + property.Name;
                            }
                        }

                        dataMemberName = columnName;
                    }

                    property.DataMember = new ModelDataMember() {
                        Name = dataMemberName,
                        IsRequired = multiplicity != null && Multiplicity11.Equals(multiplicity)
                    };
                    if (property.DataDescription.IsPrimaryKey || !string.IsNullOrEmpty(property.DataDescription.ReferenceType)) {
                        property.DataDescription.Libelle = property.Name;
                    }

                    classe.AddProperty(property);

                    if (!string.IsNullOrEmpty(property.Stereotype) && property.Stereotype != StereotypeDefaultProperty && property.Stereotype != StereotypePrimaryKey) {
                        RegisterError(Category.Error, "Propriété " + classe.Name + "." + property.Name + " : seul les stéréotypes '" + StereotypeDefaultProperty + "' et '" + StereotypePrimaryKey + "' sont acceptés.");
                    }
                }
            }
        }

        /// <summary>
        /// Retourne un domaine a partir de sa clef de raccourci.
        /// </summary>
        /// <param name="domainCode">Code du domaine recherché.</param>
        /// <returns>Le domaine.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = "Non internationalisé")]
        private ModelDomain GetDomainByCode(string domainCode) {
            foreach (ModelDomain domaine in _currentModelRoot.UsableDomains.Values) {
                if (domainCode.Equals(domaine.Code)) {
                    return domaine;
                }
            }

            throw new KeyNotFoundException("Le domaine lié au code " + domainCode + " est introuvable.");
        }

        /// <summary>
        /// Retourne la liste des identifiants d'attributs eap marqués comme clés primaires.
        /// </summary>
        /// <param name="classExtensionNode">Le noeud XML contenant l'extention de la classe.</param>
        /// <returns>La collection des identifiants d'attributs EAP identifiant les clés primaires de la classe.</returns>
        private ICollection<string> GetClassPrimaryKeysIdList(XmlNode classExtensionNode) {
            ICollection<string> ret = new Collection<string>();

            // Récupération des identifiants des objets eap clés primaires
            XmlNodeList pkNodeList = classExtensionNode.SelectNodes(NodePrimaryKey, _currentNsManager);
            if (pkNodeList != null) {
                foreach (XmlNode pkNode in pkNodeList) {
                    ret.Add(pkNode.ParentNode.Attributes[PropertyIdRef].Value);
                }
            }

            return ret;
        }

        /// <summary>
        /// Ajoute les classes d'héritage.
        /// </summary>
        /// <param name="elementsExtension">Le noeud XML contenant les extentions d'éléments.</param>
        /// <param name="modelFile">Nom du fichier modèle en cours de traitement.</param>
        private void BuildModelNamespaceGeneralizations(XmlNode elementsExtension, string modelFile) {
            if (elementsExtension == null) {
                throw new ArgumentNullException("elementsExtension");
            }

            foreach (XmlNode connectorChildNode in elementsExtension.SelectNodes(NodeGeneralizations, _currentNsManager)) {
                if (connectorChildNode != null) {
                    XmlNode generalizationNode = connectorChildNode.ParentNode;
                    ModelClass[] classAssociationTab = GetModelClassAssociation(generalizationNode, modelFile);
                    classAssociationTab[0].ParentClass = classAssociationTab[1];
                    classAssociationTab[0].AddUsing(classAssociationTab[1].Namespace);
                }
            }
        }

        /// <summary>
        /// Retourne les 2 classes concernées par une association.
        /// </summary>
        /// <param name="associationNode">Le noeud association concerné.</param>
        /// <param name="modelFile">Nom du fichier modèle en cours de traitement.</param>
        /// <returns>Les 2 classes concernées par l'association.</returns>
        private ModelClass[] GetModelClassAssociation(XmlNode associationNode, string modelFile) {
            XmlNode objectSourceNode = associationNode.SelectSingleNode(NodeSource, _currentNsManager);
            XmlNode objectTargetNode = associationNode.SelectSingleNode(NodeTarget, _currentNsManager);

            if (objectSourceNode != null && objectTargetNode != null) {
                ModelClass class1 = _classByIdMap[modelFile + ":" + objectSourceNode.Attributes[PropertyIdRef].Value];
                ModelClass class2 = _classByIdMap[modelFile + ":" + objectTargetNode.Attributes[PropertyIdRef].Value];
                return new ModelClass[] { class1, class2 };
            }

            return null;
        }

        /// <summary>
        /// Ajoute les propriétés correspondantes aux clés étrangères dans les objets.
        /// </summary>
        /// <param name="elementsExtension">Le noeud XML contenant les extentions d'éléments.</param>
        /// <param name="modelFile">Nom du fichier modèle en cours de traitement.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = "Non internationalisé")]
        private void BuildModelNamespaceAssociations(XmlNode elementsExtension, string modelFile) {
            if (elementsExtension == null) {
                throw new ArgumentNullException("elementsExtension");
            }

            foreach (XmlNode connectorChildNode in elementsExtension.SelectNodes(NodeAssociations, _currentNsManager)) {
                if (connectorChildNode != null) {
                    XmlNode associationNode = connectorChildNode.ParentNode;
                    XmlNode sourceNode = associationNode.SelectSingleNode(NodeSource, _currentNsManager);
                    XmlNode targetNode = associationNode.SelectSingleNode(NodeTarget, _currentNsManager);
                    string name = associationNode.SelectSingleNode(NodeLabels, _currentNsManager).Attributes[PropertyAssociationName].Value;
                    string columnName = null;
                    XmlNode columnNameNode = associationNode.SelectSingleNode(NodeTagColumnName, _currentNsManager);
                    if (columnNameNode != null) {
                        columnName = columnNameNode.Attributes[PropertyValue].Value;
                    }

                    XmlAttribute associationCodeAttribute = associationNode.SelectSingleNode(NodeStyle, _currentNsManager).Attributes[PropertyValue];
                    string code = (associationCodeAttribute != null) ? _regexAssociationName.Replace(associationCodeAttribute.Value, "$1") : string.Empty;
                    XmlAttribute commentAttribute = associationNode.SelectSingleNode(NodeComment, _currentNsManager).Attributes[PropertyValue];
                    string comment = (commentAttribute == null) ? null : commentAttribute.Value;
                    string multiplicitySource = sourceNode.SelectSingleNode(NodeType, _currentNsManager).Attributes[PropertyMultiplicity].Value;
                    if ("1".Equals(multiplicitySource)) {
                        multiplicitySource = Multiplicity11;
                    }

                    string multiplicityTarget = targetNode.SelectSingleNode(NodeType, _currentNsManager).Attributes[PropertyMultiplicity].Value;
                    if ("1".Equals(multiplicityTarget)) {
                        multiplicityTarget = Multiplicity11;
                    }

                    XmlAttribute roleSourceAttribute = sourceNode.SelectSingleNode(NodeRole, _currentNsManager).Attributes[PropertyName];
                    string roleSourceName = (roleSourceAttribute == null) ? string.Empty : roleSourceAttribute.Value;
                    XmlAttribute roleTargetAttribute = targetNode.SelectSingleNode(NodeRole, _currentNsManager).Attributes[PropertyName];
                    string roleTargetName = (roleTargetAttribute == null) ? string.Empty : roleTargetAttribute.Value;

                    ModelClass[] classAssociationTab;
                    try {
                        classAssociationTab = GetModelClassAssociation(associationNode, modelFile);
                    } catch (Exception ex) {
                        throw new NotSupportedException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                @"Impossible de retrouver les classes de l'association suivante : [Code]:{0}, [Nom]:{1}. 
                                Vérifier que la classe avec la multiplicité la plus faible est bien définie avant l'autre classe.
                                L'ordre de parsing des modèles est défini dans Build\class_generator.conf.",
                                code,
                                name),
                            ex);
                    }

                    ModelClass classSource = classAssociationTab[0];
                    ModelClass classTarget = classAssociationTab[1];

                    string typeAssociation = associationNode.SelectSingleNode(NodeProperties, _currentNsManager).Attributes[PropertyEAType].Value;
                    bool isTargetContientSource = AssociationComposition.Equals(sourceNode.SelectSingleNode(NodeType, _currentNsManager).Attributes[PropertyAggregation].Value);
                    bool isSourceContientTarget = AssociationComposition.Equals(targetNode.SelectSingleNode(NodeType, _currentNsManager).Attributes[PropertyAggregation].Value);

                    if (multiplicitySource != Multiplicity01 && multiplicitySource != Multiplicity11 && multiplicitySource != Multiplicty0N && multiplicitySource != Multiplicty1N) {
                        RegisterError(Category.Error, "La multiplicité " + multiplicitySource + " du lien [" + code + "] entre les classes [" + classSource.Name + "] et [" + classTarget.Name + "] n'est pas gérée.");
                        continue;
                    }

                    if (multiplicityTarget != Multiplicity01 && multiplicityTarget != Multiplicity11 && multiplicityTarget != Multiplicty0N && multiplicityTarget != Multiplicty1N) {
                        RegisterError(Category.Error, "La multiplicité " + multiplicityTarget + " du lien [" + code + "] entre les classes [" + classSource.Name + "] et [" + classTarget.Name + "] n'est pas gérée.");
                        continue;
                    }

                    bool multiplicityOk = false;
                    if (multiplicitySource == Multiplicty0N && (multiplicityTarget == Multiplicity01 || multiplicityTarget == Multiplicity11 || multiplicityTarget == Multiplicty0N)) {
                        multiplicityOk = true;
                    }

                    if (multiplicityTarget == Multiplicty0N && (multiplicitySource == Multiplicity01 || multiplicitySource == Multiplicity11 || multiplicitySource == Multiplicty0N)) {
                        multiplicityOk = true;
                    }

                    if (!multiplicityOk && classTarget.IsTable && classSource.IsTable) {
                        RegisterError(Category.Error, "L'association [" + code + "] (" + multiplicitySource + "/" + multiplicityTarget + ") entre les classes [" + classSource.Name + "] et [" + classTarget.Name + "] n'est pas gérée.");
                        continue;
                    }

                    if (EATypeAssociation.Equals(typeAssociation) && classSource.Stereotype == StereotypeStatique && multiplicitySource == Multiplicty0N && classTarget.Stereotype != StereotypeStatique) {
                        RegisterError(Category.Error, "L'association [" + code + "] (" + multiplicitySource + "/" + multiplicityTarget + ") entre les classes [" + classSource.Name + "] statique et [" + classTarget.Name + "] n'est pas gérée.");
                        continue;
                    }

                    if (classTarget.Stereotype == StereotypeStatique && multiplicityTarget == Multiplicty0N && !(multiplicitySource == Multiplicty0N) && classSource.Stereotype != StereotypeStatique) {
                        RegisterError(Category.Error, "L'association [" + code + "] (" + multiplicitySource + "/" + multiplicityTarget + ") entre les classes [" + classSource.Name + "] et [" + classTarget.Name + "] statique n'est pas gérée.");
                        continue;
                    }

                    if (classSource.Stereotype == StereotypeReference && multiplicitySource == Multiplicty0N && classTarget.Stereotype != StereotypeStatique && classTarget.Stereotype != StereotypeReference && classTarget.IsTable) {
                        RegisterError(Category.Error, "L'association [" + code + "] (" + multiplicitySource + "/" + multiplicityTarget + ") entre les classes [" + classSource.Name + "] reference et [" + classTarget.Name + "] n'est pas gérée.");
                        continue;
                    }

                    if (classTarget.Stereotype == StereotypeReference && multiplicityTarget == Multiplicty0N && !(multiplicitySource == Multiplicty0N) && classSource.Stereotype != StereotypeStatique && classSource.Stereotype != StereotypeReference && !classSource.IgnoreReferenceToReference) {
                        RegisterError(Category.Error, "L'association [" + code + "] (" + multiplicitySource + "/" + multiplicityTarget + ") entre les classes [" + classSource.Name + "] et [" + classTarget.Name + "] reference n'est pas gérée.");
                        continue;
                    }

                    if (string.IsNullOrEmpty(name)) {
                        RegisterError(Category.Error, "L'association [" + code + "] ne porte pas le libellé de la propriété associée à la clef étrangère.");
                        continue;
                    }

                    bool isComposition = !string.IsNullOrEmpty(typeAssociation) && EATypeAggregation.Equals(typeAssociation);
                    if (isComposition) {

                        // Si composition il faut traiter la cardinalité target et l'ajouter dans la classe source ou inversement.
                        ModelProperty property;
                        if (isTargetContientSource) {
                            property = ParserHelper.BuildClassCompositionProperty(classSource, multiplicitySource, name, code, string.IsNullOrEmpty(comment) ? name : comment);
                            property.Class = classTarget;
                            classTarget.AddProperty(property);
                            classTarget.AddUsing(classSource.Namespace);
                        } else if (isSourceContientTarget) {
                            property = ParserHelper.BuildClassCompositionProperty(classTarget, multiplicityTarget, name, code, string.IsNullOrEmpty(comment) ? name : comment);
                            property.Class = classSource;
                            classSource.AddProperty(property);
                            classSource.AddUsing(classTarget.Namespace);
                        } else {
                            RegisterError(Category.Error, "La composition [" + code + "] entre les classes [" + classSource.Name + "] et [" + classTarget.Name + "] doit définir l'élément qui contient l'autre.");
                            continue;
                        }

                        IndexAssociation(property, comment);
                    } else {
                        bool treated = false;
                        if (Multiplicity01.Equals(multiplicityTarget) || Multiplicity11.Equals(multiplicityTarget)) {

                            // On ajoute la clé primaire de la classe target dans la classe source
                            ModelProperty property = ParserHelper.BuildClassAssociationProperty(classTarget, classSource, multiplicityTarget, roleSourceName, name, columnName);
                            if (classSource.DataContract.IsPersistent && !classTarget.DataContract.IsPersistent) {
                                RegisterError(Category.Error, "L'association [" + code + "] de multiplicité 0..1/1..1 entre la classe persistente [" + classSource.Name + "] et la classe non persistente [" + classTarget.Name + "] n'est pas possible.");
                                continue;
                            }

                            property.Class = classSource;
                            classSource.AddProperty(property);
                            IndexAssociation(property, comment);
                            treated = true;
                        }

                        if (Multiplicity01.Equals(multiplicitySource) || Multiplicity11.Equals(multiplicitySource)) {

                            // On ajoute la clé primaire de la classe source dans la classe target
                            ModelProperty property = ParserHelper.BuildClassAssociationProperty(classSource, classTarget, multiplicitySource, roleTargetName, name, columnName);
                            if (classTarget.DataContract.IsPersistent && !classSource.DataContract.IsPersistent) {
                                RegisterError(Category.Error, "L'association [" + code + "] de multiplicité 0..1/1..1 entre la classe persistente [" + classTarget.Name + "] et la classe non persistente [" + classSource.Name + "] n'est pas possible.");
                                continue;
                            }

                            property.Class = classTarget;
                            classTarget.AddProperty(property);
                            IndexAssociation(property, comment);
                            treated = true;
                        }

                        if (Multiplicty0N.Equals(multiplicitySource) && Multiplicty0N.Equals(multiplicityTarget)) {

                            // On ajoute une liste de la clé primaire de la classe target dans la classe source
                            if (classSource.DataContract.IsPersistent) {
                                RegisterError(Category.Error, "L'association [" + code + "] de multiplicité 0..N/0..N entre la classe persistente [" + classSource.Name + "] et la classe [" + classTarget.Name + "] n'est pas possible.");
                                continue;
                            }

                            ModelProperty property = ParserHelper.BuildClassAssociationListProperty(classTarget, classSource, roleSourceName, name);
                            property.Class = classSource;
                            classSource.AddProperty(property);
                            treated = true;
                        }

                        if (!treated) {
                            RegisterError(Category.Bug, "Le lien d'association [" + code + "] entre les classes [" + classSource.Name + "] et [" + classTarget.Name + "] nommé " + name + " (code=" + code + ") n'est pas géré actuellement.");
                        }
                    }
                }
            }
        }
    }
}
