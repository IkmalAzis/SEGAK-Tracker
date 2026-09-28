using SegakTracker.Core.Services;

namespace SEGAK_Tracker.Services;

public sealed class ShellNavigationService(IServiceProvider services) : INavigationService
{
    public Task ShowMainAppAsync() =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (Application.Current is { } app)
            {
                app.MainPage = services.GetRequiredService<AppShell>();
            }
        });

    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null) =>
        MainThread.InvokeOnMainThreadAsync(() => parameters is null
            ? Shell.Current.GoToAsync(route)
            : Shell.Current.GoToAsync(route, parameters));

    public Task GoBackAsync() =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(".."));
}
