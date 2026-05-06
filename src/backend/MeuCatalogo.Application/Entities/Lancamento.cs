namespace MeuCatalogo.Application.Entities;

public enum LancamentoTipo
{
    Receber,
    Pagar
}

public enum LancamentoStatus
{
    Pendente,
    Pago,
    Atrasado,
    Cancelado,
    Parcial
}

public enum LancamentoOperacao
{
    Simples,
    Transferencia
}

public enum LancamentoTipoTransferencia
{
    EntreContas,
    PagamentoFatura
}

public class Lancamento : BaseEntity
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public LancamentoTipo Tipo { get; set; }
    public LancamentoStatus Status { get; set; } = LancamentoStatus.Pendente;
    public string? Observacoes { get; set; }

    public Guid? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public Guid? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public Guid? ContaId { get; set; }
    public Conta? Conta { get; set; }

    public Guid? CategoriaFinanceiraId { get; set; }
    public CategoriaFinanceira? CategoriaFinanceira { get; set; }

    public Guid? SubcategoriaFinanceiraId { get; set; }
    public SubcategoriaFinanceira? SubcategoriaFinanceira { get; set; }

    public LancamentoOperacao Operacao { get; set; } = LancamentoOperacao.Simples;
    public LancamentoTipoTransferencia? TipoTransferencia { get; set; }

    public Guid? LancamentoTransferenciaId { get; set; }
    public Lancamento? LancamentoTransferencia { get; set; }

    public short? ParcelaAtual { get; set; }
    public short? ParcelaTotal { get; set; }

    public Guid? FaturaId { get; set; }
    public Fatura? Fatura { get; set; }

    public Guid? RecorrenciaId { get; set; }
    public Recorrencia? Recorrencia { get; set; }

    public Guid? ComprovanteFinanceiroId { get; set; }
    public ComprovanteFinanceiro? ComprovanteFinanceiro { get; set; }

    public bool Realizado { get; set; }

    public ICollection<LancamentoBaixa> Baixas { get; set; } = new List<LancamentoBaixa>();
}
