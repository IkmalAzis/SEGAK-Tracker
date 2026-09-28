using SQLite;

namespace SegakTracker.Core.Models;

/// <summary>One practice attempt of the full SEGAK battery.</summary>
[Table("FitnessRecords")]
public sealed class FitnessRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>When the attempt was recorded, stored in UTC.</summary>
    public DateTime RecordedAtUtc { get; set; }

    /// <summary>SQLite does not keep <see cref="DateTimeKind"/>, so restore it before converting.</summary>
    [Ignore]
    public DateTime RecordedAtLocal => DateTime.SpecifyKind(RecordedAtUtc, DateTimeKind.Utc).ToLocalTime();

    public Gender Gender { get; set; }

    public int Age { get; set; }

    /// <summary>Pulse per minute after the 3-minute step test.</summary>
    public int StepTestPulse { get; set; }

    public int PushUps { get; set; }

    public int PartialCurlUps { get; set; }

    public double SitAndReachCm { get; set; }

    public double GetValue(FitnessTest test) => test switch
    {
        FitnessTest.StepTest => StepTestPulse,
        FitnessTest.PushUp => PushUps,
        FitnessTest.PartialCurlUp => PartialCurlUps,
        FitnessTest.SitAndReach => SitAndReachCm,
        _ => throw new ArgumentOutOfRangeException(nameof(test), test, null),
    };
}
