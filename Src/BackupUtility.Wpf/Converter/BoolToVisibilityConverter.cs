namespace BackupUtilities.Wpf.Converter;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

/// <summary>
/// Converter to convert between <c>bool</c> and <see cref="Visibility"/>.
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : ConverterMarkupExtension<BoolToVisibilityConverter>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BoolToVisibilityConverter"/> class.
    /// </summary>
    public BoolToVisibilityConverter()
    {
    }

    /// <inheritdoc />
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null && value is bool)
        {
            bool actualVisibility = (bool)value;

            if (parameter is string && string.Equals(((string)parameter).ToLowerInvariant(), "true"))
            {
                actualVisibility = !actualVisibility;
            }

            return actualVisibility ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    /// <inheritdoc />
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Conversion from Visibility to bool is not supported");
    }
}
