using CommunityToolkit.Mvvm.ComponentModel;

namespace MeuCatalogo.Features;

public abstract partial class BasePageViewModel : ObservableObject
{
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _title;

    partial void SetupTitle();
}
