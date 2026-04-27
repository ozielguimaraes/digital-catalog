using System.Globalization;
using MeuCatalogo.Features.Pedido.Domain;

namespace MeuCatalogo.Features.Pedido;

public sealed class PedidoStatusLabelConverter : IValueConverter
{
    public static readonly PedidoStatusLabelConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is PedidoStatus status ? PedidoStatusInfo.Label(status).ToUpperInvariant() : "—";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
