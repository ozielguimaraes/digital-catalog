namespace MeuCatalogo.Features.Pedido.Domain;

public sealed record PedidoInfo
{
    public required Guid Id { get; init; }
    public required Guid ClienteId { get; init; }
    public required string ClienteNome { get; init; }
    public int QuantidadeItens { get; init; }
    public decimal ValorTotal { get; init; }
    public PedidoStatus Status { get; init; }
    public DateTime DataCriacao { get; init; }

    public string Numero => $"#{Id.ToString("N")[..6].ToUpperInvariant()}";

    public string DataLabel
    {
        get
        {
            var local = DataCriacao.ToLocalTime();
            return $"{local:dd MMM}".ToLowerInvariant();
        }
    }
}
