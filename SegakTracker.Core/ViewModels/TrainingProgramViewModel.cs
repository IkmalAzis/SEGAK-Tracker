using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using SegakTracker.Core.Data;
using SegakTracker.Core.Services;

namespace SegakTracker.Core.ViewModels;

public sealed partial class TrainingProgramViewModel(
    ITrainingProgressRepository progress,
    INavigationService navigation,
    IDialogService dialogs,
    TimeProvider timeProvider) : ViewModelBase
{
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
            Exercises.Clear();
            foreach (var card in ExerciseCardViewModel.Build(completed))
            {
                Exercises.Add(card);
            }
        }
        catch (Exception)
        {
            await dialogs.ShowAlertAsync("Something went wrong", "Could not load training progress.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task OpenExerciseAsync(ExerciseCardViewModel? card) =>
        card is null
            ? Task.CompletedTask
            : navigation.GoToAsync(Routes.ExerciseDetail, new Dictionary<string, object> { ["id"] = card.Id });
}
