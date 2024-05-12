namespace BackupUtilities.Services;

using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default-Implementation of <see cref="IFolderEnumerator"/>.
/// </summary>
public class FolderEnumerator : ScanOperationBase, IFolderEnumerator
{
    private readonly ILogger<FolderEnumerator> _logger;
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
        : base(projectManager)
    {
        _logger = logger;
        _scanStatus = longRunningOperationManager.FullScanStatus.FolderScanStatus;
    }

    /// <inheritdoc />
    public async Task EnumerateFoldersAsync()
    {
        await SpawnAndFinishLongRunningTaskAsync(_scanStatus, async (currentProject, currentScan) =>
        {
            var connection = currentProject.Data.Connection;
            var settingsRepository = currentProject.Data.SettingsRepository;
            var folderRepository = currentProject.Data.FolderRepository;

            await currentScan.UpdateFolderScanDataAsync(connection, false, DateTime.Now, null);

            var settings = await settingsRepository.GetSettingsAsync(null);

            using (var transaction = connection.BeginTransaction())
            {
                await folderRepository.MarkAllFoldersAsUntouchedAsync();
                transaction.Commit();
            }

            using (var transaction = connection.BeginTransaction())
            {
                var rootPath = settings.RootPath;
                var rootFolder = await folderRepository.SaveFullPathAsync(rootPath, DriveType.Working);

                var fullPath = await folderRepository.GetFullPathForFolderAsync(rootFolder);
                foreach (var folder in fullPath)
                {
                    await folderRepository.TouchFolderAsync(folder);
                }

                foreach (var subDirectory in Directory.GetDirectories(rootPath))
                {
                    await EnumerateFoldersRecursiveAsync(folderRepository, rootFolder, subDirectory, settings);
                }

                transaction.Commit();
            }

            await currentScan.UpdateFolderScanDataAsync(connection, true, currentScan.Data.FolderScanStartDate, DateTime.Now);
        });
    }

    private async Task EnumerateFoldersRecursiveAsync(
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
        var currentFolder = await folderRepository.GetFolderAsync(parentFolder.Id, folderName);
        if (currentFolder != null)
        {
            await folderRepository.TouchFolderAsync(currentFolder);
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
            await folderRepository.SaveFolderAsync(currentFolder);
        }

        foreach (var subDirectory in Directory.GetDirectories(path))
        {
            await EnumerateFoldersRecursiveAsync(folderRepository, currentFolder, subDirectory, settings);
        }
    }
}
