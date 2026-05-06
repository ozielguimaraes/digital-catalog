using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.DTOs.Responses;

public class LancamentoResponse
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public LancamentoTipo Tipo { get; set; }
    public LancamentoStatus Status { get; set; }
    public string? Observacoes { get; set; }
    public Guid? PedidoId { get; set; }
    public Guid? FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Guid? ContaId { get; set; }
    public string? ContaNome { get; set; }

    public Guid? CategoriaFinanceiraId { get; set; }
    public string? CategoriaFinanceiraNome { get; set; }
    public string? CategoriaFinanceiraIcone { get; set; }
    public string? CategoriaFinanceiraCor { get; set; }

    public Guid? SubcategoriaFinanceiraId { get; set; }
    public string? SubcategoriaFinanceiraNome { get; set; }

    public LancamentoOperacao Operacao { get; set; }
    public LancamentoTipoTransferencia? TipoTransferencia { get; set; }
    public Guid? LancamentoTransferenciaId { get; set; }

    public short? ParcelaAtual { get; set; }
    public short? ParcelaTotal { get; set; }

    public Guid? FaturaId { get; set; }
    public Guid? RecorrenciaId { get; set; }
    public Guid? ComprovanteFinanceiroId { get; set; }

    public bool Realizado { get; set; }
    public decimal ValorBaixado { get; set; }
    public decimal ValorEmAberto { get; set; }
}

public class FinanceiroResumoResponse
{
    public decimal TotalAReceber { get; set; }
    public decimal TotalAPagar { get; set; }
    public decimal SaldoPrevisto { get; set; }
    public decimal RecebidoNoMes { get; set; }
    public decimal PagoNoMes { get; set; }
    public string PeriodoLabel { get; set; } = string.Empty;
}
