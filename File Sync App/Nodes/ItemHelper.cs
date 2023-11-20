using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace File_Sync_App.Nodes
{
    class ItemHelper : DependencyObject
    {
        #region dependency properties

        /// <summary>
        /// Identifies the IsExpanded attached dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.RegisterAttached
        (
            "IsExpanded",
            typeof(bool?),
            typeof(ItemHelper)
        );

        #endregion dependency properties

        #region is expanded

        /// <summary>
        /// Sets the value of the IsExpanded attached dependency property on a given object.
        /// </summary>
        /// <param name="element">The element to set the property on.</param>
        /// <param name="IsChecked">The value to set the property to.</param>
        public static void SetIsExpanded(DependencyObject element, bool? IsChecked)
        {
            element.SetValue(IsExpandedProperty, IsChecked);
        }

        /// <summary>
        /// Gets the value of the IsExpanded attached dependency property from a given object.
        /// </summary>
        /// <param name="element">The element to get the property from.</param>
        /// <returns>The value of the IsExpanded property.</returns>
        public static bool? GetIsExpanded(DependencyObject element)
        {
            return (bool?)element.GetValue(IsExpandedProperty);
        }

        #endregion is expanded
    }
}
