using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Library.Resources
{
    /// <summary>
    /// Represents a bar in a bar chart, with properties for value, maximum value, height, and color.
    /// </summary>
    public partial class Bar : UserControl, INotifyPropertyChanged
    {
        #region variables

        #region backing fields

        /// <summary>
        /// Backing field for Value.
        /// </summary>
        private double _value;

        /// <summary>
        /// Backing field for MaxValue.
        /// </summary>
        private double _maxValue;

        /// <summary>
        /// Backing field for BarHeight.
        /// </summary>
        private double _barHeight;

        /// <summary>
        /// Backing field for Color.
        /// </summary>
        private Brush _color;

        /// <summary>
        /// Backing field for BackgroundColor.
        /// </summary>
        private Brush _backgroundColor;

        #endregion backing fields

        /// <summary>
        /// Gets or sets the value of the bar. This also updates the bar's height and notifies of property change.
        /// </summary>
        public double Value
        {
            get { return this._value; }
            set
            {
                this._value = value;
                UpdateBarHeight();
                NotifyPropertyChanged("Value");
            }
        }

        /// <summary>
        /// Gets or sets the maximum value for the bar. This also updates the bar's height and notifies of property change.
        /// </summary>
        public double MaxValue
        {
            get
            {
                return this._maxValue;
            }
            set
            {
                this._maxValue = value;
                UpdateBarHeight();
                NotifyPropertyChanged("Value");
            }
        }

        /// <summary>
        /// Gets the height of the bar and notifies of property change.
        /// </summary>
        public double BarHeight
        {
            get
            {
                return this._barHeight;
            }
            private set
            {
                this._barHeight = value;
                NotifyPropertyChanged("BarHeight");
            }
        }

        /// <summary>
        /// Gets or sets the color of the bar and notifies of property change.
        /// </summary>
        public Brush Color
        {
            get
            {
                return this._color;
            }
            set
            {
                this._color = value;
                NotifyPropertyChanged("Color");
            }
        }

        /// <summary>
        /// Gets or sets the color of the bar and notifies of property change.
        /// </summary>
        public Brush BackgroundColor
        {
            get
            {
                return this._backgroundColor;
            }
            set
            {
                this._backgroundColor = value;
                NotifyPropertyChanged("BackgroundColor");
            }
        }

        #endregion variables

        /// <summary>
        /// Initializes a new instance of the Bar class, setting the DataContext and default color.
        /// </summary>
        public Bar()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Color = Brushes.Black;
            this.BackgroundColor = Brushes.Transparent;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #region listeners

        /// <summary>
        /// Updates the bar height when the control is loaded.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateBarHeight();
        }

        /// <summary>
        /// Updates the bar height when the size of the grid changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateBarHeight();
        }

        #endregion listeners

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="info">The name of the property that changed.</param>
        private void NotifyPropertyChanged(string info)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// Updates the height of the bar based on its value and maximum value.
        /// </summary>
        private void UpdateBarHeight()
        {
            if(this._maxValue > 0)
            {
                var percent = this._value / this._maxValue;
                this.BarHeight = percent * this.ActualHeight;
            }

        }
    }
}