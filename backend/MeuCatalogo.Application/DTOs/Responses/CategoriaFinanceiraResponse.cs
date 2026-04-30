using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.DTOs.Responses;

public class CategoriaFinanceiraResponse
{
    public Guid Id { get; set; }
    public CategoriaFinanceiraTipo Tipo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string IconeNome { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public byte? Ordem { get; set; }
    public bool Ativo { get; set; }
    public List<SubcategoriaFinanceiraResponse> Subcategorias { get; set; } = new();
}

public class SubcategoriaFinanceiraResponse
{
    public Guid Id { get; set; }
    public Guid CategoriaFinanceiraId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? IconeNome { get; set; }
    public string? Cor { get; set; }
    public byte? Ordem { get; set; }
    public bool Ativo { get; set; }
}
