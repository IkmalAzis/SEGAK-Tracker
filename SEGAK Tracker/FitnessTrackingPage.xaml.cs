using SegakTracker.Core.ViewModels;

namespace SEGAK_Tracker;

public partial class FitnessTrackingPage : ContentPage
{
    private readonly FitnessTrackingViewModel _viewModel;

    public FitnessTrackingPage(FitnessTrackingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Load();
    }
}
