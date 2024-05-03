namespace BackupUtilities.Data.Interfaces;

using System.Data;
using System.Threading.Tasks;

/// <summary>
/// Base Interface for all repositories.
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Initialize the database with the data for this repository.
    /// </summary>
    /// <param name="connection">The connection on which the repository should be initialized.</param>
    /// <param name="version">The version for which it should be initialized.</param>
    /// <returns>An awaitable task.</returns>
    Task InitAsync(IDbConnection connection, int version);
}
