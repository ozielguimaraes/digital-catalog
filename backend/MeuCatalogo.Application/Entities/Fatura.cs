namespace MeuCatalogo.Application.Entities;

public class Fatura : BaseEntity
{
    public Guid ContaId { get; set; }
    public Conta? Conta { get; set; }

    public int Mes { get; set; }
    public int Ano { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public DateTime DataVencimento { get; set; }
    public decimal? ValorPago { get; set; }

    public ICollection<Lancamento> Lancamentos { get; set; } = new List<Lancamento>();
}
