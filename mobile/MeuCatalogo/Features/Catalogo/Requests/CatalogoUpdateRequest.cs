namespace MeuCatalogo.Features.Catalogo.Requests;

public class CatalogoUpdateRequest
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
}

public class CatalogoCreateRequest
{
    public string Nome { get; set; }
}
