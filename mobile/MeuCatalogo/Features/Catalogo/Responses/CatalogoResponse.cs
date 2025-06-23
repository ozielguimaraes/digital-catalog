namespace MeuCatalogo.Features.Catalogo.Responses;

public sealed class CatalogoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string NomeCurto { get; set; }
    public string Email { get; set; }
    public string NumeroWhatsapp { get; set; }
    public string Descricao { get; set; }
}
