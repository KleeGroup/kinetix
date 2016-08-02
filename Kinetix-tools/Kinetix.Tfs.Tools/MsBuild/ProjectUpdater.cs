using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Tfs.Tools.Client;
using Microsoft.Build.Evaluation;

namespace Kinetix.Tfs.Tools.MsBuild {

    /// <summary>
    /// Updater de projet MSBuild.
    /// </summary>
    public class ProjectUpdater {

        /// <summary>
        /// Client TFS.
        /// </summary>
        private readonly TfsClient _client;

        /// <summary>
        /// Créé une nouvelle instance de ProjectUpdater.
        /// </summary>
        /// <param name="client">Client TFS.</param>
        private ProjectUpdater(TfsClient client) {
            _client = client;
        }

        /// <summary>
        /// Créé un ProjectUpdater.
        /// </summary>
        /// <param name="client">Client TFS.</param>
        /// <returns>ProjectUpdater.</returns>
        public static ProjectUpdater Create(TfsClient client) {
            return new ProjectUpdater(client);
        }

        /// <summary>
        /// Ajoute un Item à un projet MSBuild.
        /// </summary>
        /// <param name="projetFilePath">Chemin du csproj.</param>
        /// <param name="item">Item à ajouter.</param>
        public void AddItem(string projetFilePath, ProjectItem item) {
            this.AddItems(projetFilePath, new List<ProjectItem> { item });
        }

        /// <summary>
        /// Ajoute une liste d'items à un projet MSBuild.
        /// </summary>
        /// <param name="projetFilePath">Chemin du csproj.</param>
        /// <param name="items">Liste d'items à ajouter.</param>
        public void AddItems(string projetFilePath, ICollection<ProjectItem> items) {

            Project project = new Project(projetFilePath);

            var missingItems = items.Where(x => !HasItem(project, x.ItemPath));

            if (!missingItems.Any()) {
                project.ProjectCollection.UnloadProject(project);
                return;
            }

            foreach (var item in missingItems) {
                Console.WriteLine("Project adding " + item.ItemPath + "...");
                project.AddItem(item.BuildAction, item.ItemPath);
            }

            Console.WriteLine("Project checkout : " + projetFilePath);
            _client.CheckOut(projetFilePath);

            project.Save(projetFilePath);
            project.ProjectCollection.UnloadProject(project);
        }

        /// <summary>
        /// Indique si un projet possède un item donné.
        /// </summary>
        /// <param name="project">Project.</param>
        /// <param name="itemPath">Chemin relatif de l'item dans le projet.</param>
        /// <returns><code>True</code> si l'item existe.</returns>
        private static bool HasItem(Project project, string itemPath) {
            return project.Items.Any(x => x.EvaluatedInclude == itemPath);
        }
    }
}
