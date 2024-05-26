namespace BackupUtilities.Data.Interfaces;

using System;
using System.Globalization;

/// <summary>
/// Extension methods to convert between <see cref="DateTime"/> and the string value that can be stored
/// inside the sqlite database.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts the given dateTime into a string to be stored inside the database.
    /// </summary>
    /// <param name="dateTime">The date/time object to be stored.</param>
    /// <returns>A string that can be stored in the database.</returns>
    public static string ToSqlite(this DateTime dateTime)
    {
        return dateTime.ToString("dd/MM/yyyy HH:mm:ss:fffffff", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the string stored inside the database back into a <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="value">The string representation of the date.</param>
    /// <returns>The <see cref="DateTime"/> object.</returns>
    public static DateTime FromSqlite(this string value)
    {
        var dateTime = DateTime.ParseExact(value, "dd/MM/yyyy HH:mm:ss:fffffff", CultureInfo.InvariantCulture);
        DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return dateTime;
    }
}
