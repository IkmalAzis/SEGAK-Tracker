using SegakTracker.Core.Models;

namespace SegakTracker.Core.Scoring;

/// <summary>Score cut-offs for all four tests for one age and gender.</summary>
/// <param name="IsEstimated">
/// True when the row could not be confirmed against a published SEGAK table and was
/// interpolated from neighbouring ages. The UI shows a notice for these rows.
/// </param>
public sealed record AgeNorms(
    ScoreThresholds StepTest,
    ScoreThresholds PushUp,
    ScoreThresholds PartialCurlUp,
    ScoreThresholds SitAndReach,
    bool IsEstimated = false)
{
    public ScoreThresholds For(FitnessTest test) => test switch
    {
        FitnessTest.StepTest => StepTest,
        FitnessTest.PushUp => PushUp,
        FitnessTest.PartialCurlUp => PartialCurlUp,
        FitnessTest.SitAndReach => SitAndReach,
        _ => throw new ArgumentOutOfRangeException(nameof(test), test, null),
    };
}

/// <summary>
/// SEGAK norm tables (Standard Kecergasan Fizikal Kebangsaan, KPM) for ages 10-17.
/// </summary>
/// <remarks>
/// Values were compiled from publicly available copies of the KPM "Panduan SEGAK" norm
/// tables. Rows marked estimated were not available in full and are interpolated from the
/// neighbouring ages. If you have the official booklet, correct the numbers here; every
/// other part of the app reads from this one table.
/// </remarks>
public static class SegakNorms
{
    public const int MinAge = 10;
    public const int MaxAge = 17;

    // Shorthands to keep the table readable: pulse = at most, others = at least.
    private static ScoreThresholds Pulse(int s5, int s4, int s3, int s2) => ScoreThresholds.AtMost(s5, s4, s3, s2);
    private static ScoreThresholds Min(int s5, int s4, int s3, int s2) => ScoreThresholds.AtLeast(s5, s4, s3, s2);

    private static readonly Dictionary<int, AgeNorms> Male = new()
    {
        [10] = new(Pulse(79, 101, 125, 148), Min(15, 13, 9, 7), Min(18, 15, 11, 8), Min(37, 32, 25, 19)),
        [11] = new(Pulse(78, 101, 124, 147), Min(16, 13, 9, 7), Min(19, 16, 12, 8), Min(39, 32, 25, 18)),
        [12] = new(Pulse(77, 100, 123, 146), Min(18, 15, 11, 8), Min(20, 16, 12, 8), Min(39, 32, 25, 19)),
        [13] = new(Pulse(76, 98, 121, 143), Min(25, 21, 15, 11), Min(21, 17, 13, 9), Min(42, 34, 25, 16)),
        [14] = new(Pulse(76, 98, 121, 143), Min(27, 22, 17, 12), Min(22, 18, 13, 9), Min(44, 35, 26, 17)),
        [15] = new(Pulse(75, 97, 120, 142), Min(30, 24, 18, 13), Min(23, 19, 14, 10), Min(46, 37, 27, 17)),
        [16] = new(Pulse(74, 96, 119, 140), Min(30, 25, 19, 13), Min(23, 19, 14, 10), Min(47, 37, 27, 17)),
        [17] = new(Pulse(75, 97, 120, 142), Min(30, 24, 18, 13), Min(23, 19, 14, 10), Min(46, 37, 27, 17), IsEstimated: true),
    };

    private static readonly Dictionary<int, AgeNorms> Female = new()
    {
        [10] = new(Pulse(84, 108, 133, 158), Min(21, 17, 13, 9), Min(18, 15, 11, 8), Min(35, 30, 24, 18)),
        [11] = new(Pulse(84, 108, 132, 157), Min(21, 17, 13, 9), Min(19, 16, 12, 8), Min(37, 31, 25, 19), IsEstimated: true),
        [12] = new(Pulse(83, 107, 132, 156), Min(21, 18, 13, 9), Min(20, 16, 12, 8), Min(39, 32, 25, 19)),
        [13] = new(Pulse(82, 106, 130, 154), Min(21, 18, 14, 9), Min(21, 17, 12, 8), Min(39, 32, 26, 20), IsEstimated: true),
        [14] = new(Pulse(81, 104, 128, 152), Min(22, 19, 14, 10), Min(22, 18, 13, 9), Min(39, 33, 27, 21)),
        [15] = new(Pulse(80, 102, 126, 149), Min(23, 19, 14, 10), Min(20, 17, 12, 8), Min(40, 34, 28, 21), IsEstimated: true),
        [16] = new(Pulse(78, 100, 124, 146), Min(23, 19, 14, 10), Min(19, 16, 11, 8), Min(41, 35, 28, 21)),
        [17] = new(Pulse(78, 100, 124, 146), Min(23, 19, 14, 10), Min(19, 16, 11, 8), Min(41, 35, 28, 21), IsEstimated: true),
    };

    public static bool IsSupportedAge(int age) => age is >= MinAge and <= MaxAge;

    public static AgeNorms Get(Gender gender, int age)
    {
        if (!IsSupportedAge(age))
        {
            throw new ArgumentOutOfRangeException(nameof(age), age, $"SEGAK norms cover ages {MinAge}-{MaxAge}.");
        }

        return gender switch
        {
            Gender.Male => Male[age],
            Gender.Female => Female[age],
            _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, null),
        };
    }

    /// <summary>Every row in the table, for validation in tests.</summary>
    internal static IEnumerable<(Gender Gender, int Age, AgeNorms Norms)> All() =>
        Male.Select(kv => (Gender.Male, kv.Key, kv.Value))
            .Concat(Female.Select(kv => (Gender.Female, kv.Key, kv.Value)));
}
