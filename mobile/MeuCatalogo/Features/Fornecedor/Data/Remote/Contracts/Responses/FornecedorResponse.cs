namespace MeuCatalogo.Features.Fornecedor.Data.Remote.Contracts.Responses;

public sealed record FornecedorResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Categoria { get; init; }
    public string? NomeContato { get; init; }
    public string? Email { get; init; }
    public string? Telefone { get; init; }
    public string? Documento { get; init; }
    public string? Observacoes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
