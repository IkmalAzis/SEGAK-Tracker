using SegakTracker.Core.Models;
using SegakTracker.Core.Scoring;

namespace SegakTracker.Tests.Scoring;

public class SegakScorerTests
{
    [Theory]
    // Boundaries of the 13-year-old boys' push-up row: 25+ / 21-24 / 15-20 / 11-14 / <=10.
    [InlineData(40, 5)]
    [InlineData(25, 5)]
    [InlineData(24, 4)]
    [InlineData(21, 4)]
    [InlineData(20, 3)]
    [InlineData(15, 3)]
    [InlineData(14, 2)]
    [InlineData(11, 2)]
    [InlineData(10, 1)]
    [InlineData(0, 1)]
    public void PushUp_Male13_UsesPublishedBands(int pushUps, int expected)
    {
        Assert.Equal(expected, SegakScorer.Score(Gender.Male, 13, FitnessTest.PushUp, pushUps));
    }

    [Theory]
    // Step test pulse: lower is better. 10-year-old boys: <=79 / 80-101 / 102-125 / 126-148 / >=149.
    [InlineData(60, 5)]
    [InlineData(79, 5)]
    [InlineData(80, 4)]
    [InlineData(101, 4)]
    [InlineData(102, 3)]
    [InlineData(125, 3)]
    [InlineData(126, 2)]
    [InlineData(148, 2)]
    [InlineData(149, 1)]
    [InlineData(190, 1)]
    public void StepTest_Male10_LowerPulseScoresHigher(int pulse, int expected)
    {
        Assert.Equal(expected, SegakScorer.Score(Gender.Male, 10, FitnessTest.StepTest, pulse));
    }

    [Theory]
    // 16-year-old girls' sit and reach: 41+ / 35-40 / 28-34 / 21-27 / <=20. Half centimetres round down a band.
    [InlineData(41, 5)]
    [InlineData(40.5, 4)]
    [InlineData(35, 4)]
    [InlineData(34.5, 3)]
    [InlineData(28, 3)]
    [InlineData(21, 2)]
    [InlineData(20.5, 1)]
    public void SitAndReach_Female16_HandlesFractionalCentimetres(double cm, int expected)
    {
        Assert.Equal(expected, SegakScorer.Score(Gender.Female, 16, FitnessTest.SitAndReach, cm));
    }

    [Fact]
    public void GenderSelectsDifferentNorms()
    {
        // 16 push-ups: a 10-year-old boy is in the top band, a 10-year-old girl is not.
        Assert.Equal(5, SegakScorer.Score(Gender.Male, 10, FitnessTest.PushUp, 16));
        Assert.Equal(3, SegakScorer.Score(Gender.Female, 10, FitnessTest.PushUp, 16));
    }

    [Theory]
    [InlineData(9)]
    [InlineData(18)]
    [InlineData(-1)]
    public void UnsupportedAge_Throws(int age)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SegakScorer.Score(Gender.Male, age, FitnessTest.PushUp, 10));
    }

    [Fact]
    public void Evaluate_SumsAllFourTestsAndGrades()
    {
        var record = new FitnessRecord
        {
            Gender = Gender.Male,
            Age = 13,
            StepTestPulse = 90,   // 77-98 => 4
            PushUps = 28,         // 25+ => 5
            PartialCurlUps = 18,  // 17-20 => 4
            SitAndReachCm = 30,   // 25-33 => 3
        };

        var result = SegakScorer.Evaluate(record);

        Assert.Equal(4, result.For(FitnessTest.StepTest).Score);
        Assert.Equal(5, result.For(FitnessTest.PushUp).Score);
        Assert.Equal(4, result.For(FitnessTest.PartialCurlUp).Score);
        Assert.Equal(3, result.For(FitnessTest.SitAndReach).Score);
        Assert.Equal(16, result.Total);
        Assert.Equal(SegakGrade.B, result.Grade);
        Assert.False(result.UsesEstimatedNorms);
    }

    [Fact]
    public void Evaluate_ReportsGapToNextBand()
    {
        var record = new FitnessRecord
        {
            Gender = Gender.Male,
            Age = 13,
            StepTestPulse = 100,  // 99-121 => 3, needs <=98 for 4 => 2 bpm lower
            PushUps = 25,         // already 5
            PartialCurlUps = 14,  // 13-16 => 3, needs 17 => 3 more
            SitAndReachCm = 33.5, // 25-33 => 3, needs 34 => 0.5 more
        };

        var result = SegakScorer.Evaluate(record);

        Assert.Equal(2, result.For(FitnessTest.StepTest).PointsToNextScore);
        Assert.Null(result.For(FitnessTest.PushUp).PointsToNextScore);
        Assert.Equal(3, result.For(FitnessTest.PartialCurlUp).PointsToNextScore);
        Assert.Equal(0.5, result.For(FitnessTest.SitAndReach).PointsToNextScore);
    }

    [Fact]
    public void Evaluate_FlagsEstimatedNorms()
    {
        var record = new FitnessRecord { Gender = Gender.Female, Age = 13, StepTestPulse = 90, PushUps = 10, PartialCurlUps = 10, SitAndReachCm = 30 };

        Assert.True(SegakScorer.Evaluate(record).UsesEstimatedNorms);
    }

    [Fact]
    public void Evaluate_NullRecord_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SegakScorer.Evaluate(null!));
    }
}
