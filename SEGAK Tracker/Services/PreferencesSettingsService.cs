using SegakTracker.Core.Models;
using SegakTracker.Core.Services;

namespace SEGAK_Tracker.Services;

/// <summary>Stores small settings in the platform key/value store (SharedPreferences / NSUserDefaults).</summary>
public sealed class PreferencesSettingsService(IPreferences preferences) : ISettingsService
{
    private const string OnboardingKey = "onboarding_completed";
    private const string GenderKey = "last_gender";
    private const string AgeKey = "last_age";

    public bool HasCompletedOnboarding
    {
        get => preferences.Get(OnboardingKey, false);
        set => preferences.Set(OnboardingKey, value);
    }

    public Gender? LastGender
    {
        get => preferences.Get(GenderKey, -1) is var value && Enum.IsDefined(typeof(Gender), value) ? (Gender)value : null;
        set
        {
            if (value is null)
            {
                preferences.Remove(GenderKey);
            }
            else
            {
                preferences.Set(GenderKey, (int)value.Value);
            }
        }
    }

    public int? LastAge
    {
        get => preferences.Get(AgeKey, -1) is var value and >= 0 ? value : null;
        set
        {
            if (value is null)
            {
                preferences.Remove(AgeKey);
            }
            else
            {
                preferences.Set(AgeKey, value.Value);
            }
        }
    }

    public void Clear()
    {
        preferences.Remove(OnboardingKey);
        preferences.Remove(GenderKey);
        preferences.Remove(AgeKey);
    }
}
