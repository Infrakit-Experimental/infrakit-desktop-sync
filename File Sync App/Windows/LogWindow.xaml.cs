using Library;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace File_Sync_App.Windows
{
    /// <summary>
    /// Interaktionslogik für LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogWindow"/> class.
        /// </summary>
        public LogWindow()
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
                 .Where(s => s.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
                 .Select(e => Path.GetFileNameWithoutExtension(e))
                 .ToArray();

            // If there are no log files, show a message to the user and return.
            if (files.Length == 0)
            {
                var languages = Utils.Language.getRDict();

                MessageBox.Show(
                    languages["logs.noLogs.message"].ToString(),
                    languages["logs.logs.caption"].ToString(),
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

                if (first)
                {
                    first = false;

                    firstDate = date;
                    dateCounter = date;
                    continue;
                }

                if (date.AddDays(-1) != dateCounter)
                {
                    this.dpLogs.BlackoutDates.Add(
                        new CalendarDateRange(dateCounter.AddDays(1).ToDateTime(new TimeOnly()), date.AddDays(-1).ToDateTime(new TimeOnly())));
                }

                dateCounter = date;
            }

            this.dpLogs.DisplayDateStart = firstDate.ToDateTime(new TimeOnly());
            this.dpLogs.DisplayDateEnd = dateCounter.ToDateTime(new TimeOnly());

            #endregion date picker

            this.dpLogs.SelectedDate = dateCounter.ToDateTime(new TimeOnly());

            #endregion existing logs
        }

        #region listener

        /// <summary>
        /// Handles the SelectedDateChanged event of the date picker.
        /// </summary>
        /// <param name="sender">The date picker.</param>
        /// <param name="e">The event arguments.</param>
        private void dpLogs_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDateTime = dpLogs.SelectedDate;

            if (!selectedDateTime.HasValue) return;

            var selectedDate = DateOnly.FromDateTime(selectedDateTime.Value);

            this.tbData.Text = "";

            var valid = !this.dpLogs.BlackoutDates.Contains(selectedDate.ToDateTime(new TimeOnly()));
            if (!valid)
            {
                var languages = Utils.Language.getRDict();

                MessageBox.Show(
                    languages["logs.notValid.message"].ToString(),
                    languages["logs.logs.caption"].ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                this.dpLogs.SelectedDate = null;

                return;
            }

            this.tbData.Text = System.IO.File.ReadAllText(Path.Combine(Utils.Log.directory, selectedDate.ToString("yyyyMMdd") + ".log"));
        }

        #endregion listener
    }
}
