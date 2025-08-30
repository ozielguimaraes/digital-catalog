using System.Globalization;
using MeuCatalogo.Features.Produto.Models;

namespace MeuCatalogo.Converters;

public class NullToDefaultTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var categoria = value as CategoriaModel;
        string textoPadrao = parameter as string ?? string.Empty;

        return categoria?.Nome ?? textoPadrao;
    }

    public object? ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
