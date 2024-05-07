namespace BackupUtilities.Services.Interfaces;

using System;

/// <summary>
/// The implementation of this interface is used to dispatch actions onto the UI thread.
/// </summary>
public interface IUiDispatcherService
{
    /// <summary>
    /// Post an action to be executed on the UI thread.
    /// </summary>
    /// <param name="action">The action to be run on the UI thread.</param>
    void Post(Action action);

    /// <summary>
    /// Check that we are currently running on the UI thread.
    /// </summary>
    void CheckUiThread();

    /// <summary>
    /// Tests if the current thread is the UI thread.
    /// </summary>
    /// <returns><c>true</c> if we are running on the UI thread; <c>false</c> otherwise.</returns>
    bool CheckAccess();
}
