using SegakTracker.Core.Models;
using SQLite;

namespace SegakTracker.Core.Data;

/// <summary>
/// Owns the app's single SQLite connection. Tables are created lazily on first use so
/// start-up never blocks on disk I/O.
/// </summary>
public sealed class SegakDatabase : IAsyncDisposable
{
    public const string FileName = "segak_tracker.db3";

    private const SQLiteOpenFlags OpenFlags =
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex;

    private readonly string _databasePath;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    public SegakDatabase(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _databasePath = databasePath;
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
        {
            return _connection;
        }

        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_connection is null)
            {
                var connection = new SQLiteAsyncConnection(_databasePath, OpenFlags);
                await connection.CreateTableAsync<FitnessRecord>().ConfigureAwait(false);
                await connection.CreateTableAsync<TaskCompletion>().ConfigureAwait(false);
                _connection = connection;
            }

            return _connection;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync().ConfigureAwait(false);
            _connection = null;
        }

        _initLock.Dispose();
    }
}
