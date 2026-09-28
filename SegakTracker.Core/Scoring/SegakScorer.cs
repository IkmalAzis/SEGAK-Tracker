using SegakTracker.Core.Models;

namespace SegakTracker.Core.Scoring;

/// <summary>Score for a single test within a result.</summary>
/// <param name="PointsToNextScore">
/// How much the measurement must improve to reach the next score band, or null when already at 5.
/// Always positive: for the step test it is how many beats per minute lower the pulse must be.
/// </param>
public sealed record TestScore(FitnessTest Test, double Value, int Score, double? PointsToNextScore);

public sealed record SegakResult(IReadOnlyList<TestScore> Scores, int Total, SegakGrade Grade, bool UsesEstimatedNorms)
{
    public TestScore For(FitnessTest test) => Scores.First(s => s.Test == test);
}

public static class SegakScorer
{
    public static int Score(Gender gender, int age, FitnessTest test, double value) =>
        SegakNorms.Get(gender, age).For(test).ScoreFor(value);

    public static SegakResult Evaluate(FitnessRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var norms = SegakNorms.Get(record.Gender, record.Age);
        var scores = FitnessTestInfo.All
            .Select(test =>
            {
                var thresholds = norms.For(test);
                var value = record.GetValue(test);
                var score = thresholds.ScoreFor(value);
                return new TestScore(test, value, score, GapToNextScore(thresholds, value, score));
            })
            .ToList();

        var total = scores.Sum(s => s.Score);
        return new SegakResult(scores, total, SegakGrading.GradeFor(total, record.Age), norms.IsEstimated);
    }

    private static double? GapToNextScore(ScoreThresholds thresholds, double value, int score)
    {
        if (score >= 5)
        {
            return null;
        }

        var target = thresholds.TargetFor(score + 1);
        return thresholds.LowerIsBetter ? value - target : target - value;
    }
}
