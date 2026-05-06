namespace MeuCatalogo.Features.Cliente.Domain;

public sealed record ClienteInfo
{
    public required Guid Id { get; init; }
    public required string Nome { get; init; }
    public string? Email { get; init; }
    public string? Telefone { get; init; }
    public string? InformacoesAdicionais { get; init; }

    public string Iniciais
    {
        get
        {
            var partes = Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return "?";
            if (partes.Length == 1) return partes[0][..1].ToUpperInvariant();
            return string.Concat(partes[0][..1], partes[^1][..1]).ToUpperInvariant();
        }
    }
}
