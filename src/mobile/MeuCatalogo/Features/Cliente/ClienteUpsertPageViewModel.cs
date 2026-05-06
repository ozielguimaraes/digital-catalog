using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Cliente.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Cliente.Domain;
using MeuCatalogo.Features.Cliente.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Cliente;

public sealed partial class ClienteUpsertPageViewModel(
    ILogger<ClienteUpsertPageViewModel> logger,
    UpsertClienteUseCase upsertClienteUseCase,
    INavigationService navigationService) : BasePageViewModel, IQueryAttributable
{
    [ObservableProperty] private Guid? _id;
    [ObservableProperty] private string _titulo = "Novo cliente";

    [ObservableProperty] private string _nome = string.Empty;
    [ObservableProperty] private string? _nomeErrorMessage;

    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _emailErrorMessage;

    [ObservableProperty] private string? _telefone;

    [ObservableProperty] private string? _informacoesAdicionais;

    [ObservableProperty] private bool _isSaving;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("Cliente", out var v) || v is not ClienteInfo c) return;
        Id = c.Id;
        Nome = c.Nome;
        Email = c.Email;
        Telefone = c.Telefone;
        InformacoesAdicionais = c.InformacoesAdicionais;
        Titulo = "Editar cliente";
    }

    partial void OnNomeChanged(string value)
        => NomeErrorMessage = string.IsNullOrWhiteSpace(value) ? "Informe o nome" : null;

    partial void OnEmailChanged(string? value)
        => EmailErrorMessage = IsValidEmail(value) ? null : "E-mail inválido";

    [RelayCommand]
    private async Task Salvar()
    {
        try
        {
            if (IsSaving) return;

            OnNomeChanged(Nome);
            OnEmailChanged(Email);
            if (NomeErrorMessage is not null || EmailErrorMessage is not null) return;

            if (!HasInternetConnection())
            {
                await Toast.Make("Sem conexão com a internet", ToastDuration.Long).Show();
                return;
            }

            IsSaving = true;

            var request = new ClienteUpsertRequest
            {
                Nome = Nome.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email!.Trim(),
                Telefone = string.IsNullOrWhiteSpace(Telefone) ? null : Telefone,
                InformacoesAdicionais = string.IsNullOrWhiteSpace(InformacoesAdicionais) ? "—" : InformacoesAdicionais
            };

            var resp = await upsertClienteUseCase.ExecuteAsync(Id, request);
            if (resp.RetornouComErro)
            {
                await Toast.Make(resp.MensagemDeErro ?? "Erro ao salvar cliente", ToastDuration.Long).Show();
                return;
            }

            await navigationService.PopAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao salvar cliente");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private Task Voltar() => navigationService.PopAsync();

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return true;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch { return false; }
    }
}
