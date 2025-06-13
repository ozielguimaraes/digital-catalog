namespace MeuCatalogo.Application.DTOs;

public class CatalogoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public ICollection<ProdutoDto>? Produtos { get; set; }
}

public class CatalogoCreateDto
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
}

public class CatalogoUpdateDto
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
}
