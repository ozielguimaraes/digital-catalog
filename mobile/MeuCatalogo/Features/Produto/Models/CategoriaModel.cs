namespace MeuCatalogo.Features.Produto.Models;

public class CategoriaModel
{
    public CategoriaModel() { }

    public CategoriaModel(string nome, string descricao, Guid catalogoId)
    {
        Nome = nome;
        Descricao = descricao;
        CatalogoId = catalogoId;
    }

    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Guid CatalogoId { get; set; }
}
