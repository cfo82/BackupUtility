namespace BackupUtilities.Services.Services.Scans;

using System;
using System.Data;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Services.Interfaces.Scans;
using BackupUtilities.Services.Interfaces.Status;
using BackupUtilities.Services.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default-Implementation of <see cref="IOrphanedFileScan"/>.
/// </summary>
public class OrphanedFileScan : ScanOperationBase, IOrphanedFileScan
{
    private readonly ILogger<OrphanedFileScan> _logger;
    private readonly IFileSystemService _fileSystemService;
    private readonly IScanStatus _scanStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrphanedFileScan"/> class.
    /// </summary>
    /// <param name="logger">A new logger instance to be used.</param>
    /// <param name="fileSystemService">The file system access.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    public OrphanedFileScan(
        ILogger<OrphanedFileScan> logger,
        IFileSystemService fileSystemService,
        IProjectManager projectManager,
        IScanStatusManager longRunningOperationManager)
        : base(projectManager)
    {
        _logger = logger;
        _fileSystemService = fileSystemService;
        _scanStatus = longRunningOperationManager.FullScanStatus.OrphanedFileScanStatus;
    }

    /// <inheritdoc />
    public async Task EnumerateOrphanedFilesAsync()
    {
        await SpawnAndFinishLongRunningTaskAsync(_scanStatus, async (currentProject, currentScan) =>
        {
            var connection = currentProject.Data.Connection;
            var settingsRepository = currentProject.Data.SettingsRepository;
            var folderRepository = currentProject.Data.FolderRepository;
            var orphanedFilesRepository = currentProject.Data.OrphanedFileRepository;

            await currentScan.UpdateOrphanedFilesScanDataAsync(connection, false, DateTime.Now, null);

            var settings = await settingsRepository.GetSettingsAsync(null);

            using (var transaction = connection.BeginTransaction())
            {
                await _scanStatus.UpdateAsync("Enumerate orphaned files...", null);
                await orphanedFilesRepository.DeleteAllAsync();
                await EnumerateOrphanedFilesRecursiveAsync(connection, folderRepository, orphanedFilesRepository, settings.MirrorPath, settings.RootPath, settings);
                transaction.Commit();
            }

            using (var transaction = connection.BeginTransaction())
            {
                await _scanStatus.UpdateAsync("Remove duplication marks from folders...", null);
                await folderRepository.RemoveAllDuplicateMarks(DriveType.Mirror);
                transaction.Commit();
            }

            using (var transaction = connection.BeginTransaction())
            {
                await _scanStatus.UpdateAsync("Scan for entire folders still residing on working drive...", null);
                var folderCount = await folderRepository.GetFolderCount(DriveType.Mirror);
                var rootFolders = await folderRepository.GetRootFolders(DriveType.Mirror);
                foreach (var rootFolder in rootFolders)
                {
                    await RunDuplicateFolderAnalysisRecursiveAsync(folderRepository, orphanedFilesRepository, rootFolder, folderCount, new FolderCounter { Index = 0 });
                }

                transaction.Commit();
            }

            await currentScan.UpdateOrphanedFilesScanDataAsync(connection, true, currentScan.Data.FolderScanStartDate, DateTime.Now);
        });
    }

    private async Task EnumerateOrphanedFilesRecursiveAsync(
        IDbConnection connection,
        IFolderRepository folderRepository,
        IOrphanedFileRepository orphanedFilesRepository,
        string mirrorPath,
        string workingPath,
        Settings settings)
    {
        if (settings.IgnoredFolders.Any(d => string.Equals(d.Path, mirrorPath)))
        {
            return;
        }

        var status = $"Searching Orphaned Files in Directory '{mirrorPath}'...";
        _logger.LogInformation(status);
        await _scanStatus.UpdateAsync(status, null);

        foreach (var subDirectory in _fileSystemService.GetDirectories(mirrorPath))
        {
            await EnumerateOrphanedFilesRecursiveAsync(
                connection,
                folderRepository,
                orphanedFilesRepository,
                subDirectory,
                Path.Combine(workingPath, Path.GetFileName(subDirectory)),
                settings);
        }

        var files = _fileSystemService.GetFiles(mirrorPath);
        if (files.Any())
        {
            foreach (var file in files)
            {
                await CheckFileAsync(
                    folderRepository,
                    orphanedFilesRepository,
                    file,
                    Path.Combine(workingPath, Path.GetFileName(file)));
            }
        }
    }

    private async Task CheckFileAsync(
        IFolderRepository folderRepository,
        IOrphanedFileRepository orphanedFilesRepository,
        string mirrorFilePath,
        string workingFilePath)
    {
        if (!_fileSystemService.FileExists(workingFilePath))
        {
            _logger.LogError("Discovered orphaned file {EndangeredFile}.", mirrorFilePath);

            var hash = ComputeChecksum(mirrorFilePath);

            var directoryName = Path.GetDirectoryName(mirrorFilePath);
            if (directoryName == null)
            {
                throw new InvalidOperationException("Unexpected null for directoryName.");
            }

            var fileName = Path.GetFileName(mirrorFilePath);
            if (fileName == null)
            {
                throw new InvalidOperationException("Unexpected null for fileName.");
            }

            var fileInfo = new FileInfo(mirrorFilePath);

            var folder = await folderRepository.SaveFullPathAsync(
                directoryName,
                DriveType.Mirror);

            var fullPath = await folderRepository.GetFullPathForFolderAsync(folder);
            foreach (var folderToMark in fullPath)
            {
                await folderRepository.TouchFolderAsync(folderToMark);
            }

            await orphanedFilesRepository.SaveOrphanedFileAsync(
                new OrphanedFile()
                {
                    ParentId = folder.Id,
                    Name = fileName,
                    Size = fileInfo.Length,
                    Hash = hash,
                });
        }
    }

    private string ComputeChecksum(string file)
    {
        using var stream = new BufferedStream(_fileSystemService.OpenFileToRead(file), 1200000);
        using var sha = SHA512.Create();
        byte[] checksum = sha.ComputeHash(stream);
        var fullHash = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();

        return fullHash;
    }

    private async Task RunDuplicateFolderAnalysisRecursiveAsync(
        IFolderRepository folderRepository,
        IOrphanedFileRepository orphanedFileRepository,
        Folder folder,
        long folderCount,
        FolderCounter folderCounter)
    {
        double percentage = folderCounter.Index / (double)(folderCount - 1);
        ++folderCounter.Index;
        await _scanStatus.UpdateAsync(percentage);

        var subFolders = await folderRepository.GetSubFoldersAsync(folder);
        foreach (var subFolder in subFolders)
        {
            await RunDuplicateFolderAnalysisRecursiveAsync(folderRepository, orphanedFileRepository, subFolder, folderCount, folderCounter);
        }

        var files = await orphanedFileRepository.EnumerateOrphanedFilesByFolderAsync(folder, false);

        await CalculateAndSaveDuplicationLevel(folderRepository, folder, subFolders, files);

        await CalculateAndSaveFolderHashAsync(folderRepository, folder, subFolders, files);
    }

    private async Task CalculateAndSaveDuplicationLevel(
        IFolderRepository folderRepository,
        Folder folder,
        IEnumerable<Folder> subFolders,
        IEnumerable<OrphanedFile> files)
    {
        var allFilesAreDuplicates = files.All(f => f.NumCopiesOnLiveDrive > 0);
        var hasDuplicates = files.Any(f => f.NumCopiesOnLiveDrive > 0);

        var allSubFoldersAreDuplicates = !subFolders.Any() || subFolders.All(f => f.IsDuplicate == FolderDuplicationLevel.EntireContentAreDuplicates);
        var subFoldersContainDuplicates = !subFolders.Any() || subFolders.Any(f => f.IsDuplicate > 0);

        FolderDuplicationLevel duplicationLevel = FolderDuplicationLevel.None;
        if (hasDuplicates || subFoldersContainDuplicates)
        {
            duplicationLevel = FolderDuplicationLevel.ContainsDuplicates;
        }

        if (allFilesAreDuplicates && allSubFoldersAreDuplicates && (files.Any() || subFolders.Any()))
        {
            duplicationLevel = FolderDuplicationLevel.EntireContentAreDuplicates;
        }

        await folderRepository.MarkFolderAsDuplicate(folder, duplicationLevel);
    }

    private async Task CalculateAndSaveFolderHashAsync(
        IFolderRepository folderRepository,
        Folder folder,
        IEnumerable<Folder> subFolders,
        IEnumerable<OrphanedFile> files)
    {
        if (!files.Any() && !subFolders.Any(s => !string.IsNullOrEmpty(s.Hash)))
        {
            return;
        }

        var hashContent = new List<byte>();
        foreach (var file in files.OrderBy(f => f.Hash))
        {
            var hashString = file.Hash;
            hashContent.AddRange(Enumerable
                        .Range(0, hashString.Length / 2)
                        .Select(i => hashString.Substring(i * 2, 2))
                        .Select(s => Convert.ToByte(s, 16)));
        }

        foreach (var subfolder in subFolders)
        {
            var hashString = subfolder.Hash;
            if (!string.IsNullOrEmpty(hashString))
            {
                hashContent.AddRange(Enumerable
                            .Range(0, hashString.Length / 2)
                            .Select(i => hashString.Substring(i * 2, 2))
                            .Select(s => Convert.ToByte(s, 16)));
            }
        }

        using var folderHash = SHA512.Create();
        var folderChecksum = folderHash.ComputeHash(hashContent.ToArray());
        var hash = BitConverter.ToString(folderChecksum).Replace("-", string.Empty).ToLower();

        await folderRepository.SaveFolderHashAsync(folder, hash);

        var copiesOnWorkingDrive = await folderRepository.EnumerateDuplicatesOfFolder(folder, DriveType.Working);

        if (copiesOnWorkingDrive.Any())
        {
            await folderRepository.MarkFolderAsDuplicate(folder, FolderDuplicationLevel.HashIdenticalToOtherFolder);
        }
    }

    private class FolderCounter
    {
        public long Index { get; set; }
    }
}
