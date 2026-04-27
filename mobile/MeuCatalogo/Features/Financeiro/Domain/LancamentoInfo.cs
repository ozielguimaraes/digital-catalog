namespace MeuCatalogo.Features.Financeiro.Domain;

public enum LancamentoTipo { Receber, Pagar }

public enum LancamentoStatus { Pendente, Pago, Atrasado, Cancelado }

public sealed record LancamentoInfo
{
    public required Guid Id { get; init; }
    public required string Descricao { get; init; }
    public decimal Valor { get; init; }
    public DateTime DataVencimento { get; init; }
    public DateTime? DataPagamento { get; init; }
    public LancamentoTipo Tipo { get; init; }
    public LancamentoStatus Status { get; init; }
    public string? FornecedorNome { get; init; }
    public Guid? PedidoId { get; init; }

    public string VencimentoLabel
    {
        get
        {
            var local = DataVencimento.ToLocalTime();
            return Status switch
            {
                LancamentoStatus.Pago when DataPagamento.HasValue => $"pago em {DataPagamento.Value.ToLocalTime():dd MMM}".ToLowerInvariant(),
                LancamentoStatus.Atrasado => $"vencido {local:dd MMM}".ToLowerInvariant(),
                _ => $"vence {local:dd MMM}".ToLowerInvariant()
            };
        }
    }
}

public sealed record FinanceiroResumoInfo
{
    public decimal TotalAReceber { get; init; }
    public decimal TotalAPagar { get; init; }
    public decimal SaldoPrevisto { get; init; }
    public decimal RecebidoNoMes { get; init; }
    public decimal PagoNoMes { get; init; }
    public string PeriodoLabel { get; init; } = string.Empty;
}
