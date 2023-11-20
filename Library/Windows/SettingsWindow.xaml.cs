using System.Windows;
using System.Windows.Controls;

namespace Library.Windows
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsWindow"/> class.
        /// </summary>
        public SettingsWindow()
        {
            // Set the resource dictionary for the current language.
            this.setRDict();

            // Initializes the components of the window.
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
        }

        #region listeners

        /// <summary>
        /// Sets the resource dictionary of the window based on the selected language.
        /// </summary>
        /// <param name="sender">The ComboBox that was changed.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void cbLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.setRDict((this.cbLanguages.SelectedItem as ComboBoxItem).Name);
        }

        /// <summary>
        /// Sets the margin of the grid containing the settings controls based on whether the scroll viewer is scrollable.
        /// </summary>
        /// <param name="sender">The scroll viewer.</param>
        /// <param name="e">The scroll changed event arguments.</param>
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
        /// Saves the selected language setting and closes the window.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Utils.Language.set((this.cbLanguages.SelectedItem as ComboBoxItem).Name);

            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Closes the window without saving any changes.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">The routed event arguments.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion controls

        #endregion listeners
    }
}