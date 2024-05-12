namespace BackupUtilities.Wpf.ViewModels.Shared;

using System.Globalization;

/// <summary>
/// Utility class to convert the file size into a human readable string.
/// </summary>
public static class SizeUtility
{
    /// <summary>
    /// Converts the given size into the size string displayed on the UI.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    /// <returns>The string to display on the UI.</returns>
    public static string ToFileSizeString(this long size)
    {
        var bytesString = size.ToString("N0", CultureInfo.CurrentCulture);

        var kilobytes = size / 1024;
        var megabytes = kilobytes / 1024;
        var gigabytes = megabytes / 1024;

        if (gigabytes > 0)
        {
            return $"{gigabytes} GB ({bytesString} Bytes)";
        }

        if (megabytes > 0)
        {
            return $"{megabytes} MB ({bytesString} Bytes)";
        }

        if (kilobytes > 0)
        {
            return $"{kilobytes} KB ({bytesString} Bytes)";
        }

        return $"{bytesString} Bytes";
    }
}
