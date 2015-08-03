﻿namespace DeepConfigEditor.Converters
{
	using System;
	using System.Collections;
	using System.Globalization;
	using System.Windows;
	using System.Windows.Data;

	/// <summary>
	/// A value converter that converts values into equivalent Visibility values.
	/// For Boolean values, this maps true/false into Visible/Collapsed.
	/// For String values, this maps null and String.Empty into Collapsed.
	/// For collection values, this maps empty collections into Collapsed.
	/// For other values, this maps null into Collapsed.
	/// 
	/// If the ConverterParameter is set to Inverse, then Visible is returned
	/// in place of Collapsed, and vice-versa.
	/// </summary>
	public sealed class VisibilityConverter : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(Visibility))
			{
				throw new ArgumentOutOfRangeException("targetType", "VisibilityConverter can only convert to Visibility");
			}

			Visibility visibility = Visibility.Visible;

			if (value == null)
			{
				visibility = Visibility.Collapsed;
			}
			else if (value is bool)
			{
				visibility = (bool)value ? Visibility.Visible : Visibility.Collapsed;
			}
			else if (value is string)
			{
                visibility = string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
			}
			else if (value is IEnumerable)
			{
				IEnumerable enumerable = (IEnumerable)value;
				if (enumerable.GetEnumerator().MoveNext() == false)
				{
					visibility = Visibility.Collapsed;
				}
			}

			if ((parameter is string) &&
                (string.Compare((string)parameter, "Inverse", StringComparison.OrdinalIgnoreCase) == 0))
			{
				visibility = (visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
			}

			return visibility;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Visibility))
			{
				throw new ArgumentOutOfRangeException("value", "VisibilityConverter can only convert from Visibility");
			}

			if (targetType == typeof(bool))
			{
				return ((Visibility)value == Visibility.Visible) ? true : false;
			}

			throw new ArgumentOutOfRangeException("targetType", "VisibilityConverter can only convert to Boolean");
		}
	}
}
