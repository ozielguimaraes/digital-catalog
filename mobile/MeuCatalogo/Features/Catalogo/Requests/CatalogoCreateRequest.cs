namespace MeuCatalogo.Features.Catalogo.Requests;

public sealed class CatalogoCreateRequest
{
    public CatalogoCreateRequest(string nome, string nomeCurto, string numeroWhatsapp, string email, string descricao)
    {
        Nome = nome;
        NomeCurto = nomeCurto;
        NumeroWhatsapp = numeroWhatsapp;
        Email = email;
        Descricao = descricao;
    }

    public string Nome { get; init; }
    public string NomeCurto { get; init; }
    public string NumeroWhatsapp { get; init; }
    public string Email { get; init; }
    public string Descricao { get; init; }
}
