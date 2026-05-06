using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.DTOs.Responses;

public class ContaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public ContaTipo Tipo { get; set; }
    public string Cor { get; set; } = string.Empty;
    public byte? Ordem { get; set; }
    public decimal? Limite { get; set; }
    public byte? DiaFechamento { get; set; }
    public byte? DiaVencimento { get; set; }
    public decimal SaldoInicial { get; set; }
    public bool Ativo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
