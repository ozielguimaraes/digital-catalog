namespace MeuCatalogo.Features.Fornecedor.Domain;

public sealed record FornecedorInfo
{
    public required Guid Id { get; init; }
    public required string Nome { get; init; }
    public string? Categoria { get; init; }
    public string? NomeContato { get; init; }
    public string? Email { get; init; }
    public string? Telefone { get; init; }
    public string? Documento { get; init; }
    public string? Observacoes { get; init; }
}
