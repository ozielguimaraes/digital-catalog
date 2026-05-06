using CommunityToolkit.Mvvm.ComponentModel;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Pedido;

public sealed partial class PedidoNovoItemModel : ObservableObject
{
    [ObservableProperty] private ProdutoResponse _produto;
    [ObservableProperty] private int _quantidade = 1;
    [ObservableProperty] private decimal _subtotal;

    public PedidoNovoItemModel(ProdutoResponse produto)
    {
        _produto = produto;
        Recalcular();
    }

    public decimal PrecoUnitario => Produto.PrecoComDesconto ?? Produto.Preco;

    partial void OnQuantidadeChanged(int value) => Recalcular();
    partial void OnProdutoChanged(ProdutoResponse value) => Recalcular();

    private void Recalcular() => Subtotal = PrecoUnitario * Math.Max(0, Quantidade);
}
