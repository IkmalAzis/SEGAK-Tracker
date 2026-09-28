using CommunityToolkit.Mvvm.Input;
using SegakTracker.Core.Services;

namespace SegakTracker.Core.ViewModels;

public sealed partial class OnboardingViewModel(ISettingsService settings, INavigationService navigation) : ViewModelBase
{
    [RelayCommand]
    private async Task StartAsync()
    {
        settings.HasCompletedOnboarding = true;
        await navigation.ShowMainAppAsync();
    }
}
