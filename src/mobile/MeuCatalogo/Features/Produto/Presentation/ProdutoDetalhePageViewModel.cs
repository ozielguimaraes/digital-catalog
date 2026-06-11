using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Features.Produto.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Produto.Presentation;

public sealed partial class ProdutoDetalhePageViewModel : BasePageViewModel, IQueryAttributable
{
    private readonly ILogger<ProdutoDetalhePageViewModel> _logger;
    private readonly GetProdutoForEditOfflineFirstUseCase _getProdutoForEditOfflineFirstUseCase;
    private readonly INavigationService _navigationService;
    private ProdutoResponse? _produtoCarregado;

    public ProdutoDetalhePageViewModel(
        ILogger<ProdutoDetalhePageViewModel> logger,
        GetProdutoForEditOfflineFirstUseCase getProdutoForEditOfflineFirstUseCase,
        INavigationService navigationService)
    {
        _logger = logger;
        _getProdutoForEditOfflineFirstUseCase = getProdutoForEditOfflineFirstUseCase;
        _navigationService = navigationService;
        Imagens = [];
    }

    [ObservableProperty] private ProdutoEntity? _entity;
    [ObservableProperty] private string _nome = string.Empty;
    [ObservableProperty] private decimal _preco;
    [ObservableProperty] private decimal? _precoComDesconto;
    [ObservableProperty] private string? _categoriaNome;
    [ObservableProperty] private string? _informacoesAdicionais;
    [ObservableProperty] private ObservableCollection<ProdutoImagemResponse> _imagens;
    [ObservableProperty] private string? _capaUrl;
    [ObservableProperty] private bool _temDesconto;
    [ObservableProperty] private bool _temGaleria;
    [ObservableProperty] private bool _temInformacoes;
    [ObservableProperty] private bool _temCategoria;
    [ObservableProperty] private bool _isLoading;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Produto", out var value) && value is ProdutoEntity entity)
        {
            Entity = entity;
            Nome = entity.Nome;
            Preco = entity.Preco;
            PrecoComDesconto = entity.PrecoComDesconto;
            CategoriaNome = entity.CategoriaNome;
            TemCategoria = !string.IsNullOrWhiteSpace(entity.CategoriaNome);
            TemDesconto = entity.PrecoComDesconto is > 0 && entity.PrecoComDesconto < entity.Preco;
            CapaUrl = entity.ThumbnailUrl;
            InformacoesAdicionais = entity.InformacoesAdicionais;
            TemInformacoes = !string.IsNullOrWhiteSpace(entity.InformacoesAdicionais);
            _ = LoadImagensAsync(entity);
        }
    }

    private async Task LoadImagensAsync(ProdutoEntity entity)
    {
        try
        {
            IsLoading = true;
            var response = await _getProdutoForEditOfflineFirstUseCase.ExecuteAsync(entity);
            _produtoCarregado = response;
            Imagens.Clear();
            foreach (var img in response.Imagens.OrderBy(i => i.Ordem))
                Imagens.Add(img);

            TemGaleria = Imagens.Count > 0;

            var principal = Imagens.FirstOrDefault(i => i.IsPrincipal) ?? Imagens.FirstOrDefault();
            if (principal is not null)
                CapaUrl = principal.Images?.Medium ?? principal.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar detalhe do produto {Id}", entity.Id);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Editar()
    {
        if (Entity is null) return;

        var response = _produtoCarregado ?? await _getProdutoForEditOfflineFirstUseCase.ExecuteAsync(Entity);

        // Substitui o Detalhe na pilha (pop antes de push) em vez de empilhar:
        // duas rotas globais empilhadas (Detalhe + Editar) travam o back no Shell.
        await _navigationService.PopAsync();
        await Shell.Current.GoToAsync(nameof(ProdutoAdicionarPage), true,
            new Dictionary<string, object> { { "Produto", response } });
    }

    [RelayCommand]
    private async Task Voltar()
    {
        await _navigationService.PopAsync();
    }
}
