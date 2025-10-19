using System;

namespace Panacean.Data.Interface;

/// <summary>
/// Database manager interface - provides database initialization and access
/// </summary>
public interface IDatabaseManager : IDisposable
{
    /// <summary>
    /// Get database instance for direct operations
    /// </summary>
    /// <returns>FreeSql database instance</returns>
    IFreeSql GetDatabase();

    /// <summary>
    /// Database file path
    /// </summary>
    string DbFilePath { get; }
}
