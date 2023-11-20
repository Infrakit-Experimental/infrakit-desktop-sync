using File_Sync_App.Nodes;
using Library;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace File_Sync_App.Windows
{
    /// <summary>
    /// Interaktionslogik für SyncProtocolWindow.xaml
    /// </summary>
    public partial class SyncProtocolWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProtocolWindow"/> class.
        /// </summary>
        public SyncProtocolWindow()
        {
            // Set the resource dictionary.
            this.setRDict();

            // Initialize the window components.
            InitializeComponent();

            // Load existing logs.
            #region existing logs

            // Get a list of all the log files in the log directory.
            string[] files = Directory.EnumerateFiles(Utils.Log.directory, "*.*")
                 .Select(p => Path.GetFileName(p))
                 .Where(s => s.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                 .Select(e => Path.GetFileNameWithoutExtension(e))
                 .ToArray();

            // If there are no log files, show a message to the user and return.
            if (files.Length == 0)
            {
                var languages = Utils.Language.getRDict();

                MessageBox.Show(
                    languages["logs.noLogs.message"].ToString(),
                    languages["logs.protocols.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            // Set the available dates.
            #region date picker

            DateOnly firstDate;
            DateOnly dateCounter;

            var first = true;

            foreach (var file in files)
            {
                var date = DateOnly.ParseExact(file, "yyyyMMdd");
                
                if(first)
                {
                    first = false;

                    firstDate = date;
                    dateCounter = date;
                    continue;
                }

                if (date.AddDays(-1) != dateCounter)
                {
                    this.dpProtocols.BlackoutDates.Add(
                        new CalendarDateRange(dateCounter.AddDays(1).ToDateTime(new TimeOnly()), date.AddDays(-1).ToDateTime(new TimeOnly())));
                }

                dateCounter = date;
            }

            this.dpProtocols.DisplayDateStart = firstDate.ToDateTime(new TimeOnly());
            this.dpProtocols.DisplayDateEnd = dateCounter.ToDateTime(new TimeOnly());

            #endregion date picker

            this.dpProtocols.SelectedDate = dateCounter.ToDateTime(new TimeOnly());

            #endregion existing logs
        }

        #region listener

        /// <summary>
        /// Handles the SelectionChanged event of the date picker.
        /// </summary>
        /// <param name="sender">The date picker.</param>
        /// <param name="e">The event arguments.</param>
        private void dpProtocols_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDateTime = dpProtocols.SelectedDate;

            if (!selectedDateTime.HasValue) return;

            var selectedDate = DateOnly.FromDateTime(selectedDateTime.Value);

            this.cbProtocols.Items.Clear();
            this.tvData.Items.Clear();

            var valid = !this.dpProtocols.BlackoutDates.Contains(selectedDate.ToDateTime(new TimeOnly()));
            if (!valid)
            {
                var languages = Utils.Language.getRDict();

                MessageBox.Show(
                    languages["logs.notValid.message"].ToString(),
                    languages["logs.protocols.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                this.dpProtocols.SelectedDate = null;

                return;
            }

            #region read logs

            var nodes = Utils.Log.get(selectedDate);

            foreach (var node in nodes)
            {
                var link = new Log.Sync(node, selectedDate);

                var item = new ComboBoxItem();
                item.Content = link.timestamp.ToString("HH:mm:ss");
                item.Tag = link;

                this.cbProtocols.Items.Add(item);
            }

            this.cbProtocols.SelectedIndex = this.cbProtocols.Items.Count - 1;

            #endregion read logs
        }

        /// <summary>
        /// Handles the SelectedDateChanged event of the combo box.
        /// </summary>
        /// <param name="sender">The combo box.</param>
        /// <param name="e">The event arguments.</param>
        private void cbProtocols_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem?)this.cbProtocols.SelectedValue;

            if (item is null) return;

            var sync = (Log.Sync)item.Tag;

            this.tvData.Items.Clear();
            foreach (var link in sync.links)
            {
                this.tvData.Items.Add(link.getTVItem());
            }

            this.tvData.Items.Refresh();

            this.btnOpenAll_Click(this.btnOpenAll, new RoutedEventArgs(Button.ClickEvent));
        }

        /// <summary>
        /// Handles the Click event of the Open All button.
        /// </summary>
        /// <param name="sender">The Open All button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnOpenAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in this.tvData.Items)
            {
                TreeViewItem treeItem = (TreeViewItem)item;

                if (treeItem != null)
                {
                    this.expandAll(treeItem, true);
                    treeItem.IsExpanded = true;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the Close All button.
        /// </summary>
        /// <param name="sender">The Close All button.</param>
        /// <param name="e">The event arguments.</param>
        private void btnCloseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in this.tvData.Items)
            {
                TreeViewItem treeItem = (TreeViewItem)item;

                if (treeItem != null)
                {
                    this.expandAll(treeItem, false);
                    treeItem.IsExpanded = false;
                }
            }
        }

        /// <summary>
        /// Expands or collapses all tree view items.
        /// </summary>
        /// <param name="items">The tree view items to expand or collapse.</param>
        /// <param name="expand">Whether to expand or collapse the tree view items.</param>
        private void expandAll(TreeViewItem items, bool expand)
        {
            foreach (var item in items.Items)
            {
                TreeViewItem treeItem = (TreeViewItem)item;

                if (treeItem != null)
                {
                    this.expandAll(treeItem, expand);
                    treeItem.IsExpanded = expand;
                }
            }
        }

        #endregion listener
    }
}