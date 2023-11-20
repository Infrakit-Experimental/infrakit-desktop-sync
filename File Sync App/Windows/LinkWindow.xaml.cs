using File_Sync_App.InputWindows;
using File_Sync_App.Nodes;
using File_Sync_App.Nodes.Folders;
using Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace File_Sync_App
{
    /// <summary>
    /// Interaktionslogik für LinkWindow.xaml
    /// </summary>
    public partial class LinkWindow : Window
    {
        #region variables

        /// <summary>
        /// The link being created or edited.
        /// </summary>
        internal Link link;

        /// <summary>
        /// The original name of the link, if this is an edit operation.
        /// </summary>
        private string? oldName;

        /// <summary>
        /// A list of existing links, used to check for duplicate link names.
        /// </summary>
        private List<Link> existingLinks;

        /// <summary>
        /// The local data for the link.
        /// </summary>
        public ObservableCollection<LFolder> localData { get; set; }

        /// <summary>
        /// The Infrakit data for the link.
        /// </summary>
        public ObservableCollection<IKFolder> infrakitData { get; set; }

        #endregion variables

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkWindow"/> class.
        /// </summary>
        /// <param name="existingLinks">A list of existing links.</param>
        /// <param name="loadProjects">Whether to load the list of Infrakit projects.</param>
        public LinkWindow(List<Link> existingLinks, bool loadProjects = true)
        {
            // Set the resource dictionary.
            this.setRDict();

            // Create new lists for the local and Infrakit data.
            this.localData = new();
            this.infrakitData = new();

            // Store the existing links.
            this.existingLinks = existingLinks;

            // Initialize the window components.
            InitializeComponent();

            // If the loadProjects parameter is false, return.
            if (!loadProjects) return;

            // Load the list of Infrakit projects into the UI.
            this.loadInfrakitProjects();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkWindow"/> class for editing an existing link.
        /// </summary>
        /// <param name="existingLinks">A list of existing links.</param>
        /// <param name="link">The link to edit.</param>
        internal LinkWindow(List<Link> existingLinks, Link link) : this(existingLinks, false)
        {
            // Set the title of the window to indicate that this is an edit operation.
            this.Title = Utils.Language.getMessage("link.title.edit");

            // Set the link name text box to the value of the link's name.
            this.tbxLinkName.Text = link.name;

            // Store the original name of the link.
            this.oldName = link.name;

            // Set the active checkbox to the value of the link's active property.
            this.cbActive.IsChecked = link.active;

            // Set the upload and download checkboxes to the values of the link's syncUpload and syncDownload properties.
            this.cbUpload.IsChecked = link.syncUpload;
            this.cbDownload.IsChecked = link.syncDownload;

            // Set the local data.
            #region setup local data

            this.localData = new()
            {
                link.localData.clone()
            };

            this.tvLocalData.Items.Refresh();

            #endregion setup local data

            // Set the Infrakit data.
            #region setup infrakit data

            this.infrakitData = new()
            {
                link.infrakitData.clone()
            };

            // Set the Infrakit project display including the right buttons.
            #region setup project

            this.cmdProjects.Visibility = Visibility.Collapsed;
            this.tbProjects.Visibility = Visibility.Visible;

            this.btnInfrakitProject.Visibility = Visibility.Visible;

            this.tbProjects.Text = link.infrakitProject.name;
            this.tbProjects.Tag = link.infrakitProject.uuid;
            this.tbProjects.ToolTip = link.infrakitProject.id;

            #endregion setup project

            this.tvInfrakitData.Items.Refresh();

            #endregion setup infrakit data
        }

        #endregion constructors

        #region listeners

        /// <summary>
        /// Handles the Click event of the Update button.
        /// </summary>
        /// <param name="sender">The Update button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            #region update local

            if (this.localData is not null && this.localData.Count > 0)
            {
                var data = this.localData.First();
                if(!data.update(true))
                {
                    this.localData.Clear();
                }

                if ((this.cbUpload.IsChecked.HasValue && !this.cbUpload.IsChecked.Value) &&
                    (this.localData is not null && this.localData.Count > 0))
                {
                    this.localData.First().isChecked = null;
                }
            }
            this.tvLocalData.Items.Refresh();

            #endregion update local

            #region update infrakit

            if (this.infrakitData is not null && this.infrakitData.Count > 0)
            {
                var data = this.infrakitData.First();
                if(!data.update(true))
                {
                    this.infrakitData.Clear();
                }

                if ((this.cbDownload.IsChecked.HasValue && !this.cbDownload.IsChecked.Value) &&
                    (this.infrakitData is not null && this.infrakitData.Count > 0))
                {
                    this.infrakitData.First().isChecked = null;
                }

            }
            this.tvInfrakitData.Items.Refresh();

            #endregion update infrakit
        }

        #region local data

        /// <summary>
        /// Handles the Click event of the Local Data button.
        /// </summary>
        /// <param name="sender">The Local Data button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnLocalData_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fBD = new();

            var result = fBD.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK) return;

            try
            {
                var root = Path.GetFileName(fBD.SelectedPath);

                var folders = Directory.GetDirectories(fBD.SelectedPath, "*", SearchOption.AllDirectories);
                var files = Directory.GetFiles(fBD.SelectedPath, "*", SearchOption.AllDirectories);

                List<string> folderStructure = new();
                folderStructure.AddRange(folders);
                folderStructure.AddRange(files);

                this.localData.Clear();
                this.localData.Add(new LFolder(root, fBD.SelectedPath, folderStructure));

                this.tvLocalData.Items.Refresh();

                if (this.cbUpload.IsChecked.HasValue && !this.cbUpload.IsChecked.Value)
                {
                    this.localData.First().setAll(false);
                    this.localData.First().isChecked = null;
                    LinkWindow.expandAll(this.localData.First(), false);
                }

                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                var message = ex.Message.Split("'");
                var path = message[1];

                var languages = Utils.Language.getRDict();
                Utils.Log.write("link.getLocalFileError: " + path);
                MessageBox.Show(
                    languages["link.getLocalFileError.message"].ToString() + " (" + path + ")",
                    languages["link.getLocalFileError.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
            catch (Exception ex)
            {
                var languages = Utils.Language.getRDict();
                Utils.Log.write("link.getLocalFileError: " + ex.Message);
                MessageBox.Show(
                    languages["link.defaultError.message"].ToString(),
                    languages["link.getLocalFileError.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
        }

        /// <summary>
        /// Handles the Click event of the Local Data checkbox.
        /// </summary>
        /// <param name="sender">The Local Data checkbox.</param>
        /// <param name="e">The event arguments.</param>
        private void cbLocalData_Click(object sender, RoutedEventArgs e)
        {
            this.cbData_Click(this.localData[0], e.Source as System.Windows.Controls.CheckBox);
            this.tvLocalData.Items.Refresh();
        }

        #region open / close all

        /// <summary>
        /// Handles the Click event of the Open All Local button.
        /// </summary>
        /// <param name="sender">The Open All Local button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnOpenAllLocal_Click(object sender, RoutedEventArgs e)
        {
            LinkWindow.expandAll(this.localData.First(), true);
        }

        /// <summary>
        /// Handles the Click event of the Close All Local button.
        /// </summary>
        /// <param name="sender">The Close All Local button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnCloseAllLocal_Click(object sender, RoutedEventArgs e)
        {
            LinkWindow.expandAll(this.localData.First(), false);
        }

        #endregion open / close all

        #region checkboxes

        /// <summary>
        /// Handles the Checked event of the Upload checkbox.
        /// </summary>
        /// <param name="sender">The Upload checkbox.</param>
        /// <param name="e">The event arguments.</param>
        private void cbUpload_Checked(object sender, RoutedEventArgs e)
        {
            this.svLocal.IsEnabled = true;
            this.btnCloseAllLocal.IsEnabled = true;
            this.btnOpenAllLocal.IsEnabled = true;

            if (this.localData is null || this.localData.Count == 0) return;

            this.localData.First().setAll(false);
            LinkWindow.expandAll(this.localData.First(), true);
            this.tvLocalData.Items.Refresh();
        }

        /// <summary>
        /// Handles the Unchecked event of the Upload checkbox.
        /// </summary>
        /// <param name="sender">The Upload checkbox.</param>
        /// <param name="e">The event arguments.</param>
        private void cbUpload_Unchecked(object sender, RoutedEventArgs e)
        {
            this.svLocal.IsEnabled = false;
            this.btnCloseAllLocal.IsEnabled = false;
            this.btnOpenAllLocal.IsEnabled = false;

            if (this.localData is null || this.localData.Count == 0) return;

            this.localData.First().setAll(false);
            this.localData.First().isChecked = null;
            LinkWindow.expandAll(this.localData.First(), false);
            this.tvLocalData.Items.Refresh();
        }

        #endregion checkboxes

        #endregion local data

        #region Infrakit data

        #region projects

        /// <summary>
        /// Handles the Click event of the Infrakit Project button.
        /// </summary>
        /// <param name="sender">The Infrakit Project button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnInfrakitProject_Click(object sender, RoutedEventArgs e)
        {
            if (!this.loadInfrakitProjects()) return;

            this.tbProjectsTitle.Visibility    = Visibility.Visible;
            this.cmdProjects.Visibility        = Visibility.Visible;

            this.tbProjects.Visibility         = Visibility.Collapsed;
            this.btnInfrakitProject.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the Infrakit Projects combo box.
        /// </summary>
        /// <param name="sender">The Infrakit Projects combo box.</param>
        /// <param name="e">The event arguments.</param>
        private void cmdProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedProject = this.cmdProjects.SelectedValue as ComboBoxItem;

            this.infrakitData.Clear();
            this.tvInfrakitData.Items.Refresh();

            if (selectedProject is null)
            {
                this.btnInfrakitData.Visibility = Visibility.Collapsed;
                return;
            }

            this.btnInfrakitData.Visibility = Visibility.Visible;
        }

        #endregion projects

        /// <summary>
        /// Handles the Click event of the Infrakit Data button.
        /// </summary>
        /// <param name="sender">The Infrakit Data button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnInfrakitData_Click(object sender, RoutedEventArgs e)
        {
            var selectedProject = this.cmdProjects.SelectedItem as ComboBoxItem;

            var folderStructure = API.Project.getFolders((Guid)selectedProject.Tag, -1);

            this.infrakitData.Clear();

            if (folderStructure is null) return;

            foreach (var folder in folderStructure)
            {
                if (folder.depth != 0) continue;

                SelectFolderWindow sFW = new(new IKFolder(folder, folderStructure));

                bool? ok = sFW.ShowDialog();

                if (ok.HasValue && !ok.Value || !ok.HasValue) continue;

                this.infrakitData.Add(sFW.root);
            }

            this.tvInfrakitData.Items.Refresh();

            if (this.cbDownload.IsChecked.HasValue && !this.cbDownload.IsChecked.Value)
            {
                this.infrakitData.First().setAll(false);
                this.infrakitData.First().isChecked = null;
                LinkWindow.expandAll(this.infrakitData.First(), false);
            }
        }

        /// <summary>
        /// Handles the Click event of the Infrakit Data checkbox.
        /// </summary>
        /// <param name="sender">The Infrakit Data checkbox.</param>
        /// <param name="e">The event arguments.</param>
        private void cbInfrakitData_Click(object sender, RoutedEventArgs e)
        {
            this.cbData_Click(this.infrakitData[0], e.Source as System.Windows.Controls.CheckBox);
            this.tvInfrakitData.Items.Refresh();
        }

        #region open / close all

        /// <summary>
        /// Handles the Click event of the Open All Infrakit button.
        /// </summary>
        /// <param name="sender">The Open All Infrakit button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnOpenAllInfrakit_Click(object sender, RoutedEventArgs e)
        {
            LinkWindow.expandAll(this.infrakitData.First(), true);
        }

        /// <summary>
        /// Handles the Click event of the Close All Infrakit button.
        /// </summary>
        /// <param name="sender">The Close All Infrakit button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnCloseAllInfrakit_Click(object sender, RoutedEventArgs e)
        {
            LinkWindow.expandAll(this.infrakitData.First(), false);
        }

        #endregion open / close all

        #region checkboxes

        /// <summary>
        /// Handles the Checked event of the Download checkbox.
        /// </summary>
        /// <param name="sender">The Download checkbox.</param>
        /// <param name="e">The event arguments.</param>
        private void cbDownload_Checked(object sender, RoutedEventArgs e)
        {
            this.svInfrakit.IsEnabled = true;
            this.btnOpenAllInfrakit.IsEnabled = true;
            this.btnCloseAllInfrakit.IsEnabled = true;

            if (this.infrakitData is null || this.infrakitData.Count == 0) return;

            this.infrakitData.First().setAll(false);
            LinkWindow.expandAll(this.infrakitData.First(), true);
            this.tvInfrakitData.Items.Refresh();
        }

        /// <summary>
        /// Handles the Unchecked event of the Download checkbox.
        /// </summary>
        /// <param name="sender">The Download checkbox.</param>
        /// <param name="e">The event arguments.</param>
        private void cbDownload_Unchecked(object sender, RoutedEventArgs e)
        {
            this.svInfrakit.IsEnabled = false;
            this.btnOpenAllInfrakit.IsEnabled = false;
            this.btnCloseAllInfrakit.IsEnabled = false;

            if (this.infrakitData is null || this.infrakitData.Count == 0) return;

            this.infrakitData.First().setAll(false);
            this.infrakitData.First().isChecked = null;
            LinkWindow.expandAll(this.infrakitData.First(), false);
            this.tvInfrakitData.Items.Refresh();   
        }

        #endregion checkboxes

        #endregion Infrakit data

        #region controls

        /// <summary>
        /// Handles the Click event of the OK button.
        /// </summary>
        /// <param name="sender">The OK button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            #region check if input is valid

            bool syncUpload = false;
            if (this.cbUpload.IsChecked.HasValue)
            {
                syncUpload = this.cbUpload.IsChecked.Value;
            }

            bool syncDowload = false;
            if (this.cbDownload.IsChecked.HasValue)
            {
                syncDowload = this.cbDownload.IsChecked.Value;
            }

            bool lHasData = false;
            var lData = this.localData.FirstOrDefault();
            if (lData is not null)
            {
                if (syncUpload)
                {
                    lHasData = lData.hasCheckedFolders();
                }
                else
                {
                    lHasData = true;
                }
            }

            bool iHasData = false;
            var iData = this.infrakitData.FirstOrDefault();
            if (iData is not null)
            {
                if (syncUpload)
                {
                    iHasData = iData.hasCheckedFolders();
                }
                else
                {
                    iHasData = true;
                }
            }

            if (this.tbxLinkName.Text == "" || !lHasData || !iHasData || (!syncUpload && !syncDowload))
            {
                var languages = Utils.Language.getRDict();

                MessageBox.Show(
                    languages["link.okError.message"].ToString(),
                    languages["link.okError.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            if(this.oldName is null || !this.tbxLinkName.Text.Equals(this.oldName))
            {
                foreach (var eLink in this.existingLinks)
                {
                    if (eLink.name.Equals(this.tbxLinkName.Text))
                    {
                        var languages = Utils.Language.getRDict();
                        MessageBox.Show(
                            languages["link.nameExistingError.message"].ToString(),
                            languages["link.nameExistingError.caption"].ToString(),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                        return;
                    }
                }
            }

            #endregion check if input is valid

            #region create Link

            #region get Infrakit project information

            int id;
            Guid uuid;
            string name;

            if (this.cmdProjects.Visibility == Visibility.Collapsed)
            {
                var sProject = this.tbProjects;

                id = (int)sProject.ToolTip;
                uuid = (Guid)sProject.Tag;
                name = sProject.Text;
            }
            else
            {
                var sProject = this.cmdProjects.SelectedValue as ComboBoxItem;

                id = (int)sProject.ToolTip;
                uuid = (Guid)sProject.Tag;
                name = (string)sProject.Content;
            }

            var infrakitProject = new Library.Models.Project(id, uuid, name);

            #endregion get Infrakit project information

            var active = false;
            if(this.cbActive.IsChecked.HasValue)
            {
                active = this.cbActive.IsChecked.Value;
            }

            this.link = new
            (
                this.tbxLinkName.Text,
                this.localData.First(),
                this.infrakitData.First(),
                active,
                syncUpload,
                syncDowload,
                infrakitProject
            );

            #endregion create Link

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

        #region others

        /// <summary>
        /// Handles the Click event of the checkbox for the given data type.
        /// </summary>
        /// <param name="data">The data type.</param>
        /// <param name="checkBox">The checkbox.</param>
        private void cbData_Click(Folder data, System.Windows.Controls.CheckBox source)
        {
            if(source.DataContext.GetType().IsSubclassOf(typeof(Folder)))
            {
                ((Folder)source.DataContext).setChecked();
            }
            data.reparseChecked();

            var isChecked = source.IsChecked;

            if (!isChecked.HasValue)
            {
                var node = source.DataContext as Node;
                node.isChecked = true;
            }
        }

        /// <summary>
        /// Loads the list of Infrakit projects into the UI.
        /// </summary>
        /// <returns>True if the load was successful, false otherwise.</returns>
        private bool loadInfrakitProjects()
        {
            var projects = API.Project.get();

            this.infrakitData.Clear();
            this.tvInfrakitData.Items.Refresh();

            this.cmdProjects.Items.Clear();

            this.tbProjects.Text    = null;
            this.tbProjects.Tag     = null;
            this.tbProjects.ToolTip = null;

            if (projects is null)
            {
                this.btnInfrakitProject.Visibility = Visibility.Visible;

                this.tbProjectsTitle.Visibility    = Visibility.Collapsed;
                this.cmdProjects.Visibility        = Visibility.Collapsed;
                this.tbProjects.Visibility         = Visibility.Collapsed;

                this.cbDownload.IsChecked = false;

                return false;
            }

            foreach (var project in projects)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = project.name;
                cbi.Tag = project.uuid;
                cbi.ToolTip = project.id;

                this.cmdProjects.Items.Add(cbi);
            }

            return true;
        }

        /// <summary>
        /// Expands or collapses all tree view items.
        /// </summary>
        /// <param name="items">The tree view items to expand or collapse.</param>
        /// <param name="expand">Whether to expand or collapse the tree view items.</param>
        private static void expandAll(Node node, bool expand)
        {
            if (node is null) return;

            ItemHelper.SetIsExpanded(node, expand);

            if (!node.GetType().IsSubclassOf(typeof(Folder))) return;

            var children = ((Folder)node).children;

            if (children is null) return;

            foreach (Node f in children)
            {
                LinkWindow.expandAll(f, expand);
            }
        }

        #endregion others
    }
}