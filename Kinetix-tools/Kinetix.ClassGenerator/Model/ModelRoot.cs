using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kinetix.ClassGenerator.Model {

    /// <summary>
    /// Cette classe représente la racine du modèle objet.
    /// </summary>
    public sealed class ModelRoot : IModelObject {

        private readonly Dictionary<string, ModelDomain> _mapDomains = new Dictionary<string, ModelDomain>();
        private readonly Dictionary<string, ModelDomain> _mapShortCuts = new Dictionary<string, ModelDomain>();

        /// <summary>
        /// Constructeur du modèle.
        /// </summary>
        public ModelRoot() {
            Namespaces = new Dictionary<string, ModelNamespace>();
        }

        /// <summary>
        /// Retourne le libellé du modèle.
        /// </summary>
        public string Label {
            get;
            set;
        }

        /// <summary>
        /// Retourne le nom du modèle.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le nom du fichier.
        /// </summary>
        public string ModelFile {
            get;
            set;
        }

        /// <summary>
        /// Liste des namespaces du modèle.
        /// </summary>
        public Dictionary<string, ModelNamespace> Namespaces {
            get;
            private set;
        }

        /// <summary>
        /// Liste des domaines utilisables du modèle.
        /// </summary>
        public Dictionary<string, ModelDomain> UsableDomains {
            get {
                Dictionary<string, ModelDomain> retValue = new Dictionary<string, ModelDomain>(_mapDomains);
                foreach (string key in _mapShortCuts.Keys) {
                    retValue.Add(key, _mapShortCuts[key]);
                }

                return retValue;
            }
        }

        /// <summary>
        /// Retourne la liste non éditable de tous les nouveaux domaines déclarés dans le domaine.
        /// </summary>
        public ReadOnlyCollection<ModelDomain> CreatedDomains {
            get {
                return new ReadOnlyCollection<ModelDomain>(new List<ModelDomain>(_mapDomains.Values));
            }
        }

        /// <summary>
        /// Ajouter un namespace à la liste.
        /// </summary>
        /// <param name="nmspace">Le namespace à ajouter.</param>
        /// <exception cref="System.ArgumentNullException">Si nmspace est null.</exception>
        public void AddNamespace(ModelNamespace nmspace) {
            if (nmspace == null) {
                throw new ArgumentNullException("nmspace");
            }

            Namespaces.Add(nmspace.Name, nmspace);
        }

        /// <summary>
        /// Ajoute un domaine à la liste.
        /// </summary>
        /// <param name="id">Identifiant OOM du domaine.</param>
        /// <param name="domain">Le domaine à ajouter.</param>
        /// <exception cref="System.ArgumentNullException">Si le paramètre domain est null.</exception>
        /// <exception cref="System.NotSupportedException">Si un raccourci de domaine existe déja pour la même clef.</exception>
        public void AddDomain(string id, ModelDomain domain) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException("id");
            }

            if (domain == null) {
                throw new ArgumentNullException("domain");
            }

            if (string.IsNullOrEmpty(domain.Code)) {
                throw new ArgumentNullException("domain", "domain.Code is null.");
            }

            if (_mapShortCuts.ContainsKey(domain.Code)) {
                throw new NotSupportedException("Un raccourci de domaine existe déja pour le code " + domain.Code);
            }

            _mapDomains.Add(id, domain);
        }

        /// <summary>
        /// Ajoute un raccourci de domaine au modèle.
        /// </summary>
        /// <param name="id">Clef du raccourci.</param>
        /// <param name="domain">Le domaine en question.</param>
        /// <exception cref="System.NotSupportedException">Si un raccourci possède la même clef dans le fichier, ou si un domaine possède la même clef.</exception>
        public void AddDomainShortcut(string id, ModelDomain domain) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException("id");
            }

            if (domain == null) {
                throw new ArgumentNullException("domain");
            }

            if (_mapShortCuts.ContainsKey(id)) {
                throw new NotSupportedException("Raccourci de domaine déja défini pour la clef " + id);
            }

            if (_mapDomains.ContainsKey(id)) {
                throw new NotSupportedException("Un domaine définit déja la clef " + id);
            }

            _mapShortCuts.Add(id, domain);
        }

        /// <summary>
        /// Retourne un domaine a partir de son identifiant.
        /// </summary>
        /// <param name="domainCode">Code du domaine (DO_ID, DO_CD etc.).</param>
        /// <returns>Le domaine.</returns>
        public ModelDomain GetDomainByCode(string domainCode) {
            foreach (ModelDomain domain in UsableDomains.Values) {
                if (domain.Code == domainCode) {
                    return domain;
                }
            }

            throw new KeyNotFoundException("Le domaine possédant le code " + domainCode + " n'existe pas.");
        }

        /// <summary>
        /// Retourne si un domaine correspondant au code a été déclaré.
        /// </summary>
        /// <param name="domainCode">Code du domaine.</param>
        /// <returns><code>True</code> si le domaine a été trouvé, <code>False</code> sinon.</returns>
        public bool HasDomainByCode(string domainCode) {
            foreach (ModelDomain domain in UsableDomains.Values) {
                if (domain.Code == domainCode) {
                    return true;
                }
            }

            return false;
        }
    }
}
