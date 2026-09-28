using SegakTracker.Core.Services;

namespace SEGAK_Tracker.Services;

public sealed class MauiDialogService : IDialogService
{
    public Task ShowAlertAsync(string title, string message, string cancel = "OK") =>
        MainThread.InvokeOnMainThreadAsync(() => CurrentPage.DisplayAlert(title, message, cancel));

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel) =>
        MainThread.InvokeOnMainThreadAsync(() => CurrentPage.DisplayAlert(title, message, accept, cancel));

    private static Page CurrentPage =>
        Shell.Current as Page
        ?? Application.Current?.MainPage
        ?? throw new InvalidOperationException("No page is available to show a dialog.");
}
