namespace MeuCatalogo.Features.Produto.Requests;

public sealed class ProdutoUpdateRequest(
    string nome,
    Guid categoriaId,
    decimal preco,
    decimal? precoComDesconto,
    string? informacoesAdicionais)
{
    public string Nome { get; set; } = nome;
    public Guid CategoriaId { get; set; } = categoriaId;
    public decimal Preco { get; set; } = preco;
    public decimal? PrecoComDesconto { get; set; } = precoComDesconto;
    public string? InformacoesAdicionais { get; set; } = informacoesAdicionais;
}
