using SegakTracker.Core.ViewModels;

namespace SEGAK_Tracker;

public partial class TrainingProgramPage : ContentPage
{
    private readonly TrainingProgramViewModel _viewModel;

    public TrainingProgramPage(TrainingProgramViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
