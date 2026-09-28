using SegakTracker.Core.Models;
using SegakTracker.Core.Training;

namespace SegakTracker.Tests.Training;

public class TrainingCatalogTests
{
    [Fact]
    public void EverySegakTestHasExactlyOneProgramme()
    {
        foreach (var test in FitnessTestInfo.All)
        {
            Assert.Single(TrainingCatalog.All, e => e.Test == test);
        }
    }

    [Fact]
    public void TaskIdsAreGloballyUnique()
    {
        var ids = TrainingCatalog.All.SelectMany(e => e.Tasks).Select(t => t.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void ExerciseIdsAreUniqueAndFindable()
    {
        foreach (var exercise in TrainingCatalog.All)
        {
            Assert.Same(exercise, TrainingCatalog.Find(exercise.Id));
        }

        Assert.Null(TrainingCatalog.Find("does-not-exist"));
        Assert.Null(TrainingCatalog.Find(null));
    }

    [Fact]
    public void TasksHavePositiveDurations()
    {
        Assert.All(TrainingCatalog.All.SelectMany(e => e.Tasks), t => Assert.True(t.Minutes > 0));
    }

    [Fact]
    public void TotalTaskCountMatches()
    {
        Assert.Equal(TrainingCatalog.All.Sum(e => e.Tasks.Count), TrainingCatalog.TotalTaskCount);
    }
}
