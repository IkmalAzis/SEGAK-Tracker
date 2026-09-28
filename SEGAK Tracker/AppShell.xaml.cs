using SegakTracker.Core.Services;

namespace SEGAK_Tracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(Routes.ExerciseDetail, typeof(ExerciseDetailPage));
        }
    }
}
