namespace BackupUtilities.Data.Repositories;

using System;
using System.Data;
using System.Globalization;
using Dapper;

/// <summary>
/// This type handler can convert between string and DateTime values for sqlite.
/// </summary>
public class SqliteDateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    /// <inheritdoc />
    public override DateTime Parse(object value)
    {
        if (!(value is string))
        {
            throw new ArgumentException("The argument must be a string.", nameof(value));
        }

        var dateTime = DateTime.ParseExact((string)value, "dd/MM/yyyy HH:mm:ss:fffffff", CultureInfo.InvariantCulture);
        DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return dateTime;
    }

    /// <inheritdoc />
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        DateTime.SpecifyKind(value, DateTimeKind.Utc);
        parameter.Value = value.ToString("dd/MM/yyyy HH:mm:ss:fffffff", CultureInfo.InvariantCulture);
    }
}
