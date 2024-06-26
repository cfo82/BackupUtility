namespace BackupUtilities.Services.Services;

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
    private readonly IFileSystemService _fileSystemService;
    private IBackupProject? _currentProject;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectManager"/> class.
    /// </summary>
    /// <param name="fileSystemService">The file system access.</param>
    public ProjectManager(
        IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
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
                _currentProject?.Dispose();
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

        if (!_fileSystemService.FileExists(projectPath))
        {
            throw new ArgumentException("The argument must point to an existing project file.", nameof(projectPath));
        }

        if (CurrentProject != null)
        {
            CloseProject();
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

        if (_fileSystemService.FileExists(projectPath))
        {
            throw new ArgumentException("Cannot overwrite existing files.", nameof(projectPath));
        }

        if (CurrentProject != null)
        {
            CloseProject();
        }

        var data = new DbContextData(projectPath);

        await data.InitAsync();

        CurrentProject = await BackupProject.CreateBackupProjectAsync(data);

        return CurrentProject;
    }

    /// <inheritdoc />
    public void CloseProject()
    {
        if (CurrentProject == null)
        {
            throw new InvalidOperationException("There is no project opened at the moment.");
        }

        CurrentProject = null;
    }
}
