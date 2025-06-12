using CommunityToolkit.Mvvm.ComponentModel;
using MeuCatalogo.Extensions;
using Microsoft.Maui.Networking;

namespace MeuCatalogo.Features;

public abstract partial class BasePageViewModel : ObservableObject
{
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _title;

    protected bool HasInternetConnection() => Connectivity.NetworkAccess.HasInternetConnection();

    partial void SetupTitle();
}
