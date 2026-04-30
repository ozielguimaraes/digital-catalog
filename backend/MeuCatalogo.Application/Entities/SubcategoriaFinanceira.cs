namespace MeuCatalogo.Application.Entities;

public class SubcategoriaFinanceira : BaseEntity
{
    public Guid CategoriaFinanceiraId { get; set; }
    public CategoriaFinanceira? CategoriaFinanceira { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string? IconeNome { get; set; }
    public string? Cor { get; set; }
    public byte? Ordem { get; set; }
}
