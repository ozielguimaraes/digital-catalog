using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Catalogo;

public sealed partial class CatalogoPublicaPageViewModel : BasePageViewModel, IQueryAttributable
{
    private readonly ILogger<CatalogoPublicaPageViewModel> _logger;
    private readonly IProdutoLocalRepository _produtoLocalRepository;
    private readonly INavigationService _navigationService;

    public CatalogoPublicaPageViewModel(
        ILogger<CatalogoPublicaPageViewModel> logger,
        IProdutoLocalRepository produtoLocalRepository,
        INavigationService navigationService)
    {
        _logger = logger;
        _produtoLocalRepository = produtoLocalRepository;
        _navigationService = navigationService;
        Produtos = [];
    }

    [ObservableProperty] private CatalogoInfo? _catalogo;
    [ObservableProperty] private string _nome = string.Empty;
    [ObservableProperty] private string? _nomeCurto;
    [ObservableProperty] private ObservableCollection<ProdutoEntity> _produtos;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isEmpty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Catalogo", out var value) && value is CatalogoInfo info)
        {
            Catalogo = info;
            Nome = info.Nome;
            NomeCurto = info.NomeCurto;
            _ = LoadProdutosAsync(info.Id);
        }
    }

    private async Task LoadProdutosAsync(Guid catalogoId)
    {
        try
        {
            IsLoading = true;
            var produtos = await _produtoLocalRepository.GetByCatalogoIdAsync(catalogoId.ToString());
            Produtos.Clear();
            foreach (var p in produtos)
                Produtos.Add(p);

            IsEmpty = Produtos.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar visualização pública do catálogo {Id}", catalogoId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Voltar()
    {
        await _navigationService.PopAsync();
    }
}
