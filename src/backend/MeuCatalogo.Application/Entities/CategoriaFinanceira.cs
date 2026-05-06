namespace MeuCatalogo.Application.Entities;

public enum CategoriaFinanceiraTipo
{
    Receita,
    Despesa
}

public class CategoriaFinanceira : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public CategoriaFinanceiraTipo Tipo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string IconeNome { get; set; } = "tag";
    public string Cor { get; set; } = "#9E9E9E";
    public byte? Ordem { get; set; }

    public ICollection<SubcategoriaFinanceira> Subcategorias { get; set; } = new List<SubcategoriaFinanceira>();
}
