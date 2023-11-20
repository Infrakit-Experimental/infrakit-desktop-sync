using System;
using System.Collections.Generic;

namespace Library.Models
{
    /// <summary>
    /// Represents an folder.
    /// </summary>
    public class Folder
    {
        /// <summary>
        /// The id of the folder.
        /// </summary>
        public int? id { get; set; }

        /// <summary>
        /// The unique identifier of the folder.
        /// </summary>
        public Guid uuid { get; set; }

        /// <summary>
        /// The name of the folder.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The path to the folder.
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// The unique identifier of the parent folder, or null if the folder is a root folder.
        /// </summary>
        public Guid? parentFolderUuid { get; set; }

        /// <summary>
        /// The depth of the folder in the folder structure.
        /// </summary>
        public int? depth { get; set; }

        /// <summary>
        /// The project that the folder belongs to.
        /// </summary>
        public Project? project { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Folder"/> class.
        /// </summary>
        /// <param name="uuid">The unique identifier of the folder.</param>
        /// <param name="name">The name of the folder.</param>
        /// <param name="path">The path to the folder.</param>
        /// <param name="parentFolderUuid">The unique identifier of the parent folder, or null if the folder is a root folder.</param>
        /// <param name="depth">The depth of the folder in the folder structure.</param>
        /// <param name="id">The database ID of the folder, or null if the folder has not yet been saved to the database.</param>
        /// <param name="project">The project that the folder belongs to, or null if the folder does not belong to a project.</param>
        public Folder(Guid uuid, string name, string path, Guid? parentFolderUuid = null, int? depth = null, int? id = null, Project? project = null)
        {
            this.uuid = uuid;
            this.name = name;
            this.path = path;

            this.parentFolderUuid = parentFolderUuid;

            this.depth = depth;
            this.id = id;
            this.project = project;
        }

        /// <summary>
        /// A class that represents a folder structure.
        /// </summary>
        public class Structur : Folder
        {
            /// <summary>
            /// A list of the child folders of this folder.
            /// </summary>
            public List<Structur> children;

            /// <summary>
            /// Initializes a new instance of the <see cref="Structur"/> class.
            /// </summary>
            /// <param name="metaData">The metadata of the folder.</param>
            /// <param name="folderStructure">A list of all folders in the folder structure.</param>
            public Structur(Folder metaData, List<Folder> folderStructure) : base(metaData.uuid, metaData.name, metaData.path, metaData.parentFolderUuid, metaData.depth)
            {
                #region get child folders

                List<Folder> childFolders = new();

                foreach (var folder in folderStructure)
                {
                    if (folder.parentFolderUuid is not null && folder.parentFolderUuid.Equals(metaData.uuid))
                    {
                        childFolders.Add(folder);
                    }
                }

                #endregion get child folders

                this.children = new();
                foreach (var childFolder in childFolders)
                {
                    this.children.Add(new Structur(childFolder, folderStructure));
                }
            }

            /// <summary>
            /// Gets the root folder of the folder structure.
            /// </summary>
            /// <param name="folderStructure">A list of all folders in the folder structure.</param>
            /// <returns>The root folder of the folder structure, or null if not found.</returns>
            public static Structur? getFolderStructur(List<Folder> folderStructure)
            {
                foreach (var folder in folderStructure)
                {
                    if (folder.depth != 0) continue;

                    return new Structur(folder, folderStructure);
                }

                return null;
            }

            /// <summary>
            /// Uploads the folder structure to the API.
            /// </summary>
            /// <param name="root">The GUID of the root folder.</param>
            /// <param name="includeRoot">Whether to include the root folder in the upload.</param>
            public void upload(Guid root, bool includeRoot = true)
            {
                if (includeRoot)
                {
                    var tmp = API.Folder.post(root, this.name);
                    if (!tmp.HasValue) return;

                    root = tmp.Value;
                }

                foreach (var child in this.children)
                {
                    child.upload(root);
                }
            }
        }
    }
}
