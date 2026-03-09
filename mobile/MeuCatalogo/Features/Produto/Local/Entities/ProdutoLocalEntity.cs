using SQLite;

namespace MeuCatalogo.Features.Produto.Local.Entities;

[Table("produtos")]
public sealed class ProdutoLocalEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid CatalogoId { get; set; }

    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public decimal? PrecoComDesconto { get; set; }
    public string? InformacoesAdicionais { get; set; }
    public Guid CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public int? EstoqueQuantidade { get; set; }
    public int? EstoqueQuantidadeMinima { get; set; }
    public int? EstoqueQuantidadeMaxima { get; set; }
    public bool EstoqueDisponivel { get; set; }
    public bool EstoqueEhIlimitado { get; set; }
    public int SyncStatus { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
