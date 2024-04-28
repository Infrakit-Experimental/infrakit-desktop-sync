using File_Sync_App.Nodes.Files;
using Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace File_Sync_App.Nodes.Folders
{
    /// <summary>
    /// Represents a local folder.
    /// </summary>
    public class LFolder : Folder
    {
        #region variables

        /// <summary>
        /// The path to the folder on the local disk.
        /// </summary>
        public new string pos
        {
            get => base.pos;
            set => base.pos = value;
        }

        #endregion variables

        #region constructors

        /// <summary>
        /// Initializes a new instance of the LFolder class.
        /// </summary>
        /// <param name="name">The name of the folder.</param>
        /// <param name="rootPath">The root path of the folder structure.</param>
        /// <param name="structure">The folder structure.</param>
        /// <param name="parent">The parent folder, or null if this is the root folder.</param>
        public LFolder(string name, string rootPath, List<string> structure, Folder? parent = null) : base(name, false, parent, false)
        {
            pos = rootPath;

            #region get child folders

            var childrenList = new List<string>();

            foreach (var s in structure)
            {
                if (s.StartsWith(rootPath + "\\"))
                {
                    var element = s.Replace(rootPath + "\\", "");

                    if (!element.Contains("\\"))
                    {
                        childrenList.Add(element);
                    }
                }
            }

            #endregion get child folders

            foreach (var child in childrenList)
            {
                var path = rootPath + "\\" + child;

                if (child.Contains("."))
                {
                    DateTime timestamp = System.IO.File.GetLastWriteTimeUtc(path);

                    children.Add(new LFile(path, child, timestamp, false, this));
                }
                else
                {
                    children.Add(new LFolder(child, path, structure, this));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LFolder"/> class.
        /// </summary>
        /// <param name="pos">The string identifier of the folder.</param>
        /// <param name="content">The content of the folder.</param>
        /// <param name="isChecked">A boolean indicating whether the folder is checked.</param>
        /// <param name="parent">The parent folder of the current folder.</param>
        /// <param name="isDeleted">A boolean indicating whether the folder is deleted.</param>
        internal LFolder(string pos, string content, bool? isChecked, Folder? parent, bool isDeleted) : base(content, isChecked, parent, isDeleted)
        {
            this.pos = pos;
        }

        #endregion constructors

        #region im-/export

        /// <summary>
        /// Initializes a new instance of the LFolder class from an XML node.
        /// </summary>
        /// <param name="xmlNode">The XML node to initialize from.</param>
        /// <param name="parent">The parent folder, or null if this is the root folder.</param>
        internal LFolder(XmlNode xmlNode, Folder? parent = null) : base(xmlNode, parent)
        {
            #region pos

            if (parent is null)
            {
                if (xmlNode.Attributes is not null)
                {
                    var pathNode = xmlNode.Attributes["path"];
                    if (pathNode is not null)
                    {
                        pos = pathNode.Value;
                    }
                }
            }
            else
            {
                pos = Path.Combine(parent.pos, content);
            }

            #endregion pos

            #region children

            if (!xmlNode.HasChildNodes) return;

            foreach (var child in xmlNode.ChildNodes)
            {
                switch (((XmlNode)child).Name)
                {
                    case "Folder":
                        children.Add(new LFolder((XmlNode)child, this));
                        break;

                    case "File":
                        children.Add(new LFile((XmlNode)child, this));
                        break;
                }
            }

            #endregion children
        }

        /// <summary>
        /// Gets an XML node that represents the folder.
        /// </summary>
        /// <param name="doc">The XML document to create the node in.</param>
        /// <returns>An XML node that represents the folder.</returns>
        internal new XmlNode getXmlNode(XmlDocument doc)
        {
            var node = base.getXmlNode(doc);

            #region path

            if (parent is null)
            {
                var path = doc.CreateAttribute("path");
                path.Value = pos;
                node.Attributes.Append(path);
            }

            #endregion path

            return node;
        }

        #endregion im-/export

        /// <summary>
        /// Clones the folder and its child nodes.
        /// </summary>
        /// <param name="parent">The parent folder of the cloned folder, or null if the cloned folder is the root folder.</param>
        /// <returns>A clone of the folder and its child nodes.</returns>
        public LFolder clone(LFolder? parent = null)
        {
            var folder = new LFolder(this.pos, this.content, this.isChecked, parent, this.isDeleted);

            foreach (var child in children)
            {
                switch (child)
                {
                    case LFolder f:
                        folder.children.Add(f.clone(folder));
                        break;

                    case LFile f:
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
        public new LFolder? getRoot()
        {
            var root = base.getRoot();

            if (root == null) return null;

            return (LFolder)root;
        }

        /// <summary>
        /// Gets the list of child folders of the folder.
        /// </summary>
        /// <returns>A list of child folders of the folder.</returns>
        internal new List<LFolder> getFolderChildren()
        {
            List<LFolder> children = new();

            if (this.children is null) return children;

            foreach (var child in this.children)
            {
                if (child.GetType() != typeof(LFolder)) continue;

                children.Add((LFolder)child);
            }

            return children;
        }

        /// <summary>
        /// Gets the list of child files of the folder.
        /// </summary>
        /// <returns>A list of child files of the folder.</returns>
        internal new List<LFile> getFileChildren()
        {
            List<LFile> children = new();

            if (this.children is null) return children;

            foreach (var child in this.children)
            {
                if (child.GetType() != typeof(LFile)) continue;

                children.Add((LFile)child);
            }

            return children;
        }

        #endregion get

        #region sync

        /// <summary>
        /// Synchronizes the folder with Infrakit.
        /// </summary>
        /// <param name="infrakit">The Infrakit object to synchronize with.</param>
        /// <returns>A Log.Folder object that represents the synchronized folder.</returns>
        internal Log.Folder sync(IKFolder infrakit)
        {
            if (!this.update())
            {
                return new Log.Folder(content, Log.SyncStatus.Error, Log.SyncStatus.NotSynced);
            }

            var logRoot = new Log.Folder(content, Log.SyncStatus.Synced, Log.SyncStatus.NotSynced);

            if (children.Count == 0) return logRoot;

            #region folders

            foreach (var lFolder in this.getFolderChildren())
            {
                if (!lFolder.isToSync())
                {
                    logRoot.children.Add(new Log.Folder(lFolder.content, Log.SyncStatus.NotSynced, Log.SyncStatus.NotSynced));
                    continue;
                }

                var folder = infrakit.get(lFolder);

                #region delete

                if (lFolder.isDeleted)
                {
                    if (folder is null)
                    {
                        logRoot.children.Add(lFolder.remove());
                    }
                    else
                    {
                        logRoot.children.Add(lFolder.delete((IKFolder)folder));
                    }

                    continue;
                }

                #endregion delete

                #region create

                if (folder is null)
                {
                    logRoot.children.Add(lFolder.create(infrakit));
                    continue;
                }

                #endregion create

                logRoot.children.Add(lFolder.sync((IKFolder)folder));
            }

            #endregion folders

            #region files

            foreach (var lFile in this.getFileChildren())
            {
                var lToSync = lFile.isToSync();

                if (lToSync == Log.SyncStatus.NotSynced || lToSync == Log.SyncStatus.NoChanges)
                {
                    logRoot.children.Add(new Log.Folder(lFile.content, lToSync, Log.SyncStatus.NotSynced));
                    continue;
                }

                var file = infrakit.get(lFile);

                #region delete

                if (lToSync == Log.SyncStatus.Deleted)
                {
                    if (file is null)
                    {
                        logRoot.children.Add(lFile.remove());
                    }
                    else
                    {
                        logRoot.children.Add(lFile.delete((IKFile)file));
                    }

                    continue;
                }

                #endregion delete

                #region create

                if (file is null)
                {
                    logRoot.children.Add(lFile.create(infrakit));
                    continue;
                }

                #endregion create

                logRoot.children.Add(lFile.sync((IKFile)file));
            }

            #endregion files

            return logRoot;
        }

        /// <summary>
        /// Creates the folder in Infrakit.
        /// </summary>
        /// <param name="ikParent">The Infrakit parent folder to create the folder in.</param>
        /// <returns>A Log.Folder object that represents the created folder.</returns>
        internal Log.Folder create(IKFolder ikParent)
        {
            if (!this.update())
            {
                return new Log.Folder(content, Log.SyncStatus.Error, Log.SyncStatus.NotSynced);
            }

            #region create folder

            var target = API.Folder.post(ikParent.pos, this.content);

            if (!target.HasValue)
            {
                Utils.Log.write("sync.failed.local.folder.create: \"" + this.content + "\"");
                return new Log.Folder(content, Log.SyncStatus.Error, Log.SyncStatus.NotSynced);
            }

            Utils.Log.write("sync.successful.local.folder: \"" + this.content + "\"");

            #region new folder

            var isChecked = false;
            if (ikParent.isChecked.HasValue)
            {
                isChecked = ikParent.isChecked.Value;
            }

            var newIKFolder = new IKFolder(target.Value, this.content, isChecked, ikParent);
            ikParent.children.Add(newIKFolder);

            #endregion new folder

            #endregion create folder

            var log = new Log.Folder(content, Log.SyncStatus.Synced, Log.SyncStatus.Added);

            #region children

            foreach (var lFolder in getFolderChildren())
            {
                if (!lFolder.isToSync())
                {
                    log.children.Add(new Log.Folder(lFolder.content, Log.SyncStatus.NotSynced, Log.SyncStatus.NotExisting));
                    continue;
                }

                log.children.Add(lFolder.create(newIKFolder));
            }

            foreach (var lFile in getFileChildren())
            {
                var lToSync = lFile.isToSync();
                if (lToSync == Log.SyncStatus.NotSynced || lToSync == Log.SyncStatus.NoChanges)
                {
                    log.children.Add(new Log.File(lFile.content, lToSync, Log.SyncStatus.NotExisting));
                    continue;
                }

                if (lToSync == Log.SyncStatus.Deleted)
                {
                    Utils.Log.write("sync.failed.local.folder.create: \"" + content + "\"");
                    log.children.Add(new Log.File(lFile.content, Log.SyncStatus.Error, Log.SyncStatus.NotExisting));
                    continue;
                }

                log.children.Add(lFile.create(newIKFolder));
            }

            #endregion children

            return log;
        }

        /// <summary>
        /// Deletes the folder in Infrakit.
        /// </summary>
        /// <param name="ikTarget">The Infrakit folder to delete.</param>
        /// <returns>A Log.Folder object that represents the deleted folder.</returns>
        internal Log.Folder delete(IKFolder ikTarget)
        {
            this.parent.children.Remove(this);

            var logRoot = new Log.Folder(content, Log.SyncStatus.NotExisting, Log.SyncStatus.Removed);

            #region children

            foreach(var lfolder in this.getFolderChildren())
            {
                var folder = ikTarget.get(lfolder);

                if (folder is null)
                {
                    logRoot.children.Add(lfolder.remove());
                }
                else
                {
                    logRoot.children.Add(lfolder.delete((IKFolder)folder));
                }
            }

            foreach (var lfile in this.getFileChildren())
            {
                var file = ikTarget.get(lfile);

                if (file is null)
                {
                    logRoot.children.Add(lfile.remove());
                }
                else
                {
                    logRoot.children.Add(lfile.delete((IKFile)file));
                }
            }

            #endregion children

            #region delete folder
            
            if (MainWindow.deleteFoldersAndFiles)
            {
                var ikFolder = API.Folder.getMetadata(ikTarget.pos);
                if (!API.deleteFileFolder(ikFolder.project.id, ikFolder.id.Value))
                {
                    ikTarget.isChecked = false;
                    logRoot.statusLocal = Log.SyncStatus.Error;
                }
                else
                {
                    ikTarget.parent.children.Remove(ikTarget);
                    logRoot.statusLocal = Log.SyncStatus.Deleted;
                }
            }
            else
            {
                ikTarget.setAll(false);
            }

            #endregion delete folder

            return logRoot;
        }

        /// <summary>
        /// Gets the log for an not existing folder.
        /// </summary>
        /// <returns>A Log.Folder object that represents the removed folder.</returns>
        internal Log.Folder remove()
        {
            this.parent.children.Remove(this);

            var log = new Log.Folder(content, Log.SyncStatus.NotExisting, Log.SyncStatus.NotExisting);

            foreach (var child in this.children)
            {
                switch (child)
                {
                    case LFolder f:
                        log.children.Add(f.remove());
                        break;

                    case LFile f:
                        log.children.Add(f.remove());
                        break;
                }
            }

            children.Clear();

            return log;
        }

        #endregion sync

        #region update

        /// <summary>
        /// Updates the folder and its child nodes with the latest information from the local disk.
        /// </summary>
        /// <returns>True if the update was successful, false otherwise.</returns>
        internal bool update()
        {
            #region get folders & files

            if (!Directory.Exists(this.pos)) return false;

            var folders = Directory.GetDirectories(this.pos, "*", SearchOption.TopDirectoryOnly);
            var files = Directory.GetFiles(this.pos, "*", SearchOption.TopDirectoryOnly);

            #endregion get folders & files

            #region folders

            #region add new folders

            foreach (var path in folders)
            {
                var name = Path.GetFileName(path);
                if (exists(name, true)) continue;

                children.Add(new LFolder(path, name, this.isChecked, this, this.isDeleted));
            }

            #endregion add new folders

            #region update folders

            foreach (var folder in this.getFolderChildren())
            {
                if (!Directory.Exists(folder.pos))
                {
                    folder.isDeleted = true;
                }
            }

            #endregion update folders

            #endregion folders

            #region files

            #region add new files

            foreach (var path in files)
            {
                var name = Path.GetFileName(path);

                var file = this.get(name, false);

                if (file is not null) continue;

                DateTime timestamp = System.IO.File.GetLastWriteTimeUtc(path);
                children.Add(new LFile(path, name, timestamp, isChecked, this));
            }

            #endregion add new files

            #region update files

            foreach (var file in this.getFileChildren())
            {
                if (!System.IO.File.Exists(file.pos))
                {
                    file.timestamp = null;
                    continue;
                }

                file.timestamp = System.IO.File.GetLastWriteTimeUtc(file.pos);
            }

            #endregion update files

            #endregion files

            return true;
        }

        /// <summary>
        /// Updates the folder and its child nodes with the latest information from the local disk.
        /// </summary>
        /// <param name="updateAll">Whether to update all of the child nodes, even if they are unchecked.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        internal override bool update(bool updateAll)
        {
            #region get folders & files

            if (!Directory.Exists(this.pos)) return false;

            var folders = Directory.GetDirectories(this.pos, "*", SearchOption.TopDirectoryOnly);
            var files = Directory.GetFiles(this.pos, "*", SearchOption.TopDirectoryOnly);

            #endregion get folders & files

            var isChecked = false;
            if (this.isChecked.HasValue)
            {
                isChecked = this.isChecked.Value;
            }

            #region folders

            #region update & remove not existing folders

            foreach (var child in getFolderChildren())
            {
                if (!updateAll && child.isChecked.HasValue && !child.isChecked.Value) continue;

                if (!child.update(updateAll))
                {
                    children.Remove(child);
                }
            }

            #endregion update & remove not existing folders

            #region add new folders

            foreach (var path in folders)
            {
                var name = Path.GetFileName(path);
                if (exists(name, true)) continue;

                var newFolder = new LFolder(path, name, this.isChecked, this, this.isDeleted);

                if (updateAll || isChecked)
                {
                    newFolder.update(updateAll);
                }

                children.Add(newFolder);
            }

            #endregion add new folders

            #endregion folders

            #region files

            #region remove not existing files

            foreach (var child in getFileChildren())
            {
                if (!System.IO.File.Exists(child.pos))
                {
                    children.Remove(child);
                }
            }

            #endregion remove not existing files

            #region update & add new files

            foreach (var path in files)
            {
                var name = Path.GetFileName(path);

                DateTime timestamp = System.IO.File.GetLastWriteTimeUtc(path);

                var file = get(name, false);

                if (file is not null)
                {
                    file.timestamp = timestamp;
                    continue;
                }

                children.Add(new LFile(path, name, timestamp, isChecked, this));
            }

            #endregion update & add new files

            #endregion files

            reparseChecked();

            return true;
        }

        #endregion update
    }
}