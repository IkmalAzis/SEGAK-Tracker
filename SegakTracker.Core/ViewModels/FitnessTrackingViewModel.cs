using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SegakTracker.Core.Data;
using SegakTracker.Core.Models;
using SegakTracker.Core.Scoring;
using SegakTracker.Core.Services;

namespace SegakTracker.Core.ViewModels;

/// <summary>The "record your score" form.</summary>
public sealed partial class FitnessTrackingViewModel(
    IFitnessRecordRepository records,
    ISettingsService settings,
    IDialogService dialogs,
    INavigationService navigation,
    TimeProvider timeProvider) : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMale), nameof(IsFemale), nameof(PushUpLabel))]
    private Gender? _gender;

    [ObservableProperty] private string? _ageText;
    [ObservableProperty] private string? _pulseText;
    [ObservableProperty] private string? _pushUpsText;
    [ObservableProperty] private string? _curlUpsText;
    [ObservableProperty] private string? _sitAndReachText;

    [ObservableProperty] private string? _genderError;
    [ObservableProperty] private string? _ageError;
    [ObservableProperty] private string? _pulseError;
    [ObservableProperty] private string? _pushUpsError;
    [ObservableProperty] private string? _curlUpsError;
    [ObservableProperty] private string? _sitAndReachError;

    public bool IsMale
    {
        get => Gender == Models.Gender.Male;
        set
        {
            if (value)
            {
                Gender = Models.Gender.Male;
            }
        }
    }

    public bool IsFemale
    {
        get => Gender == Models.Gender.Female;
        set
        {
            if (value)
            {
                Gender = Models.Gender.Female;
            }
        }
    }

    /// <summary>Girls do the modified (knee) push-up in SEGAK.</summary>
    public string PushUpLabel => Gender == Models.Gender.Female ? "Push up (modified) :" : "Push up :";

    /// <summary>Pre-fills gender and age from the last saved attempt.</summary>
    public void Load()
    {
        Gender ??= settings.LastGender;
        if (string.IsNullOrWhiteSpace(AgeText) && settings.LastAge is { } age)
        {
            AgeText = age.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var validation = FitnessInputValidator.Validate(
            new FitnessInput(Gender, AgeText, PulseText, PushUpsText, CurlUpsText, SitAndReachText));
        ShowErrors(validation);
        if (validation.Record is not { } record)
        {
            return;
        }

        record.RecordedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        SegakResult result;
        IsBusy = true;
        try
        {
            await records.AddAsync(record);
            settings.LastGender = record.Gender;
            settings.LastAge = record.Age;
            result = SegakScorer.Evaluate(record);
        }
        catch (Exception)
        {
            await dialogs.ShowAlertAsync("Could not save", "Your score was not saved. Please try again.");
            return;
        }
        finally
        {
            IsBusy = false;
        }

        ClearScores();
        var viewProgress = await dialogs.ConfirmAsync("Score saved", BuildSummary(result), "View progress", "OK");
        if (viewProgress)
        {
            await navigation.GoToAsync("//" + Routes.ProgressList);
        }
    }

    [RelayCommand]
    private void Clear()
    {
        ClearScores();
        ShowErrors(null);
    }

    private void ClearScores()
    {
        PulseText = null;
        PushUpsText = null;
        CurlUpsText = null;
        SitAndReachText = null;
    }

    private void ShowErrors(FitnessValidationResult? validation)
    {
        GenderError = validation?.ErrorFor(FitnessField.Gender);
        AgeError = validation?.ErrorFor(FitnessField.Age);
        PulseError = validation?.ErrorFor(FitnessField.StepTestPulse);
        PushUpsError = validation?.ErrorFor(FitnessField.PushUps);
        CurlUpsError = validation?.ErrorFor(FitnessField.PartialCurlUps);
        SitAndReachError = validation?.ErrorFor(FitnessField.SitAndReachCm);
    }

    internal static string BuildSummary(SegakResult result)
    {
        var text = new StringBuilder()
            .Append("Your score: ").Append(result.Total).Append('/').Append(SegakGrading.MaxTotal).AppendLine()
            .Append("Grade: ").Append(result.Grade).Append(" (").Append(result.Grade.Description()).AppendLine(")")
            .AppendLine();

        foreach (var score in result.Scores)
        {
            text.Append(score.Test.DisplayName()).Append(": ").Append(score.Score).AppendLine("/5");
        }

        if (result.UsesEstimatedNorms)
        {
            text.AppendLine().Append("Note: the norms for this age are estimated and may differ slightly from the official SEGAK table.");
        }

        return text.ToString().TrimEnd();
    }
}
