namespace DeepConfigEditor.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using DeepConfigEditor.Config;

    public sealed class WindowStateConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (AppearanceConfig.StartState)value;

            if (state == AppearanceConfig.StartState.Normal)
            {
                return WindowState.Normal;
            }

            return (state == AppearanceConfig.StartState.Maximized) ? WindowState.Maximized : WindowState.Minimized;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (WindowState)value;

            if (state == WindowState.Normal)
            {
                return AppearanceConfig.StartState.Normal;
            }

            return (state == WindowState.Maximized) ? AppearanceConfig.StartState.Maximized : AppearanceConfig.StartState.Minimized;
        }
    }
}
