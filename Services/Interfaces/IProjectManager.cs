namespace BackupUtilities.Services.Interfaces;

using System;
using System.Threading.Tasks;

/// <summary>
/// Manages the current instance of <see cref="IBackupProject"/>.
/// </summary>
public interface IProjectManager
{
    /// <summary>
    /// Event that is fired when <see cref="CurrentProject"/> changes.
    /// </summary>
    event EventHandler<EventArgs>? CurrentProjectChanged;

    /// <summary>
    /// Gets the current backup project.
    /// </summary>
    IBackupProject? CurrentProject { get; }

    /// <summary>
    /// Open a project.
    /// </summary>
    /// <param name="projectPath">The absolute path to the project.</param>
    /// <returns>The newly opened project.</returns>
    Task<IBackupProject?> OpenProjectAsync(string projectPath);

    /// <summary>
    /// Create a new project.
    /// </summary>
    /// <param name="projectPath">The absolute path to the project.</param>
    /// <returns>The newly created project.</returns>
    Task<IBackupProject?> CreateProjectAsync(string projectPath);
}
