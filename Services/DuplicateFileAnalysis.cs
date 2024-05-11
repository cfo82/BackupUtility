namespace BackupUtilities.Services;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default-Implementation of <see cref="IDuplicateFileAnalysis"/>.
/// </summary>
public class DuplicateFileAnalysis : IDuplicateFileAnalysis
{
    private readonly ILogger<DuplicateFileAnalysis> _logger;
    private readonly IProjectManager _projectManager;
    private readonly IScanStatus _scanStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFileAnalysis"/> class.
    /// </summary>
    /// <param name="logger">A new logger instance to be used.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    public DuplicateFileAnalysis(
        ILogger<DuplicateFileAnalysis> logger,
        IProjectManager projectManager,
        IScanStatusManager longRunningOperationManager)
    {
        _logger = logger;
        _projectManager = projectManager;
        _scanStatus = longRunningOperationManager.FullScanStatus.DuplicateFileAnalysisStatus;
    }

    /// <inheritdoc />
    public async Task RunDuplicateFileAnalysis()
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
                        var folderRepository = currentProject.Data.FolderRepository;
                        var fileRepository = currentProject.Data.FileRepository;

                        await currentScan.UpdateDuplicateFileAnalysisDataAsync(connection, false, DateTime.Now, null);

                        await RemoveAllDuplicationMarksAsync(connection, folderRepository, fileRepository);

                        await ProcessTreeAsync(connection, folderRepository, fileRepository);

                        await ProcessDuplicateFoldersAsync(connection, folderRepository);

                        await currentScan.UpdateDuplicateFileAnalysisDataAsync(connection, true, currentScan.Data.FolderScanStartDate, DateTime.Now);
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

    private async Task RemoveAllDuplicationMarksAsync(IDbConnection connection, IFolderRepository folderRepository, IFileRepository fileRepository)
    {
        using var transaction = connection.BeginTransaction();

        await _scanStatus.UpdateAsync("Remove duplication marks from files...", null);
        await fileRepository.RemoveAllDuplicateMarks(connection);

        await _scanStatus.UpdateAsync("Remove duplication marks from folders...", null);
        await folderRepository.RemoveAllDuplicateMarks(connection, DriveType.Working);

        transaction.Commit();
    }

    private async Task ProcessTreeAsync(
        IDbConnection connection,
        IFolderRepository folderRepository,
        IFileRepository fileRepository)
    {
        using var transaction = connection.BeginTransaction();

        var folderCount = await folderRepository.GetFolderCount(connection, DriveType.Working);
        var rootFolders = await folderRepository.GetRootFolders(connection, DriveType.Working);
        var hashesOfDuplicateFiles = (await fileRepository.FindHashesOfDuplicateFilesAsync(connection)).ToImmutableHashSet();
        foreach (var folder in rootFolders)
        {
            await ProcessTreeRecursiveAsync(connection, folderRepository, fileRepository, hashesOfDuplicateFiles, folder, folderCount, new FolderCounter { Index = 0 });
        }

        transaction.Commit();
    }

    private async Task ProcessTreeRecursiveAsync(
        IDbConnection connection,
        IFolderRepository folderRepository,
        IFileRepository fileRepository,
        ImmutableHashSet<string> hashesOfDuplicateFiles,
        Folder folder,
        long folderCount,
        FolderCounter folderCounter)
    {
        double percentage = folderCounter.Index / (double)(folderCount - 1);
        ++folderCounter.Index;
        var status = $"{folderCounter.Index} / {folderCount}: Scanning for duplicates and calculating folder hash...";
        await _scanStatus.UpdateAsync(status, percentage);
        _logger.LogInformation(status);

        var subFolders = await folderRepository.GetSubFoldersAsync(connection, folder);
        foreach (var subFolder in subFolders)
        {
            await ProcessTreeRecursiveAsync(connection, folderRepository, fileRepository, hashesOfDuplicateFiles, subFolder, folderCount, folderCounter);
        }

        var files = await fileRepository.EnumerateFilesByFolderAsync(connection, folder);
        foreach (var file in files)
        {
            if (hashesOfDuplicateFiles.Contains(file.Hash))
            {
                await fileRepository.MarkFileAsDuplicate(connection, file);
            }
        }

        await CalculateAndSaveDuplicationLevel(connection, folderRepository, folder, subFolders, files);

        await CalculateAndSaveFolderHashAsync(connection, folderRepository, folder, subFolders, files);
    }

    private async Task CalculateAndSaveDuplicationLevel(
        IDbConnection connection,
        IFolderRepository folderRepository,
        Folder folder,
        IEnumerable<Folder> subFolders,
        IEnumerable<File> files)
    {
        var allFilesAreDuplicates = files.All(f => f.IsDuplicate > 0);
        var hasDuplicates = files.Any(f => f.IsDuplicate > 0);

        var allSubFoldersAreDuplicates = subFolders.Any() && subFolders.All(f => f.IsDuplicate == FolderDuplicationLevel.EntireContentAreDuplicates);
        var subFoldersContainDuplicates = subFolders.Any() && subFolders.Any(f => f.IsDuplicate > 0);

        FolderDuplicationLevel duplicationLevel = FolderDuplicationLevel.None;
        if (hasDuplicates || subFoldersContainDuplicates)
        {
            duplicationLevel = FolderDuplicationLevel.ContainsDuplicates;
        }

        if ((!files.Any() || allFilesAreDuplicates) && (!subFolders.Any() || allSubFoldersAreDuplicates))
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
        IEnumerable<File> files)
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

        await folderRepository.SaveFolderHashAsync(connection, folder, hash);
    }

    private async Task ProcessDuplicateFoldersAsync(IDbConnection connection, IFolderRepository folderRepository)
    {
        using var transaction = connection.BeginTransaction();

        var foldersWithDuplicates = await folderRepository.FindDuplicateFoldersAsync(connection, DriveType.Working);

        foreach (var folder in foldersWithDuplicates)
        {
            await folderRepository.MarkFolderAsDuplicate(connection, folder, FolderDuplicationLevel.HashIdenticalToOtherFolder);
        }

        transaction.Commit();
    }

    /*private async Task EnumerateAndMarkDuplicates(
        IDbConnection connection,
        IFolderRepository folderRepository,
        IFileRepository fileRepository,
        List<FolderTreeNode> rootFolders,
        Dictionary<long, FolderTreeNode> folderSet)
    {
        using var transaction = connection.BeginTransaction();

        await _longRunningOperationManager.UpdateOperationAsync("Find duplicate files based on hash comparison...", null);
        var duplicates = await fileRepository.FindDuplicateFilesAsync(connection);
        long count = 0;
        foreach (var d in duplicates)
        {
            ++count;
        }

        await _longRunningOperationManager.UpdateOperationAsync("Store duplication status in database and build folder tree...", 0);
        long index = 0;
        foreach (var d in duplicates)
        {
            double percentage = (double)index / (double)count * 100;
            await _longRunningOperationManager.UpdateOperationAsync(percentage);
            ++index;

            foreach (var file in d.Files)
            {
                await fileRepository.MarkFileAsDuplicate(connection, file);

                await UpdateFolderTree(folderRepository, connection, file.ParentId, rootFolders, folderSet);
            }
        }

        await _longRunningOperationManager.UpdateOperationAsync("Compute duplication status for folders and store in db...", 0);
        var folderCounter = new FolderCounter { Index = 0 };
        foreach (var root in rootFolders)
        {
            await RunDuplicateFolderAnalysisRecursive(folderRepository, fileRepository, connection, root, folderSet.Count, folderCounter);
        }

        transaction.Commit();
    }

    private async Task<FolderTreeNode> UpdateFolderTree(
        IFolderRepository folderRepository,
        IDbConnection connection,
        long folderId,
        List<FolderTreeNode> rootFolders,
        Dictionary<long, FolderTreeNode> folderSet)
    {
        if (folderSet.TryGetValue(folderId, out var existingItem))
        {
            return existingItem;
        }

        var folder = await folderRepository.GetFolderAsync(connection, folderId);
        if (folder == null)
        {
            throw new InvalidOperationException($"Unable to find the folder with id {folderId}.");
        }

        FolderTreeNode? parentItem = null;
        if (folder.ParentId.HasValue)
        {
            parentItem = await UpdateFolderTree(folderRepository, connection, folder.ParentId.Value, rootFolders, folderSet);
        }

        var item = new FolderTreeNode(folder, parentItem);
        folderSet.Add(folderId, item);

        if (parentItem == null)
        {
            rootFolders.Add(item);
        }

        return item;
    }

    private async Task RunDuplicateFolderAnalysisRecursive(
        IFolderRepository folderRepository,
        IFileRepository fileRepository,
        IDbConnection connection,
        FolderTreeNode node,
        long folderCount,
        FolderCounter folderCounter)
    {
        foreach (var child in node.SubFolders)
        {
            await RunDuplicateFolderAnalysisRecursive(folderRepository, fileRepository, connection, child, folderCount, folderCounter);
        }

        double percentage = (double)folderCounter.Index / (double)folderCount * 100;
        ++folderCounter.Index;
        await _longRunningOperationManager.UpdateOperationAsync(percentage);

        var files = await fileRepository.EnumerateFilesByFolderAsync(connection, node.Folder);
        var allFilesAreDuplicates = files.All(f => f.IsDuplicate > 0);
        var hasDuplicates = files.Any(f => f.IsDuplicate > 0);

        var subFolders = await folderRepository.GetSubFoldersAsync(connection, node.Folder);
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

        await folderRepository.MarkFolderAsDuplicate(connection, node.Folder, duplicationLevel);
    }

    private async Task CalculateFolderHashes(
        IDbConnection connection,
        IFolderRepository folderRepository,
        IFileRepository fileRepository,
        List<FolderTreeNode> rootFolders,
        Dictionary<long, FolderTreeNode> folderSet)
    {
        using var transaction = connection.BeginTransaction();

        await _longRunningOperationManager.UpdateOperationAsync("Calculate folder hash values...", 0);

        var folderHashSet = new Dictionary<string, List<FolderTreeNode>>();
        var folderCounter = new FolderCounter { Index = 0 };

        foreach (var node in rootFolders)
        {
            await CalculateFolderHashesRecursive(folderRepository, fileRepository, connection, node, folderSet, folderHashSet, folderCounter);
        }

        foreach (var nodeList in folderHashSet.Values.Where(l => l.Count > 1))
        {
            foreach (var node in nodeList)
            {
                await folderRepository.MarkFolderAsDuplicate(connection, node.Folder, FolderDuplicationLevel.HashIdenticalToOtherFolder);
            }
        }

        transaction.Commit();
    }

    private async Task CalculateFolderHashesRecursive(
        IFolderRepository folderRepository,
        IFileRepository fileRepository,
        IDbConnection connection,
        FolderTreeNode node,
        Dictionary<long, FolderTreeNode> folderSet,
        Dictionary<string, List<FolderTreeNode>> folderHashSet,
        FolderCounter folderCounter)
    {
        foreach (var child in node.SubFolders)
        {
            await CalculateFolderHashesRecursive(folderRepository, fileRepository, connection, child, folderSet, folderHashSet, folderCounter);
        }

        double percentage = (double)folderCounter.Index / (double)folderSet.Count * 100;
        ++folderCounter.Index;
        await _longRunningOperationManager.UpdateOperationAsync(percentage);

        if (node.Folder.IsDuplicate != FolderDuplicationLevel.EntireContentAreDuplicates)
        {
            return;
        }

        var hashContent = new List<byte>();
        var files = await fileRepository.EnumerateFilesByFolderAsync(connection, node.Folder);
        foreach (var file in files)
        {
            var hashString = file.Hash;
            hashContent.AddRange(Enumerable
                        .Range(0, hashString.Length / 2)
                        .Select(i => hashString.Substring(i * 2, 2))
                        .Select(s => Convert.ToByte(s, 16)));
        }

        foreach (var subfolder in node.SubFolders)
        {
            var hashString = subfolder.Folder.Hash;
            hashContent.AddRange(Enumerable
                        .Range(0, hashString.Length / 2)
                        .Select(i => hashString.Substring(i * 2, 2))
                        .Select(s => Convert.ToByte(s, 16)));
        }

        using var folderHash = SHA512.Create();
        var folderChecksum = folderHash.ComputeHash(hashContent.ToArray());
        var hash = BitConverter.ToString(folderChecksum).Replace("-", string.Empty).ToLower();

        await folderRepository.SaveFolderHashAsync(connection, node.Folder, hash);

        if (!folderHashSet.ContainsKey(hash))
        {
            folderHashSet.Add(hash, new());
        }

        if (folderHashSet.TryGetValue(hash, out var nodeList))
        {
            nodeList.Add(node);
        }
    }

    private class FolderTreeNode
    {
        public FolderTreeNode(Folder folder, FolderTreeNode? parentNode)
        {
            Folder = folder;
            SubFolders = new();
            parentNode?.SubFolders.Add(this);
        }

        /// <summary>
        /// Gets the folder that his node represents.
        /// </summary>
        public Folder Folder { get; }

        /// <summary>
        /// Gets a list containing all subfolders of this folder.
        /// </summary>
        public List<FolderTreeNode> SubFolders { get; }
    }*/

    private class FolderCounter
    {
        public long Index { get; set; }
    }
}
