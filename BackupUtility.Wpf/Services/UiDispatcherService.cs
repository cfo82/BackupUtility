namespace BackupUtilities.Wpf.Services;

using System;
using System.Threading;
using System.Windows;
using BackupUtilities.Services.Interfaces;

/// <summary>
/// Implementation of <see cref="IUiDispatcherService"/> for WPF.
/// </summary>
public class UiDispatcherService : IUiDispatcherService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UiDispatcherService"/> class.
    /// </summary>
    public UiDispatcherService()
    {
    }

    /// <inheritdoc />
    public void Post(Action action)
    {
        if (Application.Current != null)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
        }
    }

    /// <inheritdoc />
    public void CheckUiThread()
    {
        if (Application.Current.Dispatcher.Thread != Thread.CurrentThread)
        {
            throw new InvalidOperationException("Must be running on the UI thread.");
        }
    }
}
