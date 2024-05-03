namespace BackupUtilities.Services;

using System;
using System.IO;
using System.Threading.Tasks;
using BackupUtilities.Data.Repositories;
using BackupUtilities.Services.Interfaces;

/// <summary>
/// Default implementation of <see cref="IProjectManager"/>.
/// </summary>
public class ProjectManager : IProjectManager
{
    private IBackupProject? _currentProject;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectManager"/> class.
    /// </summary>
    public ProjectManager()
    {
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? CurrentProjectChanged;

    /// <inheritdoc />
    public IBackupProject? CurrentProject
    {
        get
        {
            return _currentProject;
        }

        set
        {
            if (_currentProject != value)
            {
                _currentProject = value;
                CurrentProjectChanged?.Invoke(this, new EventArgs());
            }
        }
    }

    /// <inheritdoc />
    public async Task<IBackupProject?> OpenProjectAsync(string projectPath)
    {
        if (!Path.IsPathRooted(projectPath))
        {
            throw new ArgumentException("The argument must be an absolute path.", nameof(projectPath));
        }

        if (!File.Exists(projectPath))
        {
            throw new ArgumentException("The argument must point to an existing project file.", nameof(projectPath));
        }

        var data = new DbContextData(projectPath);

        await data.InitAsync();

        CurrentProject = await BackupProject.CreateBackupProjectAsync(data);

        return CurrentProject;
    }

    /// <inheritdoc />
    public async Task<IBackupProject?> CreateProjectAsync(string projectPath)
    {
        if (!Path.IsPathRooted(projectPath))
        {
            throw new ArgumentException("The argument must be an absolute path.", nameof(projectPath));
        }

        if (File.Exists(projectPath))
        {
            throw new ArgumentException("Cannot overwrite existing files.", nameof(projectPath));
        }

        var data = new DbContextData(projectPath);

        await data.InitAsync();

        CurrentProject = await BackupProject.CreateBackupProjectAsync(data);

        return CurrentProject;
    }
}
