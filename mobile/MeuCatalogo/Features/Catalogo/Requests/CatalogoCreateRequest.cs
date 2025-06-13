namespace MeuCatalogo.Features.Catalogo.Requests;

public class CatalogoCreateRequest
{
    public CatalogoCreateRequest(string nome)
    {
        Nome = nome;
    }

    public string Nome { get; init; }
}
