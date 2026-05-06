using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Auth.UseCases;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Catalogo.UseCases;
using MeuCatalogo.Features.Produto;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Home;

public sealed partial class HomePageViewModel : BasePageViewModel
{
    private readonly ILogger<HomePageViewModel> _logger;
    private readonly GetCatalogosLocalUseCase _getCatalogosLocalUseCase;
    private readonly IProdutoLocalRepository _produtoLocalRepository;
    private readonly GetCurrentUserUseCase _getCurrentUserUseCase;
    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;

    public HomePageViewModel(
        ILogger<HomePageViewModel> logger,
        GetCatalogosLocalUseCase getCatalogosLocalUseCase,
        IProdutoLocalRepository produtoLocalRepository,
        GetCurrentUserUseCase getCurrentUserUseCase,
        ISettingsService settingsService,
        INavigationService navigationService)
    {
        _logger = logger;
        _getCatalogosLocalUseCase = getCatalogosLocalUseCase;
        _produtoLocalRepository = produtoLocalRepository;
        _getCurrentUserUseCase = getCurrentUserUseCase;
        _settingsService = settingsService;
        _navigationService = navigationService;
        ProdutosRecentes = [];
    }

    [ObservableProperty] private string _saudacao = "Olá";
    [ObservableProperty] private string _nomeUsuario = string.Empty;
    [ObservableProperty] private string? _catalogoEmUsoNome;
    [ObservableProperty] private int _totalCatalogos;
    [ObservableProperty] private int _totalProdutos;
    [ObservableProperty] private ObservableCollection<ProdutoEntity> _produtosRecentes;
    [ObservableProperty] private bool _temProdutosRecentes;
    [ObservableProperty] private bool _temCatalogoEmUso;
    [ObservableProperty] private bool _isRefreshing;

    [RelayCommand]
    private async Task Carregar()
    {
        try
        {
            IsBusy = true;

            var hora = DateTime.Now.Hour;
            Saudacao = hora switch
            {
                >= 5 and < 12 => "Bom dia",
                >= 12 and < 18 => "Boa tarde",
                _ => "Boa noite"
            };

            var user = await _getCurrentUserUseCase.ExecuteAsync();
            if (user is not null)
                NomeUsuario = user.Nome.Split(' ')[0];

            var catalogos = await _getCatalogosLocalUseCase.ExecuteAsync();
            TotalCatalogos = catalogos.Count;

            var emUso = _settingsService.CatalogoEmUso;
            TemCatalogoEmUso = emUso is not null;
            CatalogoEmUsoNome = emUso?.Nome;

            if (emUso is not null)
            {
                var produtos = (await _produtoLocalRepository.GetByCatalogoIdAsync(emUso.Id.ToString())).ToList();
                TotalProdutos = produtos.Count;

                ProdutosRecentes.Clear();
                foreach (var p in produtos.OrderByDescending(p => p.LastModified).Take(5))
                    ProdutosRecentes.Add(p);

                TemProdutosRecentes = ProdutosRecentes.Count > 0;
            }
            else
            {
                TotalProdutos = 0;
                ProdutosRecentes.Clear();
                TemProdutosRecentes = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar dashboard");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task VerCatalogos()
        => await _navigationService.NavigateToAsync($"//{nameof(CatalogoListaPage)}");

    [RelayCommand]
    private async Task VerProdutos()
        => await _navigationService.NavigateToAsync($"//{nameof(ProdutoListaPage)}");

    [RelayCommand]
    private async Task NovoCatalogo()
        => await _navigationService.NavigateToAsync(nameof(CatalogoAdicionarPage));

    [RelayCommand]
    private async Task NovoProduto()
        => await _navigationService.NavigateToAsync(nameof(ProdutoAdicionarPage));
}
