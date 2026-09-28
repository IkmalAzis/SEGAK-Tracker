using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SegakTracker.Core.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    /// <summary>Formats a measurement without trailing zeros: 31 or 31.5.</summary>
    protected internal static string FormatValue(double value) => value.ToString("0.#", CultureInfo.CurrentCulture);

    protected static DateOnly Today(TimeProvider timeProvider) => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
}
