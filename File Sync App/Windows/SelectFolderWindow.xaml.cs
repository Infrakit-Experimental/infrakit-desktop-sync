using File_Sync_App.Nodes.Folders;
using Library;
using System.Collections.ObjectModel;
using System.Windows;
using static Library.Utils;

namespace File_Sync_App.InputWindows
{
    /// <summary>
    /// Interaktionslogik für SelectFolderWindow.xaml
    /// </summary>
    public partial class SelectFolderWindow : Window
    {
        #region variables

        /// <summary>
        /// The list of folders to display in the tree view.
        /// </summary>
        public ObservableCollection<Folder> data { get; set; }

        /// <summary>
        /// The index of the selected folder in the tree view.
        /// </summary>
        public int selecedIndex { get; set; }

        /// <summary>
        /// The root folder of the tree view.
        /// </summary>
        public IKFolder root;

        #endregion variables

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectFolderWindow"/> class.
        /// </summary>
        /// <param name="root">The root folder of the tree view.</param>
        public SelectFolderWindow(IKFolder root)
        {
            // Set the resource dictionary.
            this.setRDict();

            // Initialize the window components.
            InitializeComponent();

            // Set the folderstructure and the root folder
            this.root = root;
            this.data = new()
            {
                root
            };
        }

        #region listeners

        /// <summary>
        /// Handles the SelectedItemChanged event of the tree view.
        /// </summary>
        /// <param name="sender">The tree view.</param>
        /// <param name="e">The event arguments.</param>
        private void tvFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.root = (IKFolder)e.NewValue;
        }

        #region controls

        /// <summary>
        /// Handles the Click event of the Ok button.
        /// </summary>
        /// <param name="sender">The Ok button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            var folder = (Folder)this.tvFolders.SelectedItem;
            int countFolders = folder.countFolders();

            var autoDownload = bool.Parse(Settings.get("autoDownloadDocs"));

            if(!autoDownload)
            {
                int max = int.Parse(Settings.getAttribute("autoDownloadDocs", "count"));

                autoDownload = countFolders <= max;
            }

            if (autoDownload)
            {
                this.root.addDocuments();
            }
            else
            {
                var languages = Utils.Language.getRDict();

                var result = MessageBox.Show(
                    languages["selectFolder.loadDocsQuestion.message"].ToString(),
                    languages["selectFolder.loadDocsQuestion.caption"].ToString(),
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question
                );

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        this.root.addDocuments();
                        break;

                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the Cancel button.
        /// </summary>
        /// <param name="sender">The Cancel button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion controls

        #endregion listeners
    }
}