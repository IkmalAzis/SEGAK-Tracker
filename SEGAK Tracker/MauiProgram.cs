using Microsoft.Extensions.Logging;
using SEGAK_Tracker.Services;
using SegakTracker.Core.Data;
using SegakTracker.Core.Services;
using SegakTracker.Core.ViewModels;

namespace SEGAK_Tracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var services = builder.Services;

            // Platform services
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton(Preferences.Default);
            services.AddSingleton<ISettingsService, PreferencesSettingsService>();
            services.AddSingleton<IDialogService, MauiDialogService>();
            services.AddSingleton<INavigationService, ShellNavigationService>();

            // Local database, kept in the app's private data folder.
            services.AddSingleton(_ => new SegakDatabase(Path.Combine(FileSystem.AppDataDirectory, SegakDatabase.FileName)));
            services.AddSingleton<IFitnessRecordRepository, SqliteFitnessRecordRepository>();
            services.AddSingleton<ITrainingProgressRepository, SqliteTrainingProgressRepository>();

            // Shell and pages (Shell resolves registered pages through DI).
            services.AddTransient<AppShell>();
            services.AddTransient<OnboardingPage>().AddTransient<OnboardingViewModel>();
            services.AddTransient<HomePage>().AddTransient<HomeViewModel>();
            services.AddTransient<FitnessTrackingPage>().AddTransient<FitnessTrackingViewModel>();
            services.AddTransient<TrainingProgramPage>().AddTransient<TrainingProgramViewModel>();
            services.AddTransient<ExerciseDetailPage>().AddTransient<ExerciseDetailViewModel>();
            services.AddTransient<ProgressListPage>().AddTransient<ProgressViewModel>();

            return builder.Build();
        }
    }
}
