using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SegakTracker.Core.Data;
using SegakTracker.Core.Services;
using SegakTracker.Core.Training;

namespace SegakTracker.Core.ViewModels;

public sealed partial class TrainingTaskItemViewModel(TrainingTask task, bool isCompleted, Action onChanged) : ObservableObject
{
    public string Id => task.Id;

    public string Label => task.Label;

    [ObservableProperty]
    private bool _isCompleted = isCompleted;

    partial void OnIsCompletedChanged(bool value) => onChanged();
}

/// <summary>Today's checklist for one exercise programme.</summary>
public sealed partial class ExerciseDetailViewModel(
    ITrainingProgressRepository progress,
    INavigationService navigation,
    IDialogService dialogs,
    TimeProvider timeProvider) : ViewModelBase
{
    private Exercise? _exercise;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _tagline = string.Empty;

    [ObservableProperty]
    private string _focus = string.Empty;

    [ObservableProperty]
    private string _imageName = string.Empty;

    [ObservableProperty]
    private string _accentColor = "#8E24AA";

    [ObservableProperty]
    private string _taskCountText = string.Empty;

    [ObservableProperty]
    private string _completedText = string.Empty;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    public ObservableCollection<TrainingTaskItemViewModel> Tasks { get; } = [];

    public async Task LoadAsync(string? exerciseId)
    {
        _exercise = TrainingCatalog.Find(exerciseId);
        if (_exercise is null)
        {
            await dialogs.ShowAlertAsync("Not found", "That training programme does not exist.");
            await navigation.GoBackAsync();
            return;
        }

        IsBusy = true;
        try
        {
            Title = _exercise.Title;
            Tagline = _exercise.Tagline;
            Focus = _exercise.Focus;
            ImageName = _exercise.ImageName;
            AccentColor = _exercise.AccentColor;
            TaskCountText = _exercise.Tasks.Count == 1 ? "1 task" : $"{_exercise.Tasks.Count} tasks";

            var completed = await progress.GetCompletedTaskIdsAsync(Today(timeProvider));
            Tasks.Clear();
            foreach (var task in _exercise.Tasks)
            {
                Tasks.Add(new TrainingTaskItemViewModel(task, completed.Contains(task.Id), OnTaskToggled));
            }

            UpdateCompletedText();
            HasUnsavedChanges = false;
        }
        catch (Exception)
        {
            await dialogs.ShowAlertAsync("Something went wrong", "Could not load today's tasks.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_exercise is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await progress.SaveDayAsync(
                Today(timeProvider),
                _exercise.Tasks.Select(t => t.Id),
                Tasks.Where(t => t.IsCompleted).Select(t => t.Id));
            HasUnsavedChanges = false;
        }
        catch (Exception)
        {
            await dialogs.ShowAlertAsync("Could not save", "Your ticks were not saved. Please try again.");
            return;
        }
        finally
        {
            IsBusy = false;
        }

        await navigation.GoBackAsync();
    }

    /// <summary>Asks before discarding unsaved ticks. Returns true when it is fine to leave.</summary>
    public async Task<bool> ConfirmLeaveAsync() =>
        !HasUnsavedChanges
        || await dialogs.ConfirmAsync("Unsaved changes", "Leave without saving today's ticks?", "Leave", "Stay");

    [RelayCommand]
    private async Task BackAsync()
    {
        if (await ConfirmLeaveAsync())
        {
            HasUnsavedChanges = false;
            await navigation.GoBackAsync();
        }
    }

    private void OnTaskToggled()
    {
        HasUnsavedChanges = true;
        UpdateCompletedText();
    }

    private void UpdateCompletedText() =>
        CompletedText = $"{Tasks.Count(t => t.IsCompleted)} of {Tasks.Count} completed";
}
