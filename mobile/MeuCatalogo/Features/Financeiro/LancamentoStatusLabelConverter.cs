using System.Globalization;
using MeuCatalogo.Features.Financeiro.Domain;

namespace MeuCatalogo.Features.Financeiro;

public sealed class LancamentoStatusLabelConverter : IValueConverter
{
    public static readonly LancamentoStatusLabelConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is LancamentoStatus s ? Label(s) : "—";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static string Label(LancamentoStatus s) => s switch
    {
        LancamentoStatus.Pago => "PAGO",
        LancamentoStatus.Pendente => "PENDENTE",
        LancamentoStatus.Atrasado => "ATRASADO",
        LancamentoStatus.Cancelado => "CANCELADO",
        _ => "—"
    };
}
