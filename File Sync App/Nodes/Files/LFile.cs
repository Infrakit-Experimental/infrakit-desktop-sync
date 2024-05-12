using File_Sync_App.Nodes.Folders;
using Library;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;

namespace File_Sync_App.Nodes.Files
{
    /// <summary>
    /// Represents a local file.
    /// </summary>
    public class LFile : File
    {
        #region variables

        /// <summary>
        /// The path of the file on the local machine.
        /// </summary>
        public new string pos
        {
            get => base.pos;
            set => base.pos = value;
        }

        /// <summary>
        /// The last time the file was changed on the local machine.
        /// </summary>
        internal DateTime? lastChanged;

        #endregion variables

        #region constructors

        /// <summary>
        /// Initializes a new instance of the LFile class.
        /// </summary>
        /// <param name="pos">The path of the file on the local machine.</param>
        /// <param name="content">The content of the file.</param>
        /// <param name="timestamp">The timestamp of the file.</param>
        /// <param name="isChecked">Whether the file is checked.</param>
        /// <param name="parent">The parent folder of the file.</param>
        internal LFile(string pos, string content, DateTime? timestamp, bool? isChecked, Folder parent, DateTime? lastChanged = null) : base(content, timestamp, isChecked, parent)
        {
            this.pos = pos;

            this.lastChanged = lastChanged;
        }

        #endregion constructors

        #region im-/export

        /// <summary>
        /// Initializes a new instance of the LFile class from an XML node.
        /// </summary>
        /// <param name="xmlNode">The XML node representing the file.</param>
        /// <param name="parent">The parent folder of the file.</param>
        internal LFile(XmlNode xmlNode, Folder parent) : base(xmlNode, parent)
        {
            if (xmlNode.Attributes is null) return;

            #region last changed

            var lastChangedNode = xmlNode.Attributes["lastChanged"];
            if (lastChangedNode is not null)
            {
                var lastChangedString = lastChangedNode.Value;

                if (!lastChangedString.Equals("Null"))
                {
                    DateTime parsedDate;
                    if (DateTime.TryParseExact(lastChangedString, "dd.MM.yyyy HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        this.lastChanged = parsedDate;
                    }
                }
            }

            #endregion last changed

            #region pos

            this.pos = System.IO.Path.Combine(parent.pos, content);

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

            #region last changed

            var lastChangedNode = doc.CreateAttribute("lastChanged");

            if (this.lastChanged.HasValue)
            {
                lastChangedNode.Value = this.lastChanged.Value.ToString("dd.MM.yyyy HH:mm:ss.FFFFFFF");
            }
            else
            {
                lastChangedNode.Value = "Null";
            }

            node.Attributes.Append(lastChangedNode);

            #endregion last changed

            return node;
        }

        #endregion im-/export

        /// <summary>
        /// Clones the file.
        /// </summary>
        /// <param name="parent">The parent folder of the cloned file.</param>
        /// <returns>A clone of the file.</returns>
        public LFile clone(LFolder parent)
        {
            return new LFile(this.pos, this.content, this.timestamp, this.isChecked, parent, this.lastChanged);
        }

        #region sync

        /// <summary>
        /// Syncs the file with the Infrakit server.
        /// </summary>
        /// <param name="infrakit">The Infrakit file.</param>
        /// <returns>A Log.File object representing the sync status of the file.</returns>
        internal Log.File sync(IKFile infrakit)
        {
            if (this.isDeleted())
            {
                return this.delete(infrakit);
            }

            return this.upload(infrakit);
        }

        /// <summary>
        /// Creates a new file on the Infrakit server.
        /// </summary>
        /// <param name="ikParent">The Infrakit folder to create the file in.</param>
        /// <returns>A Log.File object representing the sync status of the file.</returns>
        internal Log.File create(IKFolder ikParent)
        {
            #region create

            var result = this.upload(ikParent.pos);

            if (!result.HasValue || !result.Value.status)
            {
                return new Log.File(content, Log.SyncStatus.Error, Log.SyncStatus.NotSynced);
            }

            #endregion create

            #region new file

            var isChecked = false;
            if (ikParent.isChecked.HasValue)
            {
                isChecked = ikParent.isChecked.Value;
            }

            var doc = result.Value.doc;

            var ikFile = new IKFile(doc.uuid, doc.name, doc.timestamp, doc.version, isChecked, ikParent);
            ikParent.children.Add(ikFile);

            #endregion new file

            this.lastChanged = this.timestamp;
            ikFile.lastVersion = ikFile.version;

            return new Log.File(this.content, Log.SyncStatus.Synced, Log.SyncStatus.Added, doc.timestamp);
        }

        /// <summary>
        /// Deletes the file from the Infrakit server.
        /// </summary>
        /// <param name="ikTarget">The Infrakit file to delete.</param>
        /// <returns>A Log.File object representing the sync status of the file.</returns>
        internal Log.File delete(IKFile ikTarget)
        {
            this.parent.children.Remove(this);

            if (!Settings.deleteFoldersAndFiles)
            {
                ikTarget.isChecked = false;
                return new Log.File(content, Log.SyncStatus.NotExisting, Log.SyncStatus.Removed);
            }

            if (!API.Document.delete(ikTarget.pos))
            {
                ikTarget.isChecked = false;
                return new Log.File(content, Log.SyncStatus.Error, Log.SyncStatus.Removed);
            }

            ikTarget.parent.children.Remove(ikTarget);

            return new Log.File(content, Log.SyncStatus.NotExisting, Log.SyncStatus.Deleted);
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

        #region upload

        /// <summary>
        /// Uploads the file to the Infrakit server.
        /// </summary>
        /// <param name="infrakit">The Infrakit file to upload to.</param>
        /// <returns>A Log.File object representing the sync status of the file.</returns>
        internal Log.File upload(IKFile infrakit)
        {
            var result = this.upload(((IKFolder)infrakit.parent).pos);

            if (!result.HasValue)
            {
                return new Log.File(this.content, Log.SyncStatus.Error, Log.SyncStatus.NotSynced);
            }

            this.lastChanged = this.timestamp;

            if (!result.Value.status)
            {
                infrakit.lastVersion = infrakit.version;
                return new Log.File(this.content, Log.SyncStatus.Synced, Log.SyncStatus.NoChanges);
            }

            infrakit.lastVersion = result.Value.doc.version;

            return new Log.File(this.content, Log.SyncStatus.Synced, Log.SyncStatus.Changed, result.Value.doc.timestamp);
        }

        /// <summary>
        /// Uploads the file to the Infrakit server.
        /// </summary>
        /// <param name="target">The Infrakit folder to upload the file to.</param>
        /// <returns>
        /// A tuple containing
        /// a boolean indicating whether the upload was successful and
        /// a Library.Models.Document instance representing the uploaded document,
        /// or null if the upload failed.
        /// </returns>
        private (bool status, Library.Models.Document? doc)? upload(Guid? target)
        {
            if (!target.HasValue)
            {
                var languages = Utils.Language.getRDict();

                Utils.Log.write("sync.failed.local.file.noTarget: \"" + this.content + "\"");
                Utils.AutoClosingMessageBox.Show(
                    languages["links.syncFailed.noTarget.message"].ToString(),
                    languages["links.syncFailed.local.caption"].ToString(),
                    MessageBoxImage.Error,
                    API.maxErrorDisplayTime,
                    API.newErrorThread
                );

                return null;
            }

            #region check for forbidden file extensions

            string fileExtension = Path.GetExtension(this.pos);

            if (Utils.forbiddenFileExtensions.Contains(fileExtension))
            {
                if (!Settings.fileTypeErrorShown)
                {
                    var languages = Utils.Language.getRDict();
                    MessageBox.Show(
                        Utils.Language.getMessage("file.error.forbiddenType.message"),
                        LibraryUtils.getMessage("api.document.getUploudURL"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    Settings.fileTypeErrorShown = true;
                }

                Utils.Log.write("sync.failed.local.file.forbiddenType: \"" + this.content + "\"");
                this.isChecked = false;
                return null;
            }

            #endregion check for forbidden file extensions

            var result = API.Document.upload(this.pos, target.Value);

            if (!result.HasValue)
            {
                Utils.Log.write("sync.failed.local.file.upload: \"" + this.content + "\"");
                return null;
            }

            if (result.Value.status == API.Document.Status.InvalidExtension)
            {
                Utils.Log.write("sync.failed.local.file.forbiddenType: \"" + this.content + "\"");
                this.isChecked = false;
                return null;
            }

            if (result.Value.status != API.Document.Status.Successful)
            {
                Utils.Log.write("sync.failed.local.file.upload: \"" + this.content + "\"");
                return null;
            }

            Utils.Log.write("sync.successful.local.file: \"" + this.content + "\"");

            if (result.Value.status != API.Document.Status.Successful) return (false, null);

            return (true, result.Value.doc);
        }

        #endregion upload

        #region is?

        /// <summary>
        /// Checks if the file has changed.
        /// </summary>
        /// <returns>True if the file has changed, false otherwise.</returns>
        protected override bool isChanged()
        {
            if (this.lastChanged is null) return true;

            if (!this.timestamp.HasValue) return true;

            if (this.timestamp.Value.CompareTo(this.lastChanged) <= 0) return false;

            return true;
        }

        /// <summary>
        /// Checks if the file has been deleted.
        /// </summary>
        /// <returns>True if the file has been deleted, false otherwise.</returns>
        protected override bool isDeleted()
        {
            if (this.timestamp.HasValue) return false;

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