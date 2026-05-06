namespace MeuCatalogo.Features.Pedido.Domain;

// Mirrors backend StatusPedido enum (MeuCatalogo.Application.Entities.StatusPedido).
public enum PedidoStatus
{
    Novo,
    Confirmado,
    AguardandoPagamento,
    Pago,
    Preparando,
    ProntoParaRetirada,
    ProntoParaEntrega,
    ACaminho,
    Entregue,
    Finalizado,
    Cancelado
}

public static class PedidoStatusInfo
{
    public static string Label(PedidoStatus status) => status switch
    {
        PedidoStatus.Novo => "Novo",
        PedidoStatus.Confirmado => "Confirmado",
        PedidoStatus.AguardandoPagamento => "Aguardando pgto.",
        PedidoStatus.Pago => "Pago",
        PedidoStatus.Preparando => "Em produção",
        PedidoStatus.ProntoParaRetirada => "Pronto p/ retirada",
        PedidoStatus.ProntoParaEntrega => "Pronto p/ entrega",
        PedidoStatus.ACaminho => "A caminho",
        PedidoStatus.Entregue => "Entregue",
        PedidoStatus.Finalizado => "Finalizado",
        PedidoStatus.Cancelado => "Cancelado",
        _ => "—"
    };
}
