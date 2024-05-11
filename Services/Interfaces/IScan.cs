namespace BackupUtilities.Services.Interfaces;

using System.Data;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;

/// <summary>
/// Represents a scan.
/// </summary>
public interface IScan
{
    /// <summary>
    /// Fired whenever scan data changes.
    /// </summary>
    event EventHandler<EventArgs>? Changed;

    /// <summary>
    /// Gets the settings that this scan uses.
    /// </summary>
    Settings Settings { get; }

    /// <summary>
    /// Gets the scan instance.
    /// </summary>
    Data.Interfaces.Scan Data { get; }

    /// <summary>
    /// Update the status of the overall full scan.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <param name="startDate">The date when the scan started.</param>
    /// <param name="finishedDate">The date when the scan finished.</param>
    /// <returns>A task for async programming.</returns>
    Task UpdateFullScanDataAsync(IDbConnection connection, DateTime? startDate, DateTime? finishedDate);

    /// <summary>
    /// Update the status flags about the folder scan.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <param name="finished">Flag indicating whether the scan has been finished.</param>
    /// <param name="startDate">The date when the scan started.</param>
    /// <param name="finishedDate">The date when the scan finished.</param>
    /// <returns>A task for async programming.</returns>
    Task UpdateFolderScanDataAsync(IDbConnection connection, bool finished, DateTime? startDate, DateTime? finishedDate);

    /// <summary>
    /// Update the status flags about the file scan.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <param name="initialized">Indicates whether the file scan has finished initializing.</param>
    /// <param name="finished">Flag indicating whether the scan has been finished.</param>
    /// <param name="startDate">The date when the scan started.</param>
    /// <param name="finishedDate">The date when the scan finished.</param>
    /// <returns>A task for async programming.</returns>
    Task UpdateFileScanDataAsync(IDbConnection connection, bool initialized, bool finished, DateTime? startDate, DateTime? finishedDate);

    /// <summary>
    /// Update the status flags about the duplicate file analysis.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <param name="finished">Flag indicating whether the scan has been finished.</param>
    /// <param name="startDate">The date when the scan started.</param>
    /// <param name="finishedDate">The date when the scan finished.</param>
    /// <returns>A task for async programming.</returns>
    Task UpdateDuplicateFileAnalysisDataAsync(IDbConnection connection, bool finished, DateTime? startDate, DateTime? finishedDate);

    /// <summary>
    /// Update the status flags about the orphaned file scan.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <param name="finished">Flag indicating whether the scan has been finished.</param>
    /// <param name="startDate">The date when the scan started.</param>
    /// <param name="finishedDate">The date when the scan finished.</param>
    /// <returns>A task for async programming.</returns>
    Task UpdateOrphanedFilesScanDataAsync(IDbConnection connection, bool finished, DateTime? startDate, DateTime? finishedDate);
}
