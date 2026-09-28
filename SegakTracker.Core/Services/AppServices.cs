using SegakTracker.Core.Models;

namespace SegakTracker.Core.Services;

/// <summary>Small key/value settings that survive restarts (backed by MAUI Preferences in the app).</summary>
public interface ISettingsService
{
    bool HasCompletedOnboarding { get; set; }

    /// <summary>Gender from the last saved record, used to pre-fill the form.</summary>
    Gender? LastGender { get; set; }

    /// <summary>Age from the last saved record, used to pre-fill the form.</summary>
    int? LastAge { get; set; }

    void Clear();
}

public interface IDialogService
{
    Task ShowAlertAsync(string title, string message, string cancel = "OK");

    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
}

public static class Routes
{
    public const string Home = "home";
    public const string FitnessTracking = "fitnessTracking";
    public const string TrainingProgram = "trainingProgram";
    public const string ProgressList = "progressList";
    public const string ExerciseDetail = "exerciseDetail";
}

public interface INavigationService
{
    /// <summary>Leaves onboarding and shows the main app shell.</summary>
    Task ShowMainAppAsync();

    /// <summary>Navigates to a route; absolute routes (e.g. "//progressList") switch flyout sections.</summary>
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);

    Task GoBackAsync();
}
