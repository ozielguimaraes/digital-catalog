namespace MeuCatalogo.Application.DTOs;

public class EstoqueDto
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public int? Quantidade { get; set; }
    public int? QuantidadeMinima { get; set; }
    public int? QuantidadeMaxima { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class EstoqueCreateDto
{
    public int Quantidade { get; set; }
    public int? QuantidadeMinima { get; set; }
    public int? QuantidadeMaxima { get; set; }
}

public class EstoqueUpdateDto
{
    public int Quantidade { get; set; }
    public int? QuantidadeMinima { get; set; }
    public int? QuantidadeMaxima { get; set; }
}
