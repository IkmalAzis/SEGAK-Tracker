using SegakTracker.Core.Data;
using SegakTracker.Core.Models;

namespace SegakTracker.Tests.Data;

public class FitnessRecordRepositoryTests : IAsyncLifetime
{
    private readonly TempDatabase _db = new();
    private SqliteFitnessRecordRepository _repo = null!;

    public Task InitializeAsync()
    {
        _repo = new SqliteFitnessRecordRepository(_db.Database);
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => _db.DisposeAsync().AsTask();

    [Fact]
    public async Task AddedRecordsRoundTripAllFields()
    {
        var when = new DateTime(2026, 3, 14, 8, 30, 0, DateTimeKind.Utc);
        var id = await _repo.AddAsync(Records.Make(when, pushUps: 22, pulse: 95, curlUps: 17, reach: 31.5, gender: Gender.Female, age: 15));

        var saved = Assert.Single(await _repo.GetAllAsync());
        Assert.Equal(id, saved.Id);
        Assert.True(id > 0);
        Assert.Equal(when, saved.RecordedAtUtc);
        Assert.Equal(Gender.Female, saved.Gender);
        Assert.Equal(15, saved.Age);
        Assert.Equal(95, saved.StepTestPulse);
        Assert.Equal(22, saved.PushUps);
        Assert.Equal(17, saved.PartialCurlUps);
        Assert.Equal(31.5, saved.SitAndReachCm);
    }

    [Fact]
    public async Task GetAllReturnsOldestFirst()
    {
        var day = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await _repo.AddAsync(Records.Make(day.AddDays(2), pushUps: 3));
        await _repo.AddAsync(Records.Make(day, pushUps: 1));
        await _repo.AddAsync(Records.Make(day.AddDays(1), pushUps: 2));

        var all = await _repo.GetAllAsync();

        Assert.Equal([1, 2, 3], all.Select(r => r.PushUps));
    }

    [Fact]
    public async Task DeleteRemovesOnlyThatRecord()
    {
        var day = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var first = await _repo.AddAsync(Records.Make(day));
        var second = await _repo.AddAsync(Records.Make(day.AddDays(1)));

        await _repo.DeleteAsync(first);

        Assert.Equal(second, Assert.Single(await _repo.GetAllAsync()).Id);
    }

    [Fact]
    public async Task DeleteAllEmptiesTable()
    {
        await _repo.AddAsync(Records.Make(DateTime.UtcNow));
        await _repo.AddAsync(Records.Make(DateTime.UtcNow));

        await _repo.DeleteAllAsync();

        Assert.Empty(await _repo.GetAllAsync());
    }

    [Fact]
    public async Task DataSurvivesReopeningTheDatabase()
    {
        var path = Path.Combine(Path.GetTempPath(), $"segak-reopen-{Guid.NewGuid():N}.db3");
        try
        {
            await using (var first = new SegakDatabase(path))
            {
                await new SqliteFitnessRecordRepository(first).AddAsync(Records.Make(DateTime.UtcNow, pushUps: 42));
            }

            await using var second = new SegakDatabase(path);
            var reloaded = Assert.Single(await new SqliteFitnessRecordRepository(second).GetAllAsync());
            Assert.Equal(42, reloaded.PushUps);
        }
        finally
        {
            File.Delete(path);
        }
    }
}

public class TrainingProgressRepositoryTests : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 9, 28);
    private readonly TempDatabase _db = new();
    private SqliteTrainingProgressRepository _repo = null!;

    public Task InitializeAsync()
    {
        _repo = new SqliteTrainingProgressRepository(_db.Database, new FakeTimeProvider(new DateTimeOffset(2026, 9, 28, 10, 0, 0, TimeSpan.Zero)));
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => _db.DisposeAsync().AsTask();

    [Fact]
    public async Task SaveDayStoresCompletedTasks()
    {
        await _repo.SaveDayAsync(Today, ["a", "b", "c"], ["a", "c"]);

        Assert.Equal(new HashSet<string> { "a", "c" }, await _repo.GetCompletedTaskIdsAsync(Today));
    }

    [Fact]
    public async Task SaveDayClearsUncheckedTasksInScopeOnly()
    {
        await _repo.SaveDayAsync(Today, ["a", "b"], ["a", "b"]);
        await _repo.SaveDayAsync(Today, ["x"], ["x"]);

        await _repo.SaveDayAsync(Today, ["a", "b"], ["b"]);

        Assert.Equal(new HashSet<string> { "b", "x" }, await _repo.GetCompletedTaskIdsAsync(Today));
    }

    [Fact]
    public async Task SavingTwiceDoesNotDuplicate()
    {
        await _repo.SaveDayAsync(Today, ["a"], ["a"]);
        await _repo.SaveDayAsync(Today, ["a"], ["a"]);

        Assert.Single(await _repo.GetCompletedTaskIdsAsync(Today));
    }

    [Fact]
    public async Task CompletedIdsOutsideScopeAreIgnored()
    {
        await _repo.SaveDayAsync(Today, ["a"], ["a", "not-in-scope"]);

        Assert.Equal(new HashSet<string> { "a" }, await _repo.GetCompletedTaskIdsAsync(Today));
    }

    [Fact]
    public async Task DaysAreIndependent()
    {
        await _repo.SaveDayAsync(Today, ["a"], ["a"]);

        Assert.Empty(await _repo.GetCompletedTaskIdsAsync(Today.AddDays(1)));
        Assert.Single(await _repo.GetCompletedTaskIdsAsync(Today));
    }

    [Fact]
    public async Task DeleteAllClearsEveryDay()
    {
        await _repo.SaveDayAsync(Today, ["a"], ["a"]);
        await _repo.SaveDayAsync(Today.AddDays(-1), ["a"], ["a"]);

        await _repo.DeleteAllAsync();

        Assert.Empty(await _repo.GetCompletedTaskIdsAsync(Today));
        Assert.Empty(await _repo.GetCompletedTaskIdsAsync(Today.AddDays(-1)));
    }
}
