using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.DTOs.Requests;

public class ContaRequest
{
    public string Nome { get; set; } = string.Empty;
    public ContaTipo Tipo { get; set; }
    public string Cor { get; set; } = "#3F51B5";
    public byte? Ordem { get; set; }
    public decimal? Limite { get; set; }
    public byte? DiaFechamento { get; set; }
    public byte? DiaVencimento { get; set; }
    public decimal SaldoInicial { get; set; }
}
