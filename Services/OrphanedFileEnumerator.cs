namespace BackupUtilities.Services;

using System;
using System.Data;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default-Implementation of <see cref="IOrphanedFileEnumerator"/>.
/// </summary>
public class OrphanedFileEnumerator : IOrphanedFileEnumerator
{
    private readonly ILogger<OrphanedFileEnumerator> _logger;
    private readonly IProjectManager _projectManager;
    private readonly ILongRunningOperationManager _longRunningOperationManager;
    private readonly IErrorHandler _errorHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrphanedFileEnumerator"/> class.
    /// </summary>
    /// <param name="logger">A new logger instance to be used.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    /// <param name="errorHandler">The error handler to use.</param>
    public OrphanedFileEnumerator(
        ILogger<OrphanedFileEnumerator> logger,
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
    public async Task EnumerateOrphanedFilesAsync()
    {
        try
        {
            await _longRunningOperationManager.BeginOperationAsync("Orphaned File Enumeration");

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
                        var orphanedFilesRepository = _projectManager.CurrentProject.Data.OrphanedFileRepository;

                        var settings = await settingsRepository.GetSettingsAsync(connection);

                        /*using (var transaction = connection.BeginTransaction())
                        {
                            await _longRunningOperationManager.UpdateOperationAsync("Enumerate orphaned files...", null);
                            await orphanedFilesRepository.DeleteAllAsync(connection);
                            await EnumerateOrphanedFilesRecursiveAsync(connection, folderRepository, orphanedFilesRepository, settings.MirrorPath, settings.RootPath, settings);
                            transaction.Commit();
                        }*/

                        using (var transaction = connection.BeginTransaction())
                        {
                            await _longRunningOperationManager.UpdateOperationAsync("Remove duplication marks from folders...", null);
                            await folderRepository.RemoveAllDuplicateMarks(connection, DriveType.Mirror);
                            transaction.Commit();
                        }

                        using (var transaction = connection.BeginTransaction())
                        {
                            await _longRunningOperationManager.UpdateOperationAsync("Scan for entire folders still residing on working drive...", null);
                            var folderCount = await folderRepository.GetFolderCount(connection, DriveType.Mirror);
                            var rootFolders = await folderRepository.GetRootFolders(connection, DriveType.Mirror);
                            foreach (var rootFolder in rootFolders)
                            {
                                await RunDuplicateFolderAnalysisRecursiveAsync(folderRepository, orphanedFilesRepository, connection, rootFolder, folderCount, new FolderCounter { Index = 0 });
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
        await _longRunningOperationManager.UpdateOperationAsync(status, null);

        foreach (var subDirectory in Directory.GetDirectories(mirrorPath))
        {
            await EnumerateOrphanedFilesRecursiveAsync(
                connection,
                folderRepository,
                orphanedFilesRepository,
                subDirectory,
                Path.Combine(workingPath, Path.GetFileName(subDirectory)),
                settings);
        }

        var files = Directory.GetFiles(mirrorPath);
        if (files.Length > 0)
        {
            foreach (var file in files)
            {
                await CheckFileAsync(
                    connection,
                    folderRepository,
                    orphanedFilesRepository,
                    file,
                    Path.Combine(workingPath, Path.GetFileName(file)));
            }
        }
    }

    private async Task CheckFileAsync(
        IDbConnection connection,
        IFolderRepository folderRepository,
        IOrphanedFileRepository orphanedFilesRepository,
        string mirrorFilePath,
        string workingFilePath)
    {
        if (!System.IO.File.Exists(workingFilePath))
        {
            _logger.LogError("Discovered orphaned file {EndangeredFile}.", mirrorFilePath);

            var hash = ComputeChecksum(mirrorFilePath);

            var directoryName = System.IO.Path.GetDirectoryName(mirrorFilePath);
            if (directoryName == null)
            {
                throw new InvalidOperationException("Unexpected null for directoryName.");
            }

            var fileName = System.IO.Path.GetFileName(mirrorFilePath);
            if (fileName == null)
            {
                throw new InvalidOperationException("Unexpected null for fileName.");
            }

            var folder = await folderRepository.SaveFullPathAsync(
                connection,
                directoryName,
                Data.Interfaces.DriveType.Mirror);

            await orphanedFilesRepository.SaveOrphanedFileAsync(
                connection,
                new OrphanedFile()
                {
                    ParentId = folder.Id,
                    Name = fileName,
                    Hash = hash,
                });
        }
    }

    private string ComputeChecksum(string file)
    {
        using var stream = new BufferedStream(System.IO.File.OpenRead(file), 1200000);
        using var sha = SHA512.Create();
        byte[] checksum = sha.ComputeHash(stream);
        var fullHash = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();

        return fullHash;
    }

    private async Task RunDuplicateFolderAnalysisRecursiveAsync(
        IFolderRepository folderRepository,
        IOrphanedFileRepository orphanedFileRepository,
        IDbConnection connection,
        Folder folder,
        long folderCount,
        FolderCounter folderCounter)
    {
        double percentage = (double)folderCounter.Index / (double)folderCount * 100;
        ++folderCounter.Index;
        await _longRunningOperationManager.UpdateOperationAsync(percentage);

        var subFolders = await folderRepository.GetSubFoldersAsync(connection, folder);
        foreach (var subFolder in subFolders)
        {
            await RunDuplicateFolderAnalysisRecursiveAsync(folderRepository, orphanedFileRepository, connection, subFolder, folderCount, folderCounter);
        }

        var files = await orphanedFileRepository.EnumerateOrphanedFilesByFolderAsync(connection, folder, false);

        await CalculateAndSaveDuplicationLevel(connection, folderRepository, folder, subFolders, files);

        await CalculateAndSaveFolderHashAsync(connection, folderRepository, folder, subFolders, files);
    }

    private async Task CalculateAndSaveDuplicationLevel(
        IDbConnection connection,
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

        await folderRepository.MarkFolderAsDuplicate(connection, folder, duplicationLevel);
    }

    private async Task CalculateAndSaveFolderHashAsync(
        IDbConnection connection,
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
        foreach (var file in files)
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

        await folderRepository.SaveFolderHashAsync(connection, folder, hash);

        var copiesOnWorkingDrive = await folderRepository.EnumerateDuplicatesOfFolder(connection, folder, DriveType.Working);

        if (copiesOnWorkingDrive.Any())
        {
            await folderRepository.MarkFolderAsDuplicate(connection, folder, FolderDuplicationLevel.HashIdenticalToOtherFolder);
        }
    }

    private class FolderCounter
    {
        public long Index { get; set; }
    }
}
