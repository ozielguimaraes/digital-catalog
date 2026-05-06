using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.DTOs.Requests;

public class RecorrenciaRequest
{
    public CategoriaFinanceiraTipo Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public Guid ContaId { get; set; }
    public Guid CategoriaFinanceiraId { get; set; }
    public Guid? SubcategoriaFinanceiraId { get; set; }
    public decimal? ValorPadrao { get; set; }
    public RecorrenciaPeriodicidade Periodicidade { get; set; }
    public byte? DiaDoMes { get; set; }
    public DayOfWeek? DiaDaSemana { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}
