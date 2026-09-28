using SegakTracker.Core.Models;
using SegakTracker.Core.Scoring;

namespace SegakTracker.Tests.Scoring;

public class SegakNormsTests
{
    [Fact]
    public void TableCoversEveryAgeForBothGenders()
    {
        foreach (var gender in Enum.GetValues<Gender>())
        {
            for (var age = SegakNorms.MinAge; age <= SegakNorms.MaxAge; age++)
            {
                Assert.NotNull(SegakNorms.Get(gender, age));
            }
        }
    }

    [Fact]
    public void EveryRowIsStrictlyOrderedAndPointsTheRightWay()
    {
        foreach (var (gender, age, norms) in SegakNorms.All())
        {
            foreach (var test in FitnessTestInfo.All)
            {
                var thresholds = norms.For(test);
                Assert.True(thresholds.IsMonotonic, $"{gender} {age} {test} cut-offs are not strictly ordered");
                Assert.Equal(test.LowerIsBetter(), thresholds.LowerIsBetter);
            }
        }
    }

    [Fact]
    public void EveryScoreFromOneToFiveIsReachable()
    {
        foreach (var (_, _, norms) in SegakNorms.All())
        {
            foreach (var test in FitnessTestInfo.All)
            {
                var t = norms.For(test);
                var reached = new[] { t.Score5, t.Score4, t.Score3, t.Score2 }
                    .Select(t.ScoreFor)
                    .Append(t.ScoreFor(t.LowerIsBetter ? t.Score2 + 1 : t.Score2 - 1))
                    .ToHashSet();
                Assert.Equal(new HashSet<int> { 1, 2, 3, 4, 5 }, reached);
            }
        }
    }
}
