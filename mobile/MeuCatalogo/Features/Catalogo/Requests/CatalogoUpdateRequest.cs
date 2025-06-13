namespace MeuCatalogo.Features.Catalogo.Requests;

public class CatalogoUpdateRequest
{
    public CatalogoUpdateRequest(Guid id, string nome)
    {
        Id = id;
        Nome = nome;
    }

    public Guid Id { get; init; }
    public string Nome { get; init; }
}
