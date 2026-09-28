using SegakTracker.Core.Models;
using SegakTracker.Core.Scoring;

namespace SegakTracker.Tests.ViewModels;

public class FitnessInputValidatorTests
{
    private static FitnessInput Valid() => new(Gender.Male, "13", "90", "20", "15", "30");

    [Fact]
    public void ValidInputProducesRecord()
    {
        var result = FitnessInputValidator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(13, result.Record!.Age);
    }

    [Theory]
    [InlineData("31.5", 31.5)]
    [InlineData("31,5", 31.5)]
    [InlineData(" 30 ", 30)]
    [InlineData("30.25", 30.3)]
    [InlineData("0", 0)]
    public void SitAndReachAcceptsDecimalsInEitherNotation(string text, double expected)
    {
        var result = FitnessInputValidator.Validate(Valid() with { SitAndReachCm = text });

        Assert.Equal(expected, result.Record!.SitAndReachCm);
    }

    [Theory]
    [InlineData("9")]
    [InlineData("18")]
    [InlineData("12.5")]
    [InlineData("1e1")]
    [InlineData("")]
    public void AgeOutsideSegakRangeOrNotWholeIsRejected(string age)
    {
        var result = FitnessInputValidator.Validate(Valid() with { Age = age });

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorFor(FitnessField.Age));
    }

    [Theory]
    [InlineData("39")]
    [InlineData("221")]
    [InlineData("+90")]
    [InlineData("99999999999999")]
    public void ImplausiblePulseIsRejected(string pulse)
    {
        Assert.NotNull(FitnessInputValidator.Validate(Valid() with { StepTestPulse = pulse }).ErrorFor(FitnessField.StepTestPulse));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("151")]
    [InlineData("ten")]
    public void RepetitionsMustBeWholeAndInRange(string reps)
    {
        var result = FitnessInputValidator.Validate(Valid() with { PushUps = reps, PartialCurlUps = reps });

        Assert.NotNull(result.ErrorFor(FitnessField.PushUps));
        Assert.NotNull(result.ErrorFor(FitnessField.PartialCurlUps));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("100.1")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("1,000.5")]
    public void BadSitAndReachIsRejected(string text)
    {
        Assert.NotNull(FitnessInputValidator.Validate(Valid() with { SitAndReachCm = text }).ErrorFor(FitnessField.SitAndReachCm));
    }

    [Fact]
    public void MissingGenderIsRejected()
    {
        var result = FitnessInputValidator.Validate(Valid() with { Gender = null });

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorFor(FitnessField.Gender));
    }
}
