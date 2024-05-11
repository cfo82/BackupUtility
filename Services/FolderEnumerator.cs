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
    private readonly IScanStatus _scanStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderEnumerator"/> class.
    /// </summary>
    /// <param name="logger">A new logger instance to be used.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    public FolderEnumerator(
        ILogger<FolderEnumerator> logger,
        IProjectManager projectManager,
        IScanStatusManager longRunningOperationManager)
    {
        _logger = logger;
        _projectManager = projectManager;
        _scanStatus = longRunningOperationManager.FullScanStatus.FolderScanStatus;
    }

    /// <inheritdoc />
    public async Task EnumerateFoldersAsync()
    {
        try
        {
            await _scanStatus.BeginAsync();

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

                        var connection = currentProject.Data.Connection;
                        var settingsRepository = currentProject.Data.SettingsRepository;
                        var folderRepository = currentProject.Data.FolderRepository;

                        await currentScan.UpdateFolderScanDataAsync(connection, false, DateTime.Now, null);

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

                            var fullPath = await folderRepository.GetFullPathForFolderAsync(connection, rootFolder);
                            foreach (var folder in fullPath)
                            {
                                await folderRepository.TouchFolderAsync(connection, folder);
                            }

                            foreach (var subDirectory in Directory.GetDirectories(rootPath))
                            {
                                await EnumerateFoldersRecursiveAsync(connection, folderRepository, rootFolder, subDirectory, settings);
                            }

                            transaction.Commit();
                        }

                        await currentScan.UpdateFolderScanDataAsync(connection, true, currentScan.Data.FolderScanStartDate, DateTime.Now);
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
            await _scanStatus.EndAsync();
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
        await _scanStatus.UpdateAsync(status, null);

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
