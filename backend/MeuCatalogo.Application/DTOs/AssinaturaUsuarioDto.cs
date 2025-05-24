using System;

namespace MeuCatalogo.Application.DTOs;

public class AssinaturaUsuarioDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public Guid PlanoAssinaturaId { get; set; }
    public string PlanoAssinaturaNome { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public bool Ativa { get; set; }
    public string? TransacaoId { get; set; }
    public string? MetodoPagamento { get; set; }
    public decimal ValorPago { get; set; }
    public bool RenovacaoAutomatica { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public string? MotivoCancelamento { get; set; }
    public PlanoAssinaturaDto PlanoAssinatura { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class AssinaturaUsuarioCreateDto
{
    public string UserId { get; set; }
    public Guid PlanoAssinaturaId { get; set; }
    public DateTime DataFim { get; set; }
    public bool RenovacaoAutomatica { get; set; }
    public string? TransacaoId { get; set; }
    public string? MetodoPagamento { get; set; }
    public decimal ValorPago { get; set; }
}