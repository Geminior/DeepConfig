namespace DeepConfigEditor.Components
{
    using System.Windows;
    using System.Windows.Controls;

    public class Attached : DependencyObject
    {
        public static readonly DependencyProperty EmptyMessageProperty = DependencyProperty.RegisterAttached("EmptyMessage", typeof(string), typeof(Attached), new PropertyMetadata(string.Empty));

        /// <summary>
        /// The ColumnWidths and RowHeights attached props have been copied from the CODE.Framework
        /// </summary>
        public static readonly DependencyProperty ColumnWidthsProperty = DependencyProperty.RegisterAttached("ColumnWidths", typeof(string), typeof(Attached), new PropertyMetadata("*", ColumnWidthsPropertyChanged));

        public static readonly DependencyProperty RowHeightsProperty = DependencyProperty.RegisterAttached("RowHeights", typeof(string), typeof(Attached), new PropertyMetadata("*", RowHeightsPropertyChanged));

        public static string GetEmptyMessage(DependencyObject obj)
        {
            return (string)obj.GetValue(EmptyMessageProperty);
        }

        public static void SetEmptyMessage(DependencyObject obj, string value)
        {
            obj.SetValue(EmptyMessageProperty, value);
        }
        
        /// <summary>Column widths</summary>
        /// <param name="obj">Object to set the columns widths on</param>
        /// <returns>Column width</returns>
        /// <remarks>This attached property can be attached to any UI Element to define the column width</remarks>
        public static string GetColumnWidths(DependencyObject obj)
        {
            return (string)obj.GetValue(ColumnWidthsProperty);
        }

        /// <summary>Column width</summary>
        /// <param name="obj">Object to set the column widths on</param>
        /// <param name="value">Value to set</param>
        public static void SetColumnWidths(DependencyObject obj, string value)
        {
            obj.SetValue(ColumnWidthsProperty, value);
        }

        /// <summary>Row heights</summary>
        /// <param name="obj">Object to set the row heights on</param>
        /// <returns>Column width</returns>
        /// <remarks>This attached property can be attached to any UI Element to define the row height</remarks>
        public static string GetRowHeights(DependencyObject obj)
        {
            return (string)obj.GetValue(RowHeightsProperty);
        }

        /// <summary>Row heights</summary>
        /// <param name="obj">Object to set the row heights on</param>
        /// <param name="value">Value to set</param>
        public static void SetRowHeights(DependencyObject obj, string value)
        {
            obj.SetValue(RowHeightsProperty, value);
        }

        /// <summary>
        /// Handler for column width changes
        /// </summary>
        /// <param name="d">Source object</param>
        /// <param name="e">Event arguments</param>
        private static void ColumnWidthsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as Grid;
            if (grid == null)
            {
                return;
            }

            grid.ColumnDefinitions.Clear();
            var widths = e.NewValue.ToString();
            var parts = widths.Split(',');
            foreach (var part in parts)
            {
                var width = part.ToLower();
                if (string.IsNullOrEmpty(width))
                {
                    width = "*";
                }

                if (width.EndsWith("*"))
                {
                    string starWidth = width.Replace("*", string.Empty);
                    if (string.IsNullOrEmpty(starWidth))
                    {
                        starWidth = "1";
                    }

                    var stars = double.Parse(starWidth);
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(stars, GridUnitType.Star) });
                }
                else if (width == "auto")
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                }
                else
                {
                    var pixels = double.Parse(width);
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(pixels, GridUnitType.Pixel) });
                }
            }
        }

        /// <summary>
        /// Handler for row height changes
        /// </summary>
        /// <param name="d">Source object</param>
        /// <param name="e">Event arguments</param>
        private static void RowHeightsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as Grid;
            if (grid == null)
            {
                return;
            }

            grid.RowDefinitions.Clear();
            var heights = e.NewValue.ToString();
            var parts = heights.Split(',');
            foreach (var part in parts)
            {
                var height = part.ToLower();
                if (string.IsNullOrEmpty(height))
                {
                    height = "*";
                }

                if (height.EndsWith("*"))
                {
                    string starHeight = height.Replace("*", string.Empty);
                    if (string.IsNullOrEmpty(starHeight))
                    {
                        starHeight = "1";
                    }

                    var stars = double.Parse(starHeight);
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(stars, GridUnitType.Star) });
                }
                else if (height == "auto")
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                }
                else
                {
                    var pixels = double.Parse(height);
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(pixels, GridUnitType.Pixel) });
                }
            }
        }
    }
}
