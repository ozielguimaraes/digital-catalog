namespace MeuCatalogo.Features.Fornecedor.Data.Remote.Contracts.Requests;

public sealed record FornecedorUpsertRequest
{
    public required string Nome { get; init; }
    public string? Categoria { get; init; }
    public string? NomeContato { get; init; }
    public string? Email { get; init; }
    public string? Telefone { get; init; }
    public string? Documento { get; init; }
    public string? Observacoes { get; init; }
}
