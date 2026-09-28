using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SegakTracker.Core.Data;
using SegakTracker.Core.Models;
using SegakTracker.Core.Scoring;
using SegakTracker.Core.Services;

namespace SegakTracker.Core.ViewModels;

/// <summary>One test's first-vs-latest comparison, e.g. "Push up : 24 -> 28 (+4)".</summary>
public sealed record ProgressRow(
    string TestName,
    string FirstValue,
    string LatestValue,
    string ChangeText,
    bool IsImproved,
    bool IsDeclined,
    int LatestScore,
    string Hint);

public sealed record HistoryItem(int Id, string DateText, string Summary, string Grade, int Total);

public sealed partial class ProgressViewModel(
    IFitnessRecordRepository records,
    IDialogService dialogs,
    INavigationService navigation) : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _hasRecords;

    [ObservableProperty] private string _scoreText = string.Empty;
    [ObservableProperty] private string _grade = string.Empty;
    [ObservableProperty] private string _gradeDescription = string.Empty;
    [ObservableProperty] private string _comparisonCaption = string.Empty;
    [ObservableProperty] private bool _usesEstimatedNorms;

    public bool IsEmpty => !HasRecords;

    public ObservableCollection<ProgressRow> Rows { get; } = [];

    /// <summary>Every attempt, newest first.</summary>
    public ObservableCollection<HistoryItem> History { get; } = [];

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Populate(await records.GetAllAsync());
        }
        catch (Exception)
        {
            await dialogs.ShowAlertAsync("Something went wrong", "Could not load your saved scores.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        if (!HasRecords)
        {
            return;
        }

        var confirmed = await dialogs.ConfirmAsync(
            "Reset progress?",
            "This deletes every saved SEGAK score on this device. It cannot be undone.",
            "Delete all",
            "Cancel");
        if (!confirmed)
        {
            return;
        }

        try
        {
            await records.DeleteAllAsync();
        }
        catch (Exception)
        {
            await dialogs.ShowAlertAsync("Could not reset", "Your scores were not deleted. Please try again.");
        }

        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteRecordAsync(HistoryItem? item)
    {
        if (item is null)
        {
            return;
        }

        var confirmed = await dialogs.ConfirmAsync("Delete this score?", $"{item.DateText}: {item.Summary}", "Delete", "Cancel");
        if (!confirmed)
        {
            return;
        }

        try
        {
            await records.DeleteAsync(item.Id);
        }
        catch (Exception)
        {
            await dialogs.ShowAlertAsync("Could not delete", "The score was not deleted. Please try again.");
        }

        await LoadAsync();
    }

    [RelayCommand]
    private Task RecordScoreAsync() => navigation.GoToAsync("//" + Routes.FitnessTracking);

    internal void Populate(IReadOnlyList<FitnessRecord> all)
    {
        Rows.Clear();
        History.Clear();
        HasRecords = all.Count > 0;
        if (!HasRecords)
        {
            ScoreText = string.Empty;
            Grade = string.Empty;
            GradeDescription = string.Empty;
            ComparisonCaption = string.Empty;
            UsesEstimatedNorms = false;
            return;
        }

        var first = all[0];
        var latest = all[^1];
        var latestResult = SegakScorer.Evaluate(latest);

        ScoreText = $"{latestResult.Total} Marks";
        Grade = latestResult.Grade.ToString();
        GradeDescription = latestResult.Grade.Description();
        UsesEstimatedNorms = latestResult.UsesEstimatedNorms;
        ComparisonCaption = all.Count == 1
            ? "Your first attempt. Record another to see how you improve."
            : $"First attempt ({FormatDate(first)}) -> latest ({FormatDate(latest)})";

        foreach (var test in FitnessTestInfo.All)
        {
            Rows.Add(BuildRow(test, first.GetValue(test), latestResult.For(test)));
        }

        for (var i = all.Count - 1; i >= 0; i--)
        {
            var record = all[i];
            var result = i == all.Count - 1 ? latestResult : SegakScorer.Evaluate(record);
            History.Add(new HistoryItem(
                record.Id,
                FormatDate(record),
                $"{result.Total}/{SegakGrading.MaxTotal} - Grade {result.Grade} (age {record.Age})",
                result.Grade.ToString(),
                result.Total));
        }
    }

    internal static ProgressRow BuildRow(FitnessTest test, double firstValue, TestScore latest)
    {
        // Positive = better, so a pulse that drops from 80 to 75 shows as +5 like the design.
        var improvement = test.LowerIsBetter() ? firstValue - latest.Value : latest.Value - firstValue;
        var change = improvement switch
        {
            > 0 => "+" + FormatValue(improvement),
            < 0 => "-" + FormatValue(-improvement),
            _ => "0",
        };

        var hint = latest.PointsToNextScore is { } gap
            ? test.LowerIsBetter()
                ? $"Score {latest.Score}/5 - lower your pulse by {FormatValue(gap)} {test.Unit()} for {latest.Score + 1}"
                : $"Score {latest.Score}/5 - {FormatValue(gap)} more {test.Unit()} for {latest.Score + 1}"
            : "Score 5/5 - top band!";

        return new ProgressRow(
            test.DisplayName(),
            FormatValue(firstValue),
            FormatValue(latest.Value),
            $"({change})",
            improvement > 0,
            improvement < 0,
            latest.Score,
            hint);
    }

    private static string FormatDate(FitnessRecord record) =>
        record.RecordedAtLocal.ToString("d MMM yyyy", CultureInfo.CurrentCulture);
}
