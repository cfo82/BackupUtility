namespace BackupUtilities.Wpf.Converter;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

/// <summary>
/// An implementation of <see cref="IValueConverter"/> that converts a boolean value indicating if something
/// is a duplicate into a color. <c>true</c> means a color; false means transparent.
/// </summary>
[ValueConversion(typeof(bool), typeof(Brush))]
public class DuplicatesBrushColorConverter : ConverterMarkupExtension<DuplicatesBrushColorConverter>
{
    private static readonly SolidColorBrush _darkRedBrush = new SolidColorBrush(Color.FromArgb(255, 226, 151, 160));

    /// <inheritdoc />
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            return _darkRedBrush;
        }

        return Brushes.Transparent;
    }

    /// <inheritdoc />
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
