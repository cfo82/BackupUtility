namespace BackupUtilities.Data.Interfaces;

/// <summary>
/// Repository to manage files.
/// </summary>
public interface IFileRepository : IRepository
{
    /// <summary>
    /// Find a file given its name and parent directory.
    /// </summary>
    /// <param name="parent">The parent folder.</param>
    /// <param name="name">The name of the file.</param>
    /// <returns>Instance of the file or NULL if not found.</returns>
    Task<File?> FindFileByNameAsync(Folder parent, string name);

    /// <summary>
    /// List all files from the given folder.
    /// </summary>
    /// <param name="parent">The parent folder.</param>
    /// <returns>A list of files.</returns>
    Task<IEnumerable<File>> EnumerateFilesByFolderAsync(Folder parent);

    /// <summary>
    /// Save the given file. Insert if it does not exist, update if it exists.
    /// </summary>
    /// <param name="file">The file to be saved.</param>
    /// <returns>A task for async processing.</returns>
    Task SaveFileAsync(File file);

    /// <summary>
    /// Mark all files in the repository as untouched.
    /// </summary>
    /// <returns>A task for async processing.</returns>
    Task MarkAllFilesAsUntouchedAsync();

    /// <summary>
    /// Mark the given file as touched.
    /// </summary>
    /// <param name="file">The file to be marked.</param>
    /// <returns>A task for async processing.</returns>
    Task TouchFileAsync(File file);

    /// <summary>
    /// Remove the duplicate flag from all files.
    /// </summary>
    /// <returns>A task for async processing.</returns>
    Task RemoveAllDuplicateMarks();

    /// <summary>
    /// Marks the given file as duplicate.
    /// </summary>
    /// <param name="file">The file that should be marked as duplicate.</param>
    /// <returns>A task for async programming.</returns>
    Task MarkFileAsDuplicate(File file);

    /// <summary>
    /// Search for duplicate files. This is accomplished using the file hash. This runs a query
    /// that compares the hash on the files and returns a DuplicateFiles entry for each matching hash.
    /// </summary>
    /// <returns>An enumeration of files that are duplicates.</returns>
    Task<IEnumerable<DuplicateFiles>> FindDuplicateFilesAsync();

    /// <summary>
    /// Query the hash values of all duplicate files.
    /// </summary>
    /// <returns>The list of hash values that appear multiple times within the Files table (=> duplicate files).</returns>
    Task<IEnumerable<string>> FindHashesOfDuplicateFilesAsync();

    /// <summary>
    /// Returns an enumeration of duplicate files based on the <see cref="File.IsDuplicate"/> flag.
    /// </summary>
    /// <returns>An enumeration of files that has IsDuplicate = 1.</returns>
    Task<IEnumerable<File>> EnumerateDuplicateFiles();

    /// <summary>
    /// Returns an enumeration of duplicates of the given file.
    /// </summary>
    /// /// <param name="file">The file for which duplicates are to be located.</param>
    /// <returns>An enumeration of files that are duplicates of the given file.</returns>
    Task<IEnumerable<File>> EnumerateDuplicatesOfFile(BaseFile file);

    /// <summary>
    /// Searches for files with the given hash and returns them.
    /// </summary>
    /// /// <param name="hash">The hash for which all files should be enuemrated.</param>
    /// <returns>An enumeration of files that are duplicates of the given file.</returns>
    Task<IEnumerable<File>> EnumerateDuplicatesWithHash(string hash);

    /// <summary>
    /// Delete all files from the repository.
    /// </summary>
    /// <returns>A task for async processing.</returns>
    Task DeleteAllAsync();
}
