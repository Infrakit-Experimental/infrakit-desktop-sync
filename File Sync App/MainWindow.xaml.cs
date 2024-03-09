using File_Sync_App.InputWindows;
using Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using static Library.Utils;

namespace File_Sync_App
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables

        /// <summary>
        /// A list of links to be displayed in the application.
        /// </summary>
        public List<Link> data { get; set; }

        /// <summary>
        /// A background worker that is used to automatically sync the links with the server.
        /// </summary>
        private BackgroundWorker bwAutoSync;

        /// <summary>
        /// A flag that indicates whether or not to delete folders and files when syncing.
        /// </summary>
        internal static bool deleteFoldersAndFiles;

        #endregion variables

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // Set the resource dictionary.
            this.setRDict();

            // Log in to the server.
            if (!Utils.logIn()) { return; }

            // Initialize the window components.
            InitializeComponent();

            // Get the delete folders and files flag from the settings.
            #region delete folders & files

            MainWindow.deleteFoldersAndFiles = false;

            try
            {
                MainWindow.deleteFoldersAndFiles = bool.Parse(Settings.get("deleteFoldersAndFiles"));
            }
            catch (Exception ex)
            {
                Log.write("main.errorSettings.deleteFoldersAndFiles: " + ex.GetType() + " | " + ex.Message);

                Settings.@override("deleteFoldersAndFiles", "False");

                var languages = Utils.Language.getRDict();
                MessageBox.Show(
                    languages["settings.errorLoading.message"].ToString(),
                    languages["settings.errorLoading.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            #endregion delete folders & files

            // Read the links from the XML file.
            #region read links

            this.data = new();

            Utils.Resources.path = Path.Combine(Utils.Resources.directory, "Links.xml");
            if(!File.Exists(Utils.Resources.path))
            {
                Utils.Resources.create();
            }
            else
            {
                var nodes = Utils.Resources.getBody();

                foreach (var node in nodes.ChildNodes)
                {
                    var link = new Link(node as XmlNode);

                    this.data.Add(link);
                }
            }

            #endregion read links

            // Remove old logs and protocols.
            #region remove logs

            DateOnly now = DateOnly.FromDateTime(DateTime.Now);

            #region log

            var logDeletion = false;

            try
            {
                logDeletion = bool.Parse(Settings.get("logDeletion"));
            }
            catch(Exception ex)
            {
                Log.write("main.errorSettings.logDeletion: " + ex.GetType() + " | " + ex.Message);

                Settings.@override("logDeletion", "False");

                var languages = Utils.Language.getRDict();
                MessageBox.Show(
                    languages["settings.errorLoading.message"].ToString(),
                    languages["settings.errorLoading.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            if(logDeletion)
            {
                int? logSaveDur = null;

                try
                {
                    logSaveDur = int.Parse(Settings.getAttribute("logDeletion", "duration"));
                }
                catch (Exception ex)
                {
                    Log.write("main.errorSettings.logDeletion.duration: " + ex.GetType() + " | " + ex.Message);

                    Settings.@override("logDeletion", "False");

                    var languages = Utils.Language.getRDict();
                    MessageBox.Show(
                        languages["settings.errorLoading.message"].ToString(),
                        languages["settings.errorLoading.caption"].ToString(),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }

                if(logSaveDur.HasValue)
                {
                    string[] logFiles = Directory.EnumerateFiles(Utils.Log.directory, "*.*")
                         .Select(p => Path.GetFileName(p))
                         .Where(s => s.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
                         .Select(e => Path.GetFileNameWithoutExtension(e))
                         .ToArray();

                    foreach (var file in logFiles)
                    {
                        var date = DateOnly.ParseExact(file, "yyyyMMdd");

                        var dur = now.DayNumber - date.DayNumber;

                        if (dur > logSaveDur.Value)
                        {
                            File.Delete(Path.Combine(Utils.Log.directory, file + ".log"));
                        }
                    }
                }
            }

            #endregion log

            #region sync log 

            var syncLogDeletion = false;

            try
            {
                syncLogDeletion = bool.Parse(Settings.get("syncLogDeletion"));
            }
            catch (Exception ex)
            {
                Log.write("main.errorSettings.syncLogDeletion: " + ex.GetType() + " | " + ex.Message);

                Settings.@override("syncLogDeletion", "False");

                var languages = Utils.Language.getRDict();
                MessageBox.Show(
                    languages["settings.errorLoading.message"].ToString(),
                    languages["settings.errorLoading.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            if (syncLogDeletion)
            {
                int? syncLogSaveDur = null;

                try
                {
                    syncLogSaveDur = int.Parse(Settings.getAttribute("syncLogDeletion", "duration"));
                }
                catch (Exception ex)
                {
                    Log.write("main.errorSettings.syncLogDeletion.duration: " + ex.GetType() + " | " + ex.Message);

                    Settings.@override("syncLogDeletion", "False");

                    var languages = Utils.Language.getRDict();
                    MessageBox.Show(
                        languages["settings.errorLoading.message"].ToString(),
                        languages["settings.errorLoading.caption"].ToString(),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }

                if(syncLogSaveDur.HasValue)
                {
                    string[] protocolFiles = Directory.EnumerateFiles(Utils.Log.directory, "*.*")
                     .Select(p => Path.GetFileName(p))
                     .Where(s => s.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                     .Select(e => Path.GetFileNameWithoutExtension(e))
                     .ToArray();

                    foreach (var file in protocolFiles)
                    {
                        var date = DateOnly.ParseExact(file, "yyyyMMdd");

                        var dur = now.DayNumber - date.DayNumber;

                        if (dur > syncLogSaveDur.Value)
                        {
                            System.IO.File.Delete(Path.Combine(Utils.Log.directory, file + ".xml"));
                        }
                    }
                }
            }

            #endregion sync log

            #endregion remove logs

            // Start the automatic sync if it is enabled.
            this.startAutoSync();
        }

        #region listener

        /// <summary>
        /// Logs out the current user and displays the login window.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnChangeUser_Click(object sender, RoutedEventArgs e)
        {
            API.clear();
            this.Hide();

            if(this.btnSyncAutoStop.Visibility == Visibility.Visible)
            {
                this.btnSyncAutoStop.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }

            if (!Utils.logIn()) { return; }

            this.startAutoSync();
            this.Show();
        }

        /// <summary>
        /// Displays the settings window.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new();
            var res = sw.ShowDialog();

            if (res.HasValue && res.Value)
            {
                this.setRDict();
            }
        }

        #region edit links

        /// <summary>
        /// Enables or disables the edit and remove link buttons depending on whether or not a link is selected in the list box.
        /// </summary>
        /// <param name="sender">The listbox that was changed.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void lbLinks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.lbLinks.SelectedItem is null)
            {
                this.btnEditLink.IsEnabled = false;
                this.btnRemoveLink.IsEnabled = false;
                return;
            }

            this.btnEditLink.IsEnabled = true;
            this.btnRemoveLink.IsEnabled = true;
        }

        /// <summary>
        /// Displays a dialog window for adding a new link.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnAddLink_Click(object sender, RoutedEventArgs e)
        {
            LinkWindow lw = new(this.data);

            bool? ok = lw.ShowDialog();

            if (ok.HasValue && !ok.Value || !ok.HasValue)
            {
                return;
            }

            this.data.Add(lw.link);

            var linkNode = lw.link.getXmlNode();

            Utils.Log.write("log.link.add: \"" + lw.link.name + "\"");
            Utils.Resources.add(linkNode.doc, linkNode.node);

            this.lbLinks.Items.Refresh();
        }

        /// <summary>
        /// Displays a dialog window for editing the selected link.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnEditLink_Click(object sender, RoutedEventArgs e)
        {
            var oldLink = this.lbLinks.SelectedItem as Link;

            LinkWindow lw = new(this.data, oldLink);

            bool? ok = lw.ShowDialog();

            if (ok.HasValue && !ok.Value || !ok.HasValue)
            {
                return;
            }

            var index = this.lbLinks.SelectedIndex;

            this.data[index] = lw.link;
            var linkNode = lw.link.getXmlNode();

            if(oldLink.name.Equals(lw.link.name))
            {
                Utils.Log.write("log.link.edit: \"" + lw.link.name + "\"");
            }
            else
            {
                Utils.Log.write("log.link.edit: \"" + oldLink.name + "\" -> \"" + lw.link.name + "\"");
            }

            Utils.Resources.edit(linkNode.doc, linkNode.node, "link[@name='" + oldLink.name + "']");

            this.lbLinks.Items.Refresh();
        }

        /// <summary>
        /// Displays a confirmation message box and removes the selected link if the user clicks OK.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnRemoveLink_Click(object sender, RoutedEventArgs e)
        {
            var languages = Utils.Language.getRDict();

            var result = MessageBox.Show(
                languages["main.removeLinkQuestion.message"].ToString(),
                languages["main.removeLinkQuestion.caption"].ToString(),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question
            );

            if(result == MessageBoxResult.Cancel) { return; }

            var link = this.lbLinks.SelectedItem as Link;

            this.data.RemoveAt(this.lbLinks.SelectedIndex);

            Utils.Log.write("log.link.remove: \"" + link.name + "\"");
            Utils.Resources.remove("link[@name='" + link.name + "']");

            this.lbLinks.Items.Refresh();
        }

        #endregion edit links

        #region sync

        #region manual

        /// <summary>
        /// Switches the UI to the manual sync mode.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnSyncManual_Click(object sender, RoutedEventArgs e)
        {
            this.gdSyncAutomatic.Visibility = Visibility.Collapsed;
            this.gdSyncManual.Visibility = Visibility.Visible;

            API.maxErrorDisplayTime = new TimeSpan(0, 15, 0);
        }

        /// <summary>
        /// Starts a manual sync.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnSyncManualStart_Click(object sender, RoutedEventArgs e)
        {
            var links = this.getActiveLinks();

            if (links is null) return;

            Thread syncThread = new Thread(() =>
            {
                Utils.Log.write("log.sync.manual.start");

                this.sync(links, false);

                #region success message

                var languages = Utils.Language.getRDict();

                MessageBox.Show(
                    languages["sync.linksSynced.message"].ToString(),
                    languages["sync.linksSynced.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                #endregion success message

                Utils.Log.write("log.sync.manual.end");

                this.Dispatcher.Invoke(() => this.lbLinks.Items.Refresh(), DispatcherPriority.Background);
            });
            syncThread.Start();
        }

        #endregion manual

        #region automatic

        /// <summary>
        /// Switches the UI to the automatic sync mode.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnSyncAutomatic_Click(object sender, RoutedEventArgs e)
        {
            this.gdSyncAutomatic.Visibility = Visibility.Visible;
            this.gdSyncManual.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Starts the automatic sync.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnSyncAutoStart_Click(object sender, RoutedEventArgs e)
        {
            #region check if time is valid

            var timeSpan = this.getTime();

            if (!timeSpan.HasValue)
            {
                var languages = Utils.Language.getRDict();

                Utils.Log.write("main.noValidTimespan");
                MessageBox.Show(
                    languages["main.time.message"].ToString(),
                    languages["main.time.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return;
            }

            #endregion check if time is valid

            #region save auto sync to settings

            Settings.set("autoSync", "True");

            var selectedType = this.cbTime.SelectedItem as ComboBoxItem;

            Settings.addAttribute("autoSync", "Type", selectedType.Tag.ToString());
            Settings.addAttribute("autoSync", "Time", timeSpan.Value.ToString());

            #endregion save auto sync to settings

            #region disable functions

            this.btnSyncAutoStart.Visibility = Visibility.Collapsed;
            this.btnSyncAutoStop.Visibility = Visibility.Visible;

            this.tbTimeHours.IsEnabled = false;
            this.tbTimeMinutes.IsEnabled = false;

            this.tbTime.IsEnabled = false;
            this.cbTime.IsEnabled = false;
            this.btnSyncManual.IsEnabled = false;

            #endregion disable functions

            API.maxErrorDisplayTime = timeSpan.Value - TimeSpan.FromSeconds(5);

            #region setup BackgroundWorker

            this.bwAutoSync = new();
            this.bwAutoSync.DoWork += this.autoSync_DoWork;
            this.bwAutoSync.WorkerSupportsCancellation = true;

            var args = new object[2];

            args[0] = timeSpan.Value;
            args[1] = ((ComboBoxItem)this.cbTime.SelectedItem).Name == "Fixed";

            #endregion setup BackgroundWorker

            Utils.Log.write("log.sync.automatic.start");

            this.bwAutoSync.RunWorkerAsync(args);
        }

        /// <summary>
        /// Stops the automatic sync.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnSyncAutoStop_Click(object sender, RoutedEventArgs e)
        {
            this.bwAutoSync.CancelAsync();

            Utils.Log.write("log.sync.automatic.end");

            Settings.set("autoSync", "False");

            API.maxErrorDisplayTime = new TimeSpan(0, 15, 0);

            #region enable functions

            this.btnSyncAutoStart.Visibility = Visibility.Visible;
            this.btnSyncAutoStop.Visibility = Visibility.Collapsed;

            this.tbTimeHours.IsEnabled = true;
            this.tbTimeMinutes.IsEnabled = true;

            this.tbTime.IsEnabled = true;
            this.cbTime.IsEnabled = true;
            this.btnSyncManual.IsEnabled = true;

            #endregion enable functions
        }

        #endregion automatic

        #region timespan imput

        /// <summary>
        /// Validates the input of a text box to ensure that only numbers are entered.
        /// </summary>
        /// <param name="sender">The text box that has been changed.</param>
        /// <param name="e">The event arguments.</param>
        private void tbNumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Checks the validity of the time input when the text box changes.
        /// </summary>
        /// <param name="sender">The text box that has been changed.</param>
        /// <param name="e">The event arguments.</param>
        private void tbTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.checkTimeInput();
        }

        /// <summary>
        /// Checks the validity of the time hours input when the text box changes.
        /// </summary>
        /// <param name="sender">The text box that has been changed.</param>
        /// <param name="e">The event arguments.</param>
        private void tbTimeHours_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.tbTimeHours.Text.Length == 0) return;

            var hour = int.Parse(this.tbTimeHours.Text);

            if (hour < 24) return;

            if (hour == 24)
            {
                this.tbTimeHours.Text = "0";
                return;
            }

            var languages = Utils.Language.getRDict();

            MessageBox.Show(
                languages["main.time.hours"].ToString(),
                languages["main.time.caption"].ToString(),
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );

            this.tbTimeHours.Text = "23";
        }

        /// <summary>
        /// Checks the validity of the time minutes input when the text box changes.
        /// </summary>
        /// <param name="sender">The text box that has been changed.</param>
        /// <param name="e">The event arguments.</param>
        private void tbTimeMinutes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.tbTimeMinutes.Text.Length == 0) return;

            if (int.Parse(this.tbTimeMinutes.Text) < 60) return;
            
            var languages = Utils.Language.getRDict();

            MessageBox.Show(
                languages["main.time.minutes"].ToString(),
                languages["main.time.caption"].ToString(),
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );

            this.tbTimeMinutes.Text = "59";
        }

        /// <summary>
        /// Formats the time minutes input when the text box loses focus.
        /// </summary>
        /// <param name="sender">The text box that has lost focus.</param>
        /// <param name="e">The event arguments.</param>
        private void tbTimeMinutes_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.tbTimeMinutes.Text.Length == 0) return;

            var number = int.Parse(this.tbTimeMinutes.Text);

            if (this.tbTimeMinutes.Text.Length == 1 & number < 10)
            {
                this.tbTimeMinutes.Text = "0" + this.tbTimeMinutes.Text;
            }
        }

        /// <summary>
        /// Shows or hides the time input and every input depending on the selected time unit.
        /// </summary>
        /// <param name="sender">The combo box that has been changed.</param>
        /// <param name="e">The event arguments.</param>
        private void cbTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUnit = ((ComboBoxItem)this.cbTime.SelectedItem).Tag;
            
            if(selectedUnit.Equals("Fixed"))
            {
                this.tbTime.Visibility = Visibility.Collapsed;
                this.gdTime.Visibility = Visibility.Visible;
                this.tbEvery.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.tbTime.Visibility = Visibility.Visible;
                this.gdTime.Visibility = Visibility.Collapsed;
                this.tbEvery.Visibility = Visibility.Visible;

                this.checkTimeInput();
            }
        }

        #endregion timespan imput

        #endregion sync

        /// <summary>
        /// Handles the Closing event of the Window.
        /// It terminates the application when the window is closing.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event data.</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(this.gdSyncProcess.Visibility == Visibility.Visible)
            {
                var languages = Utils.Language.getRDict();

                var res = MessageBox.Show(
                    languages["main.syncInProgress.message"].ToString(),
                    languages["main.syncInProgress.caption"].ToString(),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (res == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            Utils.Log.write("log.logOut");

            Environment.Exit(0);
        }

        #endregion listener

        /// <summary>
        /// Starts the automatic synchronization process based on the user's settings.
        /// </summary>
        private void startAutoSync()
        {
            bool autoSync;

            try
            {
                autoSync = bool.Parse(Settings.get("autoSync"));
            }
            catch (Exception ex)
            {
                Log.write("main.errorSettings.autoSync: " + ex.GetType() + " | " + ex.Message);

                Settings.@override("autoSync", "False");

                var languages = Utils.Language.getRDict();
                MessageBox.Show(
                    languages["settings.errorLoading.message"].ToString(),
                    languages["settings.errorLoading.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return;
            }

            if (autoSync)
            {
                var languages = Utils.Language.getRDict();

                var result = MessageBox.Show(
                    languages["main.startAutoSync.message"].ToString(),
                    languages["main.startAutoSync.caption"].ToString(),
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Cancel) return;

                this.btnSyncAutomatic.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                var type = "";
                var timeString = "";

                try
                {
                    type = Settings.getAttribute("autoSync", "Type");
                    timeString = Settings.getAttribute("autoSync", "Time");
                }
                catch (Exception ex)
                {
                    Log.write("main.errorSettings.autoSync.attributes: " + ex.GetType() + " | " + ex.Message);

                    Settings.@override("autoSync", "False");

                    MessageBox.Show(
                        languages["settings.errorLoading.message"].ToString(),
                        languages["settings.errorLoading.caption"].ToString(),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    return;
                }

                var time = TimeSpan.Parse(timeString);
                switch (type)
                {
                    case "Minutes":
                        this.cbTime.SelectedIndex = 0;
                        this.tbTime.Text = ((int)time.TotalMinutes).ToString();
                        break;

                    case "Hours":
                        this.cbTime.SelectedIndex = 1;
                        this.tbTime.Text = ((int)time.TotalHours).ToString();
                        break;

                    case "Days":
                        this.cbTime.SelectedIndex = 2;
                        this.tbTime.Text = ((int)time.TotalDays).ToString();
                        break;

                    case "Fixed":
                        this.cbTime.SelectedIndex = 3;

                        this.tbTimeHours.Text = time.Hours.ToString();

                        var min = time.Minutes;
                        this.tbTimeMinutes.Text = min.ToString();

                        if (min < 10)
                        {
                            this.tbTimeMinutes.Text = "0" + this.tbTimeMinutes.Text;
                        }

                        break;
                }

                if (result == MessageBoxResult.Yes)
                {
                    this.btnSyncAutoStart.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                }
            }
        }

        #region sync

        /// <summary>
        /// Retrieves a list of active links from the data.
        /// If no active links are found, it logs a message, displays a message box to inform the user, and returns null.
        /// </summary>
        /// <returns>A list of active links, or null if no active links are found.</returns>
        private List<Link>? getActiveLinks()
        {
            var links = new List<Link>();

            foreach (var link in this.data)
            {
                if (link.active)
                {
                    links.Add(link);
                }
            }

            var languages = Utils.Language.getRDict();

            if (links.Count == 0)
            {
                Utils.Log.write("main.noActiveLinks");
                MessageBox.Show(
                    languages["main.noActiveLinks.message"].ToString(),
                    languages["main.noActiveLinks.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return null;
            }

            return links;
        }

        /// <summary>
        /// Syncs a list of links between the local system and Infrakit.
        /// It updates a progress bar to display the progress of the sync operation.
        /// After all links have been synced, it exports the sync logs. During the sync process, certain UI elements are disabled and re-enabled after completion.
        /// </summary>
        /// <param name="links">A list of links to sync.</param>
        /// <param name="automatic">A boolean indicating whether the sync is automatic or manual.</param>
        private void sync(List<Link> links, bool automatic)
        {
            this.Dispatcher.Invoke(() =>
            {
                #region disable functions

                if (automatic)
                {
                    this.gdSyncAutomatic.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.gdSyncManual.Visibility = Visibility.Collapsed;
                }

                this.gdSettings.IsEnabled = false;
                this.gdEditLinks.IsEnabled = false;
                this.gdLinks.IsEnabled = false;

                #endregion disable functions

                this.gdSyncProcess.Visibility = Visibility.Visible;

                #region setup progress bar

                this.pbSync.Value = 0;
                this.pbSync.Maximum = links.Count;

                this.tbSync.Text = this.pbSync.Value + " / " + this.pbSync.Maximum + " (0%)";

                #endregion setup progress bar

            }, DispatcherPriority.Background);

            Nodes.Log.Sync logs = new Nodes.Log.Sync();

            foreach (var link in links)
            {
                logs.add(link.sync());

                #region update progress bar

                this.Dispatcher.Invoke(() =>
                {
                    this.pbSync.Value++;

                    double percent = Math.Round((this.pbSync.Value / this.pbSync.Maximum) * 100, 2);

                    this.tbSync.Text = this.pbSync.Value + " / " + this.pbSync.Maximum + " (" + percent + "%)";
                }, DispatcherPriority.Background);

                #endregion update progress bar
            }

            logs.export();

            this.Dispatcher.Invoke(() =>
            {
                #region enable functions

                this.gdSettings.IsEnabled = true;
                this.gdEditLinks.IsEnabled = true;
                this.gdLinks.IsEnabled = true;

                if (automatic)
                {
                    this.gdSyncAutomatic.Visibility = Visibility.Visible;
                }
                else
                {
                    this.gdSyncManual.Visibility = Visibility.Visible;
                }

                #endregion enable functions

                this.gdSyncProcess.Visibility = Visibility.Collapsed;

            }, DispatcherPriority.Background);
        }

        #region automatic

        /// <summary>
        /// Gets the sync time span from the user input.
        /// </summary>
        /// <returns>A TimeSpan object representing the sync time span, or null if the user input is invalid.</returns>
        private TimeSpan? getTime()
        {
            var selectedUnit = ((ComboBoxItem)this.cbTime.SelectedItem).Tag;

            int selectedSpan;

            switch (selectedUnit)
            {
                case "Minutes":
                case "Hours":
                case "Days":
                    if (this.tbTime.Text.Length == 0) return null;

                    selectedSpan = int.Parse(this.tbTime.Text);
                    break;

                case "Fixed":
                    if (this.tbTimeHours.Text.Length == 0 ||
                        this.tbTimeMinutes.Text.Length == 0) return null;

                    selectedSpan = int.Parse(this.tbTimeHours.Text);
                    break;

                default:
                    return null;
            }

            switch (selectedUnit)
            {
                case "Minutes":
                    return new TimeSpan(0, selectedSpan, 0);

                case "Hours":
                    return new TimeSpan(selectedSpan, 0, 0);

                case "Days":
                    return new TimeSpan(selectedSpan, 0, 0, 0);

                case "Fixed":
                    var selectedMinutes = int.Parse(this.tbTimeMinutes.Text);

                    return new TimeSpan(selectedSpan, selectedMinutes, 0);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Asynchronously performs automatic synchronization based on the provided arguments.
        /// If the second argument is true, it treats the first argument as a specific time of day and waits until that time to perform the sync.
        /// Otherwise, it treats the first argument as a time interval and performs the sync after that interval.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event data.</param>
        private async void autoSync_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = (object[]?)e.Argument;

            if (args is null) return;

            TimeSpan timeSpan = (TimeSpan)args[0];
            bool time = (bool)args[1];

            if (time)
            {
                while (!this.bwAutoSync.CancellationPending)
                {
                    DateTime current = DateTime.Now;
                    TimeSpan timeToGo = timeSpan - current.TimeOfDay;

                    if (timeToGo < TimeSpan.Zero)
                    {
                        timeToGo = timeToGo.Add(new TimeSpan(1,0,0,0));
                    }

                    Thread.Sleep(timeToGo);

                    this.autoSync();
                }
            }
            else
            {
                while (!this.bwAutoSync.CancellationPending)
                {
                    this.autoSync();

                    Thread.Sleep(timeSpan);
                }
            }
        }

        /// <summary>
        /// Performs automatic synchronization of active links.
        /// If no active links are found, it logs a message and displays an auto-closing message box to inform the user.
        /// The synchronization is performed in a separate task to avoid blocking the UI thread.
        /// </summary>
        private void autoSync()
        {
            var links = this.getActiveLinks();

            if (links is null) return;

            API.newErrorThread = true;

            this.sync(links, true);

            API.newErrorThread = false;
        }

        #endregion automatic

        #endregion sync

        /// <summary>
        /// Checks the validity of the time input and displays an error message if necessary.
        /// </summary>
        private void checkTimeInput()
        {
            if (this.tbTime.Text.Length == 0) return;

            var timespan = int.Parse(this.tbTime.Text);

            var type = ((ComboBoxItem)this.cbTime.SelectedItem).Tag;
            switch (type)
            {
                case "Minutes":
                    if (timespan <= 35791) return;
                    this.tbTime.Text = "35791";
                    break;

                case "Hours":
                    if (timespan <= 596) return;
                    this.tbTime.Text = "596";
                    break;

                case "Days":
                    if (timespan <= 24) return;
                    this.tbTime.Text = "24";
                    break;

                case "Fixed":
                    this.tbTime.Text = "";
                    return;
            }

            var languages = Utils.Language.getRDict();

            MessageBox.Show(
                languages["main.time.error"].ToString(),
                languages["main.time.caption"].ToString(),
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
}