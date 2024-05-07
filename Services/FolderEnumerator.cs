namespace BackupUtilities.Services;

using System.Data;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default-Implementation of <see cref="IFolderEnumerator"/>.
/// </summary>
public class FolderEnumerator : IFolderEnumerator
{
    private readonly ILogger<FolderEnumerator> _logger;
    private readonly IProjectManager _projectManager;
    private readonly ILongRunningOperationManager _longRunningOperationManager;
    private readonly IErrorHandler _errorHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderEnumerator"/> class.
    /// </summary>
    /// <param name="logger">A new logger instance to be used.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    /// <param name="errorHandler">The error handler to use.</param>
    public FolderEnumerator(
        ILogger<FolderEnumerator> logger,
        IProjectManager projectManager,
        ILongRunningOperationManager longRunningOperationManager,
        IErrorHandler errorHandler)
    {
        _logger = logger;
        _projectManager = projectManager;
        _longRunningOperationManager = longRunningOperationManager;
        _errorHandler = errorHandler;
    }

    /// <inheritdoc />
    public async Task EnumerateFoldersAsync()
    {
        try
        {
            await _longRunningOperationManager.BeginOperationAsync("Folder Enumeration", ScanType.FolderScan);

            var cs = new TaskCompletionSource();

            await Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        if (_projectManager.CurrentProject == null || !_projectManager.CurrentProject.IsReady)
                        {
                            throw new InvalidOperationException("Project is not opened or not ready.");
                        }

                        var connection = _projectManager.CurrentProject.Data.Connection;
                        var settingsRepository = _projectManager.CurrentProject.Data.SettingsRepository;
                        var folderRepository = _projectManager.CurrentProject.Data.FolderRepository;

                        var settings = await settingsRepository.GetSettingsAsync(connection, null);

                        using (var transaction = connection.BeginTransaction())
                        {
                            await folderRepository.MarkAllFoldersAsUntouchedAsync(connection);
                            transaction.Commit();
                        }

                        using (var transaction = connection.BeginTransaction())
                        {
                            var rootPath = settings.RootPath;
                            var rootFolder = await folderRepository.SaveFullPathAsync(connection, rootPath, DriveType.Working);
                            await folderRepository.TouchFolderAsync(connection, rootFolder);

                            foreach (var subDirectory in Directory.GetDirectories(rootPath))
                            {
                                await EnumerateFoldersRecursiveAsync(connection, folderRepository, rootFolder, subDirectory, settings);
                            }

                            transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.Error = ex;
                    }

                    cs.SetResult();
                },
                TaskCreationOptions.LongRunning);

            await cs.Task;
        }
        finally
        {
            await _longRunningOperationManager.EndOperationAsync();
        }
    }

    private async Task EnumerateFoldersRecursiveAsync(
        IDbConnection connection,
        IFolderRepository folderRepository,
        Folder parentFolder,
        string path,
        Settings settings)
    {
        if (settings.IgnoredFolders.Any(d => string.Equals(d.Path, path)))
        {
            return;
        }

        var status = $"Enumerating Directory '{path}'.";
        _logger.LogInformation(status);
        await _longRunningOperationManager.UpdateOperationAsync(status, null);

        var folderName = Path.GetFileName(path);
        var currentFolder = await folderRepository.GetFolderAsync(connection, parentFolder.Id, folderName);
        if (currentFolder != null)
        {
            await folderRepository.TouchFolderAsync(connection, currentFolder);
        }
        else
        {
            currentFolder = new Folder
            {
                Id = 0,
                ParentId = parentFolder.Id,
                Name = folderName,
                Touched = 1,
            };
            await folderRepository.SaveFolderAsync(connection, currentFolder);
        }

        foreach (var subDirectory in Directory.GetDirectories(path))
        {
            await EnumerateFoldersRecursiveAsync(connection, folderRepository, currentFolder, subDirectory, settings);
        }
    }
}
