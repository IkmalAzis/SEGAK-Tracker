namespace SegakTracker.Core.Scoring;

public enum SegakGrade
{
    A,
    B,
    C,
    D,
    E,
}

public static class SegakGrading
{
    public const int MinTotal = 4;
    public const int MaxTotal = 20;

    /// <summary>
    /// Converts a total score (4-20) to a grade. Primary pupils (age 12 and below) and
    /// secondary students use slightly different bands in the SEGAK guide.
    /// </summary>
    public static SegakGrade GradeFor(int total, int age)
    {
        if (total is < MinTotal or > MaxTotal)
        {
            throw new ArgumentOutOfRangeException(nameof(total), total, $"A SEGAK total is between {MinTotal} and {MaxTotal}.");
        }

        var isPrimary = age <= 12;
        return total switch
        {
            >= 18 => SegakGrade.A,
            >= 15 => SegakGrade.B,
            >= 12 => SegakGrade.C,
            >= 10 when !isPrimary => SegakGrade.C,
            >= 8 when isPrimary => SegakGrade.D,
            >= 7 when !isPrimary => SegakGrade.D,
            _ => SegakGrade.E,
        };
    }

    public static string Description(this SegakGrade grade) => grade switch
    {
        SegakGrade.A => "Excellent",
        SegakGrade.B => "Very good",
        SegakGrade.C => "Good",
        SegakGrade.D => "Fair",
        SegakGrade.E => "Needs more effort",
        _ => throw new ArgumentOutOfRangeException(nameof(grade), grade, null),
    };
}
