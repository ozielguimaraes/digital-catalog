namespace MeuCatalogo.Application.DTOs;

public sealed class CatalogoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string NomeCurto { get; set; }
    public string NumeroWhatsapp { get; set; }
    public string Email { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public ICollection<ProdutoDto>? Produtos { get; set; }
}

public sealed class CatalogoCreateDto
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string NomeCurto { get; set; }
    public string NumeroWhatsapp { get; set; }
    public string Email { get; set; }
}

public sealed class CatalogoUpdateDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string NomeCurto { get; set; }
    public string NumeroWhatsapp { get; set; }
    public string Email { get; set; }
}
