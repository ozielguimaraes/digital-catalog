using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Catalogo;

public sealed partial class CatalogoDetalhePageViewModel : BasePageViewModel, IQueryAttributable
{
    private readonly ILogger<CatalogoDetalhePageViewModel> _logger;
    private readonly IProdutoLocalRepository _produtoLocalRepository;
    private readonly INavigationService _navigationService;

    public CatalogoDetalhePageViewModel(
        ILogger<CatalogoDetalhePageViewModel> logger,
        IProdutoLocalRepository produtoLocalRepository,
        INavigationService navigationService)
    {
        _logger = logger;
        _produtoLocalRepository = produtoLocalRepository;
        _navigationService = navigationService;
    }

    [ObservableProperty] private CatalogoInfo? _catalogo;
    [ObservableProperty] private string _nome = string.Empty;
    [ObservableProperty] private string? _nomeCurto;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _numeroWhatsapp;
    [ObservableProperty] private string? _descricao;
    [ObservableProperty] private int _totalProdutos;
    [ObservableProperty] private bool _hasContato;
    [ObservableProperty] private bool _isLoading;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Catalogo", out var value) && value is CatalogoInfo info)
        {
            Catalogo = info;
            Nome = info.Nome;
            NomeCurto = info.NomeCurto;
            Email = info.Email;
            NumeroWhatsapp = info.NumeroWhatsapp;
            Descricao = info.Descricao;
            HasContato = !string.IsNullOrWhiteSpace(info.Email) || !string.IsNullOrWhiteSpace(info.NumeroWhatsapp);
            _ = LoadProdutoCountAsync(info.Id);
        }
    }

    private async Task LoadProdutoCountAsync(Guid catalogoId)
    {
        try
        {
            IsLoading = true;
            var produtos = await _produtoLocalRepository.GetByCatalogoIdAsync(catalogoId.ToString());
            TotalProdutos = produtos.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contar produtos do catálogo {Id}", catalogoId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task VerProdutos()
    {
        await _navigationService.NavigateToAsync($"//{nameof(ProdutoListaPage)}");
    }

    [RelayCommand]
    private async Task VisualizarPublica()
    {
        if (Catalogo is null) return;

        await _navigationService.NavigateToAsync(nameof(CatalogoPublicaPage),
            new Dictionary<string, object> { { "Catalogo", Catalogo } });
    }

    [RelayCommand]
    private async Task Voltar()
    {
        await _navigationService.PopAsync();
    }
}
