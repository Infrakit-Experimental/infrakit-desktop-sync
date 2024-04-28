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

            // Set the selected language in the ComboBox.
            #region set language

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

            #endregion set language

            // Set the auto download docs setting.
            #region set auto download docs count

            bool autoDownload = true;
            try
            {
                autoDownload = bool.Parse(Settings.get("autoDownloadDocs"));
            }
            catch (Exception ex)
            {
                Log.write("setting.errorLoading.autoDownloadDocs: " + ex.GetType() + " | " + ex.Message);

                Settings.@override("autoDownloadDocs", "True");

                var languages = Utils.Language.getRDict();
                MessageBox.Show(
                    languages["settings.errorLoading.message"].ToString(),
                    languages["settings.errorLoading.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            int count = (int)this.slAutoDownloadDocs.Maximum;

            
            if(!autoDownload)
            {
                try
                {
                    count = int.Parse(Settings.getAttribute("autoDownloadDocs", "count"));
                }
                catch (Exception ex)
                {
                    Log.write("setting.errorLoading.autoDownloadDocs.count: " + ex.GetType() + " | " + ex.Message);

                    Settings.@override("autoDownloadDocs", "True");
                    autoDownload = true;

                    var languages = Utils.Language.getRDict();
                    MessageBox.Show(
                        languages["settings.errorLoading.message"].ToString(),
                        languages["settings.errorLoading.caption"].ToString(),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }

            if (autoDownload)
            {
                this.slAutoDownloadDocs.IsEnabled = false;

                this.cbAutoDownloadDocs.IsChecked = true;
            }

            this.slAutoDownloadDocs.Value = count;

            #endregion set auto download docs count

            // Set the value of the log storage duration setting.
            #region set log storage

            var logDeletion = false;

            try
            {
                logDeletion = bool.Parse(Settings.get("logDeletion"));
            }
            catch (Exception ex)
            {
                Log.write("setting.errorLoading.logDeletion: " + ex.GetType() + " | " + ex.Message);

                Settings.@override("logDeletion", "False");

                var languages = Utils.Language.getRDict();
                MessageBox.Show(
                    languages["settings.errorLoading.message"].ToString(),
                    languages["settings.errorLoading.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            int logDuration = (int)this.slLogStorageDuration.Maximum;

            if (logDeletion)
            {
                try
                {
                    logDuration = int.Parse(Settings.getAttribute("logDeletion", "duration"));
                }
                catch (Exception ex)
                {
                    Log.write("setting.errorLoading.logDeletion.duration: " + ex.GetType() + " | " + ex.Message);

                    Settings.@override("logDeletion", "False");
                    logDeletion = false;

                    var languages = Utils.Language.getRDict();
                    MessageBox.Show(
                        languages["settings.errorLoading.message"].ToString(),
                        languages["settings.errorLoading.caption"].ToString(),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
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
                syncLogDeletion = bool.Parse(Settings.get("syncLogDeletion"));
            }
            catch (Exception ex)
            {
                Log.write("setting.errorLoading.syncLogDeletion: " + ex.GetType() + " | " + ex.Message);

                Settings.@override("syncLogDeletion", "False");

                var languages = Utils.Language.getRDict();
                MessageBox.Show(
                    languages["settings.errorLoading.message"].ToString(),
                    languages["settings.errorLoading.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            int syncLogDuration = (int)this.slSyncLogStorageDuration.Maximum;

            if(syncLogDeletion)
            {
                try
                {
                    syncLogDuration = int.Parse(Settings.getAttribute("syncLogDeletion", "duration"));
                }
                catch (Exception ex)
                {
                    Log.write("setting.errorLoading.syncLogDeletion.duration: " + ex.GetType() + " | " + ex.Message);

                    Settings.@override("syncLogDeletion", "False");
                    syncLogDeletion = false;

                    var languages = Utils.Language.getRDict();
                    MessageBox.Show(
                        languages["settings.errorLoading.message"].ToString(),
                        languages["settings.errorLoading.caption"].ToString(),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
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
            #region set delet files

            if (MainWindow.deleteFoldersAndFiles)
            {
                try
                {
                    this.cbDeleteFiles.IsChecked = true;

                    this.deletFilesUser = Settings.getAttribute("deleteFoldersAndFiles", "user");

                    var timestamp = Settings.getAttribute("deleteFoldersAndFiles", "timestamp");
                    var result = DateTimeOffset.Parse(timestamp, CultureInfo.InvariantCulture);
                    this.deletFilesTime = result.DateTime;

                    this.tbDeleteFiles.Text = this.deletFilesUser + " (" + this.deletFilesTime.Value.ToString("dd.MM.yyyy HH:mm:ss \"GMT\"zzz") + ")";
                    this.tbDeleteFiles.ToolTip = this.tbDeleteFiles.Text;
                }
                catch (Exception ex)
                {
                    Log.write("setting.errorLoading.deleteFoldersAndFiles.attributes: " + ex.GetType() + " | " + ex.Message);

                    Settings.@override("deleteFoldersAndFiles", "False");
                    MainWindow.deleteFoldersAndFiles = false;

                    this.deletFilesUser = null;
                    this.deletFilesTime = null;

                    var languages = Utils.Language.getRDict();
                    MessageBox.Show(
                        languages["settings.errorLoading.message"].ToString(),
                        languages["settings.errorLoading.caption"].ToString(),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }

            #endregion set delet files

            // Set the API environment options.
            this.setupEnvironments();
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

        //TODO: comment
        private void cbLogDeleteNever_Checked(object sender, RoutedEventArgs e)
        {
            this.slLogStorageDuration.IsEnabled = false;
        }

        //TODO: comment
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

        //TODO: comment
        private void cbSyncLogDeleteNever_Checked(object sender, RoutedEventArgs e)
        {
            this.slSyncLogStorageDuration.IsEnabled = false;
        }

        //TODO: comment
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

            Settings.set("autoDownloadDocs", autoDownloadDocs.ToString());

            if(autoDownloadDocs.HasValue && !autoDownloadDocs.Value)
            {
                Settings.setAttribute("autoDownloadDocs", "count", autoDownloadAnz.ToString());
            }

            #endregion autoDownloadDocs

            #region logDeletion

            var logDeletion = !this.cbLogDeleteNever.IsChecked;

            Settings.set("logDeletion", logDeletion.ToString());

            if(logDeletion.HasValue && logDeletion.Value)
            {
                var logDeletionDuration = (int)this.slLogStorageDuration.Value;
                Settings.setAttribute("logDeletion", "duration", logDeletionDuration.ToString());
            }

            #endregion logDeletion

            #region syncLogDeletion

            var syncLogDeletion = !this.cbSyncLogDeleteNever.IsChecked;

            Settings.set("syncLogDeletion", syncLogDeletion.ToString());

            if (syncLogDeletion.HasValue && syncLogDeletion.Value)
            {
                var syncLogDeletionDuration = (int)this.slSyncLogStorageDuration.Value;
                Settings.setAttribute("syncLogDeletion", "duration", syncLogDeletionDuration.ToString());
            }

            #endregion syncLogDeletion

            Utils.Language.set((this.cbLanguages.SelectedItem as ComboBoxItem).Name);

            #region delete files

            MainWindow.deleteFoldersAndFiles = this.cbDeleteFiles.IsChecked.Value;
            Settings.set("deleteFoldersAndFiles", this.cbDeleteFiles.IsChecked.ToString());

            if(MainWindow.deleteFoldersAndFiles)
            {
                Settings.setAttribute("deleteFoldersAndFiles", "user", this.deletFilesUser);
                Settings.addAttribute("deleteFoldersAndFiles", "timestamp", this.deletFilesTime.Value.ToString("yyyy-MM-dd HH:mm:sszzz"));
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

        // TODO: comment
        private void setupEnvironments(string? language = null)
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
        }
    }
}