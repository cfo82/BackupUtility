namespace BackupUtilities.Wpf.Converter;

using System;
using System.Globalization;
using System.Windows.Data;

/// <summary>
/// Converter to invert boolean values.
/// </summary>
[ValueConversion(typeof(bool), typeof(bool))]
public class InvertBoolConverter : ConverterMarkupExtension<InvertBoolConverter>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvertBoolConverter"/> class.
    /// </summary>
    public InvertBoolConverter()
    {
    }

    /// <inheritdoc />
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null && value is bool)
        {
            return !(bool)value;
        }

        return true;
    }

    /// <inheritdoc />
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}
