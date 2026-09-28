using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SegakTracker.Core.Data;
using SegakTracker.Core.Scoring;
using SegakTracker.Core.Services;
using SegakTracker.Core.Training;

namespace SegakTracker.Core.ViewModels;

public sealed partial class HomeViewModel(
    IFitnessRecordRepository records,
    ITrainingProgressRepository progress,
    INavigationService navigation,
    IDialogService dialogs,
    TimeProvider timeProvider) : ViewModelBase
{
    [ObservableProperty]
    private string _dailySummaryText = string.Empty;

    [ObservableProperty]
    private double _dailyProgress;

    [ObservableProperty]
    private string _nextTaskText = string.Empty;

    [ObservableProperty]
    private string? _nextExerciseId;

    [ObservableProperty]
    private string _latestResultText = string.Empty;

    [ObservableProperty]
    private string? _latestGrade;

    public ObservableCollection<ExerciseCardViewModel> Exercises { get; } = [];

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var completed = await progress.GetCompletedTaskIdsAsync(Today(timeProvider));
            var done = TrainingCatalog.All.SelectMany(e => e.Tasks).Count(t => completed.Contains(t.Id));
            var total = TrainingCatalog.TotalTaskCount;
            DailySummaryText = $"{done} of {total} completed";
            DailyProgress = total == 0 ? 0 : (double)done / total;

            var next = TrainingCatalog.All
                .SelectMany(e => e.Tasks.Select(t => (Exercise: e, Task: t)))
                .FirstOrDefault(x => !completed.Contains(x.Task.Id));
            if (next.Task is null)
            {
                NextTaskText = "All done for today. Great work!";
                NextExerciseId = null;
            }
            else
            {
                NextTaskText = $"Next: {next.Task.Name} - {next.Task.Minutes} minutes";
                NextExerciseId = next.Exercise.Id;
            }

            Exercises.Clear();
            foreach (var card in ExerciseCardViewModel.Build(completed))
            {
                Exercises.Add(card);
            }

            var all = await records.GetAllAsync();
            if (all.Count == 0)
            {
                LatestGrade = null;
                LatestResultText = "No SEGAK score yet. Record your first attempt!";
            }
            else
            {
                var latest = SegakScorer.Evaluate(all[^1]);
                LatestGrade = latest.Grade.ToString();
                LatestResultText = $"Latest score: {latest.Total}/{SegakGrading.MaxTotal} ({latest.Grade.Description()})";
            }
        }
        catch (Exception)
        {
            await dialogs.ShowAlertAsync("Something went wrong", "Could not load your progress.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task OpenExerciseAsync(string? exerciseId) =>
        string.IsNullOrEmpty(exerciseId)
            ? navigation.GoToAsync("//" + Routes.TrainingProgram)
            : navigation.GoToAsync(Routes.ExerciseDetail, new Dictionary<string, object> { ["id"] = exerciseId });

    [RelayCommand]
    private Task OpenNextTaskAsync() => OpenExerciseAsync(NextExerciseId);

    [RelayCommand]
    private Task SeeAllAsync() => navigation.GoToAsync("//" + Routes.TrainingProgram);

    [RelayCommand]
    private Task RecordScoreAsync() => navigation.GoToAsync("//" + Routes.FitnessTracking);

    [RelayCommand]
    private Task ViewProgressAsync() => navigation.GoToAsync("//" + Routes.ProgressList);
}
