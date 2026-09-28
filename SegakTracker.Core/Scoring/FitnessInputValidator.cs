using System.Globalization;
using SegakTracker.Core.Models;

namespace SegakTracker.Core.Scoring;

/// <summary>Raw text from the "record your score" form.</summary>
public sealed record FitnessInput(
    Gender? Gender,
    string? Age,
    string? StepTestPulse,
    string? PushUps,
    string? PartialCurlUps,
    string? SitAndReachCm);

public enum FitnessField
{
    Gender,
    Age,
    StepTestPulse,
    PushUps,
    PartialCurlUps,
    SitAndReachCm,
}

public sealed class FitnessValidationResult
{
    internal FitnessValidationResult(IReadOnlyDictionary<FitnessField, string> errors, FitnessRecord? record)
    {
        Errors = errors;
        Record = record;
    }

    public IReadOnlyDictionary<FitnessField, string> Errors { get; }

    /// <summary>The parsed record (without id or timestamp) when the input is valid.</summary>
    public FitnessRecord? Record { get; }

    public bool IsValid => Record is not null;

    public string? ErrorFor(FitnessField field) => Errors.TryGetValue(field, out var message) ? message : null;
}

/// <summary>
/// Parses and range-checks form input. Limits are generous sanity bounds that reject typos
/// (e.g. a pulse of 9 or 900) while accepting any real attempt.
/// </summary>
public static class FitnessInputValidator
{
    public const int MinPulse = 40;
    public const int MaxPulse = 220;
    public const int MaxRepetitions = 150;
    public const double MaxSitAndReachCm = 100;

    public static FitnessValidationResult Validate(FitnessInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var errors = new Dictionary<FitnessField, string>();

        if (input.Gender is null)
        {
            errors[FitnessField.Gender] = "Choose male or female.";
        }

        var age = ParseInt(input.Age, SegakNorms.MinAge, SegakNorms.MaxAge, FitnessField.Age,
            $"SEGAK covers ages {SegakNorms.MinAge} to {SegakNorms.MaxAge} (Year 4 to Form 5).", errors);
        var pulse = ParseInt(input.StepTestPulse, MinPulse, MaxPulse, FitnessField.StepTestPulse,
            $"Enter a pulse between {MinPulse} and {MaxPulse} beats per minute.", errors);
        var pushUps = ParseInt(input.PushUps, 0, MaxRepetitions, FitnessField.PushUps,
            $"Enter a whole number from 0 to {MaxRepetitions}.", errors);
        var curlUps = ParseInt(input.PartialCurlUps, 0, MaxRepetitions, FitnessField.PartialCurlUps,
            $"Enter a whole number from 0 to {MaxRepetitions}.", errors);
        var reach = ParseCentimetres(input.SitAndReachCm, errors);

        if (errors.Count > 0)
        {
            return new FitnessValidationResult(errors, null);
        }

        var record = new FitnessRecord
        {
            Gender = input.Gender!.Value,
            Age = age!.Value,
            StepTestPulse = pulse!.Value,
            PushUps = pushUps!.Value,
            PartialCurlUps = curlUps!.Value,
            SitAndReachCm = reach!.Value,
        };
        return new FitnessValidationResult(errors, record);
    }

    private static int? ParseInt(string? text, int min, int max, FitnessField field, string rangeMessage,
        Dictionary<FitnessField, string> errors)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            errors[field] = "Required.";
            return null;
        }

        if (!int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var value))
        {
            errors[field] = "Enter a whole number.";
            return null;
        }

        if (value < min || value > max)
        {
            errors[field] = rangeMessage;
            return null;
        }

        return value;
    }

    private static double? ParseCentimetres(string? text, Dictionary<FitnessField, string> errors)
    {
        const FitnessField field = FitnessField.SitAndReachCm;
        if (string.IsNullOrWhiteSpace(text))
        {
            errors[field] = "Required.";
            return null;
        }

        // Accept both "31.5" and "31,5" regardless of the device's locale.
        var normalised = text.Trim().Replace(',', '.');
        if (!double.TryParse(normalised, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
            || double.IsNaN(value) || double.IsInfinity(value))
        {
            errors[field] = "Enter a number in centimetres, e.g. 31.5.";
            return null;
        }

        if (value < 0 || value > MaxSitAndReachCm)
        {
            errors[field] = $"Enter a distance from 0 to {MaxSitAndReachCm:0} cm.";
            return null;
        }

        return Math.Round(value, 1, MidpointRounding.AwayFromZero);
    }
}
