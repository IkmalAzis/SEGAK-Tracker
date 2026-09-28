using SegakTracker.Core.Models;
using SegakTracker.Core.Scoring;
using SegakTracker.Core.ViewModels;

namespace SegakTracker.Tests.ViewModels;

public class ProgressViewModelTests
{
    private static readonly DateTime Day1 = new(2026, 3, 1, 1, 0, 0, DateTimeKind.Utc);

    private readonly InMemoryRecords _records = new();
    private readonly FakeDialogs _dialogs = new();
    private readonly ProgressViewModel _vm;

    public ProgressViewModelTests()
    {
        _vm = new ProgressViewModel(_records, _dialogs, new FakeNavigation());
    }

    [Fact]
    public async Task EmptyStateWhenNoRecords()
    {
        await _vm.LoadAsync();

        Assert.True(_vm.IsEmpty);
        Assert.Empty(_vm.Rows);
        Assert.Empty(_vm.History);
    }

    [Fact]
    public async Task ComparesFirstAndLatestLikeTheDesign()
    {
        // The mockup: bench 80 -> 75 (+5), push up 24 -> 28 (+4), curl 11 -> 19 (+8), reach 26 -> 32 (+6).
        await _records.AddAsync(Records.Make(Day1, pulse: 80, pushUps: 24, curlUps: 11, reach: 26, age: 14));
        await _records.AddAsync(Records.Make(Day1.AddDays(7), pulse: 90, pushUps: 20, curlUps: 12, reach: 27, age: 14));
        await _records.AddAsync(Records.Make(Day1.AddDays(30), pulse: 75, pushUps: 28, curlUps: 19, reach: 32, age: 14));

        await _vm.LoadAsync();

        Assert.True(_vm.HasRecords);
        Assert.Equal(["(+5)", "(+4)", "(+8)", "(+6)"], _vm.Rows.Select(r => r.ChangeText));
        Assert.All(_vm.Rows, r => Assert.True(r.IsImproved));
        Assert.Equal("80", _vm.Rows[0].FirstValue);
        Assert.Equal("75", _vm.Rows[0].LatestValue);

        var expected = SegakScorer.Evaluate(_records.Items[^1]);
        Assert.Equal($"{expected.Total} Marks", _vm.ScoreText);
        Assert.Equal(expected.Grade.ToString(), _vm.Grade);

        Assert.Equal(3, _vm.History.Count);
        Assert.Equal(_records.Items[^1].Id, _vm.History[0].Id); // newest first
    }

    [Fact]
    public void WorsePulseShowsAsDecline()
    {
        var latest = SegakScorer.Evaluate(Records.Make(Day1, pulse: 110)).For(FitnessTest.StepTest);

        var row = ProgressViewModel.BuildRow(FitnessTest.StepTest, 100, latest);

        Assert.Equal("(-10)", row.ChangeText);
        Assert.True(row.IsDeclined);
        Assert.Contains("lower your pulse by 12 bpm", row.Hint);
    }

    [Fact]
    public void FractionalChangeIsFormatted()
    {
        var latest = SegakScorer.Evaluate(Records.Make(Day1, reach: 31.5)).For(FitnessTest.SitAndReach);

        var row = ProgressViewModel.BuildRow(FitnessTest.SitAndReach, 30, latest);

        Assert.Equal("(+1.5)", row.ChangeText);
        Assert.Equal("31.5", row.LatestValue);
    }

    [Fact]
    public async Task ResetAsksFirstAndKeepsDataOnCancel()
    {
        await _records.AddAsync(Records.Make(Day1));
        await _vm.LoadAsync();
        _dialogs.ConfirmAnswer = false;

        await _vm.ResetCommand.ExecuteAsync(null);

        Assert.Single(_dialogs.Confirms);
        Assert.Single(_records.Items);
        Assert.True(_vm.HasRecords);
    }

    [Fact]
    public async Task ResetDeletesEverythingWhenConfirmed()
    {
        await _records.AddAsync(Records.Make(Day1));
        await _records.AddAsync(Records.Make(Day1.AddDays(1)));
        await _vm.LoadAsync();
        _dialogs.ConfirmAnswer = true;

        await _vm.ResetCommand.ExecuteAsync(null);

        Assert.Empty(_records.Items);
        Assert.True(_vm.IsEmpty);
        Assert.Empty(_vm.Rows);
    }

    [Fact]
    public async Task ResetWithNothingSavedDoesNotPrompt()
    {
        await _vm.LoadAsync();

        await _vm.ResetCommand.ExecuteAsync(null);

        Assert.Empty(_dialogs.Confirms);
    }

    [Fact]
    public async Task DeleteSingleRecord()
    {
        await _records.AddAsync(Records.Make(Day1, pushUps: 10));
        await _records.AddAsync(Records.Make(Day1.AddDays(1), pushUps: 20));
        await _vm.LoadAsync();
        _dialogs.ConfirmAnswer = true;

        await _vm.DeleteRecordCommand.ExecuteAsync(_vm.History[0]);

        Assert.Equal(10, Assert.Single(_records.Items).PushUps);
        Assert.Single(_vm.History);
    }
}
