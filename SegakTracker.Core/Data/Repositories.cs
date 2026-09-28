using SegakTracker.Core.Models;

namespace SegakTracker.Core.Data;

public interface IFitnessRecordRepository
{
    /// <summary>Saves a new record and returns its id.</summary>
    Task<int> AddAsync(FitnessRecord record);

    /// <summary>All records, oldest first.</summary>
    Task<IReadOnlyList<FitnessRecord>> GetAllAsync();

    Task DeleteAsync(int id);

    Task DeleteAllAsync();
}

public interface ITrainingProgressRepository
{
    Task<IReadOnlySet<string>> GetCompletedTaskIdsAsync(DateOnly day);

    /// <summary>
    /// Replaces the completion state of <paramref name="scopeTaskIds"/> on <paramref name="day"/>:
    /// tasks in <paramref name="completedTaskIds"/> become done, the rest of the scope becomes not done.
    /// Tasks outside the scope are left untouched.
    /// </summary>
    Task SaveDayAsync(DateOnly day, IEnumerable<string> scopeTaskIds, IEnumerable<string> completedTaskIds);

    Task DeleteAllAsync();
}
