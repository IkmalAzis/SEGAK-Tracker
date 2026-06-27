using Microsoft.Maui.Controls;
using System;

namespace SEGAK_Tracker
{
    public partial class MainPage : FlyoutPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // Ensure the method is inside the MainPage class
        private void OnMenuButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            Page page = null;

            switch (button.Text)
            {
                case "Fitness Tracking":
                    page = new FitnessTrackingPage(); // Ensure you have a page named FitnessTrackingPage
                    break;
                case "Training Program":
                    page = new TrainingProgramPage(); // Ensure you have a page named TrainingProgramPage
                    break;
                case "Progress List":
                    page = new ProgressListPage(); // Ensure you have a page named ProgressListPage
                    break;
            }

            if (page != null)
            {
                this.Detail = new NavigationPage(page);
                this.IsPresented = false;  // This will close the flyout menu
            }
        }
    }
}