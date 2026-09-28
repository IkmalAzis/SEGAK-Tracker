using SQLite;

namespace SegakTracker.Core.Models;

/// <summary>Marks a training task as done on a given (local) day.</summary>
[Table("TaskCompletions")]
public sealed class TaskCompletion
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed(Name = "IX_TaskCompletions_Task_Day", Order = 1, Unique = true)]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>Local calendar day in ISO format (yyyy-MM-dd).</summary>
    [Indexed(Name = "IX_TaskCompletions_Task_Day", Order = 2, Unique = true)]
    public string Day { get; set; } = string.Empty;

    public DateTime CompletedAtUtc { get; set; }
}
