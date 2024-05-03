namespace BackupUtilities.Wpf.Converter;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

/// <summary>
/// An implementation of <see cref="IValueConverter"/> that converts a boolean value indicating if something
/// is a duplicate into a color. <c>true</c> means a color; false means transparent.
/// </summary>
public class DuplicatesBrushColorConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            {
                return new SolidColorBrush(Colors.LightCoral);
            }
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
