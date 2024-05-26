namespace BackupUtilities.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Many file system operations in <see cref="System.IO"/> are static or direct classes.
/// This interfaces aims at abstracting away the functionality so that it may be mocked during
/// testing.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Check if the file at the given path exists.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>Returns a flag indicating whether a file exists at the given path or not.</returns>
    bool FileExists(string path);

    /// <summary>
    /// Open a file to read data.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The stream to read the file data from.</returns>
    Stream OpenFileToRead(string path);

    /// <summary>
    /// Get the files inside the given folder.
    /// </summary>
    /// <param name="folderPath">The path of the folder for which the files should be enumerated.</param>
    /// <returns>The files.</returns>
    IEnumerable<string> GetFiles(string folderPath);

    /// <summary>
    /// Get the subdirectories of the given folder.
    /// </summary>
    /// <param name="folderPath">The path of the folder for which the subdirectories should be enumerated.</param>
    /// <returns>The subdirectories.</returns>
    IEnumerable<string> GetDirectories(string folderPath);
}
