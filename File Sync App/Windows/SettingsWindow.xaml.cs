using File_Sync_App.Windows;
using Library;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static Library.Utils;

namespace File_Sync_App.InputWindows
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region variables

        /// <summary>
        /// The time the delet option is / was set.
        /// </summary>
        private DateTime? deletFilesTime;

        /// <summary>
        /// The user how set the delet option.
        /// </summary>
        private string? deletFilesUser;

        #endregion variables

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsWindow"/> class.
        /// </summary>
        public SettingsWindow()
        {
            // Set the resource dictionary.
            this.setRDict();

            // Initialize the window components.
            InitializeComponent();

            // Variable to detect if an error occurred
            // and therefore an error message has to be shown
            var errorOccurred = false;

            // Set the selected language in the ComboBox.
            #region set language

            try
            {
                var prevLanguage = Utils.Language.get();

                var selected = false;
                foreach (var item in this.cbLanguages.Items)
                {
                    var i = item as ComboBoxItem;

                    if (i.Name.Equals(prevLanguage))
                    {
                        i.IsSelected = true;
                        selected = true;
                        break;
                    }
                }

                if (!selected)
                {
                    this.cbLanguages.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Log.write("settings.error.loading.language: " + ex.GetType() + " | " + ex.Message);

                Utils.Settings.@override("language", "en");
                this.cbLanguages.SelectedIndex = 0;

                errorOccurred = true;
            }

            #endregion set language

            // Set the auto download docs setting.
            #region set auto download docs count

            bool autoDownload = true;
            try
            {
                autoDownload = bool.Parse(Utils.Settings.get("autoDownloadDocs"));
            }
            catch (Exception ex)
            {
                Log.write("settings.error.loading.autoDownloadDocs: " + ex.GetType() + " | " + ex.Message);

                Utils.Settings.@override("autoDownloadDocs", "True");

                errorOccurred = true;
            }

            int count = (int)this.slAutoDownloadDocs.Maximum;

            
            if(!autoDownload)
            {
                try
                {
                    count = int.Parse(Utils.Settings.getAttribute("autoDownloadDocs", "count"));
                }
                catch (Exception ex)
                {
                    Log.write("settings.error.loading.autoDownloadDocs.count: " + ex.GetType() + " | " + ex.Message);

                    Utils.Settings.@override("autoDownloadDocs", "True");
                    autoDownload = true;

                    errorOccurred = true;
                }
            }

            if (autoDownload)
            {
                this.slAutoDownloadDocs.IsEnabled = false;

                this.cbAutoDownloadDocs.IsChecked = true;
            }

            this.slAutoDownloadDocs.Value = count;

            #endregion set auto download docs count

            //Set the default file sync setting.
            #region set default file sync

            this.cbDefaultFileSync.SelectedIndex = 0;
            if (Settings.defaultFileSync.HasValue)
            {
                switch(Settings.defaultFileSync.Value)
                {
                    case Settings.FileSync.local:
                        this.cbDefaultFileSync.SelectedIndex = 1;
                        break;

                    case Settings.FileSync.infrakit:
                        this.cbDefaultFileSync.SelectedIndex = 2;
                        break;

                    case Settings.FileSync.none:
                        this.cbDefaultFileSync.SelectedIndex = 3;
                        break;
                }
            }

            #endregion set default file sync

            // Set the value of the log storage duration setting.
            #region set log storage

            var logDeletion = false;

            try
            {
                logDeletion = bool.Parse(Utils.Settings.get("logDeletion"));
            }
            catch (Exception ex)
            {
                Log.write("settings.error.loading.logDeletion: " + ex.GetType() + " | " + ex.Message);

                Utils.Settings.@override("logDeletion", "False");

                errorOccurred = true;
            }

            int logDuration = (int)this.slLogStorageDuration.Maximum;

            if (logDeletion)
            {
                try
                {
                    logDuration = int.Parse(Utils.Settings.getAttribute("logDeletion", "duration"));
                }
                catch (Exception ex)
                {
                    Log.write("settings.error.loading.logDeletion.duration: " + ex.GetType() + " | " + ex.Message);

                    Utils.Settings.@override("logDeletion", "False");
                    logDeletion = false;

                    errorOccurred = true;
                }
            }

            if (!logDeletion)
            {
                this.slLogStorageDuration.IsEnabled = false;

                this.cbLogDeleteNever.IsChecked = true;
            }

            this.slLogStorageDuration.Value = logDuration;

            #endregion set log storage

            // Set the value of the sync log storage duration setting.
            #region set sync log storage

            var syncLogDeletion = false;

            try
            {
                syncLogDeletion = bool.Parse(Utils.Settings.get("syncLogDeletion"));
            }
            catch (Exception ex)
            {
                Log.write("settings.error.loading.syncLogDeletion: " + ex.GetType() + " | " + ex.Message);

                Utils.Settings.@override("syncLogDeletion", "False");

                errorOccurred = true;
            }

            int syncLogDuration = (int)this.slSyncLogStorageDuration.Maximum;

            if(syncLogDeletion)
            {
                try
                {
                    syncLogDuration = int.Parse(Utils.Settings.getAttribute("syncLogDeletion", "duration"));
                }
                catch (Exception ex)
                {
                    Log.write("settings.error.loading.syncLogDeletion.duration: " + ex.GetType() + " | " + ex.Message);

                    Utils.Settings.@override("syncLogDeletion", "False");
                    syncLogDeletion = false;

                    errorOccurred = true;
                }
            }

            if (!syncLogDeletion)
            {
                this.slSyncLogStorageDuration.IsEnabled = false;

                this.cbSyncLogDeleteNever.IsChecked = true;
            }

            this.slSyncLogStorageDuration.Value = syncLogDuration;

            #endregion set protocol storage

            // Set the delete files setting.
            #region set delete files

            if (Settings.deleteFoldersAndFiles)
            {
                try
                {
                    this.cbDeleteFiles.IsChecked = true;

                    this.deletFilesUser = Utils.Settings.getAttribute("deleteFoldersAndFiles", "user");

                    var timestamp = Utils.Settings.getAttribute("deleteFoldersAndFiles", "timestamp");
                    var result = DateTimeOffset.Parse(timestamp, CultureInfo.InvariantCulture);
                    this.deletFilesTime = result.DateTime;

                    this.tbDeleteFiles.Text = this.deletFilesUser + " (" + this.deletFilesTime.Value.ToString("dd.MM.yyyy HH:mm:ss \"GMT\"zzz") + ")";
                    this.tbDeleteFiles.ToolTip = this.tbDeleteFiles.Text;
                }
                catch (Exception ex)
                {
                    Log.write("settings.error.loading.deleteFoldersAndFiles.attributes: " + ex.GetType() + " | " + ex.Message);

                    Utils.Settings.@override("deleteFoldersAndFiles", "False");
                    Settings.deleteFoldersAndFiles = false;

                    this.deletFilesUser = null;
                    this.deletFilesTime = null;

                    errorOccurred = true;
                }
            }

            #endregion set delet files

            // Set the API environment options.
            if(!this.setupEnvironments())
            {
                errorOccurred = true;
            }

            if (errorOccurred)
            {
                Settings.showLoadingError();
            }
        }

        #region listeners

        /// <summary>
        /// Handles the SelectionChanged event of the cbLanguages ComboBox.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void cbLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var language = (this.cbLanguages.SelectedItem as ComboBoxItem).Name;
            this.setRDict(language);
            this.setupEnvironments(language);
        }

        #region cbAutoDownloadDocs

        /// <summary>
        /// Handles the Checked event of the cbAutoDownloadDocs CheckBox.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void cbAutoDownloadDocs_Checked(object sender, RoutedEventArgs e)
        {
            this.slAutoDownloadDocs.IsEnabled = false;
        }

        /// <summary>
        /// Handles the Unchecked event of the cbAutoDownloadDocs CheckBox.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void cbAutoDownloadDocs_Unchecked(object sender, RoutedEventArgs e)
        {
            this.slAutoDownloadDocs.IsEnabled = true;
        }

        #endregion cbAutoDownloadDocs

        #region delete logs

        /// <summary>
        /// Handles the Click event of the btnViewLog Button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void btnViewLog_Click(object sender, RoutedEventArgs e)
        {
            var logWindow = new LogWindow();

            logWindow.ShowDialog();
        }

        /// <summary>
        /// Event handler for when the "cbLogDeleteNever" checkbox is checked.
        /// Disabling the log storage duration slider.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void cbLogDeleteNever_Checked(object sender, RoutedEventArgs e)
        {
            this.slLogStorageDuration.IsEnabled = false;
        }

        /// <summary>
        /// Event handler for when the "cbLogDeleteNever" checkbox is unchecked.
        /// Enables the log storage duration slider.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void cbLogDeleteNever_Unchecked(object sender, RoutedEventArgs e)
        {
            this.slLogStorageDuration.IsEnabled = true;
        }

        #endregion delete logs

        #region delete sync logs

        /// <summary>
        /// Handles the Click event of the btnViewSyncLog Button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void btnViewSyncLog_Click(object sender, RoutedEventArgs e)
        {
            var syncProtocolWindow = new SyncProtocolWindow();

            syncProtocolWindow.ShowDialog();
        }

        /// <summary>
        /// Event handler for when the "cbSyncLogDeleteNever" checkbox is checked.
        /// Disabling the sync log storage duration slider.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void cbSyncLogDeleteNever_Checked(object sender, RoutedEventArgs e)
        {
            this.slSyncLogStorageDuration.IsEnabled = false;
        }

        /// <summary>
        /// Event handler for when the "cbSyncLogDeleteNever" checkbox is unchecked.
        /// Enables the sync log storage duration slider.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void cbSyncLogDeleteNever_Unchecked(object sender, RoutedEventArgs e)
        {
            this.slSyncLogStorageDuration.IsEnabled = true;
        }

        #endregion delete sync logs

        #region cbDeleteFiles

        /// <summary>
        /// Handles the Checked event of the cbDeleteFiles CheckBox.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void cbDeleteFiles_Checked(object sender, RoutedEventArgs e)
        {
            this.deletFilesTime = DateTime.Now;
            this.deletFilesUser = Utils.activeUser;

            this.tbDeleteFiles.Text = this.deletFilesUser + " (" + this.deletFilesTime.Value.ToString("dd.MM.yyyy HH:mm:ss \"GMT\"zzz") + ")";
            this.tbDeleteFiles.ToolTip = this.tbDeleteFiles.Text;

            this.tbDeleteFiles.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the Unchecked event of the cbDeleteFiles CheckBox.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void cbDeleteFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            this.tbDeleteFiles.Visibility = Visibility.Collapsed;
            this.deletFilesUser = null;
            this.deletFilesTime = null;
        }

        #endregion cbDeleteFiles

        /// <summary>
        /// Handles the ScrollChanged event of the svSettings ScrollViewer.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void svSettings_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.svSettings.ScrollableHeight > 0)
            {
                if (this.gdSettings.Margin != new Thickness(0, 0, 10, 0))
                {
                    this.gdSettings.Margin = new Thickness(0, 0, 10, 0);
                }
            }
            else if (this.gdSettings.Margin != new Thickness(0))
            {
                this.gdSettings.Margin = new Thickness(0);
            }
        }

        #region controls

        /// <summary>
        /// Handles the Click event of the btnOk Button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            #region autoDownloadDocs

            var autoDownloadDocs = this.cbAutoDownloadDocs.IsChecked;
            var autoDownloadAnz = (int)this.slAutoDownloadDocs.Value;

            Utils.Settings.set("autoDownloadDocs", autoDownloadDocs.ToString());

            if(autoDownloadDocs.HasValue && !autoDownloadDocs.Value)
            {
                Utils.Settings.setAttribute("autoDownloadDocs", "count", autoDownloadAnz.ToString());
            }

            #endregion autoDownloadDocs

            #region select file by default

            var defaultFileSyncIdx = this.cbDefaultFileSync.SelectedIndex;

            Settings.setDefaultFileSyncByIdx(defaultFileSyncIdx);

            #endregion select file by default

            #region logDeletion

            var logDeletion = !this.cbLogDeleteNever.IsChecked;

            Utils.Settings.set("logDeletion", logDeletion.ToString());

            if(logDeletion.HasValue && logDeletion.Value)
            {
                var logDeletionDuration = (int)this.slLogStorageDuration.Value;
                Utils.Settings.setAttribute("logDeletion", "duration", logDeletionDuration.ToString());
            }

            #endregion logDeletion

            #region syncLogDeletion

            var syncLogDeletion = !this.cbSyncLogDeleteNever.IsChecked;

            Utils.Settings.set("syncLogDeletion", syncLogDeletion.ToString());

            if (syncLogDeletion.HasValue && syncLogDeletion.Value)
            {
                var syncLogDeletionDuration = (int)this.slSyncLogStorageDuration.Value;
                Utils.Settings.setAttribute("syncLogDeletion", "duration", syncLogDeletionDuration.ToString());
            }

            #endregion syncLogDeletion

            #region language

            var newlanguage = (this.cbLanguages.SelectedItem as ComboBoxItem).Name;

            Utils.Language.set(newlanguage);

            #endregion language

            #region delete files

            Settings.deleteFoldersAndFiles = this.cbDeleteFiles.IsChecked.Value;
            Utils.Settings.set("deleteFoldersAndFiles", this.cbDeleteFiles.IsChecked.ToString());

            if(Settings.deleteFoldersAndFiles)
            {
                Utils.Settings.setAttribute("deleteFoldersAndFiles", "user", this.deletFilesUser);
                Utils.Settings.addAttribute("deleteFoldersAndFiles", "timestamp", this.deletFilesTime.Value.ToString("yyyy-MM-dd HH:mm:sszzz"));
            }

            #endregion delete files

            #region change environment

            var env = (API.Environment)(this.cbEnvironment.SelectedItem as ComboBoxItem).Tag;
            
            if(API.changeEnvironment(env))
            {
                if (!Utils.logIn()) { return; }
            }

            #endregion change environment

            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel Button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion controls

        #endregion listeners

        /// <summary>
        /// Sets up the environments in the ComboBox based on the specified language.
        /// If no language is provided, the default language is used.
        /// </summary>
        /// <param name="language">
        /// The language to be used for setting up the environments.
        /// If not provided, the default language is used.
        /// </param>
        /// <returns>True if the environments are successfully set up; otherwise, false.</returns>
        private bool setupEnvironments(string? language = null)
        {
            try
            {
                this.cbEnvironment.Items.Clear();
                foreach (string env in Enum.GetNames(typeof(API.Environment)))
                {
                    var cbi = new ComboBoxItem();
                    cbi.Tag = (API.Environment)Enum.Parse(typeof(API.Environment), env);

                    cbi.Content = LibraryUtils.getMessage("settings.env." + env.ToLower(), language);

                    if (string.Equals(env, API.selectedEnv.ToString()))
                    {
                        cbi.IsSelected = true;
                    }

                    this.cbEnvironment.Items.Add(cbi);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.write("settings.error.Loading.environments: " + ex.GetType() + " | " + ex.Message);
                this.cbEnvironment.Items.Clear();

                this.cbEnvironment.IsEnabled = false;
                return false;
            }
        }
    }
}