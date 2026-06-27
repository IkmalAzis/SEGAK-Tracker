using Microsoft.Maui.Controls;
using System;
using System.Windows.Input;

namespace SEGAK_Tracker;

public partial class OnboardingPage : ContentPage
{
    public ICommand StartCommand { get; }

    public OnboardingPage()
    {
        InitializeComponent();
        StartCommand = new Command(Start);
        BindingContext = this;
    }

    private async void Start()
    {
        try
        {
            await Shell.Current.GoToAsync("///mainPage");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Navigation failed: {ex.Message}");
        }
    }


}
