namespace MeuCatalogo.Application.Entities;

public enum RecorrenciaPeriodicidade
{
    Mensal,
    Semanal,
    Anual
}

public class Recorrencia : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public CategoriaFinanceiraTipo Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public Guid ContaId { get; set; }
    public Conta? Conta { get; set; }

    public Guid CategoriaFinanceiraId { get; set; }
    public CategoriaFinanceira? CategoriaFinanceira { get; set; }

    public Guid? SubcategoriaFinanceiraId { get; set; }
    public SubcategoriaFinanceira? SubcategoriaFinanceira { get; set; }

    public decimal? ValorPadrao { get; set; }

    public RecorrenciaPeriodicidade Periodicidade { get; set; }
    public byte? DiaDoMes { get; set; }
    public DayOfWeek? DiaDaSemana { get; set; }

    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public DateTime ProximaData { get; set; }
}
