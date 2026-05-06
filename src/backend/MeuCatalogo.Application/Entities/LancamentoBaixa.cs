namespace MeuCatalogo.Application.Entities;

public class LancamentoBaixa : BaseEntity
{
    public Guid LancamentoId { get; set; }
    public Lancamento? Lancamento { get; set; }

    public DateTime Data { get; set; }
    public decimal Valor { get; set; }

    public Guid ContaId { get; set; }
    public Conta? Conta { get; set; }

    public Guid? ComprovanteFinanceiroId { get; set; }
    public ComprovanteFinanceiro? ComprovanteFinanceiro { get; set; }

    public string? Observacoes { get; set; }
}
