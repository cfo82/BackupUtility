namespace BackupUtilities.Wpf.Converter;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

/// <summary>
/// Custom Markup-Extension for converters.
/// </summary>
/// <typeparam name="T">The converter type.</typeparam>
// http://www.broculos.net/2014/04/wpf-how-to-use-converters-without.html
public abstract class ConverterMarkupExtension<T> : MarkupExtension, IValueConverter
    where T : class, new()
{
    private static T? _converter = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConverterMarkupExtension{T}"/> class.
    /// </summary>
    public ConverterMarkupExtension()
    {
    }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _converter ??= new T();
    }

    /// <inheritdoc />
    public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

    /// <inheritdoc />
    public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
}
