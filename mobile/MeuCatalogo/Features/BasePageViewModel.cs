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

    public List<string> ObterErros<T>(ApiResponse<T> response)
    {
        if (response.ProblemDetails is null)
            return [];

        var erros = response.ProblemDetails.Errors
            .SelectMany(kvp => kvp.Value.Select(mensagem => $"{kvp.Key}: {mensagem}"));

        return erros.ToList();
    }
}
