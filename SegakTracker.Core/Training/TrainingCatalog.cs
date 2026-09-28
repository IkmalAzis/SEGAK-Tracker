using SegakTracker.Core.Models;

namespace SegakTracker.Core.Training;

public sealed record TrainingTask(string Id, string Name, int Minutes)
{
    public string Label => $"{Name} - {Minutes} minutes";
}

/// <summary>A training programme that prepares for one SEGAK test.</summary>
public sealed record Exercise(
    string Id,
    FitnessTest Test,
    string Title,
    string Tagline,
    string Focus,
    string ImageName,
    string AccentColor,
    IReadOnlyList<TrainingTask> Tasks);

/// <summary>The built-in daily training programmes, one per SEGAK test.</summary>
public static class TrainingCatalog
{
    public static IReadOnlyList<Exercise> All { get; } =
    [
        new("step", FitnessTest.StepTest, "Up & down the bench", "Step up, heart strong!", "Cardiovascular endurance",
            "bench_exercise.png", "#E53935",
        [
            new("step.skipping", "Skipping rope", 10),
            new("step.jog", "Brisk walk or jog", 15),
            new("step.bench", "Step-up practice on a bench", 10),
            new("step.jacks", "Jumping jacks", 5),
            new("step.stairs", "Stair climbing", 10),
        ]),
        new("pushup", FitnessTest.PushUp, "Push up", "Push up, power up!", "Muscle strength & endurance",
            "push_up.png", "#8E24AA",
        [
            new("pushup.lift", "Lift 5 kg", 10),
            new("pushup.plank", "Plank", 10),
            new("pushup.lunge", "Lunge", 20),
            new("pushup.squat", "Squat", 20),
            new("pushup.crunch", "Crunch", 10),
        ]),
        new("curlup", FitnessTest.PartialCurlUp, "Partial curl up", "Core on, curl on!", "Abdominal strength & endurance",
            "curl_up.png", "#2E7D32",
        [
            new("curlup.practice", "Partial curl-up practice", 10),
            new("curlup.plank", "Plank", 5),
            new("curlup.legraise", "Leg raises", 10),
            new("curlup.bicycle", "Bicycle crunch", 10),
            new("curlup.deadbug", "Dead bug", 5),
        ]),
        new("sitreach", FitnessTest.SitAndReach, "Sit & reach", "Stretch further every day!", "Flexibility",
            "sit_and_reach.png", "#D81B60",
        [
            new("sitreach.hamstring", "Hamstring stretch", 5),
            new("sitreach.butterfly", "Butterfly stretch", 5),
            new("sitreach.toetouch", "Toe touch", 5),
            new("sitreach.practice", "Sit & reach practice", 10),
        ]),
    ];

    public static int TotalTaskCount { get; } = All.Sum(e => e.Tasks.Count);

    public static Exercise? Find(string? id) => All.FirstOrDefault(e => e.Id == id);

    public static Exercise For(FitnessTest test) => All.First(e => e.Test == test);
}
