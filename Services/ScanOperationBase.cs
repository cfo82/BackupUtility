namespace BackupUtilities.Services;

using System;
using System.Threading.Tasks;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Services.Interfaces.Status;

/// <summary>
/// Base class for scan operations.
/// </summary>
public class ScanOperationBase
{
    private readonly IProjectManager _projectManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanOperationBase"/> class.
    /// </summary>
    /// <param name="projectManager">The project manager.</param>
    public ScanOperationBase(
        IProjectManager projectManager)
    {
        _projectManager = projectManager;
    }

    /// <summary>
    /// Run a scan inside its own task. This method is a utility and handles all the complexity involved with starting
    /// the new long-running background task and awaiting the result.
    /// </summary>
    /// <param name="scanStatus">The scan status object that represents this scan.</param>
    /// <param name="action">An action that is to be executed. This action then runs the 'real' code.</param>
    /// <returns>A task for async programming.</returns>
    protected async Task SpawnAndFinishLongRunningTaskAsync(IScanStatus scanStatus, Func<IBackupProject, IScan, Task> action)
    {
        try
        {
            await scanStatus.BeginAsync();

            var cs = new TaskCompletionSource();

            await Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        var currentProject = _projectManager.CurrentProject;
                        var currentScan = currentProject?.CurrentScan;

                        if (currentProject == null || !currentProject.IsReady)
                        {
                            throw new InvalidOperationException("Project is not opened or not ready.");
                        }

                        if (currentScan == null)
                        {
                            throw new InvalidOperationException("No current scan is available.");
                        }

                        await action.Invoke(currentProject, currentScan);
                    }
                    catch (Exception ex)
                    {
                        cs.SetException(ex);
                    }

                    cs.SetResult();
                },
                TaskCreationOptions.LongRunning);
            await cs.Task;
        }
        finally
        {
            await scanStatus.EndAsync();
        }
    }
}
