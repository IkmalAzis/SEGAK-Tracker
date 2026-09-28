using SegakTracker.Core.Scoring;

namespace SegakTracker.Tests.Scoring;

public class SegakGradingTests
{
    [Theory]
    [InlineData(20, SegakGrade.A)]
    [InlineData(18, SegakGrade.A)]
    [InlineData(17, SegakGrade.B)]
    [InlineData(15, SegakGrade.B)]
    [InlineData(14, SegakGrade.C)]
    [InlineData(10, SegakGrade.C)]
    [InlineData(9, SegakGrade.D)]
    [InlineData(7, SegakGrade.D)]
    [InlineData(6, SegakGrade.E)]
    [InlineData(4, SegakGrade.E)]
    public void SecondaryBands(int total, SegakGrade expected)
    {
        Assert.Equal(expected, SegakGrading.GradeFor(total, age: 15));
    }

    [Theory]
    [InlineData(18, SegakGrade.A)]
    [InlineData(15, SegakGrade.B)]
    [InlineData(12, SegakGrade.C)]
    [InlineData(11, SegakGrade.D)]
    [InlineData(8, SegakGrade.D)]
    [InlineData(7, SegakGrade.E)]
    [InlineData(4, SegakGrade.E)]
    public void PrimaryBands(int total, SegakGrade expected)
    {
        Assert.Equal(expected, SegakGrading.GradeFor(total, age: 11));
    }

    [Fact]
    public void MockupExample_17MarksIsGradeB()
    {
        Assert.Equal(SegakGrade.B, SegakGrading.GradeFor(17, age: 14));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(21)]
    public void OutOfRangeTotal_Throws(int total)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SegakGrading.GradeFor(total, 13));
    }
}
