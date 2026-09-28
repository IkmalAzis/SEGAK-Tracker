using System.Globalization;
using SegakTracker.Core.Models;

namespace SegakTracker.Core.Data;

public sealed class SqliteFitnessRecordRepository(SegakDatabase database) : IFitnessRecordRepository
{
    public async Task<int> AddAsync(FitnessRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var db = await database.GetConnectionAsync().ConfigureAwait(false);
        await db.InsertAsync(record).ConfigureAwait(false);
        return record.Id;
    }

    public async Task<IReadOnlyList<FitnessRecord>> GetAllAsync()
    {
        var db = await database.GetConnectionAsync().ConfigureAwait(false);
        return await db.Table<FitnessRecord>()
            .OrderBy(r => r.RecordedAtUtc)
            .ThenBy(r => r.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(int id)
    {
        var db = await database.GetConnectionAsync().ConfigureAwait(false);
        await db.DeleteAsync<FitnessRecord>(id).ConfigureAwait(false);
    }

    public async Task DeleteAllAsync()
    {
        var db = await database.GetConnectionAsync().ConfigureAwait(false);
        await db.DeleteAllAsync<FitnessRecord>().ConfigureAwait(false);
    }
}

public sealed class SqliteTrainingProgressRepository(SegakDatabase database, TimeProvider timeProvider) : ITrainingProgressRepository
{
    public async Task<IReadOnlySet<string>> GetCompletedTaskIdsAsync(DateOnly day)
    {
        var key = ToKey(day);
        var db = await database.GetConnectionAsync().ConfigureAwait(false);
        var rows = await db.Table<TaskCompletion>()
            .Where(c => c.Day == key)
            .ToListAsync()
            .ConfigureAwait(false);
        return rows.Select(r => r.TaskId).ToHashSet(StringComparer.Ordinal);
    }

    public async Task SaveDayAsync(DateOnly day, IEnumerable<string> scopeTaskIds, IEnumerable<string> completedTaskIds)
    {
        ArgumentNullException.ThrowIfNull(scopeTaskIds);
        ArgumentNullException.ThrowIfNull(completedTaskIds);

        var key = ToKey(day);
        var scope = scopeTaskIds.ToHashSet(StringComparer.Ordinal);
        var completed = completedTaskIds.Where(scope.Contains).ToHashSet(StringComparer.Ordinal);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var db = await database.GetConnectionAsync().ConfigureAwait(false);
        await db.RunInTransactionAsync(conn =>
        {
            foreach (var taskId in scope)
            {
                conn.Execute("DELETE FROM TaskCompletions WHERE TaskId = ? AND Day = ?", taskId, key);
            }

            foreach (var taskId in completed)
            {
                conn.Insert(new TaskCompletion { TaskId = taskId, Day = key, CompletedAtUtc = now });
            }
        }).ConfigureAwait(false);
    }

    public async Task DeleteAllAsync()
    {
        var db = await database.GetConnectionAsync().ConfigureAwait(false);
        await db.DeleteAllAsync<TaskCompletion>().ConfigureAwait(false);
    }

    private static string ToKey(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
