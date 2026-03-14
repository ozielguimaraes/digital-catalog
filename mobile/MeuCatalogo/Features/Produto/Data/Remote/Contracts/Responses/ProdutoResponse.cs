using MeuCatalogo.Domain.Enums;

namespace MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

public sealed record ProdutoResponse
{
    public Guid Id { get; init; }
    public required string Nome { get; init; }
    public decimal Preco { get; init; }
    public decimal? PrecoComDesconto { get; init; }
    public string? InformacoesAdicionais { get; init; }
    public Guid CategoriaId { get; init; }
    public required string CategoriaNome { get; init; }
    public Guid CatalogoId { get; init; }
    public EstoqueResponse? Estoque { get; init; }
    public List<ProdutoImagemResponse> Imagens { get; set; } = [];
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Completed;
}

public sealed record EstoqueResponse
{
    public Guid Id { get; init; }
    public Guid ProdutoId { get; init; }
    public int? Quantidade { get; init; }
    public int? QuantidadeMinima { get; init; }
    public int? QuantidadeMaxima { get; init; }
    public bool Disponivel { get; init; }
    public bool EhIlimitado { get; init; }
}

public sealed record ProdutoImagemResponse
{
    public Guid Id { get; init; }
    public required string Url { get; set; }
    public ProdutoImagemLinksResponse Images { get; init; } = new();
    public bool IsPrincipal { get; set; }
    public int Ordem { get; set; }
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Completed;
}

public sealed record ProdutoImagemLinksResponse
{
    public string Thumbnail { get; set; } = string.Empty;
    public string Medium { get; set; } = string.Empty;
    public string Full { get; set; } = string.Empty;
}
