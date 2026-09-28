using SegakTracker.Core.Services;
using SegakTracker.Core.Training;
using SegakTracker.Core.ViewModels;

namespace SegakTracker.Tests.ViewModels;

public class TrainingViewModelTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 28, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 9, 28);

    private readonly InMemoryProgress _progress = new();
    private readonly InMemoryRecords _records = new();
    private readonly FakeDialogs _dialogs = new();
    private readonly FakeNavigation _navigation = new();
    private readonly FakeTimeProvider _clock = new(Now);

    private ExerciseDetailViewModel Detail() => new(_progress, _navigation, _dialogs, _clock);

    [Fact]
    public async Task DetailLoadsTodaysTicks()
    {
        _progress.Days[Today] = ["pushup.lift", "pushup.plank"];
        var vm = Detail();

        await vm.LoadAsync("pushup");

        Assert.Equal("Push up", vm.Title);
        Assert.Equal("5 tasks", vm.TaskCountText);
        Assert.Equal("2 of 5 completed", vm.CompletedText);
        Assert.Equal([true, true, false, false, false], vm.Tasks.Select(t => t.IsCompleted));
        Assert.False(vm.HasUnsavedChanges);
    }

    [Fact]
    public async Task TogglingUpdatesCountAndSaveNavigatesBack()
    {
        var vm = Detail();
        await vm.LoadAsync("pushup");

        vm.Tasks[2].IsCompleted = true;
        Assert.Equal("1 of 5 completed", vm.CompletedText);
        Assert.True(vm.HasUnsavedChanges);

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal(new HashSet<string> { "pushup.lunge" }, _progress.Days[Today]);
        Assert.False(vm.HasUnsavedChanges);
        Assert.Equal(1, _navigation.BackCount);
    }

    [Fact]
    public async Task UncheckingAndSavingRemovesCompletion()
    {
        _progress.Days[Today] = ["pushup.lift", "step.jog"];
        var vm = Detail();
        await vm.LoadAsync("pushup");

        vm.Tasks[0].IsCompleted = false;
        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal(new HashSet<string> { "step.jog" }, _progress.Days[Today]);
    }

    [Fact]
    public async Task TicksResetOnANewDay()
    {
        _progress.Days[Today] = ["pushup.lift"];
        _clock.Now = Now.AddDays(1);
        var vm = Detail();

        await vm.LoadAsync("pushup");

        Assert.All(vm.Tasks, t => Assert.False(t.IsCompleted));
    }

    [Fact]
    public async Task UnknownExerciseGoesBack()
    {
        var vm = Detail();

        await vm.LoadAsync("nope");

        Assert.Single(_dialogs.Alerts);
        Assert.Equal(1, _navigation.BackCount);
    }

    [Fact]
    public async Task BackWithUnsavedChangesAsks()
    {
        var vm = Detail();
        await vm.LoadAsync("pushup");
        vm.Tasks[0].IsCompleted = true;
        _dialogs.ConfirmAnswer = false;

        await vm.BackCommand.ExecuteAsync(null);

        Assert.Single(_dialogs.Confirms);
        Assert.Equal(0, _navigation.BackCount);

        _dialogs.ConfirmAnswer = true;
        await vm.BackCommand.ExecuteAsync(null);
        Assert.Equal(1, _navigation.BackCount);
    }

    [Fact]
    public async Task BackWithoutChangesDoesNotAsk()
    {
        var vm = Detail();
        await vm.LoadAsync("pushup");

        await vm.BackCommand.ExecuteAsync(null);

        Assert.Empty(_dialogs.Confirms);
        Assert.Equal(1, _navigation.BackCount);
    }

    [Fact]
    public async Task ProgramListShowsPerExerciseCounts()
    {
        _progress.Days[Today] = ["step.jog", "pushup.lift", "pushup.plank"];
        var vm = new TrainingProgramViewModel(_progress, _navigation, _dialogs, _clock);

        await vm.LoadAsync();

        Assert.Equal(TrainingCatalog.All.Count, vm.Exercises.Count);
        Assert.Equal("1 of 5 done today", vm.Exercises.Single(e => e.Id == "step").ProgressText);
        Assert.Equal("2 of 5 done today", vm.Exercises.Single(e => e.Id == "pushup").ProgressText);

        await vm.OpenExerciseCommand.ExecuteAsync(vm.Exercises[1]);
        Assert.Equal(Routes.ExerciseDetail, _navigation.Visited.Single());
        Assert.Equal(vm.Exercises[1].Id, _navigation.Parameters.Single()!["id"]);
    }

    [Fact]
    public async Task HomeSummarisesTodayAndSuggestsNextTask()
    {
        _progress.Days[Today] = TrainingCatalog.All[0].Tasks.Select(t => t.Id).ToHashSet();
        var vm = new HomeViewModel(_records, _progress, _navigation, _dialogs, _clock);

        await vm.LoadAsync();

        Assert.Equal($"5 of {TrainingCatalog.TotalTaskCount} completed", vm.DailySummaryText);
        Assert.Equal("pushup", vm.NextExerciseId);
        Assert.Equal("Next: Lift 5 kg - 10 minutes", vm.NextTaskText);
        Assert.Null(vm.LatestGrade);

        await vm.OpenNextTaskCommand.ExecuteAsync(null);
        Assert.Equal("pushup", _navigation.Parameters.Single()!["id"]);
    }

    [Fact]
    public async Task HomeCelebratesWhenEverythingIsDone()
    {
        _progress.Days[Today] = TrainingCatalog.All.SelectMany(e => e.Tasks).Select(t => t.Id).ToHashSet();
        await _records.AddAsync(Records.Make(Now.UtcDateTime));
        var vm = new HomeViewModel(_records, _progress, _navigation, _dialogs, _clock);

        await vm.LoadAsync();

        Assert.Null(vm.NextExerciseId);
        Assert.Equal(1, vm.DailyProgress);
        Assert.NotNull(vm.LatestGrade);
        Assert.StartsWith("Latest score:", vm.LatestResultText);
    }

    [Fact]
    public async Task OnboardingRemembersAndShowsApp()
    {
        var settings = new FakeSettings();
        var vm = new OnboardingViewModel(settings, _navigation);

        await vm.StartCommand.ExecuteAsync(null);

        Assert.True(settings.HasCompletedOnboarding);
        Assert.True(_navigation.ShowedMainApp);
    }
}
