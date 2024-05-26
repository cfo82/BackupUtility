namespace BackupUtilities.Console;

using System;
using BackupUtilities.Services.Interfaces;

/// <summary>
/// Simple implementation of <see cref="IUiDispatcherService"/>.
/// </summary>
public class ConsoleDispatcherService : IUiDispatcherService
{
    /// <inheritdoc />
    public void CheckUiThread()
    {
    }

    /// <inheritdoc />
    public void Post(Action action)
    {
        action.Invoke();
    }

    /// <inheritdoc />
    public bool CheckAccess()
    {
        return true;
    }
}
