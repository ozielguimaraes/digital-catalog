using System.Net;
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

    protected static List<string> ObterErros<T>(ApiResponse<T> response)
    {
        if (response.ProblemDetails is null)
            return response.MensagemDeErro is null ? [ "Houve um erro inesperado, favor entrar em contato com o suporte." ] : [response.MensagemDeErro];

        if (response.ProblemDetails.Status == (int)HttpStatusCode.Forbidden)
        {
            string? detail = response.ProblemDetails.Detail;

            return detail == null ? [] : [detail];
        }

        var erros = response.ProblemDetails.Errors
            .SelectMany(kvp => kvp.Value.Select(mensagem => $"{kvp.Key}: {mensagem}"));

        return erros.ToList();
    }
}
