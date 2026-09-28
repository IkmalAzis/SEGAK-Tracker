namespace SegakTracker.Core.Models;

/// <summary>The four scored items of the SEGAK fitness battery.</summary>
public enum FitnessTest
{
    /// <summary>Naik turun bangku (3-minute step test). Measured as pulse per minute; lower is better.</summary>
    StepTest,

    /// <summary>Tekan tubi (push-ups in 1 minute; modified push-ups for girls).</summary>
    PushUp,

    /// <summary>Ringkuk tubi separa (partial curl-ups in 1 minute).</summary>
    PartialCurlUp,

    /// <summary>Jangkauan melunjur (sit and reach), in centimetres.</summary>
    SitAndReach,
}

public static class FitnessTestInfo
{
    public static IReadOnlyList<FitnessTest> All { get; } =
        [FitnessTest.StepTest, FitnessTest.PushUp, FitnessTest.PartialCurlUp, FitnessTest.SitAndReach];

    public static string DisplayName(this FitnessTest test) => test switch
    {
        FitnessTest.StepTest => "Up & down the bench",
        FitnessTest.PushUp => "Push up",
        FitnessTest.PartialCurlUp => "Partial curl up",
        FitnessTest.SitAndReach => "Sit & reach",
        _ => throw new ArgumentOutOfRangeException(nameof(test), test, null),
    };

    public static string Unit(this FitnessTest test) => test switch
    {
        FitnessTest.StepTest => "bpm",
        FitnessTest.PushUp => "reps",
        FitnessTest.PartialCurlUp => "reps",
        FitnessTest.SitAndReach => "cm",
        _ => throw new ArgumentOutOfRangeException(nameof(test), test, null),
    };

    /// <summary>True when a smaller measurement is the better result (only the step test pulse).</summary>
    public static bool LowerIsBetter(this FitnessTest test) => test == FitnessTest.StepTest;
}
