using File_Sync_App.Nodes.Files;
using Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;
using System.Xml;

namespace File_Sync_App.Nodes.Folders
{
    /// <summary>
    /// Represents a folder on the Infrakit server.
    /// </summary>
    public class IKFolder : Folder
    {
        #region variables

        /// <summary>
        /// The UUID of the folder on the Infrakit server.
        /// </summary>
        public new Guid pos
        {
            get => base.pos;
            set => base.pos = value;
        }

        #endregion variables

        #region constructors

        /// <summary>
        /// Initializes a new instance of the IKFolder class.
        /// </summary>
        /// <param name="metaData">The folder meta data.</param>
        /// <param name="folderStructure">The folder structure.</param>
        /// <param name="parent">The parent folder.</param>
        public IKFolder(Library.Models.Folder metaData, List<Library.Models.Folder> folderStructure, Folder? parent = null) : base(metaData.name, false, parent, false)
        {
            this.pos = metaData.uuid;

            #region get child folders

            List<Library.Models.Folder> childFolders = new();

            foreach (var folder in folderStructure)
            {
                if (folder.parentFolderUuid is not null && folder.parentFolderUuid.Equals(metaData.uuid))
                {
                    childFolders.Add(folder);
                }
            }

            #endregion get child folders

            foreach (var childFolder in childFolders)
            {
                children.Add(new IKFolder(childFolder, folderStructure, this));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IKFolder"/> class.
        /// </summary>
        /// <param name="pos">The unique identifier of the folder.</param>
        /// <param name="content">The content of the folder.</param>
        /// <param name="isChecked">A boolean indicating whether the folder is checked.</param>
        /// <param name="parent">The parent folder of the current folder.</param>
        /// <param name="isDeleted">A boolean indicating whether the folder is deleted. The default value is false.</param>
        internal IKFolder(Guid pos, string content, bool? isChecked, Folder? parent, bool isDeleted = false) : base(content, isChecked, parent, isDeleted)
        {
            this.pos = pos;
        }

        #endregion constructors

        #region im-/export

        /// <summary>
        /// Creates a new IKFolder object from the given XML node.
        /// </summary>
        /// <param name="xmlNode">The XML node.</param>
        /// <param name="parent">The parent folder.</param>
        /// <returns>A new IKFolder object.</returns>
        internal IKFolder(XmlNode xmlNode, Folder? parent = null) : base(xmlNode, parent)
        {
            if (xmlNode.Attributes is not null)
            {
                #region pos

                var uuidNode = xmlNode.Attributes["uuid"];
                if (uuidNode is not null)
                {
                    pos = new Guid(uuidNode.Value);
                }

                #endregion pos
            }

            #region children

            if (!xmlNode.HasChildNodes) return;

            foreach (var child in xmlNode.ChildNodes)
            {
                switch (((XmlNode)child).Name)
                {
                    case "Folder":
                        children.Add(new IKFolder((XmlNode)child, this));
                        break;

                    case "File":
                        children.Add(new IKFile((XmlNode)child, this));
                        break;
                }
            }

            #endregion children
        }

        /// <summary>
        /// Creates an XML node for this IKFolder object.
        /// </summary>
        /// <param name="doc">The XML document.</param>
        /// <returns>An XML node.</returns>
        internal new XmlNode getXmlNode(XmlDocument doc)
        {
            var node = base.getXmlNode(doc);

            #region uuid

            var uuid = doc.CreateAttribute("uuid");
            uuid.Value = pos.ToString();
            node.Attributes.Append(uuid);

            #endregion uuid

            return node;
        }

        #endregion im-/export

        /// <summary>
        /// Creates a clone of this IKFolder object.
        /// </summary>
        /// <param name="parent">The parent folder of the clone.</param>
        /// <returns>A new IKFolder object that is a clone of this object.</returns>
        public IKFolder clone(IKFolder? parent = null)
        {
            var folder = new IKFolder(this.pos, this.content, this.isChecked, parent, this.isDeleted);

            foreach (var child in children)
            {
                switch (child)
                {
                    case IKFolder f:
                        folder.children.Add(f.clone(folder));
                        break;

                    case IKFile f:
                        folder.children.Add(f.clone(folder));
                        break;
                }
            }

            return folder;
        }

        #region get

        /// <summary>
        /// Gets the root folder of the checked folders in this folder hierarchy.
        /// </summary>
        /// <returns>The root folder of the folder hierarchy, or null if the folder is the root folder.</returns>
        public new IKFolder? getRoot()
        {
            var root = base.getRoot();

            if (root == null) return null;

            return (IKFolder)root;
        }

        /// <summary>
        /// Gets a list of the folder children of this IKFolder object.
        /// </summary>
        /// <returns>A list of IKFolder objects.</returns>
        internal new List<IKFolder> getFolderChildren()
        {
            List<IKFolder> children = new();

            if (this.children is null) return children;

            foreach (var child in this.children)
            {
                if (child.GetType() != typeof(IKFolder)) continue;

                children.Add((IKFolder)child);
            }

            return children;
        }

        /// <summary>
        /// Gets a list of the file children of this IKFolder object.
        /// </summary>
        /// <returns>A list of IKFile objects.</returns>
        internal new List<IKFile> getFileChildren()
        {
            List<IKFile> children = new();

            if (this.children is null) return children;

            foreach (var child in this.children)
            {
                if (child.GetType() != typeof(IKFile)) continue;

                children.Add((IKFile)child);
            }

            return children;
        }

        #endregion get

        /// <summary>
        /// Adds the documents from the Infrakit server to this IKFolder object.
        /// </summary>
        public void addDocuments()
        {
            foreach (var child in this.getFolderChildren())
            {
                child.addDocuments();
            }

            if (this.children is null)
            {
                this.children = new();
            }

            var docs = API.Folder.getDocuments(this.pos);

            if (docs is null || docs.Count == 0) return;

            foreach (var doc in docs)
            {
                this.children.Add(new IKFile(doc.uuid, doc.name, doc.timestamp, doc.version, false, this));
            }
        }

        #region sync

        /// <summary>
        /// Syncs this IKFolder object with the Infrakit server and the local folder.
        /// </summary>
        /// <param name="local">The local folder.</param>
        /// <returns>A Log.Folder object that contains the results of the sync operation.</returns>
        internal Log.Folder sync(LFolder local)
        {
            if (!this.update())
            {
                return new Log.Folder(this.content, Log.SyncStatus.NotSynced, Log.SyncStatus.Error);
            }

            var logRoot = new Log.Folder(content, Log.SyncStatus.NotSynced, Log.SyncStatus.Synced);

            if (children.Count == 0) return logRoot;

            #region folders

            foreach (var ikFolder in this.getFolderChildren())
            {
                if (!ikFolder.isToSync())
                {
                    logRoot.children.Add(new Log.Folder(ikFolder.content, Log.SyncStatus.NotSynced, Log.SyncStatus.NotSynced));
                    continue;
                }

                var folder = local.get(ikFolder);

                #region delete

                if (this.isDeleted)
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

            foreach (var ikFile in this.getFileChildren())
            {
                var ikToSync = ikFile.isToSync();

                if (ikToSync == Log.SyncStatus.NotSynced || ikToSync == Log.SyncStatus.NoChanges)
                {
                    logRoot.children.Add(new Log.File(ikFile.content, Log.SyncStatus.NotSynced, ikToSync));
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

        /// <summary>
        /// Creates this IKFolder object on the local file system.
        /// </summary>
        /// <param name="lParent">The parent folder on the local file system.</param>
        /// <returns>A Log.Folder object that contains the results of the create operation.</returns>
        internal Log.Folder create(LFolder lParent)
        {
            if (!this.update())
            {
                return new Log.Folder(this.content, Log.SyncStatus.NotSynced, Log.SyncStatus.Error);
            }

            #region create folder

            var target = Path.Combine(lParent.pos, this.content);
            Directory.CreateDirectory(target);

            if (!Directory.Exists(target))
            {
                Utils.Log.write("syncFailed.infrakit.folder.create: \"" + content + "\"");
                return new Log.Folder(content, Log.SyncStatus.NotSynced, Log.SyncStatus.Error);
            }

            #region new folder

            var isChecked = false;
            if (lParent.isChecked.HasValue)
            {
                isChecked = lParent.isChecked.Value;
            }

            var newLFolder = new LFolder(target, this.content, isChecked, lParent, this.isDeleted);
            lParent.children.Add(newLFolder);

            #endregion new folder

            #endregion create folder

            var log = new Log.Folder(content, Log.SyncStatus.Added, Log.SyncStatus.Synced);

            #region children

            foreach (var ikFolder in this.getFolderChildren())
            {
                if (!ikFolder.isToSync())
                {
                    log.children.Add(new Log.Folder(ikFolder.content, Log.SyncStatus.NotExisting, Log.SyncStatus.NotSynced));
                    continue;
                }

                log.children.Add(ikFolder.create(newLFolder));
            }

            foreach (var ikFile in this.getFileChildren())
            {
                var ikToSync = ikFile.isToSync();
                if (ikToSync == Log.SyncStatus.NotSynced || ikToSync == Log.SyncStatus.NoChanges)
                {
                    log.children.Add(new Log.File(ikFile.content, Log.SyncStatus.NotExisting, ikToSync));
                    continue;
                }

                if (ikToSync == Log.SyncStatus.Deleted)
                {
                    Utils.Log.write("syncFailed.infrakit.folder.create: \"" + content + "\"");
                    log.children.Add(new Log.File(ikFile.content, Log.SyncStatus.NotExisting, Log.SyncStatus.Error));
                    continue;
                }

                log.children.Add(ikFile.create(newLFolder));
            }

            #endregion children

            return log;
        }

        /// <summary>
        /// Deletes this IKFolder object and its children from the local file system.
        /// </summary>
        /// <param name="lTarget">The local folder to delete.</param>
        /// <returns>A Log.Folder object that contains the results of the delete operation.</returns>
        internal Log.Folder delete(LFolder lTarget)
        {
            this.parent.children.Remove(this);

            var logRoot = new Log.Folder(content, Log.SyncStatus.Removed, Log.SyncStatus.NotExisting);

            #region children

            foreach (var ikfolder in this.getFolderChildren())
            {
                var folder = lTarget.get(ikfolder);

                if (folder is null)
                {
                    logRoot.children.Add(ikfolder.remove());
                }
                else
                {
                    logRoot.children.Add(ikfolder.delete((LFolder)folder));
                }
            }

            foreach (var ikfile in this.getFileChildren())
            {
                var file = lTarget.get(ikfile);

                if (file is null)
                {
                    logRoot.children.Add(ikfile.remove());
                }
                else
                {
                    logRoot.children.Add(ikfile.delete((LFile)file));
                }
            }

            #endregion children

            #region delete folder

            if (MainWindow.deleteFoldersAndFiles)
            {
                Directory.Delete(lTarget.pos);

                if (Directory.Exists(lTarget.pos))
                {
                    lTarget.isChecked = false;
                    logRoot.statusLocal = Log.SyncStatus.Error;
                }
                else
                {
                    lTarget.parent.children.Remove(lTarget);
                    logRoot.statusLocal = Log.SyncStatus.Deleted;
                }
            }
            else
            {
                lTarget.setAll(false);
            }

            #endregion delete folder

            return logRoot;
        }

        /// <summary>
        /// Gets the log for an not existing folder.
        /// </summary>
        /// <returns>A Log.Folder object that contains the results of the remove operation.</returns>
        internal Log.Folder remove()
        {
            this.parent.children.Remove(this);

            var log = new Log.Folder(content, Log.SyncStatus.NotExisting, Log.SyncStatus.NotExisting);

            foreach (var child in this.children)
            {
                switch (child)
                {
                    case IKFolder f:
                        log.children.Add(f.remove());
                        break;

                    case IKFile f:
                        log.children.Add(f.remove());
                        break;
                }
            }

            this.children.Clear();
            return log;
        }

        #endregion sync

        #region update

        /// <summary>
        /// Updates this IKFolder object with the latest data from the Infrakit server.
        /// </summary>
        /// <param name="update">The latest data from the Infrakit server.</param>
        /// <returns>True if the IKFolder object was updated successfully, false otherwise.</returns>
        internal bool update()
        {
            #region get folders & files

            var update = API.Folder.get(this.pos, false, false);

            if (!update.HasValue) return false;

            var (folders, files) = update.Value;

            #endregion get folders & files

            #region update & add new folders

            foreach (var folder in this.getFolderChildren())
            {
                folder.isDeleted = true;
            }

            if (folders is not null)
            {
                foreach (var folder in folders)
                {
                    var existingFolder = this.get(folder.name, true);

                    if (existingFolder is not null)
                    {
                        existingFolder.isDeleted = false;
                        continue;
                    }

                    children.Add(new IKFolder(folder.uuid, folder.name, this.isChecked, this));
                }
            }

            #endregion update & add new folders

            #region update & add new files

            foreach (var file in this.getFileChildren())
            {
                file.timestamp = null;
                file.version = null;
            }

            if (files is not null)
            {
                foreach (var file in files)
                {
                    var existingFile = this.get(file.name, false);

                    if (existingFile is not null)
                    {
                        existingFile.timestamp = file.timestamp;
                        existingFile.version = file.version;
                        existingFile.pos = file.uuid;
                        continue;
                    }

                    children.Add(new IKFile(file.uuid, file.name, file.timestamp, file.version, isChecked, this));
                }
            }

            #endregion update & add new files

            return true;
        }

        /// <summary>
        /// Updates this IKFolder object with the latest data from the Infrakit server.
        /// </summary>
        /// <param name="updateAll">Whether to update all child folders and files.</param>
        /// <returns>True if the IKFolder object was updated successfully, false otherwise.</returns>
        internal override bool update(bool updateAll)
        {
            #region get folders & files

            var infrakit = API.Folder.get(pos, false, false);

            if (!infrakit.HasValue) return false;

            var (folders, files) = infrakit.Value;

            #endregion get folders & files

            var isChecked = false;
            if (this.isChecked.HasValue)
            {
                isChecked = this.isChecked.Value;
            }

            #region update folders

            #region update & remove not existing folders

            foreach (var child in getFolderChildren())
            {
                if (!updateAll && child.isChecked.HasValue && !child.isChecked.Value) continue;

                bool exists = false;
                foreach (var folder in folders)
                {
                    if (folder.name.Equals(child.content))
                    {
                        if (child.update(updateAll))
                        {
                            exists = true;
                        }

                        break;
                    }
                }

                if (!exists)
                {
                    children.Remove(child);
                }
            }

            #endregion update & remove not existing folders

            #region add new folders

            if (folders is not null)
            {
                foreach (var folder in folders)
                {
                    if (exists(folder.name, true)) continue;

                    var newFolder = new IKFolder(folder.uuid, folder.name, this.isChecked, this);

                    if (updateAll || isChecked)
                    {
                        newFolder.update(updateAll);
                    }

                    children.Add(newFolder);
                }
            }

            #endregion add new folders

            #endregion update folders

            #region update files

            #region remove not existing files

            foreach (var child in getFileChildren())
            {
                var remove = true;

                foreach (var doc in files)
                {
                    if (child.content.Equals(doc.name))
                    {
                        remove = false;
                        break;
                    }
                }

                if (remove)
                {
                    children.Remove(child);
                }
            }

            #endregion remove not existing files

            #region add new files

            if (files is not null)
            {
                foreach (var file in files)
                {
                    var existingFile = get(file.name, false);

                    if (existingFile is not null) continue;

                    children.Add(new IKFile(file.uuid, file.name, file.timestamp, file.version, isChecked, this));
                }
            }

            #endregion add new files

            #region update files

            if (files is not null)
            {
                foreach (var file in getFileChildren())
                {
                    var exists = false;
                    foreach (var doc in files)
                    {
                        if (doc.name.Equals(file.content))
                        {
                            file.timestamp = doc.timestamp;
                            file.version = doc.version;
                            file.pos = doc.uuid;

                            files.Remove(doc);

                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        file.timestamp = null;
                        file.version = null;
                    }
                }
            }

            #endregion update files

            #endregion update files

            reparseChecked();

            return true;
        }

        #endregion update
    }
}