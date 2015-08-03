namespace DeepConfigEditor.Converters
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

	/// <summary>
	/// A value converter that converts values to their reverse or negated value.
	/// For Boolean values, this maps true to false and vice versa
	/// For Numbers it makes positive numbers negative and vice versa.
    /// Using it for other types will fail. For the sake of simplicity this converter expects correct usage.
	/// </summary>
	public sealed class NegateConverter : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            return DoConvert(value, targetType, parameter, culture);
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
            return DoConvert(value, targetType, parameter, culture);
		}

        private object DoConvert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (targetType != value.GetType())
            {
                throw new ArgumentOutOfRangeException("targetType", "NegateConverter can only convert to same type of value");
            }

            if (value is bool)
            {
                return !(bool)value;
            }

            return -1.0 * (double)value;
        }
	}
}
