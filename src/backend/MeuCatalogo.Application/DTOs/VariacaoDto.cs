using System;

namespace MeuCatalogo.Application.DTOs;

public class VariacaoDto
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public Guid TipoVariacaoId { get; set; }
    public string TipoNome { get; set; }
    public Guid OpcaoVariacaoId { get; set; }
    public string Valor { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class VariacaoCreateDto
{
    public Guid TipoVariacaoId { get; set; }
    public Guid OpcaoVariacaoId { get; set; }
}

public class VariacaoUpdateDto
{
    public Guid TipoVariacaoId { get; set; }
    public Guid OpcaoVariacaoId { get; set; }
}
