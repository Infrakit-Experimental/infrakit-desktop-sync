using File_Sync_App.Nodes.Files;
using Library;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace File_Sync_App.Nodes.Folders
{
    /// <summary>
    /// Represents a general folder.
    /// </summary>
    public abstract class Folder : Node
    {
        #region variables

        /// <summary>
        /// The list of child nodes in this folder.
        /// </summary>
        public List<Node> children { get; set; }

        /// <summary>
        /// A boolean indicating if the Folder is deleted.
        /// </summary>
        internal bool isDeleted;

        #endregion variables

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Folder"/> class.
        /// </summary>
        /// <param name="content">The content of the folder.</param>
        /// <param name="isChecked">A boolean indicating whether the folder is checked.</param>
        /// <param name="parent">The parent folder of the current folder.</param>
        /// <param name="isDeleted">A boolean indicating whether the folder is deleted.</param>
        protected Folder(string content, bool? isChecked, Folder? parent, bool isDeleted) : base(content, isChecked, parent)
        {
            this.children = new();

            this.isDeleted = isDeleted;
        }

        #endregion constructors

        #region im-/export

        /// <summary>
        /// Initializes a new instance of the Folder class from an XML node.
        /// </summary>
        /// <param name="xmlNode">The XML node to initialize the folder from.</param>
        /// <param name="parent">The parent folder of this folder.</param>
        internal Folder(XmlNode xmlNode, Folder? parent) : base(xmlNode, parent)
        {
            this.children = new();

            this.isDeleted = false;
        }

        /// <summary>
        /// Gets an XML node that represents this folder.
        /// </summary>
        /// <param name="doc">The XML document to create the node in.</param>
        /// <returns>An XML node that represents this folder.</returns>
        protected XmlNode getXmlNode(XmlDocument doc)
        {
            var node = getXmlNode(doc, "Folder");

            #region children

            if (children is not null)
            {
                foreach (var child in children)
                {
                    switch (child)
                    {
                        case IKFolder f:
                            node.AppendChild(f.getXmlNode(doc));
                            break;

                        case LFolder f:
                            node.AppendChild(f.getXmlNode(doc));
                            break;

                        case IKFile f:
                            node.AppendChild(f.getXmlNode(doc));
                            break;

                        case LFile f:
                            node.AppendChild(f.getXmlNode(doc));
                            break;
                    }
                }
            }

            #endregion children

            return node;
        }

        #endregion im-/export

        #region get

        /// <summary>
        /// Gets the root folder of the checked folders in this folder hierarchy.
        /// </summary>
        /// <returns>The root folder of this folder hierarchy, or null if there is no root folder.</returns>
        public Folder? getRoot()
        {
            if (!isChecked.HasValue || isChecked.Value) return this;

            if (children is null) return null;

            foreach (var child in getFolderChildren())
            {
                var root = child.getRoot();

                if (root is not null) return root;
            }

            return null;
        }

        /// <summary>
        /// Gets a node with the specified name from this folder.
        /// </summary>
        /// <param name="name">The name of the node to get.</param>
        /// <param name="isFolder">Whether the node is a folder.</param>
        /// <returns>A node with the specified name from this folder, or null if the node does not exist.</returns>
        protected dynamic? get(string name, bool isFolder)
        {
            if (this.children is null) return null;

            dynamic? children = null;

            if (isFolder)
            {
                children = getFolderChildren();
            }
            else
            {
                children = getFileChildren();
            }

            if (children is null) return null;
            if (children.Count == 0) return null;

            foreach (var f in children)
            {
                if (f.content.Equals(name)) return f;
            }

            return null;
        }

        /// <summary>
        /// Gets a folder with the specified content from this folder.
        /// </summary>
        /// <param name="folder">The folder to get.</param>
        /// <returns>A folder with the specified content from this folder, or null if the folder does not exist.</returns>
        internal Folder? get(Folder folder)
        {
            if (folder is null) return null;

            return get(folder.content, true);
        }

        /// <summary>
        /// Gets a file with the specified content from this folder.
        /// </summary>
        /// <param name="file">The file to get.</param>
        /// <returns>A file with the specified content from this folder, or null if the file does not exist.</returns>
        internal File? get(File file)
        {
            if (file is null) return null;

            return get(file.content, false);
        }

        #region get children

        protected List<Folder> getFolderChildren()
        {
            List<Folder> children = new();

            if (this.children is null) return children;

            foreach (var child in this.children)
            {
                if (!child.GetType().IsSubclassOf(typeof(Folder))) continue;

                children.Add((Folder)child);
            }

            return children;
        }

        protected List<File> getFileChildren()
        {
            List<File> children = new();

            if (this.children is null) return children;

            foreach (var child in this.children)
            {
                if (!child.GetType().IsSubclassOf(typeof(File))) continue;

                children.Add((File)child);
            }

            return children;
        }

        #endregion get children

        #endregion get

        /// <summary>
        /// Counts the number of folders in this folder, including subfolders.
        /// </summary>
        /// <returns>The number of folders in this folder.</returns>
        public int countFolders()
        {
            int count = 1;

            foreach (var child in getFolderChildren())
            {
                count += child.countFolders();
            }

            return count;
        }

        /// <summary>
        /// Checks if a node with the specified name exists in this folder.
        /// </summary>
        /// <param name="name">The name of the node to check for.</param>
        /// <param name="isFolder">Whether the node is a folder.</param>
        /// <returns>True if a node with the specified name exists in this folder, false otherwise.</returns>
        protected bool exists(string name, bool isFolder)
        {
            if (this.children is null) return false;

            dynamic? children = null;

            if (isFolder)
            {
                children = getFolderChildren();
            }
            else
            {
                children = getFileChildren();
            }

            if (children is null) return false;
            if (children.Count == 0) return false;

            foreach (var f in children)
            {
                if (f.content.Equals(name)) return true;
            }

            return false;
        }

        #region set

        /// <summary>
        /// Sets the isChecked property of this folder and all of its child nodes.
        /// </summary>
        /// <param name="isChecked">The value to set the isChecked property to.</param>
        internal void setAll(bool? isChecked)
        {
            this.isChecked = isChecked;

            if (children is null) return;

            foreach (var child in children)
            {
                switch (child)
                {
                    case Folder f:
                        f.setAll(isChecked);
                        break;

                    case File f:
                        f.isChecked = isChecked;
                        break;
                }
            }
        }

        #endregion set

        #region is checked

        /// <summary>
        /// Checks if this folder or any of its child folders are checked.
        /// </summary>
        /// <returns>True if this folder or any of its child folders are checked, false otherwise.</returns>
        public bool hasCheckedFolders()
        {
            if (!isChecked.HasValue || isChecked.Value) return true;

            foreach (var child in getFolderChildren())
            {
                if (child.hasCheckedFolders())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reparses the checked nodes in this folder and its child folders.
        /// </summary>
        public void reparseChecked()
        {
            var checkedNodes = checkChecked();

            if (checkedNodes == 1)
            {
                var file = getCheckedFile();

                if (file is not null)
                {
                    setAll(false);

                    if (file.parent != null)
                    {
                        file.parent.isChecked = null;
                    }

                    file.isChecked = true;
                    return;
                }
            }

            checkChecked(checkedNodes);
        }

        /// <summary>
        /// Sets the isChecked property of this folder and all of its child nodes.
        /// </summary>
        public void setChecked()
        {
            if (children is null) return;

            if (!isChecked.HasValue) return;

            foreach (Node f in children)
            {
                f.isChecked = isChecked.Value;
            }

            foreach (Folder f in getFolderChildren())
            {
                f.setChecked();
            }

        }

        /// <summary>
        /// Gets the first checked file in this folder or its child folders.
        /// </summary>
        /// <returns>The first checked file in this folder or its child folders, or null if there is no checked file.</returns>
        private File? getCheckedFile()
        {
            if (children is null) return null;

            if (children.Count == 0) return null;

            foreach (var child in children)
            {
                switch (child)
                {
                    case Folder f:
                        var file = f.getCheckedFile();
                        if (file is not null) return file;
                        break;

                    case File f:
                        if (f.isChecked.HasValue && f.isChecked.Value) return f;
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks the checked nodes in this folder and its child folders.
        /// </summary>
        /// <returns>The number of checked nodes in this folder and its child folders.</returns>
        private int checkChecked()
        {
            int count = 0;

            foreach (var child in getFolderChildren())
            {
                count += child.checkChecked();
            }

            foreach (var child in getFileChildren())
            {
                if (child.isChecked.HasValue && child.isChecked.Value)
                {
                    count++;
                }
            }

            if (count > 0 && isChecked.HasValue && !isChecked.Value)
            {
                isChecked = null;
            }
            else if (count == 0 && !isChecked.HasValue)
            {
                isChecked = false;
            }
            else if (isChecked.HasValue && isChecked.Value)
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// Checks the checked nodes in this folder and its child folders, starting from the specified number of checked nodes.
        /// </summary>
        /// <param name="checkedNodes">The number of checked nodes to start from.</param>
        /// <returns>The number of checked nodes in this folder and its child folders.</returns>
        private int checkChecked(int checkedNodes)
        {
            int countOverall = 0;
            int countDirekt = 0;

            if (children is not null)
            {
                foreach (var child in getFolderChildren())
                {
                    countOverall += child.checkChecked(checkedNodes);
                }

                foreach (var child in children)
                {
                    if (child.isChecked.HasValue && child.isChecked.Value)
                    {
                        countOverall++;
                    }

                    if (!child.isChecked.HasValue || child.isChecked.Value)
                    {
                        countDirekt++;
                    }
                }
            }

            if (!isChecked.HasValue && checkedNodes == countOverall && countDirekt < 2)
            {
                isChecked = false;
            }

            return countOverall;
        }

        #endregion is checked

        #region sync

        /// <summary>
        /// Synchronizes the local and Infrakit folders.
        /// </summary>
        /// <param name="local">The local folder to synchronize.</param>
        /// <param name="infrakit">The Infrakit folder to synchronize.</param>
        /// <returns>A Log.Folder object that represents the synchronized folder.</returns>
        internal static Log.Folder sync(LFolder local, IKFolder infrakit)
        {
            #region update

            if (!local.update())
            {
                return new Log.Folder(local.content, Log.SyncStatus.Error, Log.SyncStatus.NotSynced);
            }

            if (!infrakit.update())
            {
                return new Log.Folder(infrakit.content, Log.SyncStatus.NotSynced, Log.SyncStatus.Error);
            }

            #endregion update

            var logRoot = new Log.Folder(local.content, Log.SyncStatus.Synced, Log.SyncStatus.Synced);

            if (local.children.Count == 0 && infrakit.children.Count == 0) return logRoot;

            #region folders

            var ikFolders = infrakit.getFolderChildren();

            foreach (var lFolder in local.getFolderChildren())
            {
                var lToSync = lFolder.isToSync();

                var folder = infrakit.get(lFolder);
                if (folder is null)
                {
                    if (!lToSync)
                    {
                        logRoot.children.Add(new Log.Folder(lFolder.content, Log.SyncStatus.NotSynced, Log.SyncStatus.NotExisting));
                        continue;
                    }

                    #region delete

                    if (lFolder.isDeleted)
                    {
                        logRoot.children.Add(lFolder.remove());
                    }

                    #endregion delete

                    #region create

                    else
                    {
                        logRoot.children.Add(lFolder.create(infrakit));
                    }

                    #endregion create

                    continue;
                }

                var ikFolder = (IKFolder)folder;
                var ikToSync = ikFolder.isToSync();

                if (!lToSync)
                {
                    if (!ikToSync)
                    {
                        logRoot.children.Add(new Log.Folder(lFolder.content, Log.SyncStatus.NotSynced, Log.SyncStatus.NotSynced));
                        ikFolders.Remove(ikFolder);
                    }
                    continue;
                }

                if (!ikToSync)
                {
                    logRoot.children.Add(lFolder.sync(ikFolder));
                    ikFolders.Remove(ikFolder);
                    continue;
                }

                #region delete

                if (lFolder.isDeleted || ikFolder.isDeleted)
                {
                    ikFolders.Remove(ikFolder);

                    if (lFolder.isDeleted && ikFolder.isDeleted)
                    {
                        local.children.Remove(lFolder);
                        infrakit.children.Remove(ikFolder);

                        logRoot.children.Add(new Log.Folder(lFolder.content, Log.SyncStatus.NotExisting, Log.SyncStatus.NotExisting));
                    }
                    else if (lFolder.isDeleted)
                    {
                        logRoot.children.Add(lFolder.delete(ikFolder));
                    }
                    else //if (ikFolder.isDeleted)
                    {
                        logRoot.children.Add(ikFolder.delete(lFolder));
                    }

                    continue;
                }

                #endregion delete

                Folder.sync(lFolder, ikFolder);
            }

            foreach (var ikFolder in ikFolders)
            {
                if (!ikFolder.isToSync())
                {
                    logRoot.children.Add(new Log.Folder(ikFolder.content, Log.SyncStatus.NotExisting, Log.SyncStatus.NotSynced));
                    continue;
                }

                var folder = local.get(ikFolder);

                #region delete

                if (ikFolder.isDeleted)
                {
                    if (folder is null)
                    {
                        logRoot.children.Add(ikFolder.remove());
                    }
                    else
                    {
                        logRoot.children.Add(ikFolder.delete((LFolder)folder));
                    }

                    continue;
                }

                #endregion delete

                #region create

                if (folder is null)
                {
                    logRoot.children.Add(ikFolder.create(local));
                    continue;
                }

                #endregion create

                logRoot.children.Add(ikFolder.sync((LFolder)folder));
            }

            #endregion folders

            #region files

            var ikFiles = infrakit.getFileChildren();

            foreach (var lFile in local.getFileChildren())
            {
                var lToSync = lFile.isToSync();

                var file = infrakit.get(lFile);
                if (file is null)
                {
                    switch (lToSync)
                    {
                        case Log.SyncStatus.NotSynced:
                        case Log.SyncStatus.NoChanges:
                            logRoot.children.Add(new Log.File(lFile.content, lToSync, Log.SyncStatus.NotExisting));
                            break;

                        #region delete

                        case Log.SyncStatus.Deleted:
                            logRoot.children.Add(lFile.remove());
                            break;

                        #endregion delete

                        #region create

                        default:
                            logRoot.children.Add(lFile.create(infrakit));
                            break;

                            #endregion create
                    }

                    continue;
                }

                var ikFile = (IKFile)file;
                ikFiles.Remove(ikFile);

                var ikToSync = ikFile.isToSync();

                if (lToSync == Log.SyncStatus.NotSynced || lToSync == Log.SyncStatus.NoChanges)
                {
                    if (ikToSync == Log.SyncStatus.NotSynced || ikToSync == Log.SyncStatus.NoChanges)
                    {
                        logRoot.children.Add(new Log.File(lFile.content, lToSync, ikToSync));
                    }
                    else
                    {
                        ikFiles.Add(ikFile);
                    }

                    continue;
                }

                if (ikToSync == Log.SyncStatus.NotSynced || ikToSync == Log.SyncStatus.NoChanges)
                {
                    logRoot.children.Add(lFile.sync(ikFile));
                    continue;
                }

                #region delete

                var lDelete = lToSync == Log.SyncStatus.Deleted;
                var ikDelete = ikToSync == Log.SyncStatus.Deleted;

                if (lDelete || ikDelete)
                {
                    ikFiles.Remove(ikFile);
                }

                if (lDelete && ikDelete)
                {
                    local.children.Remove(lFile);
                    infrakit.children.Remove(ikFile);

                    logRoot.children.Add(new Log.Folder(lFile.content, Log.SyncStatus.NotExisting, Log.SyncStatus.NotExisting));
                    continue;
                }

                if (lDelete && !ikDelete)
                {
                    logRoot.children.Add(lFile.delete(ikFile));
                    continue;
                }

                if (!lDelete && ikDelete)
                {
                    logRoot.children.Add(ikFile.delete(lFile));
                    continue;
                }

                #endregion delete

                logRoot.children.Add(File.sync(lFile, ikFile));
            }

            foreach (var ikFile in ikFiles)
            {
                var ikToSync = ikFile.isToSync();
                if (ikToSync == Log.SyncStatus.NotSynced || ikToSync == Log.SyncStatus.NoChanges)
                {
                    logRoot.children.Add(new Log.File(ikFile.content, Log.SyncStatus.NotExisting, ikToSync));
                    continue;
                }

                var file = local.get(ikFile);

                #region delete

                if (ikToSync == Log.SyncStatus.Deleted)
                {
                    if (file is null)
                    {
                        logRoot.children.Add(ikFile.remove());
                    }
                    else
                    {
                        logRoot.children.Add(ikFile.delete((LFile)file));
                    }

                    continue;
                }

                #endregion delete

                #region create

                if (file is null)
                {
                    logRoot.children.Add(ikFile.create(local));
                    continue;
                }

                #endregion create

                logRoot.children.Add(ikFile.sync((LFile)file));
            }

            #endregion files

            return logRoot;
        }

        #endregion sync

        /// <summary>
        /// Updates the folder and its child nodes.
        /// </summary>
        /// <param name="updateAll">Whether to update all child nodes, including subfolders.</param>
        /// <returns>True if the folder was updated successfully, false otherwise.</returns>
        internal abstract bool update(bool updateAll);

        /// <summary>
        /// Gets an array of the file names that are existing in the folder.
        /// </summary>
        /// <returns>An array of file names.</returns>
        internal abstract string[]? getUpdatedFileNames();

        /// <summary>
        /// Removes the deleted files from the folder and its subfolders.
        /// With the restriction that only not checked files are removed.
        /// </summary>
        /// <returns>A boolean indicating whether the folder still has any children after the files have been removed.</returns>
        internal bool removeDeletedFiles()
        {
            if (this.isDeleted)
            {
                this.parent.children.Remove(this);
                return false;
            }

            var files = this.getUpdatedFileNames();

            if(files is null)
            {
                this.parent.children.Remove(this);
                return false;
            }

            bool hasChildren = false;

            #region folders

            foreach (var folder in this.getFolderChildren())
            {
                if (!folder.removeDeletedFiles() && (!folder.isChecked.HasValue || !folder.isChecked.Value))
                {
                    folder.parent.children.Remove(folder);
                }
            }

            #endregion folders

            #region files

            foreach (var file in this.getFileChildren())
            {
                if (file.isChecked.HasValue && file.isChecked.Value)
                {
                    hasChildren = true;
                    continue;
                }

                if (!files.Contains(file.content))
                {
                    file.parent.children.Remove(file);
                    continue;
                }

                hasChildren = true;
            }

            #endregion files

            return hasChildren;
        }
    }
}