using SegakTracker.Core.Services;

namespace SEGAK_Tracker
{
    public partial class App : Application
    {
        public App(IServiceProvider services, ISettingsService settings)
        {
            InitializeComponent();

            // The design is dark-only, so keep system controls (entries, alerts) dark as well.
            UserAppTheme = AppTheme.Dark;

            // Onboarding is only shown until the student taps "Begin Your Journey" once.
            MainPage = settings.HasCompletedOnboarding
                ? services.GetRequiredService<AppShell>()
                : services.GetRequiredService<OnboardingPage>();
        }
    }
}
