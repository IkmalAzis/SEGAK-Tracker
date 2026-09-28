using SegakTracker.Core.Data;
using SegakTracker.Core.Models;

namespace SegakTracker.Tests;

/// <summary>A clock tests can set and advance.</summary>
public sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now.ToUniversalTime();

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

/// <summary>A throwaway SQLite database file per test.</summary>
public sealed class TempDatabase : IAsyncDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"segak-test-{Guid.NewGuid():N}.db3");

    public TempDatabase()
    {
        Database = new SegakDatabase(_path);
    }

    public SegakDatabase Database { get; }

    public async ValueTask DisposeAsync()
    {
        await Database.DisposeAsync();
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}

public static class Records
{
    public static FitnessRecord Make(DateTime recordedAtUtc, int pushUps = 20, int pulse = 100, int curlUps = 15, double reach = 30,
        Gender gender = Gender.Male, int age = 13) => new()
    {
        RecordedAtUtc = recordedAtUtc,
        Gender = gender,
        Age = age,
        StepTestPulse = pulse,
        PushUps = pushUps,
        PartialCurlUps = curlUps,
        SitAndReachCm = reach,
    };
}
