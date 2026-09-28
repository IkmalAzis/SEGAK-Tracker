using SegakTracker.Core.Training;

namespace SegakTracker.Core.ViewModels;

/// <summary>An exercise tile on the home and training programme screens.</summary>
public sealed class ExerciseCardViewModel(Exercise exercise, int completedToday)
{
    public string Id => exercise.Id;

    public string Title => exercise.Title;

    public string Focus => exercise.Focus;

    public string ImageName => exercise.ImageName;

    public string AccentColor => exercise.AccentColor;

    public int CompletedToday { get; } = completedToday;

    public int TaskCount => exercise.Tasks.Count;

    public string TaskCountText => TaskCount == 1 ? "1 task" : $"{TaskCount} tasks";

    public string ProgressText => $"{CompletedToday} of {TaskCount} done today";

    public double Progress => TaskCount == 0 ? 0 : (double)CompletedToday / TaskCount;

    public bool IsDoneToday => CompletedToday == TaskCount;

    internal static IReadOnlyList<ExerciseCardViewModel> Build(IReadOnlySet<string> completedTaskIds) =>
        TrainingCatalog.All
            .Select(e => new ExerciseCardViewModel(e, e.Tasks.Count(t => completedTaskIds.Contains(t.Id))))
            .ToList();
}
