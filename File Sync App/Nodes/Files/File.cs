using File_Sync_App.Nodes.Folders;
using File_Sync_App.Windows;
using Library;
using System;
using System.Threading;
using System.Xml;

namespace File_Sync_App.Nodes.Files
{
    /// <summary>
    /// Represents a general file.
    /// </summary>
    public abstract class File : Node
    {
        #region variables

        /// <summary>
        /// The timestamp of the file.
        /// </summary>
        internal DateTime? timestamp;

        #endregion variables

        #region constructors

        /// <summary>
        /// Initializes a new instance of the File class.
        /// </summary>
        /// <param name="content">The content of the file.</param>
        /// <param name="timestamp">The timestamp of the file.</param>
        /// <param name="isChecked">Whether the file is checked.</param>
        /// <param name="parent">The parent folder of the file.</param>
        protected File(string content, DateTime? timestamp, bool? isChecked, Folder? parent) : base(content, isChecked, parent)
        {
            this.timestamp = timestamp;
        }

        #endregion constructors

        #region im-/export

        /// <summary>
        /// Initializes a new instance of the File class from an XML node.
        /// </summary>
        /// <param name="xmlNode">The XML node representing the file.</param>
        /// <param name="parent">The parent folder of the file.</param>
        protected File(XmlNode xmlNode, Folder parent) : base(xmlNode, parent) { }

        /// <summary>
        /// Gets an XML node representing the file.
        /// </summary>
        /// <param name="doc">The XML document to create the XML node in.</param>
        /// <returns>An XML node representing the file.</returns>
        protected XmlNode getXmlNode(XmlDocument doc)
        {
            return getXmlNode(doc, "File");
        }

        #endregion im-/export

        #region sync

        /// <summary>
        /// Syncs the file with the Infrakit server.
        /// </summary>
        /// <param name="local">The local file.</param>
        /// <param name="infrakit">The Infrakit file.</param>
        /// <returns>A Log.File object representing the sync status of the file.</returns>
        internal static Log.File sync(LFile local, IKFile infrakit)
        {
            #region check if both are to sync

            var ikToSync = infrakit.isToSync();
            var lToSync = local.isToSync();

            #region deleted

            if (ikToSync == Log.SyncStatus.Deleted && lToSync == Log.SyncStatus.Deleted)
            {
                return new Log.File(local.content, Log.SyncStatus.Error, Log.SyncStatus.Error);
            }

            if (ikToSync == Log.SyncStatus.Deleted)
            {
                return new Log.File(local.content, lToSync, Log.SyncStatus.Error);
            }

            if (lToSync == Log.SyncStatus.Deleted)
            {
                return new Log.File(local.content, Log.SyncStatus.Error, ikToSync);
            }

            #endregion deleted

            if (ikToSync != Log.SyncStatus.Synced && lToSync != Log.SyncStatus.Synced)
            {
                return new Log.File(local.content, lToSync, ikToSync);
            }

            if (lToSync != Log.SyncStatus.Synced)
            {
                return infrakit.sync(local);
            }

            if (ikToSync != Log.SyncStatus.Synced)
            {
                return local.sync(infrakit);
            }

            #endregion check if both are to sync

            #region chose file to sync

            var isChangedLocal = local.isChanged();
            var isChangedInfrakit = infrakit.isChanged();

            if (isChangedLocal && isChangedInfrakit)
            {
                if(Settings.defaultFileSync is null)
                {
                    Log.File log = new Log.File(local.content, Log.SyncStatus.Error, Log.SyncStatus.Error);
                    var thread = new Thread(() =>
                    {
                        var sFW = new SelectFileWindow(local, infrakit);
                        sFW.ShowDialog();
                        log = sFW.log;
                    });

                    thread.SetApartmentState(ApartmentState.STA);

                    thread.Start();
                    thread.Join();

                    return log;
                }

                switch(Settings.defaultFileSync)
                {
                    case Settings.FileSync.local:
                        Utils.Log.write("log.sync.bothFilesChangedQuestion.local: \"" + local.content + "\"");
                        return local.upload(infrakit);

                    case Settings.FileSync.infrakit:
                        Utils.Log.write("log.sync.bothFilesChangedQuestion.infrakit: \"" + infrakit.content + "\"");
                        return infrakit.download(local);

                    case Settings.FileSync.none:
                        Utils.Log.write("log.sync.bothFilesChangedQuestion.none: \"" + local.content + "\"");

                        local.lastChanged = local.timestamp;
                        infrakit.lastVersion = infrakit.version;

                        return new Log.File(local.content, Log.SyncStatus.NotSynced, Log.SyncStatus.NotSynced);
                }
            }

            if (isChangedLocal)
            {
                return local.upload(infrakit);
            }

            if (isChangedInfrakit)
            {
                return infrakit.download(local);
            }

            return new Log.File(local.content, Log.SyncStatus.NoChanges, Log.SyncStatus.NoChanges);

            #endregion chose file to sync
        }

        /// <summary>
        /// Checks if the file has changed.
        /// </summary>
        /// <returns>True if the file has changed, false otherwise.</returns>
        protected abstract bool isChanged();

        /// <summary>
        /// Checks if the file has been deleted.
        /// </summary>
        /// <returns>True if the file has been deleted, false otherwise.</returns>
        protected abstract bool isDeleted();

        #endregion sync
    }
}