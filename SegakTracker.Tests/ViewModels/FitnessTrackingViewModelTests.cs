using SegakTracker.Core.Models;
using SegakTracker.Core.Scoring;
using SegakTracker.Core.Services;
using SegakTracker.Core.ViewModels;

namespace SegakTracker.Tests.ViewModels;

public class FitnessTrackingViewModelTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 28, 9, 0, 0, TimeSpan.Zero);

    private readonly InMemoryRecords _records = new();
    private readonly FakeSettings _settings = new();
    private readonly FakeDialogs _dialogs = new();
    private readonly FakeNavigation _navigation = new();
    private readonly FitnessTrackingViewModel _vm;

    public FitnessTrackingViewModelTests()
    {
        _vm = new FitnessTrackingViewModel(_records, _settings, _dialogs, _navigation, new FakeTimeProvider(Now));
    }

    private void FillValidForm()
    {
        _vm.IsMale = true;
        _vm.AgeText = "13";
        _vm.PulseText = "90";
        _vm.PushUpsText = "28";
        _vm.CurlUpsText = "18";
        _vm.SitAndReachText = "30";
    }

    [Fact]
    public async Task ValidFormIsSavedScoredAndCleared()
    {
        FillValidForm();

        await _vm.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(_records.Items);
        Assert.Equal(Gender.Male, saved.Gender);
        Assert.Equal(13, saved.Age);
        Assert.Equal(90, saved.StepTestPulse);
        Assert.Equal(28, saved.PushUps);
        Assert.Equal(18, saved.PartialCurlUps);
        Assert.Equal(30, saved.SitAndReachCm);
        Assert.Equal(Now.UtcDateTime, saved.RecordedAtUtc);

        var (title, message) = Assert.Single(_dialogs.Confirms);
        Assert.Equal("Score saved", title);
        Assert.Contains("16/20", message);
        Assert.Contains("Grade: B", message);

        // Scores are cleared for the next attempt; gender and age are kept.
        Assert.Null(_vm.PulseText);
        Assert.Null(_vm.PushUpsText);
        Assert.Equal("13", _vm.AgeText);
        Assert.True(_vm.IsMale);

        Assert.Equal(Gender.Male, _settings.LastGender);
        Assert.Equal(13, _settings.LastAge);
    }

    [Fact]
    public async Task ChoosingViewProgressNavigates()
    {
        FillValidForm();
        _dialogs.ConfirmAnswer = true;

        await _vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal(["//" + Routes.ProgressList], _navigation.Visited);
    }

    [Fact]
    public async Task InvalidFormShowsErrorsAndSavesNothing()
    {
        _vm.AgeText = "25";
        _vm.PulseText = "abc";
        _vm.PushUpsText = "";
        _vm.CurlUpsText = "-3";
        _vm.SitAndReachText = "500";

        await _vm.SaveCommand.ExecuteAsync(null);

        Assert.Empty(_records.Items);
        Assert.NotNull(_vm.GenderError);
        Assert.NotNull(_vm.AgeError);
        Assert.NotNull(_vm.PulseError);
        Assert.NotNull(_vm.PushUpsError);
        Assert.NotNull(_vm.CurlUpsError);
        Assert.NotNull(_vm.SitAndReachError);
        Assert.Empty(_dialogs.Confirms);
    }

    [Fact]
    public async Task ErrorsClearOnceFixed()
    {
        await _vm.SaveCommand.ExecuteAsync(null);
        Assert.NotNull(_vm.AgeError);

        FillValidForm();
        await _vm.SaveCommand.ExecuteAsync(null);

        Assert.Null(_vm.GenderError);
        Assert.Null(_vm.AgeError);
        Assert.Null(_vm.SitAndReachError);
    }

    [Fact]
    public async Task StorageFailureIsReportedAndFormKept()
    {
        FillValidForm();
        _records.FailWrites = true;

        await _vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Could not save", Assert.Single(_dialogs.Alerts).Title);
        Assert.Equal("28", _vm.PushUpsText);
        Assert.False(_vm.IsBusy);
    }

    [Fact]
    public void LoadPrefillsFromLastAttempt()
    {
        _settings.LastGender = Gender.Female;
        _settings.LastAge = 15;

        _vm.Load();

        Assert.True(_vm.IsFemale);
        Assert.False(_vm.IsMale);
        Assert.Equal("15", _vm.AgeText);
        Assert.Equal("Push up (modified) :", _vm.PushUpLabel);
    }

    [Fact]
    public void GenderRadioButtonsAreMutuallyExclusive()
    {
        _vm.IsMale = true;
        _vm.IsFemale = true;
        _vm.IsMale = false; // a radio button being unchecked must not clear the choice

        Assert.True(_vm.IsFemale);
        Assert.False(_vm.IsMale);
    }

    [Fact]
    public void ClearCommandResetsScoresAndErrors()
    {
        FillValidForm();
        _vm.AgeError = "x";

        _vm.ClearCommand.Execute(null);

        Assert.Null(_vm.PulseText);
        Assert.Null(_vm.SitAndReachText);
        Assert.Null(_vm.AgeError);
    }

    [Fact]
    public void SummaryMentionsEstimatedNorms()
    {
        var result = SegakScorer.Evaluate(Records.Make(DateTime.UtcNow, gender: Gender.Female, age: 13));

        Assert.Contains("estimated", FitnessTrackingViewModel.BuildSummary(result));
    }
}
