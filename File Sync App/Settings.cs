using Library;
using System.Windows;

namespace File_Sync_App
{
    internal static class Settings
    {
        /// <summary>
        /// Variable which shows whether a file type error message has already been shown during the current sync cycle.
        /// </summary>
        internal static bool fileTypeErrorShown = false;

        /// <summary>
        /// A flag that indicates whether or not to delete folders and files when syncing.
        /// </summary>
        internal static bool deleteFoldersAndFiles = false;

        #region default file sync

        /// <summary>
        /// Represents the source of the newer version of a file
        /// </summary>
        internal enum FileSync
        {
            local,
            infrakit,
            none
        }

        /// <summary>
        /// A nullable field that stores the source of the newer version of a file.
        /// Or null if the User has to deside for every file.
        /// </summary>
        internal static FileSync? defaultFileSync = null;

        /// <summary>
        /// Gets the index of the default file to be synced.
        /// </summary>
        /// <param name="type">The file synchronization type</param>
        /// <returns>The index of the default file to be synced</returns>
        internal static int getDefaultFileSyncIdx(FileSync type)
        {
            if (!Settings.defaultFileSync.HasValue) return 0;

            switch (Settings.defaultFileSync.Value)
            {
                case Settings.FileSync.local:
                    return 1;

                case Settings.FileSync.infrakit:
                    return 2;

                case Settings.FileSync.none:
                    return 3;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Sets the default file to be synced by index.
        /// </summary>
        /// <param name="idx">The index to set the default</param>
        internal static void setDefaultFileSyncByIdx(int idx)
        {
            switch (idx)
            {
                case 3:
                    Settings.defaultFileSync = Settings.FileSync.none;
                    break;

                case 2:
                    Settings.defaultFileSync = Settings.FileSync.infrakit;
                    break;

                case 1:
                    Settings.defaultFileSync = Settings.FileSync.local;
                    break;

                case 0:
                default:
                    Settings.defaultFileSync = null;
                    break;
            }
        }

        #endregion default file sync

        /// <summary>
        /// Displays an loading error message.
        /// </summary>
        internal static void showLoadingError()
        {
            MessageBox.Show(
                LibraryUtils.getMessage("error.loading.message") + " -> " + Utils.Language.getMessage("settings.error.loading.message"),
                LibraryUtils.getMessage("error.loading.caption"),
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
}
