namespace SegakTracker.Core.Scoring;

/// <summary>
/// Cut-off values that separate the five SEGAK score bands for one test.
/// For "higher is better" tests each value is the minimum needed for that score;
/// for "lower is better" tests (step test pulse) each value is the maximum allowed.
/// Anything outside the score-2 cut-off gets a score of 1.
/// </summary>
public readonly record struct ScoreThresholds(double Score5, double Score4, double Score3, double Score2, bool LowerIsBetter)
{
    public static ScoreThresholds AtLeast(double score5, double score4, double score3, double score2) =>
        new(score5, score4, score3, score2, LowerIsBetter: false);

    public static ScoreThresholds AtMost(double score5, double score4, double score3, double score2) =>
        new(score5, score4, score3, score2, LowerIsBetter: true);

    public int ScoreFor(double value)
    {
        if (LowerIsBetter)
        {
            if (value <= Score5) return 5;
            if (value <= Score4) return 4;
            if (value <= Score3) return 3;
            if (value <= Score2) return 2;
            return 1;
        }

        if (value >= Score5) return 5;
        if (value >= Score4) return 4;
        if (value >= Score3) return 3;
        if (value >= Score2) return 2;
        return 1;
    }

    /// <summary>
    /// The measurement needed to reach <paramref name="score"/> (2-5), used to tell
    /// students how far they are from the next band.
    /// </summary>
    public double TargetFor(int score) => score switch
    {
        5 => Score5,
        4 => Score4,
        3 => Score3,
        2 => Score2,
        _ => throw new ArgumentOutOfRangeException(nameof(score), score, "Only scores 2-5 have a target."),
    };

    /// <summary>True when the cut-offs are strictly ordered, i.e. the table row is well formed.</summary>
    internal bool IsMonotonic => LowerIsBetter
        ? Score5 < Score4 && Score4 < Score3 && Score3 < Score2
        : Score5 > Score4 && Score4 > Score3 && Score3 > Score2;
}
