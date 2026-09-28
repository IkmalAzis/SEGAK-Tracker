using SegakTracker.Core.Data;
using SegakTracker.Core.Models;
using SegakTracker.Core.Services;

namespace SegakTracker.Tests.ViewModels;

public sealed class FakeSettings : ISettingsService
{
    public bool HasCompletedOnboarding { get; set; }
    public Gender? LastGender { get; set; }
    public int? LastAge { get; set; }
    public void Clear() => (HasCompletedOnboarding, LastGender, LastAge) = (false, null, null);
}

public sealed class FakeDialogs : IDialogService
{
    public List<(string Title, string Message)> Alerts { get; } = [];
    public List<(string Title, string Message)> Confirms { get; } = [];

    /// <summary>Answer returned by the next confirmation prompts.</summary>
    public bool ConfirmAnswer { get; set; }

    public Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        Alerts.Add((title, message));
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        Confirms.Add((title, message));
        return Task.FromResult(ConfirmAnswer);
    }
}

public sealed class FakeNavigation : INavigationService
{
    public List<string> Visited { get; } = [];
    public List<IDictionary<string, object>?> Parameters { get; } = [];
    public int BackCount { get; private set; }
    public bool ShowedMainApp { get; private set; }

    public Task ShowMainAppAsync()
    {
        ShowedMainApp = true;
        return Task.CompletedTask;
    }

    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        Visited.Add(route);
        Parameters.Add(parameters);
        return Task.CompletedTask;
    }

    public Task GoBackAsync()
    {
        BackCount++;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryRecords : IFitnessRecordRepository
{
    private int _nextId = 1;

    public List<FitnessRecord> Items { get; } = [];

    public bool FailWrites { get; set; }

    public Task<int> AddAsync(FitnessRecord record)
    {
        if (FailWrites)
        {
            throw new IOException("disk full");
        }

        record.Id = _nextId++;
        Items.Add(record);
        return Task.FromResult(record.Id);
    }

    public Task<IReadOnlyList<FitnessRecord>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<FitnessRecord>>(Items.OrderBy(r => r.RecordedAtUtc).ThenBy(r => r.Id).ToList());

    public Task DeleteAsync(int id)
    {
        Items.RemoveAll(r => r.Id == id);
        return Task.CompletedTask;
    }

    public Task DeleteAllAsync()
    {
        Items.Clear();
        return Task.CompletedTask;
    }
}

public sealed class InMemoryProgress : ITrainingProgressRepository
{
    public Dictionary<DateOnly, HashSet<string>> Days { get; } = [];

    public Task<IReadOnlySet<string>> GetCompletedTaskIdsAsync(DateOnly day) =>
        Task.FromResult<IReadOnlySet<string>>(Days.TryGetValue(day, out var set) ? new HashSet<string>(set) : []);

    public Task SaveDayAsync(DateOnly day, IEnumerable<string> scopeTaskIds, IEnumerable<string> completedTaskIds)
    {
        var scope = scopeTaskIds.ToHashSet();
        var set = Days.TryGetValue(day, out var existing) ? existing : Days[day] = [];
        set.RemoveWhere(scope.Contains);
        set.UnionWith(completedTaskIds.Where(scope.Contains));
        return Task.CompletedTask;
    }

    public Task DeleteAllAsync()
    {
        Days.Clear();
        return Task.CompletedTask;
    }
}
