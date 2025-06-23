using System;

namespace MeuCatalogo.Application.DTOs;

public class VariacaoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public OpcaoVariacaoDto OpcaoVariacao { get; set; }
    public Guid ProdutoId { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class VariacaoCreateDto
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public Guid ProdutoId { get; set; }
}

public class VariacaoUpdateDto
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal? PrecoAdicional { get; set; }
}
