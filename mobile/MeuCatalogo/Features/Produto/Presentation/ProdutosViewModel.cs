using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Produto.Domain;
using MeuCatalogo.Features.Produto.UseCases;

namespace MeuCatalogo.Features.Produto.Presentation;

public partial class ProdutosViewModel : ObservableObject
{
    private readonly GetProdutosUseCase _getProdutosUseCase;
    private readonly CreateProdutoUseCase _createProdutoUseCase;

    public ProdutosViewModel(GetProdutosUseCase getProdutosUseCase, CreateProdutoUseCase createProdutoUseCase)
    {
        _getProdutosUseCase = getProdutosUseCase;
        _createProdutoUseCase = createProdutoUseCase;
        Produtos = new ObservableCollection<ProdutoEntity>();
    }

    [ObservableProperty]
    private ObservableCollection<ProdutoEntity> _produtos;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _novoProdutoNome = string.Empty;

    [ObservableProperty]
    private decimal _novoProdutoPreco;

    [RelayCommand]
    private async Task LoadProdutosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            // The UseCase handles the logic of querying the local DB
            // and managing background sync
            var result = await _getProdutosUseCase.ExecuteAsync(new GetProdutosRequest());

            Produtos.Clear();
            foreach (var produto in result)
            {
                Produtos.Add(produto);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CriarProdutoAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoProdutoNome)) return;

        try
        {
            IsBusy = true;

            var request = new CreateProdutoRequest
            {
                Nome = NovoProdutoNome,
                Preco = NovoProdutoPreco
            };

            // Saves locally and enqueues sync request. UI unblocks instantly.
            var novoProduto = await _createProdutoUseCase.ExecuteAsync(request);

            // Optimistic UI update
            Produtos.Add(novoProduto);

            NovoProdutoNome = string.Empty;
            NovoProdutoPreco = 0;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
