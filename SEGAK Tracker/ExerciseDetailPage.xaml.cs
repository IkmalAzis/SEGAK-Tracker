using SegakTracker.Core.ViewModels;

namespace SEGAK_Tracker;

/// <summary>Today's checklist for one exercise. Opened with the route "exerciseDetail?id=...".</summary>
public partial class ExerciseDetailPage : ContentPage, IQueryAttributable
{
    private readonly ExerciseDetailViewModel _viewModel;

    public ExerciseDetailPage(ExerciseDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue("id", out var id);
        await _viewModel.LoadAsync(id as string);
    }

    /// <summary>Android hardware back button: ask before dropping unsaved ticks.</summary>
    protected override bool OnBackButtonPressed()
    {
        if (!_viewModel.HasUnsavedChanges)
        {
            return base.OnBackButtonPressed();
        }

        _viewModel.BackCommand.Execute(null);
        return true;
    }

    private void OnTaskLabelTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: TrainingTaskItemViewModel task })
        {
            task.IsCompleted = !task.IsCompleted;
        }
    }
}
