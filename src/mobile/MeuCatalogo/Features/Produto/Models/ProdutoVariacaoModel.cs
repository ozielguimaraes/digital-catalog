using CommunityToolkit.Mvvm.ComponentModel;

namespace MeuCatalogo.Features.Produto.Models;

// Linha editável de variação (cor × tamanho) no formulário de produto.
// Preço e quantidade são texto para facilitar o binding com Entry; a conversão
// (locale-aware) acontece no ViewModel ao salvar.
public sealed partial class ProdutoVariacaoModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty] private string? _cor;
    [ObservableProperty] private string? _tamanho;
    [ObservableProperty] private string _precoString = string.Empty;
    [ObservableProperty] private string _quantidadeString = string.Empty;
}
