using File_Sync_App.Nodes;
using File_Sync_App.Nodes.Files;
using Library;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace File_Sync_App.Windows
{
    /// <summary>
    /// Interaktionslogik für SelectFileWindow.xaml
    /// </summary>
    public partial class SelectFileWindow : Window
    {
        #region variables

        /// <summary>
        /// The local file.
        /// </summary>
        private LFile local;

        /// <summary>
        /// The Infrakit file.
        /// </summary>
        private IKFile infrakit;

        /// <summary>
        /// A log entry for the selection.
        /// </summary>
        internal Log.File log;

        #endregion variables

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectFileWindow"/> class.
        /// </summary>
        /// <param name="local">The local file.</param>
        /// <param name="infrakit">The Infrakit file.</param>
        internal SelectFileWindow(LFile local, IKFile infrakit)
        {
            // Set the resource dictionary.
            this.setRDict();

            // Initialize the window components.
            InitializeComponent();

            // Set up the button to display the path of the file
            this.btnFile.Content = local.content;

            // Compare the timestamps of the local and Infrakit files and set displayed text.
            #region newer file

            // Compare the timestamps of the local and Infrakit files.
            var comparison = 0;
            if(local.timestamp.HasValue && infrakit.timestamp.HasValue)
            {
                comparison = DateTime.Compare(local.timestamp.Value, infrakit.timestamp.Value);
            }
            else if(local.timestamp.HasValue)
            {
                comparison = 1;
            }
            else if (infrakit.timestamp.HasValue)
            {
                comparison = -1;
            }

            // Set the text of the "newer file" text box based on the comparison result.
            switch(comparison)
            {
                case 1:
                    this.tbNewerFile.Text = "(" + Utils.Language.getMessage("selectFile.local") + ")";
                    break;

                case -1:
                    this.tbNewerFile.Text = "(" + Utils.Language.getMessage("selectFile.infrakit") + ")";
                    break;

                default: // 0
                    this.tbNewerFile.Text = "(" + Utils.Language.getMessage("selectFile.none") + ")";
                    break;
            }

            #endregion newer file

            // Store the local and Infrakit files for future use.
            this.local = local;
            this.infrakit = infrakit;

            // Initialize a log entry for the local file.
            this.log = new Log.File(local.content, Log.SyncStatus.Error, Log.SyncStatus.Error);
        }

        #region listener

        /// <summary>
        /// Displays the path of the file in a message box.
        /// </summary>
        /// <param name="sender">The button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                this.local.pos,
                Utils.Language.getMessage("selectFile.path"),
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        /// <summary>
        /// Syncs the local file to Infrakit.
        /// </summary>
        /// <param name="sender">The button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnLocalFile_Click(object sender, RoutedEventArgs e)
        {
            Utils.Log.write("log.sync.bothFilesChangedQuestion.local: \"" + this.local.content + "\"");
            this.log = this.local.upload(this.infrakit);
            
            if(this.cbRemember.IsChecked.HasValue && this.cbRemember.IsChecked.Value)
            {
                Link.newerFileSync = Link.FileSync.local;
            }

            this.Close();
        }

        /// <summary>
        /// Syncs the Infrakit file to the local machine.
        /// </summary>
        /// <param name="sender">The button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnInfrakitFile_Click(object sender, RoutedEventArgs e)
        {
            Utils.Log.write("log.sync.bothFilesChangedQuestion.infrakit: \"" + this.infrakit.content + "\"");
            this.log = this.infrakit.download(this.local);

            if (this.cbRemember.IsChecked.HasValue && this.cbRemember.IsChecked.Value)
            {
                Link.newerFileSync = Link.FileSync.infrakit;
            }

            this.Close();
        }

        /// <summary>
        /// Does not sync any file.
        /// </summary>
        /// <param name="sender">The button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnNone_Click(object sender, RoutedEventArgs e)
        {
            Utils.Log.write("log.sync.bothFilesChangedQuestion.none: \"" + this.local.content + "\"");
            this.log = new Log.File(this.local.content, Log.SyncStatus.NotSynced, Log.SyncStatus.NotSynced);

            this.local.lastChanged = this.local.timestamp;
            this.infrakit.lastVersion = this.infrakit.version;

            if (this.cbRemember.IsChecked.HasValue && this.cbRemember.IsChecked.Value)
            {
                Link.newerFileSync = Link.FileSync.none;
            }
            this.Close();
        }

        /// <summary>
        /// Handles the KeyDown event of the Grid. If the Enter key is pressed, it programmatically triggers the Click event of the 'btnNone' button.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event data.</param>
        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.btnNone.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        #endregion listener

    }
}