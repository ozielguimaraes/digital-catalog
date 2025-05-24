using System;

namespace MeuCatalogo.Application.Entities;

public class AssinaturaUsuario : BaseEntity
{
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public Guid PlanoAssinaturaId { get; set; }
    public PlanoAssinatura PlanoAssinatura { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public bool Ativa { get; set; }
    public string? TransacaoId { get; set; }
    public string? MetodoPagamento { get; set; }
    public decimal ValorPago { get; set; }
    public bool RenovacaoAutomatica { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public string? MotivoCancelamento { get; set; }

    public AssinaturaUsuario()
    {
        DataInicio = DateTime.UtcNow;
        Ativa = true;
    }

    public AssinaturaUsuario(string userId, Guid planoAssinaturaId, DateTime dataFim, 
        bool renovacaoAutomatica = false, string? transacaoId = null, 
        string? metodoPagamento = null, decimal valorPago = 0) : this()
    {
        UserId = userId;
        PlanoAssinaturaId = planoAssinaturaId;
        DataFim = dataFim;
        RenovacaoAutomatica = renovacaoAutomatica;
        TransacaoId = transacaoId;
        MetodoPagamento = metodoPagamento;
        ValorPago = valorPago;
    }

    public bool EstaAtiva()
    {
        return Ativa && DataFim >= DateTime.UtcNow && !DataCancelamento.HasValue;
    }

    public void Cancelar(string motivo)
    {
        Ativa = false;
        DataCancelamento = DateTime.UtcNow;
        MotivoCancelamento = motivo;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Renovar(DateTime novaDataFim, string? transacaoId = null, decimal valorPago = 0)
    {
        DataFim = novaDataFim;
        TransacaoId = transacaoId;
        ValorPago = valorPago;
        Ativa = true;
        DataCancelamento = null;
        MotivoCancelamento = null;
        DataAtualizacao = DateTime.UtcNow;
    }
}