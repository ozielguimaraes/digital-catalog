using System.Globalization;

namespace MeuCatalogo.Features.Catalogo;

public sealed class NotNullOrEmptyConverter : IValueConverter
{
    public static readonly NotNullOrEmptyConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
