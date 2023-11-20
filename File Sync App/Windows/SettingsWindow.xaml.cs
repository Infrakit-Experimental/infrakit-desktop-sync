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
        private DateTime deletFilesTime;

        /// <summary>
        /// The user sets the delet option.
        /// </summary>
        private string deletFilesUser;

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

            var autoDownload = bool.Parse(Settings.get("autoDownloadDocs"));

            int count = int.Parse(Settings.getAttribute("autoDownloadDocs", "count"));
            this.slAutoDownloadDocs.Value = count;

            if(autoDownload)
            {
                this.slAutoDownloadDocs.IsEnabled = false;

                this.cbAutoDownloadDocs.IsChecked = true;
            }

            #endregion set auto download docs count

            // Set the value of the log storage duration sliders.
            #region set log storage

            this.slLogStorageDuration.Value = Int32.Parse(Settings.get("logStorageDuration"));

            this.slProtocolStorageDuration.Value = Int32.Parse(Settings.get("protocolStorageDuration"));

            #endregion set log storage

            // Set the delete files setting.
            #region set delet files

            if (MainWindow.deleteFoldersAndFiles)
            {
                this.cbDeleteFiles.IsChecked = true;

                this.deletFilesUser = Settings.getAttribute("deleteFoldersAndFiles", "user");

                var timestamp = Settings.getAttribute("deleteFoldersAndFiles", "timestamp");
                var result = DateTimeOffset.Parse(timestamp, CultureInfo.InvariantCulture);
                this.deletFilesTime = result.DateTime;

                this.tbDeleteFiles.Text = this.deletFilesUser + " (" + this.deletFilesTime.ToString("dd.MM.yyyy HH:mm:ss \"GMT\"zzz") + ")";
                this.tbDeleteFiles.ToolTip = this.tbDeleteFiles.Text;
            }

            #endregion set delet files
        }

        #region listeners

        /// <summary>
        /// Handles the SelectionChanged event of the cbLanguages ComboBox.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void cbLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.setRDict((this.cbLanguages.SelectedItem as ComboBoxItem).Name);
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
        /// Handles the Click event of the btnViewSyncLog Button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void btnViewSyncLog_Click(object sender, RoutedEventArgs e)
        {
            var syncProtocolWindow = new SyncProtocolWindow();

            syncProtocolWindow.ShowDialog();
        }

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

            this.tbDeleteFiles.Text = this.deletFilesUser + " (" + this.deletFilesTime.ToString("dd.MM.yyyy HH:mm:ss \"GMT\"zzz") + ")";
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
            Settings.setAttribute("autoDownloadDocs", "count", autoDownloadAnz.ToString());

            #endregion autoDownloadDocs

            Settings.set("logStorageDuration", ((int)this.slLogStorageDuration.Value).ToString());
            Settings.set("protocolStorageDuration", ((int)this.slProtocolStorageDuration.Value).ToString());

            Utils.Language.set((this.cbLanguages.SelectedItem as ComboBoxItem).Name);

            #region delete files

            MainWindow.deleteFoldersAndFiles = this.cbDeleteFiles.IsChecked.Value;
            Settings.set("deleteFoldersAndFiles", this.cbDeleteFiles.IsChecked.ToString());

            if(MainWindow.deleteFoldersAndFiles)
            {
                Settings.setAttribute("deleteFoldersAndFiles", "user", Utils.activeUser);
                Settings.addAttribute("deleteFoldersAndFiles", "timestamp", this.deletFilesTime.ToString("yyyy-MM-dd HH:mm:sszzz"));
            }

            #endregion delete files

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
    }
}