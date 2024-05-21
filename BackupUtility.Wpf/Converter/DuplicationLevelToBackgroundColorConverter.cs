namespace BackupUtilities.Wpf.Converter;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using BackupUtilities.Data.Interfaces;

/// <summary>
/// Converter to convert between <c>bool</c> and <see cref="Visibility"/>.
/// </summary>
[ValueConversion(typeof(FolderDuplicationLevel), typeof(Brush))]
public class DuplicationLevelToBackgroundColorConverter : ConverterMarkupExtension<DuplicationLevelToBackgroundColorConverter>
{
    private static readonly SolidColorBrush _lightRedBrush = new SolidColorBrush(Color.FromArgb(255, 234, 211, 193));
    private static readonly SolidColorBrush _darkRedBrush = new SolidColorBrush(Color.FromArgb(255, 226, 151, 160));

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicationLevelToBackgroundColorConverter"/> class.
    /// </summary>
    public DuplicationLevelToBackgroundColorConverter()
    {
    }

    /// <inheritdoc />
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null && value is FolderDuplicationLevel)
        {
            var duplicationLevel = (FolderDuplicationLevel)value;
            switch (duplicationLevel)
            {
            case FolderDuplicationLevel.None:
                return Brushes.Transparent;

            case FolderDuplicationLevel.ContainsDuplicates:
                return _lightRedBrush;

            case FolderDuplicationLevel.EntireContentAreDuplicates:
                return _lightRedBrush;

            case FolderDuplicationLevel.HashIdenticalToOtherFolder:
                return _darkRedBrush;

            default:
                return Brushes.Transparent;
            }
        }

        return Brushes.Transparent;
    }

    /// <inheritdoc />
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Conversion from Brush to FolderDuplicationLevel is not supported");
    }
}
