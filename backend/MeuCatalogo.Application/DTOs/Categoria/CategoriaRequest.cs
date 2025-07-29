namespace MeuCatalogo.Application.DTOs.Categoria;

public class CategoriaRequest
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public Guid CatalogoId { get; set; }
}
