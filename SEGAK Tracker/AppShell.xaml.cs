namespace SEGAK_Tracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("mainPage", typeof(MainPage));
            Routing.RegisterRoute("onboardingPage", typeof(OnboardingPage));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await Shell.Current.GoToAsync("onboardingPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during navigation: {ex.Message}");
            }
        }
    }

}
