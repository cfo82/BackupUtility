namespace BackupUtilities.Services.Services;

using System.Collections.Generic;
using System.IO;
using BackupUtilities.Services.Interfaces;

/// <summary>
/// Default implementation of <see cref="IFileSystemService"/>.
/// </summary>
public class FileSystemService : IFileSystemService
{
    /// <inheritdoc />
    public bool FileExists(string path)
    {
        return System.IO.File.Exists(path);
    }

    /// <inheritdoc />
    public Stream OpenFileToRead(string path)
    {
        return System.IO.File.OpenRead(path);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetDirectories(string folderPath)
    {
        return Directory.GetDirectories(folderPath);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetFiles(string folderPath)
    {
        return Directory.GetFiles(folderPath);
    }
}
