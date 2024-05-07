namespace BackupUtilities.Data.Interfaces;

using System.Data;

/// <summary>
/// Represents a single database.
/// </summary>
public interface IDbContextData : IDisposable
{
    /// <summary>
    /// Gets the connection to the database.
    /// </summary>
    public IDbConnection Connection { get; }

    /// <summary>
    /// Gets the repository to start and manage drive scans.
    /// </summary>
    public IScanRepository ScanRepository { get; }

    /// <summary>
    /// Gets the repository containing the settings of the database.
    /// </summary>
    public ISettingsRepository SettingsRepository { get; }

    /// <summary>
    /// Gets the repository to manipulate and search the stored folders of the databse.
    /// </summary>
    public IFolderRepository FolderRepository { get; }

    /// <summary>
    /// Gets the repository to manipulate and search the stored files of the database.
    /// </summary>
    public IFileRepository FileRepository { get; }

    /// <summary>
    /// Gets the repository to store and search the detected bitrot.
    /// </summary>
    public IBitRotRepository BitRotRepository { get; }

    /// <summary>
    /// Gets the repository to store and search the dectected orphaned files.
    /// </summary>
    public IOrphanedFileRepository OrphanedFileRepository { get; }
}
