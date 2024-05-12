using File_Sync_App.Nodes.Folders;
using Library;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml;

namespace File_Sync_App.Nodes.Files
{
    /// <summary>
    /// Represents a file on the Infrakit server.
    /// </summary>
    public class IKFile : File
    {
        #region variables

        /// <summary>
        /// The UUID of the file on the Infrakit server.
        /// </summary>
        public new Guid pos
        {
            get => base.pos;
            set => base.pos = value;
        }

        /// <summary>
        /// The last version of the file on the Infrakit server.
        /// </summary>
        internal int? lastVersion;

        /// <summary>
        /// The current version of the file on the Infrakit server.
        /// </summary>
        internal int? version;

        #endregion variables

        #region constructors

        /// <summary>
        /// Initializes a new instance of the IKFile class.
        /// </summary>
        /// <param name="pos">The UUID of the file on the Infrakit server.</param>
        /// <param name="content">The content of the file.</param>
        /// <param name="timestamp">The timestamp of the file.</param>
        /// <param name="version">The version of the file on the Infrakit server.</param>
        /// <param name="isChecked">Whether the file is checked.</param>
        /// <param name="parent">The parent folder of the file.</param>
        internal IKFile(Guid pos, string content, DateTime? timestamp, int? version, bool? isChecked, Folder parent, int? lastVersion = null) : base(content, timestamp, isChecked, parent)
        {
            this.pos = pos;
            this.version = version;

            this.lastVersion = lastVersion;
        }

        #endregion constructors

        #region im-/export

        /// <summary>
        /// Initializes a new instance of the IKFile class from an XML node.
        /// </summary>
        /// <param name="xmlNode">The XML node representing the file.</param>
        /// <param name="parent">The parent folder of the file.</param>
        internal IKFile(XmlNode xmlNode, Folder parent) : base(xmlNode, parent)
        {
            if (xmlNode.Attributes is null) return;

            #region last version

            var versionNode = xmlNode.Attributes["version"];
            if (versionNode is not null)
            {
                var versionString = versionNode.Value;

                lastVersion = null;
                if (!versionString.Equals("Null"))
                {
                    lastVersion = int.Parse(versionNode.Value);
                }
            }

            #endregion last version

            #region pos

            var uuidNode = xmlNode.Attributes["uuid"];
            if (uuidNode is not null)
            {
                pos = new Guid(uuidNode.Value);
            }

            #endregion pos
        }

        /// <summary>
        /// Gets an XML node representing the file.
        /// </summary>
        /// <param name="doc">The XML document to create the XML node in.</param>
        /// <returns>An XML node representing the file.</returns>
        internal new XmlNode getXmlNode(XmlDocument doc)
        {
            var node = base.getXmlNode(doc);

            #region last version

            var versionNode = doc.CreateAttribute("version");

            if (lastVersion.HasValue)
            {
                versionNode.Value = lastVersion.Value.ToString();
            }
            else
            {
                versionNode.Value = "Null";
            }

            node.Attributes.Append(versionNode);

            #endregion last version

            #region uuid

            var uuid = doc.CreateAttribute("uuid");
            uuid.Value = pos.ToString();
            node.Attributes.Append(uuid);

            #endregion uuid

            return node;
        }

        #endregion im-/export

        /// <summary>
        /// Clones the file.
        /// </summary>
        /// <param name="parent">The parent folder of the cloned file.</param>
        /// <returns>A clone of the file.</returns>
        public IKFile clone(IKFolder parent)
        {
            return new IKFile(this.pos, this.content, this.timestamp, this.version, this.isChecked, parent, this.lastVersion);
        }

        #region sync

        /// <summary>
        /// Syncs the file with the local file.
        /// </summary>
        /// <param name="local">The local file.</param>
        /// <returns>A Log.File object representing the sync status of the file.</returns>
        internal Log.File sync(LFile local)
        {
            if (this.isDeleted())
            {
                return this.delete(local);
            }

            return this.download(local);
        }

        /// <summary>
        /// Creates the file on the local machine.
        /// </summary>
        /// <param name="lParent">The parent folder of the file.</param>
        /// <returns>A Log.File object representing the sync status of the file.</returns>
        internal Log.File create(LFolder lParent)
        {
            var target = Path.Combine(lParent.pos, this.content);

            #region create

            var result = this.download(target, this.content);

            if (!result.HasValue || !result.Value.status)
            {
                return new Log.File(content, Log.SyncStatus.NotSynced, Log.SyncStatus.Error);
            }

            #endregion create

            #region new file

            var isChecked = false;
            if (lParent.isChecked.HasValue)
            {
                isChecked = lParent.isChecked.Value;
            }

            var lFile = new LFile(target, this.content, result.Value.timestamp, isChecked, lParent);
            lParent.children.Add(lFile);

            #endregion new file

            this.lastVersion = this.version;
            lFile.lastChanged = lFile.timestamp;

            return new Log.File(this.content, Log.SyncStatus.Added, Log.SyncStatus.Synced, result.Value.timestamp);
        }

        /// <summary>
        /// Deletes the file from the local machine.
        /// </summary>
        /// <param name="lTarget">The local file to delete.</param>
        /// <returns>A Log.File object representing the sync status of the file.</returns>
        internal Log.File delete(LFile lTarget)
        {
            this.parent.children.Remove(this);

            if (!Settings.deleteFoldersAndFiles)
            { 
                lTarget.isChecked = false;
                return new Log.File(content, Log.SyncStatus.Removed, Log.SyncStatus.NotExisting);
            }

            System.IO.File.Delete(lTarget.pos);

            if (System.IO.File.Exists(lTarget.pos))
            {
                lTarget.isChecked = false;
                return new Log.File(content, Log.SyncStatus.Removed, Log.SyncStatus.Error);
            }

            lTarget.parent.children.Remove(lTarget);

            return new Log.File(content, Log.SyncStatus.Deleted, Log.SyncStatus.NotExisting);
        }

        /// <summary>
        /// Gets the log for an not existing file.
        /// </summary>
        /// <returns>A Log.File object representing the sync status of the file.</returns>
        internal Log.File remove()
        {
            this.parent.children.Remove(this);

            return new Log.File(content, Log.SyncStatus.NotExisting, Log.SyncStatus.NotExisting);
        }

        #endregion sync

        #region download

        /// <summary>
        /// Downloads the file from the Infrakit server.
        /// </summary>
        /// <param name="local">The local file to download to.</param>
        /// <returns>A Log.File object representing the sync status of the file.</returns>
        internal Log.File download(LFile local)
        {
            var result = this.download(local.pos, local.content);

            if (!result.HasValue)
            {
                return new Log.File(this.content, Log.SyncStatus.NotSynced, Log.SyncStatus.Error);
            }

            this.lastVersion = this.version;

            if (!result.Value.status)
            {
                local.lastChanged = local.timestamp;
                return new Log.File(this.content, Log.SyncStatus.NoChanges, Log.SyncStatus.Synced);
            }

            local.lastChanged = result.Value.timestamp;

            return new Log.File(this.content, Log.SyncStatus.Changed, Log.SyncStatus.Synced, result.Value.timestamp);
        }

        /// <summary>
        /// Downloads the file from the Infrakit server.
        /// </summary>
        /// <param name="target">The local file to download to.</param>, string targetFileName
        /// <param name="targetFileName">The file name of the document to be downloaded.</param>
        /// <returns>
        /// A tuple containing
        /// a boolean indicating whether the download was successful and
        /// a DateTime? indicating the last write time of the downloaded file,
        /// or null if the download failed.
        /// </returns>
        private (bool status, DateTime? timestamp)? download(string? target, string? targetFileName)
        {
            if (target is null || targetFileName is null)
            {
                var languages = Utils.Language.getRDict();

                Utils.Log.write("sync.failed.infrakit.file.noTarget: \"" + this.content + "\"");
                Utils.AutoClosingMessageBox.Show(
                    languages["links.syncFailed.noTarget.message"].ToString(),
                    languages["links.syncFailed.infrakit.caption"].ToString(),
                    MessageBoxImage.Error,
                    API.maxErrorDisplayTime,
                    API.newErrorThread
                );

                return null;
            }

            var result = API.Document.download(this.pos, target, targetFileName);

            if (!result.HasValue || result.Value.status != API.Document.Status.Successful)
            {
                Utils.Log.write("sync.failed.infrakit.file.download: \"" + this.content + "\"");
            }

            if (!result.HasValue) return null;

            if (result.Value.status == API.Document.Status.InvalidFilename ||
                result.Value.status == API.Document.Status.InvalidPath)
            {
                this.isChecked = false;
                return null;
            }

            Utils.Log.write("sync.successful.infrakit.file: \"" + this.content + "\"");

            if (result.Value.status != API.Document.Status.Successful) return (false, null);

            return (true, System.IO.File.GetLastWriteTimeUtc(target));
        }

        #endregion download

        #region is?

        /// <summary>
        /// Checks if the file has changed.
        /// </summary>
        /// <returns>True if the file has changed, false otherwise.</returns>
        protected override bool isChanged()
        {
            if (this.lastVersion is null) return true;

            if (this.version is null) return true;

            if (this.version == this.lastVersion) return false;

            return true;
        }

        /// <summary>
        /// Checks if the file has been deleted.
        /// </summary>
        /// <returns>True if the file has been deleted, false otherwise.</returns>
        protected override bool isDeleted()
        {
            if (this.version is not null) return false;

            return true;
        }

        /// <summary>
        /// Checks if the file needs to be synced.
        /// </summary>
        /// <returns>A Log.SyncStatus object representing the sync status of the file.</returns>
        internal new Log.SyncStatus isToSync()
        {
            if (!base.isToSync()) return Log.SyncStatus.NotSynced;

            if (!this.isChanged()) return Log.SyncStatus.NoChanges;

            if (this.isDeleted()) return Log.SyncStatus.Deleted;

            return Log.SyncStatus.Synced;
        }

        #endregion is?
    }
}