using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.DTOs.Requests;

public class CategoriaFinanceiraRequest
{
    public CategoriaFinanceiraTipo Tipo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string IconeNome { get; set; } = "tag";
    public string Cor { get; set; } = "#9E9E9E";
    public byte? Ordem { get; set; }
}

public class SubcategoriaFinanceiraRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? IconeNome { get; set; }
    public string? Cor { get; set; }
    public byte? Ordem { get; set; }
}
